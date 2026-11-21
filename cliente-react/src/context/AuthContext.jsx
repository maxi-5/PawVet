import { createContext, useContext, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { iniciarSesion } from '../api/clinica.js'

// Guarda la sesion (token y datos del usuario) y la comparte con toda la aplicacion
const AuthContext = createContext(null)

// lee la sesion guardada; si el token ya vencio, la descarta
function leerSesionGuardada() {
  const token = localStorage.getItem('token')
  const expira = localStorage.getItem('expira')
  const usuario = localStorage.getItem('usuario')
  if (!token || !expira || !usuario) return null
  if (new Date(expira) <= new Date()) {
    borrarSesionGuardada()
    return null
  }
  return JSON.parse(usuario)
}

function borrarSesionGuardada() {
  localStorage.removeItem('token')
  localStorage.removeItem('expira')
  localStorage.removeItem('usuario')
}

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(leerSesionGuardada)
  const [aviso, setAviso] = useState('')
  const navigate = useNavigate()

  // Sesion expirada: http.js avisa con el evento "sesion-expirada" cuando una API responde 401
  useEffect(() => {
    function alExpirar() {
      borrarSesionGuardada()
      setUsuario(null)
      setAviso('Tu sesión expiró. Inicia sesión nuevamente.')
      navigate('/login')
    }
    window.addEventListener('sesion-expirada', alExpirar)
    return () => window.removeEventListener('sesion-expirada', alExpirar)
  }, [navigate])

  async function login(email, password) {
    const datos = await iniciarSesion(email, password)
    localStorage.setItem('token', datos.token)
    localStorage.setItem('expira', datos.expira)
    // idVeterinario: solo para veterinarios con ficha vinculada (claim id_veterinario)
    const sesion = { ...datos.usuario, idVeterinario: datos.idVeterinario }
    localStorage.setItem('usuario', JSON.stringify(sesion))
    setAviso('')
    setUsuario(sesion)
  }

  function logout() {
    borrarSesionGuardada()
    setUsuario(null)
    setAviso('')
    navigate('/login')
  }

  // ayuda para preguntar por el rol: tieneRol('Administrador', 'Recepcionista')
  function tieneRol(...roles) {
    return usuario !== null && roles.includes(usuario.rol)
  }

  return (
    <AuthContext.Provider value={{ usuario, aviso, login, logout, tieneRol }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}
