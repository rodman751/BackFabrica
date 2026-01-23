using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Services
{
    public class ApkBuilderService
    {
        private readonly string _flutterProjectPath = @"D:\Desarrollo\Flutter\herramienta_case\herramienta_case";

        public async Task<string> GenerarApkAsync(string dbName, string jsonSchema)
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

                // OPCIÓN 1: Sin flutter clean (mucho más rápido)
                return await RunFlutterBuild(useClean: false);
            }
            finally
            {
                await File.WriteAllTextAsync(pathConfigFile, contenidoOriginal);
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
}