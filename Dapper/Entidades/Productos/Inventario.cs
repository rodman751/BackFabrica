namespace CapaDapper.Entidades.Productos
{
    public class Inventario
    {
        public int Id { get; set; }
        public int Producto_Id { get; set; } // FK
        public int Stock_Actual { get; set; }
        public int Stock_Minimo { get; set; }
        public string Ubicacion_Almacen { get; set; }
    }
}