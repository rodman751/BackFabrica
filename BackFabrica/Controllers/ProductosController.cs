using CapaDapper.Cadena;
using CapaDapper.DataService;
using CapaDapper.Entidades.Productos;
using Microsoft.AspNetCore.Mvc;

namespace BackFabrica.Controllers
{
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
        [HttpGet]
        public async Task<IActionResult> GetProductos([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerProductosAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProducto([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var item = await _repo.ObtenerProductoPorIdAsync(id);
            return item != null ? Ok(item) : NotFound("Producto no encontrado");
        }

        [HttpPost]
        public async Task<IActionResult> PostProducto([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Producto prod)
        {
            _dbContext.CurrentDb = dbName;
            if (prod == null) return BadRequest("Datos nulos");
            var result = await _repo.CrearProductoAsync(prod);
            return result ? StatusCode(201, "Producto creado") : BadRequest("Error al crear");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Producto prod)
        {
            _dbContext.CurrentDb = dbName;
            if (id != prod.Id) return BadRequest("ID no coincide");
            var result = await _repo.ActualizarProductoAsync(prod);
            return result ? Ok("Producto actualizado") : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarProductoAsync(id);
            return result ? Ok("Producto eliminado") : NotFound();
        }
        #endregion

        #region Endpoints Categorias
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerCategoriasAsync();
            return Ok(lista);
        }

        [HttpGet("categorias/{id}")]
        public async Task<IActionResult> GetCategoriaPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var categoria = await _repo.ObtenerCategoriaPorIdAsync(id);
            return categoria != null ? Ok(categoria) : NotFound("Categoría no encontrada");
        }

        [HttpPost("categorias")]
        public async Task<IActionResult> PostCategoria([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Categoria cat)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearCategoriaAsync(cat);
            return result ? Ok("Categoría creada") : BadRequest("Error");
        }

        [HttpPut("categorias/{id}")]
        public async Task<IActionResult> PutCategoria([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Categoria categoria)
        {
            _dbContext.CurrentDb = dbName;
            categoria.Id = id;
            var result = await _repo.ActualizarCategoriaAsync(categoria);
            return result ? Ok("Categoría actualizada") : BadRequest("Error al actualizar");
        }

        [HttpDelete("categorias/{id}")]
        public async Task<IActionResult> DeleteCategoria([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarCategoriaAsync(id);
            return result ? Ok("Categoría eliminada") : NotFound("Categoría no encontrada");
        }
        #endregion

        #region Endpoints Proveedores
        [HttpGet("proveedores")]
        public async Task<IActionResult> GetProveedores([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerProveedoresAsync();
            return Ok(lista);
        }

        [HttpGet("proveedores/{id}")]
        public async Task<IActionResult> GetProveedorPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var proveedor = await _repo.ObtenerProveedorPorIdAsync(id);
            return proveedor != null ? Ok(proveedor) : NotFound("Proveedor no encontrado");
        }

        [HttpPost("proveedores")]
        public async Task<IActionResult> PostProveedor([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Proveedor prov)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearProveedorAsync(prov);
            return result ? Ok("Proveedor creado") : BadRequest("Error");
        }

        [HttpPut("proveedores/{id}")]
        public async Task<IActionResult> PutProveedor([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Proveedor proveedor)
        {
            _dbContext.CurrentDb = dbName;
            proveedor.Id = id;
            var result = await _repo.ActualizarProveedorAsync(proveedor);
            return result ? Ok("Proveedor actualizado") : BadRequest("Error al actualizar");
        }

        [HttpDelete("proveedores/{id}")]
        public async Task<IActionResult> DeleteProveedor([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarProveedorAsync(id);
            return result ? Ok("Proveedor eliminado") : NotFound("Proveedor no encontrado");
        }
        #endregion

        #region Endpoints Inventario
        [HttpGet("inventario")]
        public async Task<IActionResult> GetInventarios([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerInventariosAsync();
            return Ok(lista);
        }

        [HttpGet("inventario/{id:int}")]
        public async Task<IActionResult> GetInventarioPorId([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var inventario = await _repo.ObtenerInventarioPorIdAsync(id);
            return inventario != null ? Ok(inventario) : NotFound("Inventario no encontrado");
        }

        [HttpGet("inventario/producto/{productoId}")]
        public async Task<IActionResult> GetInventarioPorProducto([FromHeader(Name = "X-DbName")] string dbName, int productoId)
        {
            _dbContext.CurrentDb = dbName;
            var inv = await _repo.ObtenerInventarioPorProductoAsync(productoId);
            return inv != null ? Ok(inv) : NotFound("Sin registro de inventario");
        }

        [HttpPost("inventario")]
        public async Task<IActionResult> PostInventario([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Inventario inventario)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearInventarioAsync(inventario);
            return result ? Ok("Inventario creado") : BadRequest("Error");
        }

        [HttpPut("inventario/{id}")]
        public async Task<IActionResult> PutInventario([FromHeader(Name = "X-DbName")] string dbName, int id, [FromBody] Inventario inventario)
        {
            _dbContext.CurrentDb = dbName;
            inventario.Id = id;
            var result = await _repo.ActualizarInventarioAsync(inventario);
            return result ? Ok("Inventario actualizado") : BadRequest("Error al actualizar");
        }

        [HttpPost("inventario/ajustar")]
        public async Task<IActionResult> AjustarStock([FromHeader(Name = "X-DbName")] string dbName, [FromBody] AjusteStockRequest request)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.AjustarStockAsync(request.ProductoId, request.Cantidad, request.Ubicacion);
            return result ? Ok("Stock actualizado") : BadRequest("Error al ajustar stock");
        }

        [HttpDelete("inventario/{id}")]
        public async Task<IActionResult> DeleteInventario([FromHeader(Name = "X-DbName")] string dbName, int id)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.EliminarInventarioAsync(id);
            return result ? Ok("Inventario eliminado") : NotFound("Inventario no encontrado");
        }
        #endregion
    }

    // Clase auxiliar para recibir el JSON del ajuste de stock
    public class AjusteStockRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; } // Puede ser positivo o negativo
        public string Ubicacion { get; set; }
    }
}