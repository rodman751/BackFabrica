using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text;

namespace GenAPK.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            // Cargar perfiles desde appsettings.json
            var profiles = new Dictionary<string, ProfileInfo>();
            var profilesSection = _configuration.GetSection("ConnectionProfiles");

            foreach (var profile in profilesSection.GetChildren())
            {
                profiles.Add(profile.Key, new ProfileInfo
                {
                    Name = profile["Name"] ?? profile.Key,
                    Server = profile["Server"] ?? "N/A"
                });
            }

            return View(profiles);
        }

        [HttpPost]
        public async Task<IActionResult> TestProfile([FromBody] TestProfileRequest request)
        {
            try
            {
                // Obtener el perfil
                var connectionString = _configuration[$"ConnectionProfiles:{request.ProfileKey}:ConnectionString"];

                if (string.IsNullOrEmpty(connectionString))
                {
                    return Json(new { success = false, message = "Perfil no encontrado" });
                }

                // Formatear con base de datos master para prueba
                connectionString = string.Format(connectionString, "master");

                // Probar conexión
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Guardar en sesión
                HttpContext.Session.Set("SelectedProfile", Encoding.UTF8.GetBytes(request.ProfileKey));

                return Json(new
                {
                    success = true,
                    message = $"Conexión exitosa a {connection.DataSource}"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error de conexión: {ex.Message}"
                });
            }
        }

        public class TestProfileRequest
        {
            public string ProfileKey { get; set; } = string.Empty;
        }

        public class ProfileInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Server { get; set; } = string.Empty;
        }
    }
}