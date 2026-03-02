using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text;

namespace GenAPK.Controllers
{
    /// <summary>
    /// Manages server connection profile selection for the GenAPK MVC application.
    /// Stores the selected profile in the HTTP session for use across subsequent requests.
    /// </summary>
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Displays the connection profile selection view,
        /// populated with all profiles defined in <c>appsettings.json</c>.
        /// </summary>
        public IActionResult Index()
        {
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

        /// <summary>
        /// Tests connectivity for the specified connection profile.
        /// On success, persists the profile key in the session and returns HTTP 200.
        /// </summary>
        /// <param name="request">Contains the profile key to test.</param>
        [HttpPost]
        public async Task<IActionResult> TestProfile([FromBody] TestProfileRequest request)
        {
            try
            {
                var connectionString = _configuration[$"ConnectionProfiles:{request.ProfileKey}:ConnectionString"];

                if (string.IsNullOrEmpty(connectionString))
                {
                    return Json(new { success = false, message = "Perfil no encontrado" });
                }

                connectionString = string.Format(connectionString, "master");

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

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
