using CapaDapper.Cadena;
using CapaDapper.DataService;
using CapaDapper.Dtos;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BackFabrica.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DynamicCrudController : ControllerBase
    {
        private readonly DynamicCrudService _dynamicService;
        private readonly IConfiguration _configuration;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDatabaseContext _dbContext;
        // Inyectamos IConfiguration para leer el appsettings.json
        public DynamicCrudController(DynamicCrudService dynamicService, IConfiguration configuration, IDbConnectionFactory connectionFactory, IDatabaseContext dbContext)
        {
            _dynamicService = dynamicService;
            _configuration = configuration;
            _connectionFactory = connectionFactory;
            _dbContext = dbContext;
        }

        // Método helper para convertir JsonElement a tipos .NET
        private Dictionary<string, object> ConvertJsonElementsToNetTypes(Dictionary<string, object> data)
        {
            if (data == null) return null;

            var result = new Dictionary<string, object>();
            foreach (var kvp in data)
            {
                if (kvp.Value is JsonElement element)
                {
                    result[kvp.Key] = ConvertJsonElement(element);
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        private object ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    string stringValue = element.GetString();
                    // Intentar parsear como DateTime
                    if (DateTime.TryParse(stringValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dateValue))
                    {
                        return dateValue;
                    }
                    return stringValue;
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    if (element.TryGetInt64(out long longValue))
                        return longValue;
                    if (element.TryGetDouble(out double doubleValue))
                        return doubleValue;
                    return element.GetDecimal();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    return element.ToString();
            }
        }

        // Nuevo método: Validar y mapear datos contra el esquema real
        // Nuevo método: Validar y mapear datos contra el esquema real
        private Dictionary<string, object> ValidateAndMapData(Dictionary<string, object> data, string tableName, DbSchema schema)
        {
            if (data == null || schema?.Columns == null) return data;

            // Crear un diccionario de mapeo: nombre en cualquier formato -> nombre real en BD
            var columnMap = schema.Columns
                .Where(c => c.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(
                    c => c.Name,
                    c => c.Name,
                    StringComparer.OrdinalIgnoreCase
                );

            var validatedData = new Dictionary<string, object>();

            foreach (var kvp in data)
            {
                string dataKey = kvp.Key;

                // Buscar el nombre real de la columna (case-insensitive)
                if (columnMap.TryGetValue(dataKey, out string realColumnName))
                {
                    validatedData[realColumnName] = kvp.Value;
                }
                else
                {
                    // Intento adicional: convertir PascalCase/camelCase a snake_case
                    string snakeCaseKey = ConvertToSnakeCase(dataKey);
                    if (columnMap.TryGetValue(snakeCaseKey, out string realColumnNameSnake))
                    {
                        validatedData[realColumnNameSnake] = kvp.Value;
                        Console.WriteLine($"Info: Mapeado '{dataKey}' a '{realColumnNameSnake}'");
                    }
                    else
                    {
                        // Opcional: Log de advertencia para columnas no encontradas
                        Console.WriteLine($"Advertencia: La columna '{dataKey}' no existe en el esquema de la tabla '{tableName}'");
                    }
                }
            }

            return validatedData;
        }

        // Helper: Convertir PascalCase/camelCase a snake_case
        private string ConvertToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return string.Concat(
                input.Select((x, i) => i > 0 && char.IsUpper(x)
                    ? "_" + x.ToString()
                    : x.ToString())
            ).ToLower();
        }

        [HttpPost("V2/CREADTE")]
        public async Task<IActionResult> Create(string tableName, [FromBody] DynamicRequestDto request)
        {
            try
            {
                // 1. Validaciones básicas PRIMERO
                if (request.Schema == null)
                    return BadRequest("El esquema es obligatorio.");

                if (string.IsNullOrEmpty(request.Schema.DatabaseName))
                    return BadRequest("El JSON del esquema no contiene la propiedad 'database_name'.");

                // 2. ESTABLECER el contexto de la base de datos ANTES de crear la conexión
                _dbContext.CurrentDb = request.Schema.DatabaseName;

                // 3. Convertir JsonElements a tipos .NET
                var convertedData = ConvertJsonElementsToNetTypes(request.Data);

                // 4. Validar y mapear datos contra el esquema
                var validatedData = ValidateAndMapData(convertedData, tableName, request.Schema);

                if (validatedData.Count == 0)
                    return BadRequest("No hay datos válidos para insertar.");

                // 5. Ahora sí crear la conexión (ya tiene CurrentDb configurado)
                using (var connection = _connectionFactory.CreateConnection())
                {
                    var result = await _dynamicService.InsertDynamicAsync(
                        tableName,
                        validatedData,
                        request.Schema,
                        connection.ConnectionString
                    );

                    return Ok(new { message = "Registro creado exitosamente", rowsAffected = result });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("V2/UPDATE")]
        public async Task<IActionResult> Update(string tableName, [FromBody] DynamicRequestDto request)
        {
            try
            {
                // 1. Validaciones básicas PRIMERO
                if (request.Schema == null)
                    return BadRequest("El esquema es obligatorio.");

                if (string.IsNullOrEmpty(request.Schema.DatabaseName))
                    return BadRequest("El JSON del esquema no contiene la propiedad 'database_name'.");

                // 2. ESTABLECER el contexto de la base de datos ANTES de crear la conexión
                _dbContext.CurrentDb = request.Schema.DatabaseName;

                // 3. Convertir JsonElements a tipos .NET
                var convertedData = ConvertJsonElementsToNetTypes(request.Data);

                // 4. Validar y mapear datos contra el esquema
                var validatedData = ValidateAndMapData(convertedData, tableName, request.Schema);

                if (validatedData.Count == 0)
                    return BadRequest("No hay datos válidos para actualizar.");

                // 5. Ahora sí crear la conexión (ya tiene CurrentDb configurado)
                using (var connection = _connectionFactory.CreateConnection())
                {
                    var result = await _dynamicService.UpdateDynamicAsync(
                        tableName,
                        validatedData,
                        request.Schema,
                        connection.ConnectionString
                    );

                    return Ok(new { message = "Registro actualizado exitosamente", rowsAffected = result });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("V2/GETALL")]
        public async Task<IActionResult> GetALL(string tableName, [FromBody] DynamicRequestDto request)
        {
            try
            {
                // 1. Validaciones básicas PRIMERO
                if (request.Schema == null)
                    return BadRequest("El esquema es obligatorio.");

                if (string.IsNullOrEmpty(request.Schema.DatabaseName))
                    return BadRequest("El JSON del esquema no contiene la propiedad 'database_name'.");

                // 2. ESTABLECER el contexto de la base de datos ANTES de crear la conexión
                _dbContext.CurrentDb = request.Schema.DatabaseName;

                // 3. Ahora sí crear la conexión (ya tiene CurrentDb configurado)
                using (var connection = _connectionFactory.CreateConnection())
                {
                    var result = await _dynamicService.GetAllDynamicAsync(
                        tableName,
                        request.Schema,
                        connection.ConnectionString
                    );

                    return Ok(new { message = "Datos obtenidos exitosamente", rowsAffected = result });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("V2/GETBYID/{id}")]
        public async Task<IActionResult> GetById(string tableName, int id, [FromBody] DbSchema schema)
        {
            try
            {
                // 1. Validaciones básicas PRIMERO
                if (schema == null)
                    return BadRequest("El esquema es obligatorio.");

                if (string.IsNullOrEmpty(schema.DatabaseName))
                    return BadRequest("El JSON del esquema no contiene la propiedad 'database_name'.");

                // 2. ESTABLECER el contexto de la base de datos ANTES de crear la conexión
                _dbContext.CurrentDb = schema.DatabaseName;

                // 3. Ahora sí crear la conexión (ya tiene CurrentDb configurado)
                using (var connection = _connectionFactory.CreateConnection())
                {
                    var result = await _dynamicService.GetByIdDynamicAsync(
                        tableName,
                        id,
                        schema,
                        connection.ConnectionString
                    );

                    if (result == null)
                        return NotFound(new { message = "Registro no encontrado" });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}