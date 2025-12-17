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
        // Ruta: api/Productos/categorias
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerCategoriasAsync();
            return Ok(lista);
        }

        [HttpPost("categorias")]
        public async Task<IActionResult> PostCategoria([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Categoria cat)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearCategoriaAsync(cat);
            return result ? Ok("Categoría creada") : BadRequest("Error");
        }
        #endregion

        #region Endpoints Proveedores
        // Ruta: api/Productos/proveedores
        [HttpGet("proveedores")]
        public async Task<IActionResult> GetProveedores([FromHeader(Name = "X-DbName")] string dbName)
        {
            _dbContext.CurrentDb = dbName;
            var lista = await _repo.ObtenerProveedoresAsync();
            return Ok(lista);
        }

        [HttpPost("proveedores")]
        public async Task<IActionResult> PostProveedor([FromHeader(Name = "X-DbName")] string dbName, [FromBody] Proveedor prov)
        {
            _dbContext.CurrentDb = dbName;
            var result = await _repo.CrearProveedorAsync(prov);
            return result ? Ok("Proveedor creado") : BadRequest("Error");
        }
        #endregion

        #region Endpoints Inventario
        // Ruta: api/Productos/inventario/{productoId}
        [HttpGet("inventario/{productoId}")]
        public async Task<IActionResult> GetInventario([FromHeader(Name = "X-DbName")] string dbName, int productoId)
        {
            _dbContext.CurrentDb = dbName;
            var inv = await _repo.ObtenerInventarioPorProductoAsync(productoId);
            return inv != null ? Ok(inv) : NotFound("Sin registro de inventario");
        }

        // Ruta: api/Productos/inventario/ajustar
        [HttpPost("inventario/ajustar")]
        public async Task<IActionResult> AjustarStock([FromHeader(Name = "X-DbName")] string dbName, [FromBody] AjusteStockRequest request)
        {
            _dbContext.CurrentDb = dbName;
            // request es una clase pequeñita auxiliar que definimos abajo o en otro archivo
            var result = await _repo.AjustarStockAsync(request.ProductoId, request.Cantidad, request.Ubicacion);
            return result ? Ok("Stock actualizado") : BadRequest("Error al ajustar stock");
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