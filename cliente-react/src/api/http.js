// Funcion unica para llamar a las dos APIs.
// - agrega el token JWT en el encabezado Authorization
// - convierte los errores (ProblemDetails) en mensajes en español
// - si una API responde 401 (token vencido o invalido) avisa para cerrar la sesion (Secuencia 3)

const URLS = {
  clinica: import.meta.env.VITE_API_CLINICA || 'http://localhost:5001/api/v1',
  agenda: import.meta.env.VITE_API_AGENDA || 'http://localhost:5002/api/v1'
}

const RUTA_LOGIN = '/auth/login'

export async function llamar(api, ruta, opciones = {}) {
  const { metodo = 'GET', cuerpo } = opciones
  const token = localStorage.getItem('token')

  const headers = {}
  if (cuerpo !== undefined) headers['Content-Type'] = 'application/json'
  if (token) headers['Authorization'] = `Bearer ${token}`

  let respuesta
  try {
    respuesta = await fetch(URLS[api] + ruta, {
      method: metodo,
      headers,
      body: cuerpo !== undefined ? JSON.stringify(cuerpo) : undefined
    })
  } catch (e) {
    // no hubo respuesta: API apagada o sin conexion
    throw new Error('No se pudo conectar con el servidor. Revisa que las APIs estén en ejecución e inténtalo de nuevo.')
  }

  // sesion expirada: el token ya no sirve
  if (respuesta.status === 401 && ruta !== RUTA_LOGIN) {
    window.dispatchEvent(new Event('sesion-expirada'))
    throw new Error('Tu sesión expiró. Inicia sesión nuevamente.')
  }

  // 204: la operacion resulto pero no trae contenido (PUT y DELETE)
  if (respuesta.status === 204) return null

  let datos = null
  try {
    datos = await respuesta.json()
  } catch (e) {
    datos = null
  }

  if (!respuesta.ok) {
    throw new Error(mensajeDeError(respuesta.status, datos))
  }
  return datos
}

// Traduce la respuesta de error de la API a un mensaje para el usuario
function mensajeDeError(codigo, datos) {
  // errores de validacion (400): { errors: { campo: ["mensaje"] } }
  if (datos && datos.errors) {
    const mensajes = Object.values(datos.errors).flat()
    if (mensajes.length > 0) return mensajes.join(' ')
  }
  // 403 sin detalle: la sesion sigue activa, solo falta permiso
  if (codigo === 403 && !(datos && datos.detail)) return 'No tienes permiso para realizar esta acción.'
  // errores del negocio: { detail: "mensaje" }
  if (datos && datos.detail) return datos.detail

  if (codigo === 400) return 'Los datos enviados no son válidos.'
  if (codigo === 401) return 'Correo o contraseña incorrectos.'
  if (codigo === 404) return 'No se encontró el registro solicitado.'
  if (codigo === 409) return 'La operación no se puede realizar por el estado actual del registro.'
  if (codigo === 503) return 'El servicio no está disponible. Intenta más tarde.'
  return 'Ocurrió un error inesperado. Intenta más tarde.'
}

// arma "?a=1&b=2" ignorando los valores vacios
export function consulta(parametros) {
  const p = new URLSearchParams()
  Object.entries(parametros).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== '') p.append(k, v)
  })
  const texto = p.toString()
  return texto ? '?' + texto : ''
}
