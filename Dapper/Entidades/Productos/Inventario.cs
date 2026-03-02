namespace CapaDapper.Entidades.Productos
{
    /// <summary>
    /// Represents the inventory record for a product, tracking stock levels and storage location.
    /// </summary>
    public class Inventario
    {
        /// <summary>Unique identifier of the inventory record.</summary>
        public int Id { get; set; }
        /// <summary>Foreign key referencing the associated product.</summary>
        public int Producto_Id { get; set; }
        /// <summary>Current stock quantity on hand.</summary>
        public int Stock_Actual { get; set; }
        /// <summary>Minimum stock threshold; triggers a low-stock alert when reached.</summary>
        public int Stock_Minimo { get; set; }
        /// <summary>Physical storage location within the warehouse.</summary>
        public string Ubicacion_Almacen { get; set; }
    }
}
