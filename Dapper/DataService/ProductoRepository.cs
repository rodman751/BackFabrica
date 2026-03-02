using CapaDapper.Cadena;
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using CapaDapper.Entidades.Productos;
using Microsoft.Extensions.Configuration;

namespace CapaDapper.DataService
{
    /// <summary>
    /// Implements data-access operations for the products domain, covering
    /// products, categories, suppliers, and inventory management.
    /// All queries execute against the database selected by <see cref="IDbConnectionFactory"/>.
    /// </summary>
    public class ProductosRepository : IProductosRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public ProductosRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        #region Productos
        /// <summary>Returns all products.</summary>
        public async Task<IEnumerable<Producto>> ObtenerProductosAsync()
        {
            var sql = "SELECT * FROM productos";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Producto>(sql);
        }

        /// <summary>Returns a single product by its identifier, or <c>null</c> when not found.</summary>
        public async Task<Producto> ObtenerProductoPorIdAsync(int id)
        {
            var sql = "SELECT * FROM productos WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Producto>(sql, new { Id = id });
        }

        /// <summary>
        /// Inserts a new product record. Sets <c>Created_At</c> to the current timestamp before insertion.
        /// Returns <c>false</c> on database error without propagating the exception.
        /// </summary>
        public async Task<bool> CrearProductoAsync(Producto p)
        {
            p.Created_At = DateTime.Now;

            var sql = @"
        INSERT INTO productos (
            sku,
            nombre,
            descripcion,
            precio_costo,
            precio_venta,
            categoria_id,
            proveedor_id,
            especificaciones,
            estado,
            created_at
        )
        VALUES (
            @Sku,
            @Nombre,
            @Descripcion,
            @Precio_Costo,
            @Precio_Venta,
            @Categoria_Id,
            @Proveedor_Id,
            @Especificaciones,
            @Estado,
            @Created_At
        )";

            using var conn = _connectionFactory.CreateConnection();
            try
            {
                return await conn.ExecuteAsync(sql, p) > 0;
            }
            catch (Exception ex)
            {
                _ = ex;
                return false;
            }
        }

        /// <summary>Updates all mutable fields of an existing product record.</summary>
        public async Task<bool> ActualizarProductoAsync(Producto p)
        {
            var sql = @"
        UPDATE productos
        SET
            sku = @Sku,
            nombre = @Nombre,
            descripcion = @Descripcion,
            precio_costo = @Precio_Costo,
            precio_venta = @Precio_Venta,
            categoria_id = @Categoria_Id,
            proveedor_id = @Proveedor_Id,
            especificaciones = @Especificaciones,
            estado = @Estado
        WHERE id = @Id";

            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }

