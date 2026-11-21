import { useEffect, useState } from 'react'
import { listarVeterinarios, crearVeterinario, editarVeterinario, eliminarVeterinario, listarUsuarios } from '../api/clinica.js'
import { Encabezado, Cargando, Mensaje } from '../components/Comunes.jsx'

const VACIO = { rut: '', nombre: '', especialidad: '', telefono: '', email: '', idUser: '' }

// Fichas de veterinarios (Administrador). idUser vincula la ficha con su usuario
// para que el token traiga el claim id_veterinario (Hito 2, ADR-002).
export default function Veterinarios() {
  const [lista, setLista] = useState([])
  const [usuariosVet, setUsuariosVet] = useState([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [exito, setExito] = useState('')
  const [form, setForm] = useState(null)
  const [idEditando, setIdEditando] = useState(null)

  async function cargar() {
    setCargando(true)
    setError('')
    try {
      setLista(await listarVeterinarios())
    } catch (err) {
      setError(err.message)
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => {
    cargar()
    // usuarios con rol Veterinario para vincular (primera pagina de usuarios)
    listarUsuarios(1).then((p) => setUsuariosVet(p.items.filter((u) => u.rol === 'Veterinario'))).catch(() => {})
  }, [])

  function nuevo() { setForm(VACIO); setIdEditando(null); setExito('') }
  function editar(v) {
    setForm({ rut: v.rut, nombre: v.nombre, especialidad: v.especialidad ?? '', telefono: v.telefono ?? '', email: v.email ?? '', idUser: v.idUser ?? '' })
    setIdEditando(v.idVeterinario)
    setExito('')
  }

  async function guardar(e) {
    e.preventDefault()
    setError('')
    const cuerpo = {
      rut: form.rut,
      nombre: form.nombre,
      especialidad: form.especialidad || null,
      telefono: form.telefono || null,
      email: form.email || null,
      idUser: form.idUser === '' ? null : Number(form.idUser)
    }
    try {
      if (idEditando) await editarVeterinario(idEditando, cuerpo)
      else await crearVeterinario(cuerpo)
      setExito('Veterinario guardado')
      setForm(null)
      cargar()
    } catch (err) {
      setError(err.message)
    }
  }

  async function eliminar(v) {
    if (!window.confirm(`¿Eliminar la ficha de ${v.nombre}?`)) return
    setError('')
    try {
      await eliminarVeterinario(v.idVeterinario)
      setExito('Veterinario eliminado')
      cargar()
    } catch (err) {
      setError(err.message)
    }
  }

  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value })
  const correoUsuario = (idUser) => usuariosVet.find((u) => u.idUser === idUser)?.email ?? (idUser ? `Usuario #${idUser}` : '—')

  return (
    <div>
      <Encabezado titulo="Veterinarios" descripcion="Fichas del equipo médico. Vincula cada ficha con su usuario para que vea su agenda." ayuda={[["+ Nuevo veterinario", "Crea la ficha con RUT, nombre y especialidad."], ["Usuario del sistema (lista)", "Vincula la ficha con una cuenta de rol Veterinario; sin esto no puede ver Mi agenda."], ["Editar", "Corrige los datos o cambia el usuario vinculado."], ["Eliminar", "Borra la ficha; su usuario queda sin vincular."]]} />
      <div className="filtros"><span className="espacio" /><button onClick={nuevo} title="Abre el formulario para crear la ficha de un veterinario">+ Nuevo veterinario</button></div>

      {form && (
        <form className="formulario" onSubmit={guardar}>
          <h3>{idEditando ? 'Editar veterinario' : 'Nuevo veterinario'}</h3>
          <label>RUT <input value={form.rut} onChange={cambiar('rut')} placeholder="15678234-3" required /></label>
          <label>Nombre <input value={form.nombre} onChange={cambiar('nombre')} required /></label>
          <label>Especialidad <input value={form.especialidad} onChange={cambiar('especialidad')} /></label>
          <label>Teléfono <input value={form.telefono} onChange={cambiar('telefono')} placeholder="+56912345678" /></label>
          <label>Correo <input type="email" value={form.email} onChange={cambiar('email')} /></label>
          <label>Usuario del sistema
            <select value={form.idUser} onChange={cambiar('idUser')}>
              <option value="">Sin usuario vinculado</option>
              {usuariosVet.map((u) => <option key={u.idUser} value={u.idUser}>{u.nombre} ({u.email})</option>)}
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
      {cargando ? <Cargando /> : (
        <table>
          <thead><tr><th>RUT</th><th>Nombre</th><th>Especialidad</th><th>Usuario</th><th></th></tr></thead>
          <tbody>
            {lista.map((v) => (
              <tr key={v.idVeterinario}>
                <td>{v.rut}</td><td>{v.nombre}</td><td>{v.especialidad ?? '—'}</td><td>{correoUsuario(v.idUser)}</td>
                <td className="acciones">
                  <button onClick={() => editar(v)} title="Modifica la ficha o su usuario vinculado">Editar</button>
                  <button className="peligro" onClick={() => eliminar(v)} title="Elimina la ficha del veterinario">Eliminar</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
