import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listarMascotas, crearMascota, editarMascota, eliminarMascota, listarTutores, listarEspecies, listarRazas } from '../api/clinica.js'
import { useAuth } from '../context/AuthContext.jsx'
import { Encabezado, Cargando, Mensaje, Paginacion, formatoFecha } from '../components/Comunes.jsx'

const VACIO = { nombre: '', sexo: 'Macho', fechaNacimiento: '', peso: '', microchip: '', idTutor: '', idEspecie: '', idRaza: '' }

// Mascotas (RF-04, RF-05, RF-06, HU-02)
export default function Mascotas() {
  const { tieneRol } = useAuth()
  const puedeEditar = tieneRol('Administrador', 'Recepcionista')

  const [pagina, setPagina] = useState(1)
  const [q, setQ] = useState('')
  const [datos, setDatos] = useState(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [exito, setExito] = useState('')
  const [form, setForm] = useState(null)
  const [idEditando, setIdEditando] = useState(null)

  // listas para los select del formulario
  const [tutores, setTutores] = useState([])
  const [especies, setEspecies] = useState([])
  const [razas, setRazas] = useState([])

  async function cargar() {
    setCargando(true)
    setError('')
    try {
      setDatos(await listarMascotas(pagina, q))
    } catch (err) {
      setError(err.message)
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar() }, [pagina])

  useEffect(() => {
    if (!puedeEditar) return
    listarTutores(1, '', 100).then((p) => setTutores(p.items)).catch(() => {})
    listarEspecies().then(setEspecies).catch(() => {})
  }, [])

  // al cambiar la especie se cargan solo sus razas (RF-05)
  useEffect(() => {
    if (form && form.idEspecie) listarRazas(form.idEspecie).then(setRazas).catch(() => setRazas([]))
    else setRazas([])
  }, [form?.idEspecie])

  function nuevo() { setForm(VACIO); setIdEditando(null); setExito('') }
  function editar(m) {
    setForm({
      nombre: m.nombre, sexo: m.sexo, fechaNacimiento: m.fechaNacimiento ?? '', peso: m.peso ?? '',
      microchip: m.microchip ?? '', idTutor: m.idTutor, idEspecie: m.idEspecie, idRaza: m.idRaza ?? ''
    })
    setIdEditando(m.idMascota)
    setExito('')
  }

  async function guardar(e) {
    e.preventDefault()
    setError('')
    const cuerpo = {
      nombre: form.nombre,
      sexo: form.sexo,
      fechaNacimiento: form.fechaNacimiento || null,
      peso: form.peso === '' ? null : Number(form.peso),
      microchip: form.microchip || null,
      idTutor: Number(form.idTutor),
      idEspecie: Number(form.idEspecie),
      idRaza: form.idRaza === '' ? null : Number(form.idRaza)
    }
    try {
      if (idEditando) await editarMascota(idEditando, cuerpo)
      else await crearMascota(cuerpo)
      setExito(idEditando ? 'Mascota actualizada' : 'Mascota registrada')
      setForm(null)
      cargar()
    } catch (err) {
      setError(err.message)
    }
  }

  async function eliminar(m) {
    if (!window.confirm(`¿Eliminar a ${m.nombre}?`)) return
    setError('')
    try {
      await eliminarMascota(m.idMascota)
      setExito('Mascota eliminada')
      cargar()
    } catch (err) {
      setError(err.message)
    }
  }

  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value })

  return (
    <div>
      <Encabezado titulo="Mascotas" descripcion="Fichas de los pacientes con su especie, raza, edad y tutor." ayuda={[["Buscar", "Filtra la lista por nombre o microchip."], ["+ Nueva mascota", "Abre el formulario para registrar una mascota. Al elegir la especie solo aparecen sus razas."], ["Historial", "Muestra las atenciones y vacunas de la mascota (Administrador y Veterinario)."], ["Editar", "Carga los datos de la mascota para corregirlos."], ["Eliminar", "Borra la ficha (solo Administrador)."]]} />
      <div className="filtros">
        <input placeholder="Buscar por nombre o microchip" value={q} onChange={(e) => setQ(e.target.value)} />
        <button onClick={() => { setPagina(1); cargar() }} title="Filtra el listado con el texto escrito">Buscar</button>
        <span className="espacio" />
        {puedeEditar && <button onClick={nuevo} title="Abre el formulario para registrar una mascota">+ Nueva mascota</button>}
      </div>

      {form && (
        <form className="formulario" onSubmit={guardar}>
          <h3>{idEditando ? 'Editar mascota' : 'Nueva mascota'}</h3>
          <label>Nombre <input value={form.nombre} onChange={cambiar('nombre')} required /></label>
          <label>Sexo
            <select value={form.sexo} onChange={cambiar('sexo')}><option>Macho</option><option>Hembra</option></select>
          </label>
          <label>Fecha de nacimiento <input type="date" value={form.fechaNacimiento} onChange={cambiar('fechaNacimiento')} /></label>
          <label>Peso (kg) <input type="number" step="0.01" min="0.01" value={form.peso} onChange={cambiar('peso')} /></label>
          <label>Microchip (15 dígitos) <input value={form.microchip} onChange={cambiar('microchip')} pattern="[0-9]{15}" /></label>
          <label>Tutor
            <select value={form.idTutor} onChange={cambiar('idTutor')} required>
              <option value="">Seleccione...</option>
              {tutores.map((t) => <option key={t.idTutor} value={t.idTutor}>{t.nombre} ({t.rut})</option>)}
            </select>
          </label>
          <label>Especie
            <select value={form.idEspecie} onChange={(e) => setForm({ ...form, idEspecie: e.target.value, idRaza: '' })} required>
              <option value="">Seleccione...</option>
              {especies.map((es) => <option key={es.idEspecie} value={es.idEspecie}>{es.nombre}</option>)}
            </select>
          </label>
          <label>Raza
            <select value={form.idRaza} onChange={cambiar('idRaza')}>
              <option value="">Sin raza definida</option>
              {razas.map((r) => <option key={r.idRaza} value={r.idRaza}>{r.nombre}</option>)}
            </select>
          </label>
          <div className="acciones">
            <button type="submit" title="Guarda los datos del formulario">Guardar</button>
            <button type="button" className="secundario" onClick={() => setForm(null)} title="Cierra el formulario sin guardar">Cancelar</button>
          </div>
        </form>
      )}

      <Mensaje texto={error} />
      <Mensaje tipo="exito" texto={exito} />
      {cargando ? <Cargando /> : datos && (
        <>
          {datos.items.length === 0 ? <p className="vacio">No hay mascotas registradas.</p> : (
            <table>
              <thead><tr><th>Nombre</th><th>Especie / raza</th><th>Sexo</th><th>Edad</th><th>Nacimiento</th><th>Tutor</th><th></th></tr></thead>
              <tbody>
                {datos.items.map((m) => (
                  <tr key={m.idMascota}>
                    <td>{m.nombre}</td>
                    <td>{m.especie}{m.raza ? ' / ' + m.raza : ''}</td>
                    <td>{m.sexo}</td>
                    <td>{m.edad != null ? `${m.edad} años` : '—'}</td>
                    <td>{formatoFecha(m.fechaNacimiento)}</td>
                    <td>{m.nombreTutor}</td>
                    <td className="acciones">
                      {tieneRol('Administrador', 'Veterinario') && <Link to={`/mascotas/${m.idMascota}/historial`} title="Ve las atenciones y vacunas de esta mascota">Historial</Link>}
                      {puedeEditar && <button onClick={() => editar(m)} title="Modifica los datos de esta mascota">Editar</button>}
                      {tieneRol('Administrador') && <button className="peligro" onClick={() => eliminar(m)} title="Elimina la ficha de la mascota">Eliminar</button>}
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
