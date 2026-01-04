using CapaDapper.Cadena;
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using CapaDapper.Entidades.Salud;
using Microsoft.Extensions.Configuration;

namespace CapaDapper.DataService
{
    public class SaludRepository : ISaludRepository
    {
        //INYECCION NUEVA
        private readonly IDbConnectionFactory _connectionFactory;
        public SaludRepository(IDbConnectionFactory connectionFactory)
        {

            _connectionFactory = connectionFactory;

        }

        #region Pacientes
        public async Task<IEnumerable<Paciente>> ObtenerPacientesAsync()
        {
            var sql = "SELECT * FROM pacientes";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Paciente>(sql);
        }

        public async Task<Paciente> ObtenerPacientePorIdAsync(int id)
        {
            var sql = "SELECT * FROM pacientes WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Paciente>(sql, new { Id = id });
        }

        public async Task<Paciente> ObtenerPacientePorDniAsync(string dni)
        {
            var sql = "SELECT * FROM pacientes WHERE dni = @Dni";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Paciente>(sql, new { Dni = dni });
        }

        public async Task<bool> CrearPacienteAsync(Paciente p)
        {
            // SQL Server validará que 'Antecedentes' sea un JSON válido gracias al constraint ISJSON
            var sql = @"
                INSERT INTO pacientes (dni, nombres, apellidos, fecha_nacimiento, telefono, grupo_sanguineo, antecedentes)
                VALUES (@Dni, @Nombres, @Apellidos, @FechaNacimiento, @Telefono, @GrupoSanguineo, @Antecedentes)";
            
            using var conn = _connectionFactory.CreateConnection();
            try {
                return await conn.ExecuteAsync(sql, p) > 0;
            } catch (SqlException) {
                // Tip: Si falla, 99% probable que sea DNI duplicado o JSON mal formado
                return false; 
            }
        }

        public async Task<bool> ActualizarHistoriaClinicaAsync(Paciente p)
        {
            var sql = @"
                UPDATE pacientes
                SET nombres = @Nombres, apellidos = @Apellidos, telefono = @Telefono,
                    grupo_sanguineo = @GrupoSanguineo, antecedentes = @Antecedentes
                WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> EliminarPacienteAsync(int id)
        {
            var sql = "DELETE FROM pacientes WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion

        #region Medicos
        public async Task<IEnumerable<Medico>> ObtenerMedicosAsync()
        {
            var sql = "SELECT * FROM medicos";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Medico>(sql);
        }

        public async Task<Medico> ObtenerMedicoPorIdAsync(int id)
        {
            var sql = "SELECT * FROM medicos WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Medico>(sql, new { Id = id });
        }

        public async Task<bool> CrearMedicoAsync(Medico m)
        {
            var sql = "INSERT INTO medicos (usuario_id, nombres, especialidad, numero_licencia, consultorio) VALUES (@UsuarioId, @Nombres, @Especialidad, @NumeroLicencia, @Consultorio)";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, m) > 0;
        }

        public async Task<bool> ActualizarMedicoAsync(Medico m)
        {
            var sql = @"
                UPDATE medicos
                SET usuario_id = @UsuarioId, nombres = @Nombres, especialidad = @Especialidad,
                    numero_licencia = @NumeroLicencia, consultorio = @Consultorio
                WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, m) > 0;
        }

        public async Task<bool> EliminarMedicoAsync(int id)
        {
            var sql = "DELETE FROM medicos WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
		#endregion

		#region Citas y Diagnosticos
		// Citas
		public async Task<IEnumerable<Cita>> ObtenerCitasAsync()
		{
			var sql = @"
        SELECT * FROM citas";
			using var conn = _connectionFactory.CreateConnection();
			return await conn.QueryAsync<Cita>(sql);
		}


		public async Task<Cita> ObtenerCitaPorIdAsync(int id)
		{
			var sql = @"
        SELECT * 
        FROM citas 
        WHERE id = @Id";
			using var conn = _connectionFactory.CreateConnection();
			return await conn.QueryFirstOrDefaultAsync<Cita>(sql, new { Id = id });
		}

        public async Task<bool> AgendarCitaAsync(Cita c)
        {
            var sql = @"
        INSERT INTO citas (paciente_id, medico_id, fecha_hora, motivo_consulta, estado)
        VALUES (@Paciente_Id, @Medico_Id, @Fecha_Hora, @Motivo_Consulta, 'programada')";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, c) > 0;
        }



        public async Task<IEnumerable<Cita>> ObtenerAgendaMedicoAsync(int medicoId, DateTime fecha)
        {
            // Filtramos por ID médico y por el día (ignorando la hora exacta)
            var sql = @"
                SELECT c.*, p.nombres, p.apellidos
                FROM citas c
                JOIN pacientes p ON c.paciente_id = p.id
                WHERE c.medico_id = @MedicoId
                AND CAST(c.fecha_hora AS DATE) = CAST(@Fecha AS DATE)
                ORDER BY c.fecha_hora ASC";

            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Cita>(sql, new { MedicoId = medicoId, Fecha = fecha });
        }

        public async Task<bool> ActualizarCitaAsync(Cita c)
        {
            var sql = @"
        UPDATE citas
        SET paciente_id = @Paciente_Id, medico_id = @Medico_Id, fecha_hora = @Fecha_Hora,
            motivo_consulta = @Motivo_Consulta, estado = @Estado
        WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, c) > 0;
        }


        public async Task<bool> EliminarCitaAsync(int id)
        {
            var sql = "DELETE FROM citas WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }

        // Diagnosticos
        public async Task<IEnumerable<Diagnostico>> ObtenerDiagnosticosAsync()
        {
            var sql = "SELECT * FROM diagnosticos";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryAsync<Diagnostico>(sql);
        }

        public async Task<Diagnostico> ObtenerDiagnosticoPorIdAsync(int id)
        {
            var sql = "SELECT * FROM diagnosticos WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Diagnostico>(sql, new { Id = id });
        }

        public async Task<bool> RegistrarDiagnosticoAsync(Diagnostico d)
        {
            var sql = @"
        BEGIN TRANSACTION;
            INSERT INTO diagnosticos (cita_id, descripcion_diagnostico, tratamiento_recetado, proxima_visita)
            VALUES (@Cita_Id, @Descripcion_Diagnostico, @Tratamiento_Recetado, @Proxima_Visita);

            UPDATE citas SET estado = 'completada' WHERE id = @Cita_Id;
        COMMIT;";

            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, d) > 0;
        }

        public async Task<bool> ActualizarDiagnosticoAsync(Diagnostico d)
        {
            var sql = @"
        UPDATE diagnosticos
        SET cita_id = @Cita_Id, descripcion_diagnostico = @Descripcion_Diagnostico,
            tratamiento_recetado = @Tratamiento_Recetado, proxima_visita = @Proxima_Visita
        WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, d) > 0;
        }

        public async Task<bool> EliminarDiagnosticoAsync(int id)
        {
            var sql = "DELETE FROM diagnosticos WHERE id = @Id";
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        #endregion
    }
}