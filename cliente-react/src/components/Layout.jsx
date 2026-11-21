import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'

// Opciones del menú lateral. Cada una muestra debajo qué hace
// y solo aparece para los roles que pueden usarla (misma matriz que las APIs).
const MENU = [
  { grupo: 'Día a día' },
  { to: '/', texto: 'Inicio', desc: 'Accesos rápidos de tu rol', end: true },
  { to: '/citas', texto: 'Citas', desc: 'Agendar, confirmar o cancelar horas', roles: ['Administrador', 'Recepcionista'] },
  { to: '/mi-agenda', texto: 'Mi agenda', desc: 'Tus citas del día para atender', roles: ['Veterinario'] },
  { to: '/vacunas-por-vencer', texto: 'Vacunas por vencer', desc: 'Dosis que vencen pronto' },
  { grupo: 'Registros' },
  { to: '/mascotas', texto: 'Mascotas', desc: 'Fichas de los pacientes' },
  { to: '/tutores', texto: 'Tutores', desc: 'Dueños y sus datos de contacto' },
  { grupo: 'Administración', roles: ['Administrador'] },
  { to: '/veterinarios', texto: 'Veterinarios', desc: 'Fichas del equipo médico', roles: ['Administrador'] },
  { to: '/catalogos', texto: 'Especies y razas', desc: 'Listas para clasificar mascotas', roles: ['Administrador'] },
  { to: '/usuarios', texto: 'Usuarios', desc: 'Cuentas y roles de acceso', roles: ['Administrador'] }
]

// Estructura común: menú lateral a la izquierda y la página a la derecha
export default function Layout() {
  const { usuario, logout, tieneRol } = useAuth()
  const visibles = MENU.filter((m) => !m.roles || tieneRol(...m.roles))

  return (
    <div className="app">
      <aside className="lateral">
        <div className="marca">PawVet</div>
        <div className="marca-sub">Clínica veterinaria</div>
        <nav className="menu">
          {visibles.map((m) =>
            m.grupo
              ? <div key={m.grupo} className="menu-titulo">{m.grupo}</div>
              : (
                <NavLink key={m.to} to={m.to} end={m.end} title={m.desc}>
                  {m.texto}
                  <small>{m.desc}</small>
                </NavLink>
              )
          )}
        </nav>
        <div className="sesion">
          <div><strong>{usuario.nombre}</strong></div>
          <span className="rol">{usuario.rol}</span>
          <button className="secundario" onClick={logout} title="Cierra tu sesión y vuelve a la pantalla de ingreso">
            Cerrar sesión
          </button>
        </div>
      </aside>
      <main className="contenido">
        <Outlet />
      </main>
    </div>
  )
}
