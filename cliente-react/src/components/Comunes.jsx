import { useEffect, useState } from 'react'
import { listarMascotas, listarVeterinarios } from '../api/clinica.js'

// Componentes pequeños que se repiten en todas las pantallas

// Encabezado de cada página: título, para qué sirve la pantalla y,
// plegada, la explicación de cada botón (ayuda = [['Botón', 'Qué hace'], ...])
export function Encabezado({ titulo, descripcion, ayuda }) {
  return (
    <div className="encabezado">
      <h2>{titulo}</h2>
      {descripcion && <p>{descripcion}</p>}
      {ayuda && ayuda.length > 0 && (
        <details className="ayuda-botones">
          <summary>¿Qué hace cada botón?</summary>
          <dl>
            {ayuda.map(([boton, texto]) => (
              <div key={boton} style={{ display: 'contents' }}>
                <dt>{boton}</dt>
                <dd>{texto}</dd>
              </div>
            ))}
          </dl>
        </details>
      )}
    </div>
  )
}

// Indicador de carga
export function Cargando({ texto = 'Cargando...' }) {
  return <p className="cargando" role="status">{texto}</p>
}

// Mensaje de error (rojo) o de exito (verde)
export function Mensaje({ tipo = 'error', texto }) {
  if (!texto) return null
  return <p className={tipo === 'error' ? 'mensaje error' : 'mensaje exito'} role="alert">{texto}</p>
}

// Botones Anterior / Siguiente de los listados paginados
export function Paginacion({ pagina, tamano, total, onCambiar }) {
  const paginas = Math.max(1, Math.ceil(total / tamano))
  if (total === 0) return null
  return (
    <div className="paginacion">
      <button className="secundario" onClick={() => onCambiar(pagina - 1)} disabled={pagina <= 1} title="Muestra la página anterior del listado">Anterior</button>
      <span>Página {pagina} de {paginas} ({total} registros)</span>
      <button className="secundario" onClick={() => onCambiar(pagina + 1)} disabled={pagina >= paginas} title="Muestra la página siguiente del listado">Siguiente</button>
    </div>
  )
}

// fecha y hora en formato chileno: 01-12-2026 10:30
export function formatoFechaHora(texto) {
  const fecha = new Date(texto)
  return fecha.toLocaleDateString('es-CL') + ' ' + fecha.toLocaleTimeString('es-CL', { hour: '2-digit', minute: '2-digit' })
}

export function formatoHora(texto) {
  return new Date(texto).toLocaleTimeString('es-CL', { hour: '2-digit', minute: '2-digit' })
}

// fechas sin hora (DateOnly) llegan como "2026-12-02"
export function formatoFecha(texto) {
  if (!texto) return '—'
  const [a, m, d] = texto.split('-')
  return `${d}-${m}-${a}`
}

export function hoyISO() {
  const h = new Date()
  return new Date(h.getTime() - h.getTimezoneOffset() * 60000).toISOString().slice(0, 10)
}

// Las citas guardan solo los id de mascota y veterinario (viven en otra API):
// este hook trae los nombres desde la API Clinica para mostrarlos.
// Limitacion conocida: trae como maximo 100 mascotas.
export function useNombres() {
  const [mascotas, setMascotas] = useState([])
  const [veterinarios, setVeterinarios] = useState([])

  useEffect(() => {
    listarMascotas(1, '', 100).then((p) => setMascotas(p.items)).catch(() => {})
    listarVeterinarios().then(setVeterinarios).catch(() => {})
  }, [])

  const nombreMascota = (id) => mascotas.find((m) => m.idMascota === id)?.nombre ?? `Mascota #${id}`
  const nombreVeterinario = (id) => veterinarios.find((v) => v.idVeterinario === id)?.nombre ?? `Veterinario #${id}`
  return { mascotas, veterinarios, nombreMascota, nombreVeterinario }
}

// etiqueta de color segun el estado de la cita
export function EstadoCita({ estado }) {
  const textos = { NoAsistio: 'No asistió' }
  return <span className={'estado estado-' + estado.toLowerCase()}>{textos[estado] ?? estado}</span>
}
