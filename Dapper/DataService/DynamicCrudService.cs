using CapaDapper.Dtos;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDapper.DataService
{
    public class DynamicCrudService
    {
        public DynamicCrudService() { }

        // MÉTODO: INSERTAR DINÁMICO
        public async Task<int> InsertDynamicAsync(string tableName, Dictionary<string, object> data, DbSchema schema, string connectionString)
        {
            // 1. Validar que la tabla existe en tu definición (Seguridad)
            var tableDef = schema.Tables.FirstOrDefault(t => t.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (tableDef == null) throw new Exception("Tabla no existe en la definición.");

            // 2. Filtrar columnas validas y quitar Identities (Autoincrementables)
            var validColumns = schema.Columns
                .Where(c => c.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase) && !c.Is_Identity)
                .Select(c => c.Name)
                .ToList();

            // 3. Preparar los datos que coinciden con las columnas
            var insertData = new DynamicParameters();
            var colsToInsert = new List<string>();
            var paramsToInsert = new List<string>();

            foreach (var key in data.Keys)
            {
                // Solo agregamos si la columna existe en el schema y no es identity
                if (validColumns.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    colsToInsert.Add($"[{key}]"); // [NombreColumna]
                    paramsToInsert.Add($"@{key}"); // @NombreColumna
                    insertData.Add(key, data[key]); // Dapper maneja el tipo de dato
                }
            }

            // 4. Construir el SQL
            string sql = $"INSERT INTO [{tableDef.Schema}].[{tableName}] ({string.Join(",", colsToInsert)}) VALUES ({string.Join(",", paramsToInsert)})";

            // 5. Ejecutar con Dapper
            using (var db = new SqlConnection(connectionString))
            {
                return await db.ExecuteAsync(sql, insertData);
            }
        }

        // MÉTODO: SELECT ALL DINÁMICO
        public async Task<IEnumerable<dynamic>> GetAllDynamicAsync(string tableName, DbSchema schema, string connectionString)
        {
            // Validar tabla
            var tableDef = schema.Tables.FirstOrDefault(t => t.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (tableDef == null) throw new Exception("Tabla no permitida.");

            string sql = $"SELECT * FROM [{tableDef.Schema}].[{tableName}]";

            using (var db = new SqlConnection(connectionString))
            {
                return await db.QueryAsync(sql); // Dapper retorna 'dynamic' automáticamente
            }
        }

        // MÉTODO NUEVO: GET BY ID DINÁMICO
        public async Task<dynamic> GetByIdDynamicAsync(string tableName, object id, DbSchema schema, string connectionString)
        {
            // 1. Validar tabla
            var tableDef = schema.Tables.FirstOrDefault(t => t.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (tableDef == null) throw new Exception("Tabla no permitida.");

            // 2. Buscar la Primary Key
            var pkInfo = schema.Pk_Info.FirstOrDefault(pk => pk.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (pkInfo == null) throw new Exception("No se encontró PK para esta tabla.");

            string pkColumn = pkInfo.Column;

            // 3. Construir el SQL
            string sql = $"SELECT * FROM [{tableDef.Schema}].[{tableName}] WHERE [{pkColumn}] = @Id";

            // 4. Ejecutar con Dapper
            using (var db = new SqlConnection(connectionString))
            {
                return await db.QueryFirstOrDefaultAsync(sql, new { Id = id });
            }
        }

        // MÉTODO: UPDATE DINÁMICO (Requiere saber cuál es la Primary Key)
        public async Task<int> UpdateDynamicAsync(string tableName, Dictionary<string, object> data, DbSchema schema, string connectionString)
        {
            // 1. Buscar cuál es la Primary Key en tu JSON (pk_info)
            var pkInfo = schema.Pk_Info.FirstOrDefault(pk => pk.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (pkInfo == null) throw new Exception("No se encontró PK para esta tabla.");

            string pkColumn = pkInfo.Column;

            if (!data.ContainsKey(pkColumn)) throw new Exception("El ID es necesario para actualizar.");

            // 2. Construir el SET col = @col
            var updateClauses = new List<string>();
            var parameters = new DynamicParameters();

            foreach (var kvp in data)
            {
                if (kvp.Key != pkColumn) // No actualizamos la PK
                {
                    updateClauses.Add($"[{kvp.Key}] = @{kvp.Key}");
                    parameters.Add(kvp.Key, kvp.Value);
                }
            }

            // Agregar el valor del ID para el WHERE
            parameters.Add(pkColumn, data[pkColumn]);

            string sql = $"UPDATE [{tableName}] SET {string.Join(", ", updateClauses)} WHERE [{pkColumn}] = @{pkColumn}";

            using (var db = new SqlConnection(connectionString))
            {
                return await db.ExecuteAsync(sql, parameters);
            }
        }
    }
}
