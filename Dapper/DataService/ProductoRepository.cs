using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using CapaDapper.Entidades.Productos;

namespace CapaDapper.DataService
{
    public class ProductosRepository : IProductosRepository
    {
        private readonly string _connectionString;

        public ProductosRepository(IConfiguration configuration)
        {
            // Asegúrate de que este nombre coincida con tu appsettings.json
            _connectionString = configuration.GetConnectionString("TemplateConnection");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        #region Productos
        public async Task<IEnumerable<Producto>> ObtenerProductosAsync()
        {
            var sql = "SELECT * FROM productos";
            using var conn = CreateConnection();
            return await conn.QueryAsync<Producto>(sql);
        }

        public async Task<Producto> ObtenerProductoPorIdAsync(int id)
        {
            var sql = "SELECT * FROM productos WHERE id = @Id";
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Producto>(sql, new { Id = id });
        }

        public async Task<bool> CrearProductoAsync(Producto p)
        {
            var sql = @"
                INSERT INTO productos (sku, nombre, descripcion, precio_costo, precio_venta, categoria_id, proveedor_id, especificaciones, estado)
                VALUES (@Sku, @Nombre, @Descripcion, @PrecioCosto, @PrecioVenta, @CategoriaId, @ProveedorId, @Especificaciones, @Estado);
                
                -- Opcional: Crear registro vacío en inventario automáticamente al crear producto
                DECLARE @newId INT = SCOPE_IDENTITY();
                INSERT INTO inventario (producto_id, stock_actual, stock_minimo, ubicacion_almacen) 
                VALUES (@newId, 0, 5, 'Sin Asignar');";

            using var conn = CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> ActualizarProductoAsync(Producto p)
        {
            var sql = @"
                UPDATE productos 
                SET nombre = @Nombre, descripcion = @Descripcion, precio_venta = @PrecioVenta, 
                    especificaciones = @Especificaciones, categoria_id = @CategoriaId, proveedor_id = @ProveedorId,
                    updated_at = GETDATE()
                WHERE id = @Id";
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> EliminarProductoAsync(int id)
        {
            var sql = "DELETE FROM productos WHERE id = @Id";
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Categorias
        public async Task<IEnumerable<Categoria>> ObtenerCategoriasAsync()
        {
            var sql = "SELECT * FROM categorias WHERE activo = 1";
            using var conn = CreateConnection();
            return await conn.QueryAsync<Categoria>(sql);
        }

        public async Task<bool> CrearCategoriaAsync(Categoria c)
        {
            var sql = "INSERT INTO categorias (nombre, descripcion, padre_id, activo) VALUES (@Nombre, @Descripcion, @PadreId, 1)";
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(sql, c) > 0;
        }
        #endregion

        #region Proveedores
        public async Task<IEnumerable<Proveedor>> ObtenerProveedoresAsync()
        {
            var sql = "SELECT * FROM proveedores";
            using var conn = CreateConnection();
            return await conn.QueryAsync<Proveedor>(sql);
        }

        public async Task<bool> CrearProveedorAsync(Proveedor p)
        {
            var sql = "INSERT INTO proveedores (nombre_empresa, contacto_nombre, email, telefono, sitio_web) VALUES (@NombreEmpresa, @ContactoNombre, @Email, @Telefono, @SitioWeb)";
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }
        #endregion

        #region Inventario
        public async Task<Inventario> ObtenerInventarioPorProductoAsync(int productoId)
        {
            var sql = "SELECT * FROM inventario WHERE producto_id = @Id";
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Inventario>(sql, new { Id = productoId });
        }

        public async Task<bool> AjustarStockAsync(int productoId, int cantidad, string ubicacion)
        {
            // Cantidad puede ser positiva (compra) o negativa (venta)
            var sql = @"
                UPDATE inventario 
                SET stock_actual = stock_actual + @Cantidad,
                    ubicacion_almacen = @Ubicacion
                WHERE producto_id = @ProductoId";
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(sql, new { ProductoId = productoId, Cantidad = cantidad, Ubicacion = ubicacion }) > 0;
        }
        #endregion
    }
}