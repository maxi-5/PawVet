import { useState } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'
import { Mensaje } from '../components/Comunes.jsx'

// Inicio de sesion (RF-01, HU-01, Secuencia 1)
export default function Login() {
  const { usuario, login, aviso } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)
  const navigate = useNavigate()

  if (usuario) return <Navigate to="/" replace />

  async function enviar(e) {
    e.preventDefault()
    setError('')
    setEnviando(true)
    try {
      await login(email.trim(), password)
      navigate('/')
    } catch (err) {
      setError(err.message)
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="login-fondo">
    <div className="login">
      <h1>PawVet</h1>
      <p>Clínica veterinaria · Inicia sesión para continuar</p>
      {/* aviso cuando el token vencio (Secuencia 3) */}
      <Mensaje texto={aviso} />
      <form onSubmit={enviar}>
        <label>Correo
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoFocus />
        </label>
        <label>Contraseña
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </label>
        <Mensaje texto={error} />
        <button type="submit" disabled={enviando} title="Verifica tus datos y entra al sistema">{enviando ? 'Ingresando...' : 'Ingresar'}</button>
      </form>
      <p className="pie">Ingresa con el correo y la contraseña que te entregó el administrador.</p>
    </div>
    </div>
  )
}
