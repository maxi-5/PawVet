import { useEffect, useState } from 'react'
import { listarTutores, crearTutor, editarTutor, eliminarTutor } from '../api/clinica.js'
import { useAuth } from '../context/AuthContext.jsx'
import { Encabezado, Cargando, Mensaje, Paginacion } from '../components/Comunes.jsx'

const VACIO = { rut: '', nombre: '', telefono: '+569', email: '', direccion: '' }

// Tutores (RF-03): todos ven; Administrador y Recepcionista crean/editan; solo Administrador elimina
export default function Tutores() {
  const { tieneRol } = useAuth()
  const puedeEditar = tieneRol('Administrador', 'Recepcionista')

  const [pagina, setPagina] = useState(1)
  const [q, setQ] = useState('')
  const [datos, setDatos] = useState(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [exito, setExito] = useState('')
  const [form, setForm] = useState(null) // null = sin formulario abierto
  const [idEditando, setIdEditando] = useState(null)

  async function cargar() {
    setCargando(true)
    setError('')
    try {
      setDatos(await listarTutores(pagina, q))
    } catch (err) {
      setError(err.message)
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar() }, [pagina])

  function nuevo() { setForm(VACIO); setIdEditando(null); setExito('') }
  function editar(t) {
    setForm({ rut: t.rut, nombre: t.nombre, telefono: t.telefono, email: t.email ?? '', direccion: t.direccion ?? '' })
    setIdEditando(t.idTutor)
    setExito('')
  }

  async function guardar(e) {
    e.preventDefault()
    setError('')
    const cuerpo = { ...form, email: form.email || null, direccion: form.direccion || null }
    try {
      if (idEditando) await editarTutor(idEditando, cuerpo)
      else await crearTutor(cuerpo)
      setExito(idEditando ? 'Tutor actualizado' : 'Tutor registrado')
      setForm(null)
      cargar()
    } catch (err) {
      setError(err.message)
    }
  }

  async function eliminar(t) {
    if (!window.confirm(`¿Eliminar a ${t.nombre}?`)) return
    setError('')
    try {
      await eliminarTutor(t.idTutor)
      setExito('Tutor eliminado')
      cargar()
    } catch (err) {
      setError(err.message)
    }
  }

  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value })

  return (
    <div>
      <Encabezado titulo="Tutores" descripcion="Dueños de las mascotas y sus datos de contacto." ayuda={[["Buscar", "Filtra la lista por nombre o RUT."], ["+ Nuevo tutor", "Abre el formulario para registrar un tutor (Administrador y Recepcionista)."], ["Editar", "Carga los datos del tutor en el formulario para corregirlos."], ["Eliminar", "Borra al tutor. Solo el Administrador, y solo si no tiene mascotas."], ["Guardar / Cancelar", "Guarda el formulario o lo cierra sin cambios."]]} />
      <div className="filtros">
        <input placeholder="Buscar por nombre o RUT" value={q} onChange={(e) => setQ(e.target.value)} />
        <button onClick={() => { setPagina(1); cargar() }} title="Filtra el listado con el texto escrito">Buscar</button>
        <span className="espacio" />
        {puedeEditar && <button onClick={nuevo} title="Abre el formulario para registrar un tutor">+ Nuevo tutor</button>}
      </div>

      {form && (
        <form className="formulario" onSubmit={guardar}>
          <h3>{idEditando ? 'Editar tutor' : 'Nuevo tutor'}</h3>
          <label>RUT <input value={form.rut} onChange={cambiar('rut')} placeholder="12345678-5" required /></label>
          <label>Nombre <input value={form.nombre} onChange={cambiar('nombre')} required minLength={3} /></label>
          <label>Teléfono <input value={form.telefono} onChange={cambiar('telefono')} placeholder="+56912345678" required /></label>
          <label>Correo <input type="email" value={form.email} onChange={cambiar('email')} /></label>
          <label>Dirección <input value={form.direccion} onChange={cambiar('direccion')} /></label>
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
          {datos.items.length === 0 ? <p className="vacio">No hay tutores registrados.</p> : (
            <table>
              <thead><tr><th>RUT</th><th>Nombre</th><th>Teléfono</th><th>Correo</th><th>Mascotas</th><th></th></tr></thead>
              <tbody>
                {datos.items.map((t) => (
                  <tr key={t.idTutor}>
                    <td>{t.rut}</td><td>{t.nombre}</td><td>{t.telefono}</td><td>{t.email ?? '—'}</td><td>{t.cantidadMascotas}</td>
                    <td className="acciones">
                      {puedeEditar && <button onClick={() => editar(t)} title="Modifica los datos de este tutor">Editar</button>}
                      {tieneRol('Administrador') && <button className="peligro" onClick={() => eliminar(t)} title="Elimina al tutor (solo si no tiene mascotas)">Eliminar</button>}
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
