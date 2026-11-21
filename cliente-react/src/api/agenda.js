import { llamar, consulta } from './http.js'

// ===== API Agenda y Atencion (puerto 5002) =====

// citas
export const listarCitas = (filtros) => llamar('agenda', '/citas' + consulta({ tamano: 20, ...filtros }))
export const miAgenda = (fecha) => llamar('agenda', '/citas/mi-agenda' + consulta({ fecha }))
export const obtenerCita = (id) => llamar('agenda', `/citas/${id}`)
export const crearCita = (datos) => llamar('agenda', '/citas', { metodo: 'POST', cuerpo: datos })
export const editarCita = (id, datos) => llamar('agenda', `/citas/${id}`, { metodo: 'PUT', cuerpo: datos })
export const cambiarEstadoCita = (id, estado) => llamar('agenda', `/citas/${id}/estado`, { metodo: 'PATCH', cuerpo: { estado } })
export const eliminarCita = (id) => llamar('agenda', `/citas/${id}`, { metodo: 'DELETE' })

// atenciones
export const registrarAtencion = (datos) => llamar('agenda', '/atenciones', { metodo: 'POST', cuerpo: datos })
export const atencionesDeMascota = (idMascota) => llamar('agenda', '/atenciones' + consulta({ idMascota }))

// vacunas
export const listarVacunas = (idEspecie) => llamar('agenda', '/vacunas' + consulta({ idEspecie }))
export const vacunacionesDeMascota = (idMascota) => llamar('agenda', '/vacunaciones' + consulta({ idMascota }))
export const registrarVacunacion = (datos) => llamar('agenda', '/vacunaciones', { metodo: 'POST', cuerpo: datos })
export const editarVacunacion = (id, datos) => llamar('agenda', `/vacunaciones/${id}`, { metodo: 'PUT', cuerpo: datos })
export const vacunasPorVencer = (dias) => llamar('agenda', '/vacunaciones/por-vencer' + consulta({ dias }))
