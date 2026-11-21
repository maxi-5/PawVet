import { useEffect, useState } from 'react'
import { vacunasPorVencer } from '../api/agenda.js'
import { Encabezado, Cargando, Mensaje, formatoFecha, useNombres } from '../components/Comunes.jsx'

// Vacunas cuya proxima dosis vence pronto (RF-13)
export default function PorVencer() {
  const { nombreMascota } = useNombres()
  const [dias, setDias] = useState(30)
  const [lista, setLista] = useState([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    setCargando(true)
    setError('')
    vacunasPorVencer(dias)
      .then(setLista)
      .catch((err) => setError(err.message))
      .finally(() => setCargando(false))
  }, [dias])

  return (
    <div>
      <Encabezado titulo="Vacunas por vencer" descripcion="Mascotas cuya próxima dosis vence en el plazo elegido, para avisar al tutor." ayuda={[["Próximos (lista)", "Elige el plazo: 7, 15, 30 o 60 días."]]} />
      <div className="filtros">
        <label>Próximos
          <select value={dias} onChange={(e) => setDias(Number(e.target.value))}>
            <option value={7}>7 días</option><option value={15}>15 días</option><option value={30}>30 días</option><option value={60}>60 días</option>
          </select>
        </label>
      </div>
      <Mensaje texto={error} />
      {cargando ? <Cargando /> : (
        lista.length === 0 ? <p className="vacio">No hay vacunas por vencer en ese plazo.</p> : (
          <table>
            <thead><tr><th>Mascota</th><th>Vacuna</th><th>Aplicada</th><th>Vence</th></tr></thead>
            <tbody>
              {lista.map((v) => (
                <tr key={v.idVacunacion}>
                  <td>{nombreMascota(v.idMascota)}</td><td>{v.vacuna}</td><td>{formatoFecha(v.fechaAplicacion)}</td><td>{formatoFecha(v.fechaProxima)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )
      )}
    </div>
  )
}
