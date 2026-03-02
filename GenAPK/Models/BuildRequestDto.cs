namespace GenAPK.Models
{
    /// <summary>
    /// Carries the parameters required to trigger an APK build via <c>GeneradorController</c>.
    /// </summary>
    public class BuildRequestDto
    {
        /// <summary>Name of the target database whose schema will be injected into the Flutter project.</summary>
        public string DbName { get; set; }
        /// <summary>JSON string representing the full database schema used to configure the Flutter client.</summary>
        public string SchemaJson { get; set; }
    }
}
