using CapaDapper.Cadena;
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using CapaDapper.Entidades.Productos;
using Microsoft.Extensions.Configuration;

namespace CapaDapper.DataService
{
    public class ProductosRepository : IProductosRepository
    {
        //INYECCION NUEVA
        private readonly IDbConnectionFactory _connectionFactory;
        public ProductosRepository(IDbConnectionFactory connectionFactory)
        {

            _connectionFactory = connectionFactory;

        }

        #region Productos
        public async Task<IEnumerable<Producto>> ObtenerProductosAsync()
        {
            var sql = "SELECT * FROM productos";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Producto>(sql);
        }

        public async Task<Producto> ObtenerProductoPorIdAsync(int id)
        {
            var sql = "SELECT * FROM productos WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Producto>(sql, new { Id = id });
        }

        public async Task<bool> CrearProductoAsync(Producto p)
        {
            var sql = @"
        INSERT INTO productos (sku, nombre, descripcion, precio_costo, precio_venta, categoria_id, proveedor_id, especificaciones, estado)
        VALUES (@Sku, @Nombre, @Descripcion, @Precio_Costo, @Precio_Venta, @Categoria_Id, @Proveedor_Id, @Especificaciones, @Estado)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> ActualizarProductoAsync(Producto p)
        {
            var sql = @"
        UPDATE productos
        SET sku = @Sku, nombre = @Nombre, descripcion = @Descripcion, 
            precio_costo = @Precio_Costo, precio_venta = @Precio_Venta,
            categoria_id = @Categoria_Id, proveedor_id = @Proveedor_Id,
            especificaciones = @Especificaciones, estado = @Estado
        WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> EliminarProductoAsync(int id)
        {
            var sql = "DELETE FROM productos WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Categorias
        public async Task<IEnumerable<Categoria>> ObtenerCategoriasAsync()
        {
            var sql = "SELECT * FROM categorias WHERE activo = 1";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Categoria>(sql);
        }

        public async Task<Categoria> ObtenerCategoriaPorIdAsync(int id)
        {
            var sql = "SELECT * FROM categorias WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Categoria>(sql, new { Id = id });
        }

        public async Task<bool> CrearCategoriaAsync(Categoria c)
        {
            var sql = "INSERT INTO categorias (nombre, descripcion, padre_id, activo) VALUES (@Nombre, @Descripcion, @Padre_Id, @Activo)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, c) > 0;
        }

        public async Task<bool> ActualizarCategoriaAsync(Categoria c)
        {
            var sql = @"
                UPDATE categorias
                SET nombre = @Nombre, descripcion = @Descripcion, padre_id = @PadreId, activo = @Activo
                WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, c) > 0;
        }

        public async Task<bool> EliminarCategoriaAsync(int id)
        {
            var sql = "DELETE FROM categorias WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Proveedores
        public async Task<IEnumerable<Proveedor>> ObtenerProveedoresAsync()
        {
            var sql = "SELECT * FROM proveedores";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Proveedor>(sql);
        }

        public async Task<Proveedor> ObtenerProveedorPorIdAsync(int id)
        {
            var sql = "SELECT * FROM proveedores WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Proveedor>(sql, new { Id = id });
        }

        public async Task<bool> CrearProveedorAsync(Proveedor p)
        {
            var sql = "INSERT INTO proveedores (nombre_empresa, contacto_nombre, email, telefono, sitio_web) VALUES (@NombreEmpresa, @ContactoNombre, @Email, @Telefono, @SitioWeb)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> ActualizarProveedorAsync(Proveedor p)
        {
            var sql = @"
                UPDATE proveedores
                SET nombre_empresa = @NombreEmpresa, contacto_nombre = @ContactoNombre,
                    email = @Email, telefono = @Telefono, sitio_web = @SitioWeb
                WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> EliminarProveedorAsync(int id)
        {
            var sql = "DELETE FROM proveedores WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Inventario
        public async Task<IEnumerable<Inventario>> ObtenerInventariosAsync()
        {
            var sql = "SELECT * FROM inventario";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Inventario>(sql);
        }

        public async Task<Inventario> ObtenerInventarioPorIdAsync(int id)
        {
            var sql = "SELECT * FROM inventario WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Inventario>(sql, new { Id = id });
        }

        public async Task<Inventario> ObtenerInventarioPorProductoAsync(int productoId)
        {
            var sql = "SELECT * FROM inventario WHERE producto_id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Inventario>(sql, new { Id = productoId });
        }

        public async Task<bool> CrearInventarioAsync(Inventario i)
        {
            var sql = "INSERT INTO inventario (producto_id, stock_actual, stock_minimo, ubicacion_almacen) VALUES (@Producto_Id, @Stock_Actual, @Stock_Minimo, @Ubicacion_Almacen)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, i) > 0;
        }

        public async Task<bool> ActualizarInventarioAsync(Inventario inv)
        {
            var sql = @"
                UPDATE inventario
                SET producto_id = @ProductoId, stock_actual = @StockActual,
                    stock_minimo = @StockMinimo, ubicacion_almacen = @UbicacionAlmacen
                WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, inv) > 0;
        }

        public async Task<bool> AjustarStockAsync(int productoId, int cantidad, string ubicacion)
        {
            // Cantidad puede ser positiva (compra) o negativa (venta)
            var sql = @"
                UPDATE inventario
                SET stock_actual = stock_actual + @Cantidad,
                    ubicacion_almacen = @Ubicacion
                WHERE producto_id = @ProductoId";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { ProductoId = productoId, Cantidad = cantidad, Ubicacion = ubicacion }) > 0;
        }

        public async Task<bool> EliminarInventarioAsync(int id)
        {
            var sql = "DELETE FROM inventario WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion
    }
}