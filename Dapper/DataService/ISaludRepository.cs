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
        #endregion

        #region Medicos
        Task<IEnumerable<Medico>> ObtenerMedicosAsync();
        Task<bool> CrearMedicoAsync(Medico medico);
        #endregion

        #region Citas y Diagnosticos
        // Agendar una nueva cita (Estado: programada)
        Task<bool> AgendarCitaAsync(Cita cita);
        
        // Ver la agenda de un doctor en una fecha específica
        Task<IEnumerable<Cita>> ObtenerAgendaMedicoAsync(int medicoId, DateTime fecha);
        
        // Finalizar la cita creando un diagnóstico (Cambia estado a: completada)
        Task<bool> RegistrarDiagnosticoAsync(Diagnostico diagnostico);
        #endregion
    }
}