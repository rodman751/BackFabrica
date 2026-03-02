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
    /// <summary>
    /// Generates and executes parameterized SQL statements at runtime using
    /// the database schema definition (<see cref="DbSchema"/>) supplied by the client.
    /// Enables fully schema-driven CRUD without hard-coded entity mappings.
    /// </summary>
    public class DynamicCrudService
    {
        public DynamicCrudService() { }

        /// <summary>
        /// Inserts a new row into the specified table.
        /// Only columns present in the schema definition (excluding identity columns) are included.
        /// </summary>
        /// <param name="tableName">Name of the target table.</param>
        /// <param name="data">Field name / value pairs to insert.</param>
        /// <param name="schema">Database schema used to validate columns and resolve the schema name.</param>
        /// <param name="connectionString">ADO.NET connection string for the target database.</param>
        /// <returns>Number of rows affected.</returns>
        public async Task<int> InsertDynamicAsync(string tableName, Dictionary<string, object> data, DbSchema schema, string connectionString)
        {
            var tableDef = schema.Tables.FirstOrDefault(t => t.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (tableDef == null) throw new Exception("Tabla no existe en la definición.");

            var validColumns = schema.Columns
                .Where(c => c.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase) && !c.Is_Identity)
                .Select(c => c.Name)
                .ToList();

            var insertData = new DynamicParameters();
            var colsToInsert = new List<string>();
            var paramsToInsert = new List<string>();

            foreach (var key in data.Keys)
            {
                if (validColumns.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    colsToInsert.Add($"[{key}]");
                    paramsToInsert.Add($"@{key}");
                    insertData.Add(key, data[key]);
                }
            }

            string sql = $"INSERT INTO [{tableDef.Schema}].[{tableName}] ({string.Join(",", colsToInsert)}) VALUES ({string.Join(",", paramsToInsert)})";

            using (var db = new SqlConnection(connectionString))
            {
                return await db.ExecuteAsync(sql, insertData);
            }
        }

        /// <summary>
        /// Returns all rows from the specified table.
        /// The table must be defined in the provided schema.
        /// </summary>
        /// <param name="tableName">Name of the target table.</param>
        /// <param name="schema">Database schema used to validate the table and resolve the schema name.</param>
        /// <param name="connectionString">ADO.NET connection string for the target database.</param>
        /// <returns>A collection of dynamic row objects.</returns>
        public async Task<IEnumerable<dynamic>> GetAllDynamicAsync(string tableName, DbSchema schema, string connectionString)
        {
            var tableDef = schema.Tables.FirstOrDefault(t => t.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (tableDef == null) throw new Exception("Tabla no permitida.");

            string sql = $"SELECT * FROM [{tableDef.Schema}].[{tableName}]";

            using (var db = new SqlConnection(connectionString))
            {
                return await db.QueryAsync(sql);
            }
        }

        /// <summary>
        /// Returns a single row from the specified table by its primary key value.
        /// The primary key column is resolved from the schema's <c>Pk_Info</c> collection.
        /// </summary>
        /// <param name="tableName">Name of the target table.</param>
        /// <param name="id">Primary key value of the row to retrieve.</param>
        /// <param name="schema">Database schema used to resolve the table and primary key column.</param>
        /// <param name="connectionString">ADO.NET connection string for the target database.</param>
        /// <returns>A dynamic object representing the matched row, or <c>null</c> when not found.</returns>
        public async Task<dynamic> GetByIdDynamicAsync(string tableName, object id, DbSchema schema, string connectionString)
        {
            var tableDef = schema.Tables.FirstOrDefault(t => t.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (tableDef == null) throw new Exception("Tabla no permitida.");

            var pkInfo = schema.Pk_Info.FirstOrDefault(pk => pk.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (pkInfo == null) throw new Exception("No se encontró PK para esta tabla.");

            string pkColumn = pkInfo.Column;

            string sql = $"SELECT * FROM [{tableDef.Schema}].[{tableName}] WHERE [{pkColumn}] = @Id";

            using (var db = new SqlConnection(connectionString))
            {
                return await db.QueryFirstOrDefaultAsync(sql, new { Id = id });
            }
        }

        /// <summary>
        /// Updates an existing row in the specified table.
        /// The primary key column is excluded from the SET clause and used in the WHERE predicate.
        /// </summary>
        /// <param name="tableName">Name of the target table.</param>
        /// <param name="data">Field name / value pairs including the primary key.</param>
        /// <param name="schema">Database schema used to resolve the primary key column.</param>
        /// <param name="connectionString">ADO.NET connection string for the target database.</param>
        /// <returns>Number of rows affected.</returns>
        public async Task<int> UpdateDynamicAsync(string tableName, Dictionary<string, object> data, DbSchema schema, string connectionString)
        {
            var pkInfo = schema.Pk_Info.FirstOrDefault(pk => pk.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (pkInfo == null) throw new Exception("No se encontró PK para esta tabla.");

            string pkColumn = pkInfo.Column;

            if (!data.ContainsKey(pkColumn)) throw new Exception("El ID es necesario para actualizar.");

            var updateClauses = new List<string>();
            var parameters = new DynamicParameters();

            foreach (var kvp in data)
            {
                if (kvp.Key != pkColumn)
                {
                    updateClauses.Add($"[{kvp.Key}] = @{kvp.Key}");
                    parameters.Add(kvp.Key, kvp.Value);
                }
            }

            parameters.Add(pkColumn, data[pkColumn]);

            string sql = $"UPDATE [{tableName}] SET {string.Join(", ", updateClauses)} WHERE [{pkColumn}] = @{pkColumn}";

            using (var db = new SqlConnection(connectionString))
            {
                return await db.ExecuteAsync(sql, parameters);
            }
        }
    }
}
