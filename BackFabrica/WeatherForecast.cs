namespace BackFabrica
{
    /// <summary>
    /// Default ASP.NET scaffold model. Not used by the application.
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>Forecast date.</summary>
        public DateOnly Date { get; set; }
        /// <summary>Temperature in degrees Celsius.</summary>
        public int TemperatureC { get; set; }
        /// <summary>Temperature in degrees Fahrenheit, derived from <see cref="TemperatureC"/>.</summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        /// <summary>Short description of the weather conditions.</summary>
        public string? Summary { get; set; }
    }
}
