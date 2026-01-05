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
        Task<Categoria> ObtenerCategoriaPorIdAsync(int id);
        Task<bool> CrearCategoriaAsync(Categoria categoria);
        Task<bool> ActualizarCategoriaAsync(Categoria categoria);
        Task<bool> EliminarCategoriaAsync(int id);
        #endregion

        #region Proveedores
        Task<IEnumerable<Proveedor>> ObtenerProveedoresAsync();
        Task<Proveedor> ObtenerProveedorPorIdAsync(int id);
        Task<bool> CrearProveedorAsync(Proveedor proveedor);
        Task<bool> ActualizarProveedorAsync(Proveedor proveedor);
        Task<bool> EliminarProveedorAsync(int id);
        #endregion

        #region Inventario
        Task<IEnumerable<Inventario>> ObtenerInventariosAsync();
        Task<Inventario> ObtenerInventarioPorIdAsync(int id);
        Task<Inventario> ObtenerInventarioPorProductoAsync(int productoId);
        Task<bool> CrearInventarioAsync(Inventario inventario);
        Task<bool> ActualizarInventarioAsync(Inventario inventario);
        Task<bool> AjustarStockAsync(int productoId, int cantidad, string ubicacion);
        Task<bool> EliminarInventarioAsync(int id);
        #endregion
    }
}