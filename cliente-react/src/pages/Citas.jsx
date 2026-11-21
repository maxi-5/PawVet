import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listarCitas, cambiarEstadoCita, eliminarCita } from '../api/agenda.js'
import { useAuth } from '../context/AuthContext.jsx'
import { Encabezado, Cargando, Mensaje, Paginacion, EstadoCita, formatoFechaHora, useNombres } from '../components/Comunes.jsx'

// Agenda de citas para Administrador y Recepcionista (RF-07, HU-03, HU-04)
export default function Citas() {
  const { tieneRol } = useAuth()
  const { veterinarios, nombreMascota, nombreVeterinario } = useNombres()

  const [filtros, setFiltros] = useState({ fecha: '', idVeterinario: '', estado: '' })
  const [pagina, setPagina] = useState(1)
  const [datos, setDatos] = useState(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [exito, setExito] = useState('')

  async function cargar() {
    setCargando(true)
    setError('')
    try {
      setDatos(await listarCitas({ ...filtros, pagina }))
    } catch (err) {
      setError(err.message)
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar() }, [pagina])

  // Confirmada, Cancelada o NoAsistio (diagrama de estados)
  async function cambiarEstado(cita, estado, texto) {
    if (estado === 'Cancelada' && !window.confirm('¿Cancelar esta cita?')) return
    setError('')
    setExito('')
    try {
      await cambiarEstadoCita(cita.idCita, estado)
      setExito(texto)
      cargar()
    } catch (err) {
      setError(err.message)
    }
  }

  async function eliminar(cita) {
    if (!window.confirm('¿Eliminar definitivamente esta cita?')) return
    setError('')
    try {
      await eliminarCita(cita.idCita)
      setExito('Cita eliminada')
      cargar()
    } catch (err) {
      setError(err.message)
    }
  }

  const cambiar = (campo) => (e) => setFiltros({ ...filtros, [campo]: e.target.value })
  const futura = (c) => new Date(c.fechaHora) > new Date()

  return (
    <div>
      <Encabezado titulo="Citas" descripcion="Agenda de la clínica: reserva, confirma, reprograma o cancela horas." ayuda={[["Filtrar", "Muestra solo las citas de la fecha, veterinario o estado elegidos."], ["+ Agendar cita", "Reserva una hora. Revisa que no sea feriado ni choque con otra cita del veterinario."], ["Confirmar", "Marca que el tutor confirmó; solo así el veterinario puede atenderla."], ["Reprogramar", "Cambia fecha, hora, veterinario o motivo de una cita aún no atendida."], ["Cancelar", "Anula la cita y deja libre el horario."], ["No asistió", "Aparece cuando ya pasó la hora: registra que la mascota no llegó."], ["Eliminar", "Borra la cita (solo Administrador y si no fue atendida)."]]} />
      <div className="filtros">
        <input type="date" value={filtros.fecha} onChange={cambiar('fecha')} />
        <select value={filtros.idVeterinario} onChange={cambiar('idVeterinario')}>
          <option value="">Todos los veterinarios</option>
          {veterinarios.map((v) => <option key={v.idVeterinario} value={v.idVeterinario}>{v.nombre}</option>)}
        </select>
        <select value={filtros.estado} onChange={cambiar('estado')}>
          <option value="">Todos los estados</option>
          <option>Agendada</option><option>Confirmada</option><option>Atendida</option>
          <option>Cancelada</option><option value="NoAsistio">No asistió</option>
        </select>
        <button onClick={() => { setPagina(1); cargar() }} title="Aplica la fecha, el veterinario y el estado elegidos">Filtrar</button>
        <span className="espacio" />
        <Link className="boton" to="/citas/nueva" title="Abre el formulario para reservar una hora">+ Agendar cita</Link>
      </div>

      <Mensaje texto={error} />
      <Mensaje tipo="exito" texto={exito} />
      {cargando ? <Cargando /> : datos && (
        <>
          {datos.items.length === 0 ? <p className="vacio">No hay citas con esos filtros.</p> : (
            <table>
              <thead><tr><th>Fecha y hora</th><th>Mascota</th><th>Veterinario</th><th>Motivo</th><th>Estado</th><th></th></tr></thead>
              <tbody>
                {datos.items.map((c) => (
                  <tr key={c.idCita}>
                    <td>{formatoFechaHora(c.fechaHora)} ({c.duracionMinutos} min)</td>
                    <td>{nombreMascota(c.idMascota)}</td>
                    <td>{nombreVeterinario(c.idVeterinario)}</td>
                    <td>{c.motivo}</td>
                    <td><EstadoCita estado={c.estado} /></td>
                    <td className="acciones">
                      {c.estado === 'Agendada' && futura(c) && <button onClick={() => cambiarEstado(c, 'Confirmada', 'Cita confirmada')} title="Marca que el tutor confirmó que asistirá">Confirmar</button>}
                      {(c.estado === 'Agendada' || c.estado === 'Confirmada') && futura(c) && (
                        <>
                          <Link to={`/citas/${c.idCita}/editar`} title="Cambia la fecha, hora, veterinario o motivo">Reprogramar</Link>
                          <button onClick={() => cambiarEstado(c, 'Cancelada', 'Cita cancelada')} title="Anula la cita y libera el horario">Cancelar</button>
                        </>
                      )}
                      {(c.estado === 'Agendada' || c.estado === 'Confirmada') && !futura(c) && (
                        <button onClick={() => cambiarEstado(c, 'NoAsistio', 'Marcada como no asistida')} title="Registra que la mascota no llegó a la hora">No asistió</button>
                      )}
                      {tieneRol('Administrador') && c.estado !== 'Atendida' && <button className="peligro" onClick={() => eliminar(c)} title="Borra la cita definitivamente">Eliminar</button>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
          <Paginacion pagina={datos.pagina} tamano={datos.tamano} total={datos.total} onCambiar={setPagina} />
        </>
      )}
    </div>
  )
}
