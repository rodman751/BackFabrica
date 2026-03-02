namespace CapaDapper.Entidades.Productos
{
    /// <summary>
    /// Represents a product in the inventory management module.
    /// </summary>
    public class Producto
    {
        /// <summary>Unique identifier of the product record.</summary>
        public int Id { get; set; }
        /// <summary>Stock-keeping unit code for the product.</summary>
        public string Sku { get; set; }
        /// <summary>Display name of the product.</summary>
        public string Nombre { get; set; }
        /// <summary>Detailed description of the product.</summary>
        public string Descripcion { get; set; }
        /// <summary>Cost price of the product.</summary>
        public decimal Precio_Costo { get; set; }
        /// <summary>Retail sale price of the product.</summary>
        public decimal Precio_Venta { get; set; }
        /// <summary>Optional foreign key referencing the product category.</summary>
        public int? Categoria_Id { get; set; }
        /// <summary>Optional foreign key referencing the supplier.</summary>
        public int? Proveedor_Id { get; set; }
        /// <summary>JSON-encoded technical specifications, mapped from the SQL JSON column by Dapper.</summary>
        public string Especificaciones { get; set; }
        /// <summary>Current lifecycle status of the product (e.g., <c>activo</c>, <c>descontinuado</c>).</summary>
        public string Estado { get; set; }
        /// <summary>Timestamp when the record was created.</summary>
        public DateTime Created_At { get; set; }
    }
}
