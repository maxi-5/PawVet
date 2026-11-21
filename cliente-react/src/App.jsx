import { Routes, Route } from 'react-router-dom'
import Layout from './components/Layout.jsx'
import RutaProtegida from './components/RutaProtegida.jsx'
import Login from './pages/Login.jsx'
import Inicio from './pages/Inicio.jsx'
import Tutores from './pages/Tutores.jsx'
import Mascotas from './pages/Mascotas.jsx'
import Historial from './pages/Historial.jsx'
import Citas from './pages/Citas.jsx'
import CitaForm from './pages/CitaForm.jsx'
import MiAgenda from './pages/MiAgenda.jsx'
import Atencion from './pages/Atencion.jsx'
import PorVencer from './pages/PorVencer.jsx'
import Veterinarios from './pages/Veterinarios.jsx'
import Catalogos from './pages/Catalogos.jsx'
import Usuarios from './pages/Usuarios.jsx'

const RECEPCION = ['Administrador', 'Recepcionista']
const CLINICOS = ['Administrador', 'Veterinario']
const ADMIN = ['Administrador']
const VETERINARIO = ['Veterinario']

// Rutas de la aplicacion. Cada ruta indica que roles pueden entrar (misma matriz que las APIs).
export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />

      <Route element={<RutaProtegida><Layout /></RutaProtegida>}>
        <Route path="/" element={<Inicio />} />
        <Route path="/tutores" element={<Tutores />} />
        <Route path="/mascotas" element={<Mascotas />} />
        <Route path="/vacunas-por-vencer" element={<PorVencer />} />

        <Route path="/citas" element={<RutaProtegida roles={RECEPCION}><Citas /></RutaProtegida>} />
        <Route path="/citas/nueva" element={<RutaProtegida roles={RECEPCION}><CitaForm /></RutaProtegida>} />
        <Route path="/citas/:id/editar" element={<RutaProtegida roles={RECEPCION}><CitaForm /></RutaProtegida>} />

        <Route path="/mi-agenda" element={<RutaProtegida roles={VETERINARIO}><MiAgenda /></RutaProtegida>} />
        <Route path="/citas/:id/atencion" element={<RutaProtegida roles={VETERINARIO}><Atencion /></RutaProtegida>} />
        <Route path="/mascotas/:id/historial" element={<RutaProtegida roles={CLINICOS}><Historial /></RutaProtegida>} />

        <Route path="/veterinarios" element={<RutaProtegida roles={ADMIN}><Veterinarios /></RutaProtegida>} />
        <Route path="/catalogos" element={<RutaProtegida roles={ADMIN}><Catalogos /></RutaProtegida>} />
        <Route path="/usuarios" element={<RutaProtegida roles={ADMIN}><Usuarios /></RutaProtegida>} />

        <Route path="/sin-permiso" element={<h2>No tienes permiso para ver esta página</h2>} />
        <Route path="*" element={<h2>Página no encontrada</h2>} />
      </Route>
    </Routes>
  )
}
