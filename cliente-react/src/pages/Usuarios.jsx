import { useEffect, useState } from 'react'
import { listarUsuarios, crearUsuario, eliminarUsuario, listarRoles } from '../api/clinica.js'
import { useAuth } from '../context/AuthContext.jsx'
import { Encabezado, Cargando, Mensaje, Paginacion } from '../components/Comunes.jsx'

const VACIO = { nombre: '', email: '', password: '', idRol: '' }

// Usuarios del sistema (Administrador, RF-02, HU-06)
export default function Usuarios() {
  const { usuario } = useAuth()
  const [pagina, setPagina] = useState(1)
  const [datos, setDatos] = useState(null)
  const [roles, setRoles] = useState([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [exito, setExito] = useState('')
  const [form, setForm] = useState(null)

  async function cargar() {
    setCargando(true)
    setError('')
    try {
      setDatos(await listarUsuarios(pagina))
    } catch (err) {
      setError(err.message)
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => { cargar() }, [pagina])
  useEffect(() => { listarRoles().then(setRoles).catch(() => {}) }, [])

  async function guardar(e) {
    e.preventDefault()
    setError('')
    try {
      await crearUsuario({ ...form, idRol: Number(form.idRol) })
      setExito('Usuario creado')
      setForm(null)
      cargar()
    } catch (err) {
      setError(err.message)
    }
  }

  async function eliminar(u) {
    if (!window.confirm(`¿Eliminar al usuario ${u.email}?`)) return
    setError('')
    try {
      await eliminarUsuario(u.idUser)
      setExito('Usuario eliminado')
      cargar()
    } catch (err) {
      setError(err.message)
    }
  }

  const cambiar = (campo) => (e) => setForm({ ...form, [campo]: e.target.value })

  return (
    <div>
      <Encabezado titulo="Usuarios" descripcion="Cuentas que pueden ingresar al sistema y el rol de cada una." ayuda={[["+ Nuevo usuario", "Crea una cuenta con nombre, correo, contraseña y rol."], ["Eliminar", "Borra la cuenta. No puedes eliminar tu propia cuenta."]]} />
      <div className="filtros"><span className="espacio" /><button onClick={() => { setForm(VACIO); setExito('') }} title="Abre el formulario para crear una cuenta">+ Nuevo usuario</button></div>

      {form && (
        <form className="formulario" onSubmit={guardar}>
          <h3>Nuevo usuario</h3>
          <label>Nombre <input value={form.nombre} onChange={cambiar('nombre')} required /></label>
          <label>Correo <input type="email" value={form.email} onChange={cambiar('email')} required /></label>
          <label>Contraseña (mínimo 6) <input type="password" value={form.password} onChange={cambiar('password')} required minLength={6} /></label>
          <label>Rol
            <select value={form.idRol} onChange={cambiar('idRol')} required>
              <option value="">Seleccione...</option>
              {roles.map((r) => <option key={r.idRol} value={r.idRol}>{r.nombre}</option>)}
            </select>
          </label>
          <p className="ayuda">Si el rol es Veterinario, luego vincula su ficha en la pantalla Veterinarios.</p>
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
          <table>
            <thead><tr><th>Nombre</th><th>Correo</th><th>Rol</th><th></th></tr></thead>
            <tbody>
              {datos.items.map((u) => (
                <tr key={u.idUser}>
                  <td>{u.nombre}</td><td>{u.email}</td><td>{u.rol}</td>
                  <td className="acciones">
                    {u.idUser !== usuario.idUser && <button className="peligro" onClick={() => eliminar(u)} title="Elimina la cuenta; ya no podrá ingresar">Eliminar</button>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <Paginacion pagina={datos.pagina} tamano={datos.tamano} total={datos.total} onCambiar={setPagina} />
        </>
      )}
    </div>
  )
}
