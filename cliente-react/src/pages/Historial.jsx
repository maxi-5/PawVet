import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { obtenerMascota } from '../api/clinica.js'
import { atencionesDeMascota, vacunacionesDeMascota, listarVacunas, registrarVacunacion, editarVacunacion } from '../api/agenda.js'
import { useAuth } from '../context/AuthContext.jsx'
import { Cargando, Mensaje, formatoFecha, formatoFechaHora, hoyISO, useNombres } from '../components/Comunes.jsx'

// Historial clinico de una mascota: atenciones y vacunas (CU-09, CU-10)
// Combina datos de las dos APIs.
export default function Historial() {
  const { id } = useParams()
  const { nombreVeterinario } = useNombres()
  const [mascota, setMascota] = useState(null)
  const [atenciones, setAtenciones] = useState([])
  const [vacunaciones, setVacunaciones] = useState([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [exito, setExito] = useState('')

  // registro y correccion de vacunas (solo Veterinario)
  const { tieneRol } = useAuth()
  const esVeterinario = tieneRol('Veterinario')
  const [vacunas, setVacunas] = useState([])
  const [form, setForm] = useState(null)          // null = formulario cerrado
  const [idEditando, setIdEditando] = useState(null)

  useEffect(() => {
    if (esVeterinario && mascota) listarVacunas(mascota.idEspecie).then(setVacunas).catch(() => {})
  }, [mascota])

  async function recargarVacunas() {
    setVacunaciones(await vacunacionesDeMascota(id))
  }

  function nuevaVacuna() {
    setForm({ idVacuna: '', fechaAplicacion: hoyISO(), lote: '' })
    setIdEditando(null)
    setExito('')
  }

  function corregir(v) {
    setForm({ idVacuna: v.idVacuna, fechaAplicacion: v.fechaAplicacion, lote: v.lote ?? '' })
    setIdEditando(v.idVacunacion)
    setExito('')
  }

  async function guardarVacuna(e) {
    e.preventDefault()
    setError('')
    try {
      if (idEditando) {
        await editarVacunacion(idEditando, { fechaAplicacion: form.fechaAplicacion, lote: form.lote || null })
        setExito('Vacuna corregida: la próxima dosis se recalculó')
      } else {
        await registrarVacunacion({ idMascota: Number(id), idVacuna: Number(form.idVacuna), fechaAplicacion: form.fechaAplicacion, lote: form.lote || null })
        setExito('Vacuna registrada')
      }
      setForm(null)
      await recargarVacunas()
    } catch (err) {
      setError(err.message)
    }
  }

  useEffect(() => {
    Promise.all([obtenerMascota(id), atencionesDeMascota(id), vacunacionesDeMascota(id)])
      .then(([m, a, v]) => { setMascota(m); setAtenciones(a); setVacunaciones(v) })
      .catch((err) => setError(err.message))
      .finally(() => setCargando(false))
  }, [id])

  if (cargando) return <Cargando />

  return (
    <div>
      <Mensaje texto={error} />
      {mascota && (
        <>
          <h2>Historial de {mascota.nombre}</h2>
          <p className="ayuda">Ficha clínica: atenciones de la más reciente a la más antigua y vacunas con su próxima dosis.</p>
          {esVeterinario && (
            <details className="ayuda-botones">
              <summary>¿Qué hace cada botón?</summary>
              <dl>
                <dt>+ Registrar vacuna</dt><dd>Registra una vacuna indicando la fecha en que se aplicó (sirve para vacunas puestas antes o en otra clínica).</dd>
                <dt>Corregir</dt><dd>Cambia la fecha de aplicación o el lote de una vacuna ya registrada; la próxima dosis se vuelve a calcular.</dd>
              </dl>
            </details>
          )}
          <p>{mascota.especie}{mascota.raza ? ' / ' + mascota.raza : ''} · {mascota.sexo} · {mascota.edad != null ? `${mascota.edad} años` : 'edad desconocida'} · Tutor: {mascota.nombreTutor}</p>
        </>
      )}

      <h3>Atenciones</h3>
      {atenciones.length === 0 ? <p className="vacio">Sin atenciones registradas.</p> : (
        <table>
          <thead><tr><th>Fecha</th><th>Veterinario</th><th>Diagnóstico</th><th>Tratamiento</th><th>Peso</th><th>T°</th></tr></thead>
          <tbody>
            {atenciones.map((a) => (
              <tr key={a.idAtencion}>
                <td>{formatoFechaHora(a.fecha)}</td>
                <td>{nombreVeterinario(a.idVeterinario)}</td>
                <td>{a.diagnostico}</td>
                <td>{a.tratamiento ?? '—'}</td>
                <td>{a.peso != null ? `${a.peso} kg` : '—'}</td>
                <td>{a.temperatura != null ? `${a.temperatura} °C` : '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <div className="filtros" style={{ marginTop: 22 }}>
        <strong>Vacunas</strong>
        <span className="espacio" />
        {esVeterinario && (
          <button onClick={nuevaVacuna} title="Registra una vacuna con la fecha en que se aplicó, aunque sea anterior a hoy">+ Registrar vacuna</button>
        )}
      </div>
      <Mensaje tipo="exito" texto={exito} />

      {form && (
        <form className="formulario" onSubmit={guardarVacuna}>
          <h3>{idEditando ? 'Corregir vacuna' : 'Registrar vacuna'}</h3>
          <label>Vacuna
            <select value={form.idVacuna} onChange={(e) => setForm({ ...form, idVacuna: e.target.value })} required disabled={Boolean(idEditando)}>
              <option value="">Seleccione...</option>
              {vacunas.map((v) => <option key={v.idVacuna} value={v.idVacuna}>{v.nombre} (cada {v.intervaloDias} días)</option>)}
            </select>
          </label>
          <label>Fecha de aplicación <input type="date" max={hoyISO()} value={form.fechaAplicacion} onChange={(e) => setForm({ ...form, fechaAplicacion: e.target.value })} required /></label>
          <label>Lote <input value={form.lote} onChange={(e) => setForm({ ...form, lote: e.target.value })} /></label>
          <p className="ayuda">La próxima dosis se calcula sola: fecha de aplicación + intervalo de la vacuna.</p>
          <div className="acciones">
            <button type="submit" title="Guarda la vacuna y calcula la próxima dosis">Guardar</button>
            <button type="button" className="secundario" onClick={() => setForm(null)} title="Cierra el formulario sin guardar">Cancelar</button>
          </div>
        </form>
      )}

      {vacunaciones.length === 0 ? <p className="vacio">Sin vacunas registradas.</p> : (
        <table>
          <thead><tr><th>Vacuna</th><th>Aplicada</th><th>Próxima dosis</th><th>Lote</th>{esVeterinario && <th></th>}</tr></thead>
          <tbody>
            {vacunaciones.map((v) => (
              <tr key={v.idVacunacion}>
                <td>{v.vacuna}</td><td>{formatoFecha(v.fechaAplicacion)}</td><td>{formatoFecha(v.fechaProxima)}</td><td>{v.lote ?? '—'}</td>
                {esVeterinario && (
                  <td className="acciones">
                    <button onClick={() => corregir(v)} title="Corrige la fecha de aplicación o el lote; la próxima dosis se recalcula">Corregir</button>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
