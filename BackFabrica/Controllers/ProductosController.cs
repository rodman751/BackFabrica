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

        public ProductosController(IProductosRepository repository)
        {
            _repo = repository;
        }

        #region Endpoints Productos
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var lista = await _repo.ObtenerProductosAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProducto(int id)
        {
            var item = await _repo.ObtenerProductoPorIdAsync(id);
            return item != null ? Ok(item) : NotFound("Producto no encontrado");
        }

        [HttpPost]
        public async Task<IActionResult> PostProducto([FromBody] Producto prod)
        {
            if (prod == null) return BadRequest("Datos nulos");
            var result = await _repo.CrearProductoAsync(prod);
            return result ? StatusCode(201, "Producto creado") : BadRequest("Error al crear");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, [FromBody] Producto prod)
        {
            if (id != prod.Id) return BadRequest("ID no coincide");
            var result = await _repo.ActualizarProductoAsync(prod);
            return result ? Ok("Producto actualizado") : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var result = await _repo.EliminarProductoAsync(id);
            return result ? Ok("Producto eliminado") : NotFound();
        }
        #endregion

        #region Endpoints Categorias
        // Ruta: api/Productos/categorias
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var lista = await _repo.ObtenerCategoriasAsync();
            return Ok(lista);
        }

        [HttpPost("categorias")]
        public async Task<IActionResult> PostCategoria([FromBody] Categoria cat)
        {
            var result = await _repo.CrearCategoriaAsync(cat);
            return result ? Ok("Categoría creada") : BadRequest("Error");
        }
        #endregion

        #region Endpoints Proveedores
        // Ruta: api/Productos/proveedores
        [HttpGet("proveedores")]
        public async Task<IActionResult> GetProveedores()
        {
            var lista = await _repo.ObtenerProveedoresAsync();
            return Ok(lista);
        }

        [HttpPost("proveedores")]
        public async Task<IActionResult> PostProveedor([FromBody] Proveedor prov)
        {
            var result = await _repo.CrearProveedorAsync(prov);
            return result ? Ok("Proveedor creado") : BadRequest("Error");
        }
        #endregion

        #region Endpoints Inventario
        // Ruta: api/Productos/inventario/{productoId}
        [HttpGet("inventario/{productoId}")]
        public async Task<IActionResult> GetInventario(int productoId)
        {
            var inv = await _repo.ObtenerInventarioPorProductoAsync(productoId);
            return inv != null ? Ok(inv) : NotFound("Sin registro de inventario");
        }

        // Ruta: api/Productos/inventario/ajustar
        [HttpPost("inventario/ajustar")]
        public async Task<IActionResult> AjustarStock([FromBody] AjusteStockRequest request)
        {
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