import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { miAgenda } from '../api/agenda.js'
import { Encabezado, Cargando, Mensaje, EstadoCita, formatoHora, hoyISO, useNombres } from '../components/Comunes.jsx'

// Agenda del dia del veterinario logueado (RF-09, CU-08)
export default function MiAgenda() {
  const { nombreMascota } = useNombres()
  const [fecha, setFecha] = useState(hoyISO())
  const [citas, setCitas] = useState([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    setCargando(true)
    setError('')
    miAgenda(fecha)
      .then(setCitas)
      .catch((err) => setError(err.message))
      .finally(() => setCargando(false))
  }, [fecha])

  return (
    <div>
      <Encabezado titulo="Mi agenda" descripcion="Tus citas del día ordenadas por hora. Cambia la fecha para ver otros días." ayuda={[["Atender", "Aparece en las citas confirmadas: registra diagnóstico, tratamiento, peso, temperatura y, si corresponde, la vacuna aplicada."], ["Historial", "Muestra las atenciones y vacunas anteriores de la mascota."]]} />
      <div className="filtros">
        <input type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} />
      </div>
      <Mensaje texto={error} />
      {cargando ? <Cargando /> : !error && (
        citas.length === 0 ? <p className="vacio">No tienes citas para este día.</p> : (
          <table>
            <thead><tr><th>Hora</th><th>Mascota</th><th>Motivo</th><th>Estado</th><th></th></tr></thead>
            <tbody>
              {citas.map((c) => (
                <tr key={c.idCita}>
                  <td>{formatoHora(c.fechaHora)}</td>
                  <td>{nombreMascota(c.idMascota)}</td>
                  <td>{c.motivo}</td>
                  <td><EstadoCita estado={c.estado} /></td>
                  <td className="acciones">
                    {c.estado === 'Confirmada' && <Link className="boton" to={`/citas/${c.idCita}/atencion`} title="Abre el formulario para registrar diagnóstico y tratamiento">Atender</Link>}
                    <Link to={`/mascotas/${c.idMascota}/historial`} title="Ve atenciones y vacunas anteriores de la mascota">Historial</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )
      )}
    </div>
  )
}
