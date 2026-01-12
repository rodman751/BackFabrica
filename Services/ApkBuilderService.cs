using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ApkBuilderService
    {
        // Ruta base donde vive tu proyecto Flutter (Recomiendo mover esto a appsettings.json)
        private readonly string _flutterProjectPath = @"C:\Pruebas\mi_plantilla";

        public async Task<string> GenerarApkAsync(string dbName, string jsonSchema)
        {
            // 1. DEFINIR RUTAS
            string pathConfigFile = Path.Combine(_flutterProjectPath, "lib", "config", "app_config.dart");

            // 2. LEER Y MODIFICAR EL CÓDIGO DART
            if (!File.Exists(pathConfigFile))
                throw new FileNotFoundException("No se encontró el archivo de configuración de Flutter.");

            string contenidoOriginal = await File.ReadAllTextAsync(pathConfigFile);

            // Hacemos el reemplazo. 
            // IMPORTANTE: Aseguramos que el JSON no rompa el string de Dart.
            // En un entorno productivo real, a veces es mejor pasar esto en Base64 para evitar problemas con comillas.
            string nuevoContenido = contenidoOriginal
                .Replace("{{DB_NAME_PLACEHOLDER}}", dbName)
                .Replace("{{SCHEMA_JSON_PLACEHOLDER}}", jsonSchema);

            // Sobrescribimos el archivo (OJO: Esto afecta a todos los usuarios si es concurrente)
            await File.WriteAllTextAsync(pathConfigFile, nuevoContenido);

            // 3. COMPILAR EL APK
            // Ejecutamos el proceso en segundo plano
            string outputApk = await RunFlutterBuild();

            return outputApk;
        }

        private async Task<string> RunFlutterBuild()
        {
            var tcs = new TaskCompletionSource<string>();

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                // --release es mejor para producción, --debug es más rápido para pruebas
                Arguments = "/c flutter build apk --debug",
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

                // Leer salida para logs (opcional pero recomendado)
                string output = await proc.StandardOutput.ReadToEndAsync();
                string error = await proc.StandardError.ReadToEndAsync();

                await proc.WaitForExitAsync();

                if (proc.ExitCode == 0)
                {
                    // Ruta estándar de salida de Flutter
                    string apkPath = Path.Combine(_flutterProjectPath, @"build\app\outputs\flutter-apk\app-debug.apk");
                    if (File.Exists(apkPath))
                    {
                        return apkPath;
                    }
                }

                // Si falló, lanzamos error con el log de Flutter
                throw new Exception($"Error compilando Flutter: {error}");
            }
        }
    }
}
