namespace CapaDapper.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public int? Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string ModuloOrigen { get; set; }
    }
}
