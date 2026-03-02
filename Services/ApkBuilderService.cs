using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Orchestrates the end-to-end Flutter APK build pipeline.
    /// Injects a database schema into the Flutter project configuration file,
    /// invokes the Flutter toolchain to produce a release APK, and packages
    /// the .NET and Flutter source trees into ZIP archives for distribution.
    /// The configuration file is always restored to its original placeholder content
    /// in a <c>finally</c> block, regardless of build outcome.
    /// </summary>
    public class ApkBuilderService
    {
        private readonly string _flutterProjectPath = @"D:\Desarrollo\Flutter\herramienta_case\herramienta_case";

        /// <summary>
        /// Generates a release APK for the specified database by injecting its schema into
        /// <c>app_build_config.dart</c> and running the Flutter build toolchain.
        /// Also produces ZIP archives of the .NET backend and the Flutter source code.
        /// </summary>
        /// <param name="dbName">Name of the target database used to label output files.</param>
        /// <param name="jsonSchema">JSON schema string to embed in the Flutter configuration file.</param>
        /// <returns>
        /// An <see cref="ApkBuildResult"/> containing the paths to the APK,
        /// the .NET source archive, and the Flutter source archive.
        /// </returns>
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

                string apkPath = await RunFlutterBuild(useClean: false);
                string zipPathDotNet = await ExportarCodigoFuenteAsync(dbName);
                string zipPathFlutter = await ExportarCodigoFlutterAsync(dbName);

                return new ApkBuildResult
                {
                    ApkPath = apkPath,
                    SourceCodeZipPath = zipPathDotNet,
                    FlutterSourceCodeZipPath = zipPathFlutter
                };
            }
            finally
            {
                await File.WriteAllTextAsync(pathConfigFile, contenidoOriginal);
            }
        }

        /// <summary>
        /// Creates a ZIP archive of the .NET backend solution, excluding build artefacts
        /// and IDE metadata folders (<c>bin</c>, <c>obj</c>, <c>.vs</c>, <c>.git</c>, etc.).
        /// </summary>
        /// <param name="dbName">Database name used to label the output ZIP file.</param>
        /// <returns>Absolute path to the generated ZIP archive.</returns>
        private async Task<string> ExportarCodigoFuenteAsync(string dbName)
        {
            string solutionPath = Directory.GetCurrentDirectory();
            string parentDirectory = Directory.GetParent(solutionPath)?.FullName ?? solutionPath;

            string exportFolder = Path.Combine(Path.GetTempPath(), "BackFabrica_Exports");
            Directory.CreateDirectory(exportFolder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string zipFileName = $"BackFabrica_{dbName}_{timestamp}.zip";
            string zipPath = Path.Combine(exportFolder, zipFileName);

            await Task.Run(() =>
            {
                using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    AgregarArchivosAlZip(parentDirectory, archive, parentDirectory);
                }
            });

            return zipPath;
        }

        /// <summary>
        /// Creates a ZIP archive of the Flutter project, excluding build artefacts
        /// and tool-generated folders (<c>build</c>, <c>.dart_tool</c>, <c>.git</c>, etc.).
        /// </summary>
        /// <param name="dbName">Database name used to label the output ZIP file.</param>
        /// <returns>Absolute path to the generated ZIP archive.</returns>
        private async Task<string> ExportarCodigoFlutterAsync(string dbName)
        {
            string exportFolder = Path.Combine(Path.GetTempPath(), "BackFabrica_Expoorts");
            Directory.CreateDirectory(exportFolder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string zipFileName = $"Flutter_{dbName}_{timestamp}.zip";
            string zipPath = Path.Combine(exportFolder, zipFileName);

            await Task.Run(() =>
            {
                using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    AgregarArchivosFlutterAlZip(_flutterProjectPath, archive, _flutterProjectPath);
                }
            });

            return zipPath;
        }

        /// <summary>
        /// Recursively adds Flutter project files to a ZIP archive,
        /// skipping excluded folders and lock files.
        /// </summary>
        /// <param name="directoryPath">Current directory being processed.</param>
        /// <param name="archive">Target ZIP archive.</param>
        /// <param name="baseDirectory">Root directory used to compute relative entry paths.</param>
        private void AgregarArchivosFlutterAlZip(string directoryPath, ZipArchive archive, string baseDirectory)
        {
            string[] excludedFolders = { "build", ".dart_tool", ".idea", ".git", "android/.gradle", "ios/Pods", ".fvm" };
            string[] excludedFiles = { ".lock" };

            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                string fileName = Path.GetFileName(filePath);

                if (Array.Exists(excludedFiles, ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    continue;

                string entryName = Path.GetRelativePath(baseDirectory, filePath);
                archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
            }

            foreach (string subdirectory in Directory.GetDirectories(directoryPath))
            {
                string folderName = Path.GetFileName(subdirectory);

                if (Array.Exists(excludedFolders, folder => folder.Equals(folderName, StringComparison.OrdinalIgnoreCase)))
                    continue;

                AgregarArchivosFlutterAlZip(subdirectory, archive, baseDirectory);
            }
        }

        /// <summary>
        /// Recursively adds .NET solution files to a ZIP archive,
        /// skipping build output and IDE metadata folders.
        /// </summary>
        /// <param name="directoryPath">Current directory being processed.</param>
        /// <param name="archive">Target ZIP archive.</param>
        /// <param name="baseDirectory">Root directory used to compute relative entry paths.</param>
        private void AgregarArchivosAlZip(string directoryPath, ZipArchive archive, string baseDirectory)
        {
            string[] excludedFolders = { "bin", "obj", ".vs", ".git", "node_modules", "packages" };
            string[] excludedFiles = { ".suo", ".user" };

            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                string fileName = Path.GetFileName(filePath);

                if (Array.Exists(excludedFiles, ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    continue;

                string entryName = Path.GetRelativePath(baseDirectory, filePath);
                archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
            }

            foreach (string subdirectory in Directory.GetDirectories(directoryPath))
            {
                string folderName = Path.GetFileName(subdirectory);

                if (Array.Exists(excludedFolders, folder => folder.Equals(folderName, StringComparison.OrdinalIgnoreCase)))
                    continue;

                AgregarArchivosAlZip(subdirectory, archive, baseDirectory);
            }
        }

        /// <summary>
        /// Executes the Flutter build command and returns the path to the compiled APK.
        /// Optionally runs <c>flutter clean</c> before building.
        /// </summary>
        /// <param name="useClean">
        /// When <c>true</c>, the build is preceded by <c>flutter clean</c>.
        /// Defaults to <c>false</c> for faster incremental builds.
        /// </param>
        /// <returns>Absolute path to the generated <c>app-release.apk</c>.</returns>
        /// <exception cref="Exception">Thrown when the Flutter process exits with a non-zero code.</exception>
        private async Task<string> RunFlutterBuild(bool useClean = false)
        {
            string command = useClean
                ? "/c call flutter clean & call flutter build apk --release"
                : "/c call flutter build apk --release --no-tree-shake-icons";

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

    /// <summary>
    /// Holds the file system paths for the artefacts produced by a completed APK build.
    /// </summary>
    public class ApkBuildResult
    {
        /// <summary>Absolute path to the compiled <c>app-release.apk</c> file.</summary>
        public string ApkPath { get; set; }
        /// <summary>Absolute path to the .NET backend source code ZIP archive.</summary>
        public string SourceCodeZipPath { get; set; }
        /// <summary>Absolute path to the Flutter source code ZIP archive.</summary>
        public string FlutterSourceCodeZipPath { get; set; }
    }
}
