using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Services
{
    public class ApkBuilderService
    {
        private readonly string _flutterProjectPath = @"D:\Desarrollo\Flutter\herramienta_case\herramienta_case";

        public async Task<ApkBuildResult> GenerarApkAsync(string dbName, string jsonSchema)
        {
            string pathConfigFile = Path.Combine(_flutterProjectPath, "lib", "core", "config", "app_build_config.dart");

            if (!File.Exists(pathConfigFile))
                throw new FileNotFoundException($"No se encontró el archivo de configuración en: {pathConfigFile}");

            string contenidoOriginal = await File.ReadAllTextAsync(pathConfigFile);

            try
            {
                string nuevoContenido = contenidoOriginal
                    .Replace("{{DB_NAME_PLACEHOLDER}}", dbName)
                    .Replace("{{SCHEMA_JSON_PLACEHOLDER}}", jsonSchema);

                await File.WriteAllTextAsync(pathConfigFile, nuevoContenido);

                // Generar APK
                string apkPath = await RunFlutterBuild(useClean: false);

                // Exportar código fuente de la solución .NET
                string zipPath = await ExportarCodigoFuenteAsync(dbName);

                return new ApkBuildResult
                {
                    ApkPath = apkPath,
                    SourceCodeZipPath = zipPath
                };
            }
            finally
            {
                await File.WriteAllTextAsync(pathConfigFile, contenidoOriginal);
            }
        }

        private async Task<string> ExportarCodigoFuenteAsync(string dbName)
        {
            // Obtener la ruta raíz de la solución (ajusta según tu estructura)
            string solutionPath = Directory.GetCurrentDirectory();
            string parentDirectory = Directory.GetParent(solutionPath)?.FullName ?? solutionPath;

            // Crear carpeta temporal para los exports
            string exportFolder = Path.Combine(Path.GetTempPath(), "BackFabrica_Exports");
            Directory.CreateDirectory(exportFolder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string zipFileName = $"BackFabrica_{dbName}_{timestamp}.zip";
            string zipPath = Path.Combine(exportFolder, zipFileName);

            await Task.Run(() =>
            {
                using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    // Agregar todos los archivos de la solución, excluyendo carpetas innecesarias
                    AgregarArchivosAlZip(parentDirectory, archive, parentDirectory);
                }
            });

            return zipPath;
        }

        private void AgregarArchivosAlZip(string directoryPath, ZipArchive archive, string baseDirectory)
        {
            // Carpetas y archivos a excluir
            string[] excludedFolders = { "bin", "obj", ".vs", ".git", "node_modules", "packages" };
            string[] excludedFiles = { ".suo", ".user" };

            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                string fileName = Path.GetFileName(filePath);

                // Excluir archivos temporales
                if (Array.Exists(excludedFiles, ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    continue;

                string entryName = Path.GetRelativePath(baseDirectory, filePath);
                archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
            }

            foreach (string subdirectory in Directory.GetDirectories(directoryPath))
            {
                string folderName = Path.GetFileName(subdirectory);

                // Excluir carpetas de build y temporales
                if (Array.Exists(excludedFolders, folder => folder.Equals(folderName, StringComparison.OrdinalIgnoreCase)))
                    continue;

                AgregarArchivosAlZip(subdirectory, archive, baseDirectory);
            }
        }

        private async Task<string> RunFlutterBuild(bool useClean = false)
        {
            // Comando optimizado: solo clean cuando sea necesario
            string command = useClean
                ? "/c call flutter clean & call flutter build apk --release"
                : "/c call flutter build apk --release --no-tree-shake-icons"; // Más rápido

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = command,
                WorkingDirectory = _flutterProjectPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process proc = new Process())
            {
                proc.StartInfo = psi;
                proc.Start();

                var stdoutTask = proc.StandardOutput.ReadToEndAsync();
                var stderrTask = proc.StandardError.ReadToEndAsync();

                await proc.WaitForExitAsync();

                string stdOut = await stdoutTask;
                string error = await stderrTask;

                if (proc.ExitCode == 0)
                {
                    string apkPath = Path.Combine(_flutterProjectPath, @"build\app\outputs\flutter-apk\app-release.apk");
                    if (File.Exists(apkPath))
                    {
                        return apkPath;
                    }
                }

                throw new Exception($"Error compilando Flutter. ExitCode: {proc.ExitCode}.\nError: {error}\nLog: {stdOut}");
            }
        }
    }

    public class ApkBuildResult
    {
        public string ApkPath { get; set; }
        public string SourceCodeZipPath { get; set; }
    }
}