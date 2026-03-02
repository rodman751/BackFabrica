namespace CapaDapper.Entidades.Productos
{
    /// <summary>
    /// Represents a product supplier in the inventory management module.
    /// </summary>
    public class Proveedor
    {
        /// <summary>Unique identifier of the supplier record.</summary>
        public int Id { get; set; }
        /// <summary>Legal or trade name of the supplier company.</summary>
        public string NombreEmpresa { get; set; }
        /// <summary>Full name of the primary contact person.</summary>
        public string ContactoNombre { get; set; }
        /// <summary>Contact email address.</summary>
        public string Email { get; set; }
        /// <summary>Contact phone number.</summary>
        public string Telefono { get; set; }
        /// <summary>Supplier's website URL.</summary>
        public string SitioWeb { get; set; }
    }
}