        /// <summary>Deletes a product record by its identifier.</summary>
        public async Task<bool> EliminarProductoAsync(int id)
        {
            var sql = "DELETE FROM productos WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Categorias
        /// <summary>Returns all active categories (activo = 1).</summary>
        public async Task<IEnumerable<Categoria>> ObtenerCategoriasAsync()
        {
            var sql = "SELECT * FROM categorias WHERE activo = 1";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Categoria>(sql);
        }

        /// <summary>Returns a single category by its identifier, or <c>null</c> when not found.</summary>
        public async Task<Categoria> ObtenerCategoriaPorIdAsync(int id)
        {
            var sql = "SELECT * FROM categorias WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Categoria>(sql, new { Id = id });
        }

        /// <summary>Inserts a new product category.</summary>
        public async Task<bool> CrearCategoriaAsync(Categoria c)
        {
            var sql = "INSERT INTO categorias (nombre, descripcion, padre_id, activo) VALUES (@Nombre, @Descripcion, @Padre_Id, @Activo)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, c) > 0;
        }

        /// <summary>Updates all mutable fields of an existing category. Returns <c>false</c> on database error.</summary>
        public async Task<bool> ActualizarCategoriaAsync(Categoria c)
        {
            var sql = @"
        UPDATE categorias
        SET
            nombre = @Nombre,
            descripcion = @Descripcion,
            padre_id = @Padre_Id,
            activo = @Activo
        WHERE id = @Id";

            using var conn = _connectionFactory.CreateConnection();

            try
            {
                return await conn.ExecuteAsync(sql, c) > 0;
            }
            catch (Exception ex)
            {
                _ = ex;
                return false;
            }
        }

        /// <summary>Deletes a category by its identifier.</summary>
        public async Task<bool> EliminarCategoriaAsync(int id)
        {
            var sql = "DELETE FROM categorias WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Proveedores
        /// <summary>
        /// Returns all suppliers with explicit column aliases for Dapper property mapping.
        /// </summary>
        public async Task<IEnumerable<Proveedor>> ObtenerProveedoresAsync()
        {
            var sql = @"
        SELECT
            id,
            nombre_empresa AS NombreEmpresa,
            contacto_nombre AS ContactoNombre,
            email,
            telefono,
            sitio_web AS SitioWeb
        FROM proveedores";

            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Proveedor>(sql);
        }

        /// <summary>Returns a single supplier by its identifier, or <c>null</c> when not found.</summary>
        public async Task<Proveedor> ObtenerProveedorPorIdAsync(int id)
        {
            var sql = @"
        SELECT
            id,
            nombre_empresa AS NombreEmpresa,
            contacto_nombre AS ContactoNombre,
            email,
            telefono,
            sitio_web AS SitioWeb
        FROM proveedores
        WHERE id = @Id";

            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Proveedor>(sql, new { Id = id });
        }

        /// <summary>Inserts a new supplier record.</summary>
        public async Task<bool> CrearProveedorAsync(Proveedor p)
        {
            var sql = "INSERT INTO proveedores (nombre_empresa, contacto_nombre, email, telefono, sitio_web) VALUES (@NombreEmpresa, @ContactoNombre, @Email, @Telefono, @SitioWeb)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }

        /// <summary>Updates all mutable fields of an existing supplier record.</summary>
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

        /// <summary>Deletes a supplier record by its identifier.</summary>
        public async Task<bool> EliminarProveedorAsync(int id)
        {
            var sql = "DELETE FROM proveedores WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Inventario
        /// <summary>Returns all inventory records.</summary>
        public async Task<IEnumerable<Inventario>> ObtenerInventariosAsync()
        {
            var sql = "SELECT * FROM inventario";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Inventario>(sql);
        }

        /// <summary>Returns a single inventory record by its identifier, or <c>null</c> when not found.</summary>
        public async Task<Inventario> ObtenerInventarioPorIdAsync(int id)
        {
            var sql = "SELECT * FROM inventario WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Inventario>(sql, new { Id = id });
        }

        /// <summary>Returns the inventory record associated with a specific product, or <c>null</c> when not found.</summary>
        public async Task<Inventario> ObtenerInventarioPorProductoAsync(int productoId)
        {
            var sql = "SELECT * FROM inventario WHERE producto_id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Inventario>(sql, new { Id = productoId });
        }

        /// <summary>Inserts a new inventory record for a product.</summary>
        public async Task<bool> CrearInventarioAsync(Inventario i)
        {
            var sql = @"
        INSERT INTO inventario (producto_id, stock_actual, stock_minimo, ubicacion_almacen)
        VALUES (@Producto_Id, @Stock_Actual, @Stock_Minimo, @Ubicacion_Almacen)";

            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, i) > 0;
        }

        /// <summary>Updates all mutable fields of an existing inventory record.</summary>
        public async Task<bool> ActualizarInventarioAsync(Inventario inv)
        {
            var sql = @"
        UPDATE inventario
        SET
            producto_id = @Producto_Id,
            stock_actual = @Stock_Actual,
            stock_minimo = @Stock_Minimo,
            ubicacion_almacen = @Ubicacion_Almacen
        WHERE id = @Id";

            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, inv) > 0;
        }

        /// <summary>
        /// Applies an additive stock adjustment to a product's inventory.
        /// Pass a positive <paramref name="cantidad"/> for purchases and a negative value for sales.
        /// </summary>
        /// <param name="productoId">Identifier of the product whose stock is adjusted.</param>
        /// <param name="cantidad">Units to add (positive) or subtract (negative) from current stock.</param>
        /// <param name="ubicacion">Warehouse location to record alongside the adjustment.</param>
        public async Task<bool> AjustarStockAsync(int productoId, int cantidad, string ubicacion)
        {
            var sql = @"
                UPDATE inventario
                SET stock_actual = stock_actual + @Cantidad,
                    ubicacion_almacen = @Ubicacion
                WHERE producto_id = @ProductoId";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { ProductoId = productoId, Cantidad = cantidad, Ubicacion = ubicacion }) > 0;
        }

        /// <summary>Deletes an inventory record by its identifier.</summary>
        public async Task<bool> EliminarInventarioAsync(int id)
        {
            var sql = "DELETE FROM inventario WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion
    }
}
