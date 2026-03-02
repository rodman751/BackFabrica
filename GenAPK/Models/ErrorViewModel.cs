namespace GenAPK.Models
{
    /// <summary>
    /// View model for the generic error page in the GenAPK MVC application.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>Unique identifier for the failed HTTP request, used for diagnostic tracing.</summary>
        public string? RequestId { get; set; }
        /// <summary>Returns <c>true</c> when <see cref="RequestId"/> is available and should be displayed.</summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
