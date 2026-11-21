import { llamar, consulta } from './http.js'

// ===== API Clinica e Identidad (puerto 5001) =====

export const iniciarSesion = (email, password) =>
  llamar('clinica', '/auth/login', { metodo: 'POST', cuerpo: { email, password } })

// usuarios y roles (Administrador)
export const listarUsuarios = (pagina) => llamar('clinica', '/usuarios' + consulta({ pagina, tamano: 10 }))
export const crearUsuario = (datos) => llamar('clinica', '/usuarios', { metodo: 'POST', cuerpo: datos })
export const eliminarUsuario = (id) => llamar('clinica', `/usuarios/${id}`, { metodo: 'DELETE' })
export const listarRoles = () => llamar('clinica', '/roles')

// tutores
export const listarTutores = (pagina, q, tamano = 10) => llamar('clinica', '/tutores' + consulta({ pagina, tamano, q }))
export const crearTutor = (datos) => llamar('clinica', '/tutores', { metodo: 'POST', cuerpo: datos })
export const editarTutor = (id, datos) => llamar('clinica', `/tutores/${id}`, { metodo: 'PUT', cuerpo: datos })
export const eliminarTutor = (id) => llamar('clinica', `/tutores/${id}`, { metodo: 'DELETE' })

// mascotas
export const listarMascotas = (pagina, q, tamano = 10) => llamar('clinica', '/mascotas' + consulta({ pagina, tamano, q }))
export const obtenerMascota = (id) => llamar('clinica', `/mascotas/${id}`)
export const crearMascota = (datos) => llamar('clinica', '/mascotas', { metodo: 'POST', cuerpo: datos })
export const editarMascota = (id, datos) => llamar('clinica', `/mascotas/${id}`, { metodo: 'PUT', cuerpo: datos })
export const eliminarMascota = (id) => llamar('clinica', `/mascotas/${id}`, { metodo: 'DELETE' })

// veterinarios
export const listarVeterinarios = () => llamar('clinica', '/veterinarios')
export const crearVeterinario = (datos) => llamar('clinica', '/veterinarios', { metodo: 'POST', cuerpo: datos })
export const editarVeterinario = (id, datos) => llamar('clinica', `/veterinarios/${id}`, { metodo: 'PUT', cuerpo: datos })
export const eliminarVeterinario = (id) => llamar('clinica', `/veterinarios/${id}`, { metodo: 'DELETE' })

// especies y razas
export const listarEspecies = () => llamar('clinica', '/especies')
export const crearEspecie = (nombre) => llamar('clinica', '/especies', { metodo: 'POST', cuerpo: { nombre } })
export const eliminarEspecie = (id) => llamar('clinica', `/especies/${id}`, { metodo: 'DELETE' })
export const listarRazas = (idEspecie) => llamar('clinica', '/razas' + consulta({ idEspecie }))
export const crearRaza = (nombre, idEspecie) => llamar('clinica', '/razas', { metodo: 'POST', cuerpo: { nombre, idEspecie } })
export const eliminarRaza = (id) => llamar('clinica', `/razas/${id}`, { metodo: 'DELETE' })
