namespace CapaDapper.Entidades.Productos
{
    public class Inventario
    {
        public int Id { get; set; }
        public int ProductoId { get; set; } // FK
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string UbicacionAlmacen { get; set; }
    }
}