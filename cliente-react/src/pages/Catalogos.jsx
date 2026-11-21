import { useEffect, useState } from 'react'
import { listarEspecies, crearEspecie, eliminarEspecie, listarRazas, crearRaza, eliminarRaza } from '../api/clinica.js'
import { Encabezado, Cargando, Mensaje } from '../components/Comunes.jsx'

// Catalogo de especies y razas (Administrador, RF-05)
export default function Catalogos() {
  const [especies, setEspecies] = useState([])
  const [razas, setRazas] = useState([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [nuevaEspecie, setNuevaEspecie] = useState('')
  const [nuevaRaza, setNuevaRaza] = useState({ nombre: '', idEspecie: '' })

  async function cargar() {
    setError('')
    try {
      const [e, r] = await Promise.all([listarEspecies(), listarRazas()])
      setEspecies(e)
      setRazas(r)
    } catch (err) {
      setError(err.message)
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar() }, [])

  // ejecuta una accion y recarga; los errores 409 (en uso / repetida) se muestran tal cual
  async function ejecutar(accion) {
    setError('')
    try {
      await accion()
      cargar()
      return true
    } catch (err) {
      setError(err.message)
      return false
    }
  }

  if (cargando) return <Cargando />

  return (
    <div>
      <Encabezado titulo="Especies y razas" descripcion="Listas que se usan al registrar una mascota." ayuda={[["Agregar (especies)", "Crea una especie nueva, por ejemplo Conejo."], ["Agregar (razas)", "Crea una raza dentro de la especie elegida."], ["Eliminar", "Borra la especie o raza; no se puede si está en uso."]]} />
      <Mensaje texto={error} />

      <div className="dos-columnas">
        <section>
          <h3>Especies</h3>
          <form className="en-linea" onSubmit={(e) => { e.preventDefault(); ejecutar(() => crearEspecie(nuevaEspecie)).then((ok) => ok && setNuevaEspecie('')) }}>
            <input value={nuevaEspecie} onChange={(e) => setNuevaEspecie(e.target.value)} placeholder="Nueva especie" required />
            <button type="submit" title="Agrega la especie escrita">Agregar</button>
          </form>
          <ul className="lista">
            {especies.map((e) => (
              <li key={e.idEspecie}>{e.nombre}
                <button className="peligro" title="Elimina la especie si no tiene razas ni mascotas" onClick={() => window.confirm(`¿Eliminar ${e.nombre}?`) && ejecutar(() => eliminarEspecie(e.idEspecie))}>Eliminar</button>
              </li>
            ))}
          </ul>
        </section>

        <section>
          <h3>Razas</h3>
          <form className="en-linea" onSubmit={(e) => { e.preventDefault(); ejecutar(() => crearRaza(nuevaRaza.nombre, Number(nuevaRaza.idEspecie))).then((ok) => ok && setNuevaRaza({ nombre: '', idEspecie: nuevaRaza.idEspecie })) }}>
            <input value={nuevaRaza.nombre} onChange={(e) => setNuevaRaza({ ...nuevaRaza, nombre: e.target.value })} placeholder="Nueva raza" required />
            <select value={nuevaRaza.idEspecie} onChange={(e) => setNuevaRaza({ ...nuevaRaza, idEspecie: e.target.value })} required>
              <option value="">Especie...</option>
              {especies.map((e) => <option key={e.idEspecie} value={e.idEspecie}>{e.nombre}</option>)}
            </select>
            <button type="submit" title="Agrega la raza a la especie elegida">Agregar</button>
          </form>
          <ul className="lista">
            {razas.map((r) => (
              <li key={r.idRaza}>{r.nombre} <small>({r.especie})</small>
                <button className="peligro" title="Elimina la raza si ninguna mascota la usa" onClick={() => window.confirm(`¿Eliminar ${r.nombre}?`) && ejecutar(() => eliminarRaza(r.idRaza))}>Eliminar</button>
              </li>
            ))}
          </ul>
        </section>
      </div>
    </div>
  )
}
