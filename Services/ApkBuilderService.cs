using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Services
{
	public class ApkBuilderService
	{
		// 1. RUTA ACTUALIZADA A TU PROYECTO FLUTTER REAL
		private readonly string _flutterProjectPath = @"D:\Desarrollo\Flutter\herramienta_case\herramienta_case";

		// Mantenemos la firma con jsonSchema para que no rompa el controlador
		public async Task<string> GenerarApkAsync(string dbName, string jsonSchema)
		{
			// 2. RUTA AL ARCHIVO DE CONFIGURACIÓN QUE CREAMOS EN FLUTTER
			// Apunta a: lib/core/config/app_build_config.dart
			string pathConfigFile = Path.Combine(_flutterProjectPath, "lib", "core", "config", "app_build_config.dart");

			if (!File.Exists(pathConfigFile))
				throw new FileNotFoundException($"No se encontró el archivo de configuración en: {pathConfigFile}");

			// Guardar contenido original para restaurar después
			string contenidoOriginal = await File.ReadAllTextAsync(pathConfigFile);

			try
			{

				// 3. REEMPLAZO DE VARIABLES
				// CORREGIDO: Ahora coincide con el placeholder de Flutter

				string nuevoContenido = contenidoOriginal
					.Replace("{{DB_NAME_PLACEHOLDER}}", dbName) // <--- ¡AQUÍ ESTABA EL ERROR!
					.Replace("{{SCHEMA_JSON_PLACEHOLDER}}", jsonSchema);

				// Sobrescribir archivo temporalmente
				await File.WriteAllTextAsync(pathConfigFile, nuevoContenido);

				// 4. COMPILAR
				return await RunFlutterBuild();
			}
			finally
			{
				// 5. RESTAURAR (CRÍTICO PARA MANTENER LA PLANTILLA LIMPIA)
				await File.WriteAllTextAsync(pathConfigFile, contenidoOriginal);
			}
		}

		private async Task<string> RunFlutterBuild()
		{
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = "cmd.exe",
				// Mantenemos el clean y build
				Arguments = "/c call flutter clean & call flutter build apk --release",
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

				// === CORRECCIÓN ANTI-BLOQUEO (DEADLOCK FIX) ===
				// Leemos los flujos de texto ASÍNCRONAMENTE mientras el proceso corre.
				// Esto evita que el buffer se llene y congele la compilación.
				var stdoutTask = proc.StandardOutput.ReadToEndAsync();
				var stderrTask = proc.StandardError.ReadToEndAsync();

				// Ahora sí podemos esperar tranquilos a que termine
				await proc.WaitForExitAsync();

				// Recuperamos el texto final
				string stdOut = await stdoutTask;
				string error = await stderrTask;
				// ===============================================

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