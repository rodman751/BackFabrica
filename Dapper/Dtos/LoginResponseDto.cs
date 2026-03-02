namespace CapaDapper.Dtos
{
    /// <summary>
    /// Carries the result of a successful login operation, including the signed JWT token
    /// and the authenticated user's profile data.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>Signed JWT token to include in subsequent API requests.</summary>
        public string Token { get; set; }
        /// <summary>Unique identifier of the authenticated user.</summary>
        public int? Id { get; set; }
        /// <summary>Username of the authenticated user.</summary>
        public string Username { get; set; }
        /// <summary>Email address of the authenticated user.</summary>
        public string Email { get; set; }
        /// <summary>Role assigned to the authenticated user (e.g., <c>admin</c>, <c>user</c>).</summary>
        public string Role { get; set; }
        /// <summary>Module origin that the user belongs to, used for client-side routing.</summary>
        public string ModuloOrigen { get; set; }
    }
}
