using CapaDapper.DataService;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackFabrica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcutosController : ControllerBase
    {
       
        private readonly IDbMetadataRepository _repository;

        public ProcutosController(IDbMetadataRepository repository)
        {
            _repository = repository;
        }

        // GET: api/<ProcutosController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ProcutosController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ProcutosController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string value)
        {
            bool resp = await _repository.CreateProd(); // Assuming CreateProd might take the value

            // Correct comparison: use 'resp' directly, NOT 'resp = true'
            if (resp)
            {
                // Return 201 Created, which is appropriate for a successful POST operation
                // The object inside Created() is often the newly created resource or a confirmation.
                return CreatedAtAction(nameof(Get), new { id = "newId" }, new { success = true, value = value });
                // Alternatively, for simplicity:
                // return Ok(new { success = true, value = value });
            }
            else
            {
                // Return 400 Bad Request if the repository indicates failure (e.g., validation fail)
                return BadRequest(new { success = false, message = "Product creation failed in repository." });
            }
        }

        // PUT api/<ProcutosController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ProcutosController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
