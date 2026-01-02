using CapaDapper.Entidades.Productos;

namespace CapaDapper.DataService
{
    public interface IProductosRepository
    {
        #region Productos
        Task<IEnumerable<Producto>> ObtenerProductosAsync();
        Task<Producto> ObtenerProductoPorIdAsync(int id);
        Task<bool> CrearProductoAsync(Producto producto);
        Task<bool> ActualizarProductoAsync(Producto producto);
        Task<bool> EliminarProductoAsync(int id);
        #endregion

        #region Categorias
        Task<IEnumerable<Categoria>> ObtenerCategoriasAsync();
        Task<bool> CrearCategoriaAsync(Categoria categoria);
        #endregion

        #region Proveedores
        Task<IEnumerable<Proveedor>> ObtenerProveedoresAsync();
        Task<bool> CrearProveedorAsync(Proveedor proveedor);
        #endregion

        #region Inventario
        Task<Inventario> ObtenerInventarioPorProductoAsync(int productoId);
        Task<bool> AjustarStockAsync(int productoId, int cantidad, string ubicacion);
        #endregion
    }
}