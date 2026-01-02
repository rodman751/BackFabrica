using CapaDapper.Entidades.Salud;

namespace CapaDapper.DataService
{
    public interface ISaludRepository
    {
        #region Pacientes
        Task<IEnumerable<Paciente>> ObtenerPacientesAsync();
        Task<Paciente> ObtenerPacientePorIdAsync(int id);
        Task<Paciente> ObtenerPacientePorDniAsync(string dni);
        Task<bool> CrearPacienteAsync(Paciente paciente);
        Task<bool> ActualizarHistoriaClinicaAsync(Paciente paciente);
        Task<bool> EliminarPacienteAsync(int id);
        #endregion

        #region Medicos
        Task<IEnumerable<Medico>> ObtenerMedicosAsync();
        Task<Medico> ObtenerMedicoPorIdAsync(int id);
        Task<bool> CrearMedicoAsync(Medico medico);
        Task<bool> ActualizarMedicoAsync(Medico medico);
        Task<bool> EliminarMedicoAsync(int id);
        #endregion

        #region Citas y Diagnosticos
        // Citas
        Task<IEnumerable<Cita>> ObtenerCitasAsync();
        Task<Cita> ObtenerCitaPorIdAsync(int id);
        Task<bool> AgendarCitaAsync(Cita cita);
        Task<IEnumerable<Cita>> ObtenerAgendaMedicoAsync(int medicoId, DateTime fecha);
        Task<bool> ActualizarCitaAsync(Cita cita);
        Task<bool> EliminarCitaAsync(int id);

        // Diagnosticos
        Task<IEnumerable<Diagnostico>> ObtenerDiagnosticosAsync();
        Task<Diagnostico> ObtenerDiagnosticoPorIdAsync(int id);
        Task<bool> RegistrarDiagnosticoAsync(Diagnostico diagnostico);
        Task<bool> ActualizarDiagnosticoAsync(Diagnostico diagnostico);
        Task<bool> EliminarDiagnosticoAsync(int id);
        #endregion
    }
}