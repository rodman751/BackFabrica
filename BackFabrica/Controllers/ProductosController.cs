using CapaDapper.Cadena;
using CapaDapper.DataService;
using CapaDapper.Entidades.Productos;
using Microsoft.AspNetCore.Mvc;

namespace BackFabrica.Controllers
{
    /// <summary>
    /// Manages products, categories, suppliers, and inventory for the backend module.
    /// All endpoints require the target database name via the <c>X-DbName</c> header.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly IProductosRepository _repo;
        private readonly IDatabaseContext _dbContext;

        public ProductosController(IProductosRepository repository, IDatabaseContext dbContext)
        {
            _repo = repository;
            _dbContext = dbContext;
        }

        #region Endpoints Productos
        /// <summary>
        /// Returns all products from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet]
        public async Task<IActionResult> GetProductos([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerProductosAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single product by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the product.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProducto([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var item = await _repo.ObtenerProductoPorIdAsync(id);
            return item != null ? Ok(item) : NotFound("Producto no encontrado");
        }

        /// <summary>
        /// Creates a new product record.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="prod">Product data to persist.</param>
        [HttpPost]
        public async Task<IActionResult> PostProducto([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Producto prod)
        {
            _dbContext.CurrentDb = dbName;
            if (prod == null) return BadRequest("Datos nulos");
            var result = await _repo.CrearProductoAsync(prod);
            return result ? StatusCode(201, "Producto creado") : BadRequest("Error al crear");
        }

        /// <summary>
        /// Updates an existing product by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the product to update.</param>
        /// <param name="prod">Updated product data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Producto prod)
        {
            _dbContext.CurrentDb = dbName;
            if (id != prod.Id) return BadRequest("ID no coincide");
            var result = await _repo.ActualizarProductoAsync(prod);
            return result ? Ok("Producto actualizado") : NotFound();
        }

        /// <summary>
        /// Deletes a product by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the product to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarProductoAsync(id);
            return result ? Ok("Producto eliminado") : NotFound();
        }
        #endregion

        #region Endpoints Categorias
        /// <summary>
        /// Returns all active categories from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerCategoriasAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single category by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the category.</param>
        [HttpGet("categorias/{id}")]
        public async Task<IActionResult> GetCategoriaPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var categoria = await _repo.ObtenerCategoriaPorIdAsync(id);
            return categoria != null ? Ok(categoria) : NotFound("Categoría no encontrada");
        }

        /// <summary>
        /// Creates a new product category.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="cat">Category data to persist.</param>
        [HttpPost("categorias")]
        public async Task<IActionResult> PostCategoria([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Categoria cat)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearCategoriaAsync(cat);
            return result ? Ok("Categoría creada") : BadRequest("Error");
        }

        /// <summary>
        /// Updates an existing category by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the category to update.</param>
        /// <param name="categoria">Updated category data.</param>
        [HttpPut("categorias/{id}")]
        public async Task<IActionResult> PutCategoria([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Categoria categoria)
        {
            _dbContext.CurrentDb = dbName;
            categoria.Id = id;
            var result = await _repo.ActualizarCategoriaAsync(categoria);
            return result ? Ok("Categoría actualizada") : BadRequest("Error al actualizar");
        }

        /// <summary>
        /// Deletes a category by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the category to delete.</param>
        [HttpDelete("categorias/{id}")]
        public async Task<IActionResult> DeleteCategoria([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarCategoriaAsync(id);
            return result ? Ok("Categoría eliminada") : NotFound("Categoría no encontrada");
        }
        #endregion

        #region Endpoints Proveedores
        /// <summary>
        /// Returns all suppliers from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("proveedores")]
        public async Task<IActionResult> GetProveedores([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerProveedoresAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single supplier by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the supplier.</param>
        [HttpGet("proveedores/{id}")]
        public async Task<IActionResult> GetProveedorPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var proveedor = await _repo.ObtenerProveedorPorIdAsync(id);
            return proveedor != null ? Ok(proveedor) : NotFound("Proveedor no encontrado");
        }

        /// <summary>
        /// Creates a new supplier record.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="prov">Supplier data to persist.</param>
        [HttpPost("proveedores")]
        public async Task<IActionResult> PostProveedor([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Proveedor prov)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearProveedorAsync(prov);
            return result ? Ok("Proveedor creado") : BadRequest("Error");
        }

        /// <summary>
        /// Updates an existing supplier by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the supplier to update.</param>
        /// <param name="proveedor">Updated supplier data.</param>
        [HttpPut("proveedores/{id}")]
        public async Task<IActionResult> PutProveedor([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Proveedor proveedor)
        {
            _dbContext.CurrentDb = dbName;
            proveedor.Id = id;
            var result = await _repo.ActualizarProveedorAsync(proveedor);
            return result ? Ok("Proveedor actualizado") : BadRequest("Error al actualizar");
        }

        /// <summary>
        /// Deletes a supplier by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the supplier to delete.</param>
        [HttpDelete("proveedores/{id}")]
        public async Task<IActionResult> DeleteProveedor([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarProveedorAsync(id);
            return result ? Ok("Proveedor eliminado") : NotFound("Proveedor no encontrado");
        }
        #endregion

        #region Endpoints Inventario
        /// <summary>
        /// Returns all inventory records from the specified database.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        [HttpGet("inventario")]
        public async Task<IActionResult> GetInventarios([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerInventariosAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Returns a single inventory record by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Unique identifier of the inventory record.</param>
        [HttpGet("inventario/{id:int}")]
        public async Task<IActionResult> GetInventarioPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var inventario = await _repo.ObtenerInventarioPorIdAsync(id);
            return inventario != null ? Ok(inventario) : NotFound("Inventario no encontrado");
        }

        /// <summary>
        /// Returns the inventory record associated with a specific product.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="productoId">Identifier of the product whose inventory is requested.</param>
        [HttpGet("inventario/producto/{productoId}")]
        public async Task<IActionResult> GetInventarioPorProducto([FromHeader(Name = "X-DbName")] string dbName, int productoId)
        {
            _dbContext.CurrentDb = dbName;
            var inv = await _repo.ObtenerInventarioPorProductoAsync(productoId);
            return inv != null ? Ok(inv) : NotFound("Sin registro de inventario");
        }

        /// <summary>
        /// Creates a new inventory record for a product.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="inventario">Inventory data to persist.</param>
        [HttpPost("inventario")]
        public async Task<IActionResult> PostInventario([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Inventario inventario)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearInventarioAsync(inventario);
            return result ? Ok("Inventario creado") : BadRequest("Error");
        }

        /// <summary>
        /// Updates an existing inventory record by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the inventory record to update.</param>
        /// <param name="inventario">Updated inventory data.</param>
        [HttpPut("inventario/{id}")]
        public async Task<IActionResult> PutInventario([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Inventario inventario)
        {
            _dbContext.CurrentDb = dbName;
            inventario.Id = id;
            var result = await _repo.ActualizarInventarioAsync(inventario);
            return result ? Ok("Inventario actualizado") : BadRequest("Error al actualizar");
        }

        /// <summary>
        /// Applies a stock adjustment (positive for purchases, negative for sales) to a product's inventory.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="request">Adjustment details including product ID, quantity delta, and warehouse location.</param>
        [HttpPost("inventario/ajustar")]
        public async Task<IActionResult> AjustarStock([FromHeader(Name = "X-DbName")] string dbName, [FromBody] AjusteStockRequest request)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.AjustarStockAsync(request.ProductoId, request.Cantidad, request.Ubicacion);
            return result ? Ok("Stock actualizado") : BadRequest("Error al ajustar stock");
        }

        /// <summary>
        /// Deletes an inventory record by its identifier.
        /// </summary>
        /// <param name="dbName">Target database name supplied via the <c>X-DbName</c> header.</param>
        /// <param name="id">Identifier of the inventory record to delete.</param>
        [HttpDelete("inventario/{id}")]
        public async Task<IActionResult> DeleteInventario([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarInventarioAsync(id);
            return result ? Ok("Inventario eliminado") : NotFound("Inventario no encontrado");
        }
        #endregion
    }

    /// <summary>
    /// Request payload for a stock adjustment operation.
    /// </summary>
    public class AjusteStockRequest
    {
        public int ProductoId { get; set; }
        /// <summary>Units to add (positive) or subtract (negative) from current stock.</summary>
        public int Cantidad { get; set; }
        public string Ubicacion { get; set; }
    }
}
