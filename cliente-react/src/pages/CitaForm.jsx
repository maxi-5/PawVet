import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { obtenerCita, crearCita, editarCita } from '../api/agenda.js'
import { Cargando, Mensaje, useNombres } from '../components/Comunes.jsx'

// Agendar o reprogramar una cita (RF-07, RF-08, RF-12, HU-03, Secuencia 2)
export default function CitaForm() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { mascotas, veterinarios } = useNombres()

  const [form, setForm] = useState({ idMascota: '', idVeterinario: '', fechaHora: '', duracionMinutos: 30, motivo: '' })
  const [cargando, setCargando] = useState(Boolean(id))
  const [enviando, setEnviando] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id) return
    obtenerCita(id)
      .then((c) => setForm({
        idMascota: c.idMascota, idVeterinario: c.idVeterinario,
        fechaHora: c.fechaHora.slice(0, 16), duracionMinutos: c.duracionMinutos, motivo: c.motivo
      }))
      .catch((err) => setError(err.message))
      .finally(() => setCargando(false))
  }, [id])

  async function guardar(e) {
    e.preventDefault()
    setError('')
    setEnviando(true)
    const cuerpo = {
      idMascota: Number(form.idMascota),
      idVeterinario: Number(form.idVeterinario),
      fechaHora: form.fechaHora,
      duracionMinutos: Number(form.duracionMinutos),
      motivo: form.motivo
    }
    try {
      if (id) await editarCita(id, cuerpo)
      else await crearCita(cuerpo)
      navigate('/citas')
    } catch (err) {
      // aqui llegan los mensajes de la API: feriado (400), tope de horario (409),
      // mascota inexistente (400) o API Clinica sin respuesta (503)
      setError(err.message)
    } finally {
      setEnviando(false)
    }
  }

  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value })

  if (cargando) return <Cargando />

  return (
    <form className="formulario" onSubmit={guardar}>
      <h2>{id ? 'Reprogramar cita' : 'Agendar cita'}</h2>
      <p>Al guardar se verifica que la mascota y el veterinario existan, que el día no sea feriado y que el veterinario no tenga otra cita a esa hora.</p>
      <label>Mascota
        <select value={form.idMascota} onChange={cambiar('idMascota')} required>
          <option value="">Seleccione...</option>
          {mascotas.map((m) => <option key={m.idMascota} value={m.idMascota}>{m.nombre} — {m.especie} ({m.nombreTutor})</option>)}
        </select>
      </label>
      <label>Veterinario
        <select value={form.idVeterinario} onChange={cambiar('idVeterinario')} required>
          <option value="">Seleccione...</option>
          {veterinarios.map((v) => <option key={v.idVeterinario} value={v.idVeterinario}>{v.nombre}{v.especialidad ? ` (${v.especialidad})` : ''}</option>)}
        </select>
      </label>
      <label>Fecha y hora <input type="datetime-local" value={form.fechaHora} onChange={cambiar('fechaHora')} required /></label>
      <label>Duración (minutos) <input type="number" min="15" max="120" step="5" value={form.duracionMinutos} onChange={cambiar('duracionMinutos')} required /></label>
      <label>Motivo <textarea value={form.motivo} onChange={cambiar('motivo')} required minLength={3} maxLength={300} /></label>
      <Mensaje texto={error} />
      <div className="acciones">
        <button type="submit" disabled={enviando} title="Revisa feriados y topes de horario y guarda la cita">{enviando ? 'Verificando disponibilidad...' : 'Guardar cita'}</button>
        <button type="button" className="secundario" onClick={() => navigate('/citas')} title="Vuelve a la agenda sin guardar">Volver</button>
      </div>
    </form>
  )
}
