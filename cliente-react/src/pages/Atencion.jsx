import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { obtenerCita, registrarAtencion, listarVacunas, registrarVacunacion } from '../api/agenda.js'
import { obtenerMascota } from '../api/clinica.js'
import { Cargando, Mensaje, formatoFechaHora, hoyISO } from '../components/Comunes.jsx'

// Registrar la atencion de una cita confirmada (RF-10, HU-05) y, opcionalmente, una vacuna (RF-11)
export default function Atencion() {
  const { id } = useParams()
  const navigate = useNavigate()

  const [cita, setCita] = useState(null)
  const [mascota, setMascota] = useState(null)
  const [vacunas, setVacunas] = useState([])
  const [form, setForm] = useState({ diagnostico: '', tratamiento: '', peso: '', temperatura: '' })
  const [vacuna, setVacuna] = useState({ idVacuna: '', lote: '', fecha: hoyISO() })
  const [cargando, setCargando] = useState(true)
  const [enviando, setEnviando] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    async function cargar() {
      try {
        const c = await obtenerCita(id)
        setCita(c)
        const m = await obtenerMascota(c.idMascota)
        setMascota(m)
        setVacunas(await listarVacunas(m.idEspecie))
      } catch (err) {
        setError(err.message)
      } finally {
        setCargando(false)
      }
    }
    cargar()
  }, [id])

  async function guardar(e) {
    e.preventDefault()
    setError('')
    setEnviando(true)
    try {
      const atencion = await registrarAtencion({
        idCita: Number(id),
        diagnostico: form.diagnostico,
        tratamiento: form.tratamiento || null,
        peso: form.peso === '' ? null : Number(form.peso),
        temperatura: form.temperatura === '' ? null : Number(form.temperatura)
      })
      if (vacuna.idVacuna) {
        try {
          await registrarVacunacion({
            idMascota: cita.idMascota,
            idVacuna: Number(vacuna.idVacuna),
            fechaAplicacion: vacuna.fecha,
            lote: vacuna.lote || null,
            idAtencion: atencion.idAtencion
          })
        } catch (err) {
          // la atencion ya quedo guardada: no se debe volver a enviar
          setError('La atención se guardó, pero la vacuna no: ' + err.message)
          setCita({ ...cita, estado: 'Atendida' })
          return
        }
      }
      navigate(`/mascotas/${cita.idMascota}/historial`)
    } catch (err) {
      setError(err.message)
    } finally {
      setEnviando(false)
    }
  }

  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value })

  if (cargando) return <Cargando />
  if (!cita) return <Mensaje texto={error} />
  if (cita.estado === 'Atendida') {
    return (
      <div>
        <Mensaje texto={error || 'Esta cita ya fue atendida.'} />
        <button onClick={() => navigate(`/mascotas/${cita.idMascota}/historial`)} title="Ve la ficha clínica de la mascota">Ver historial</button>
      </div>
    )
  }

  return (
    <form className="formulario" onSubmit={guardar}>
      <h2>Atención de {mascota?.nombre ?? 'la mascota'}</h2>
      <p>{formatoFechaHora(cita.fechaHora)} — {cita.motivo}</p>
      {mascota && <p>{mascota.especie}{mascota.raza ? ' / ' + mascota.raza : ''} · Tutor: {mascota.nombreTutor}</p>}

      <label>Diagnóstico <textarea value={form.diagnostico} onChange={cambiar('diagnostico')} required minLength={3} /></label>
      <label>Tratamiento <textarea value={form.tratamiento} onChange={cambiar('tratamiento')} /></label>
      <label>Peso (kg) <input type="number" step="0.01" min="0.01" value={form.peso} onChange={cambiar('peso')} /></label>
      <label>Temperatura (°C) <input type="number" step="0.1" min="30" max="45" value={form.temperatura} onChange={cambiar('temperatura')} /></label>

      <h3>Vacuna aplicada (opcional)</h3>
      <label>Vacuna
        <select value={vacuna.idVacuna} onChange={(e) => setVacuna({ ...vacuna, idVacuna: e.target.value })}>
          <option value="">No se aplicó vacuna</option>
          {vacunas.map((v) => <option key={v.idVacuna} value={v.idVacuna}>{v.nombre} (cada {v.intervaloDias} días)</option>)}
        </select>
      </label>
      {vacuna.idVacuna && (
        <>
          <label>Fecha de aplicación <input type="date" max={hoyISO()} value={vacuna.fecha} onChange={(e) => setVacuna({ ...vacuna, fecha: e.target.value })} required /></label>
          <label>Lote <input value={vacuna.lote} onChange={(e) => setVacuna({ ...vacuna, lote: e.target.value })} /></label>
          <p className="ayuda">La próxima dosis se calcula sola: fecha de aplicación + intervalo de la vacuna.</p>
        </>
      )}

      <Mensaje texto={error} />
      <div className="acciones">
        <button type="submit" disabled={enviando} title="Guarda la atención y deja la cita como Atendida">{enviando ? 'Guardando...' : 'Registrar atención'}</button>
        <button type="button" className="secundario" onClick={() => navigate('/mi-agenda')} title="Vuelve a tu agenda sin guardar">Volver</button>
      </div>
    </form>
  )
}
