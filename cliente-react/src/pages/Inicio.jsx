import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'

// Pantalla de bienvenida con accesos segun el rol
export default function Inicio() {
  const { usuario, tieneRol } = useAuth()

  return (
    <div>
      <h2>Hola, {usuario.nombre}</h2>
      <p className="ayuda">Elige qué quieres hacer. Cada tarjeta explica lo que hace; también tienes todo en el menú de la izquierda.</p>
      <div className="tarjetas">
        {tieneRol('Administrador', 'Recepcionista') && (
          <Link className="tarjeta" to="/citas/nueva"><strong>Agendar cita</strong><span>Reserva una hora para una mascota. Avisa si es feriado o si el veterinario ya está ocupado.</span></Link>
        )}
        {tieneRol('Administrador', 'Recepcionista') && (
          <Link className="tarjeta" to="/citas"><strong>Citas</strong><span>Revisa la agenda y confirma, reprograma o cancela horas.</span></Link>
        )}
        {tieneRol('Veterinario') && (
          <Link className="tarjeta" to="/mi-agenda"><strong>Mi agenda del día</strong><span>Tus pacientes de hoy. Registra el diagnóstico de cada cita confirmada.</span></Link>
        )}
        <Link className="tarjeta" to="/mascotas"><strong>Mascotas</strong><span>Busca, registra o edita mascotas y revisa su historial.</span></Link>
        <Link className="tarjeta" to="/tutores"><strong>Tutores</strong><span>Datos de contacto de los dueños de las mascotas.</span></Link>
        <Link className="tarjeta" to="/vacunas-por-vencer"><strong>Vacunas por vencer</strong><span>Mascotas que deben vacunarse en los próximos 30 días.</span></Link>
      </div>
      {tieneRol('Veterinario') && !usuario.idVeterinario && (
        <p className="mensaje error">Tu usuario no está vinculado a una ficha de veterinario. Pide al Administrador que la vincule.</p>
      )}
    </div>
  )
}
