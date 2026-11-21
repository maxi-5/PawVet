import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'

// Bloquea las rutas: sin sesion vuelve al login; con un rol no permitido muestra "sin permiso" (RF-16)
export default function RutaProtegida({ roles, children }) {
  const { usuario } = useAuth()

  if (!usuario) {
    return <Navigate to="/login" replace />
  }
  if (roles && !roles.includes(usuario.rol)) {
    return <Navigate to="/sin-permiso" replace />
  }
  return children
}
