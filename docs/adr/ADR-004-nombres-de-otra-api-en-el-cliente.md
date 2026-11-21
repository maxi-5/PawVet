# ADR-004 — El cliente resuelve los nombres de mascotas y veterinarios de las citas

- **Estado:** Aceptada (Sprint 2)
- **Decisores:** Maximiliano Cuevas (PO), Vicente Bravo (SM)

## Contexto
Por ADR-001, una cita guarda solo `idMascota` e `idVeterinario`. Las pantallas de citas, agenda e historial necesitan mostrar nombres.

## Decisión
El cliente React carga una vez por pantalla la lista de veterinarios y la primera página de mascotas (hasta 100) desde la API Clínica y cruza los id localmente (`useNombres`). La API Agenda no consulta nombres para los listados.

## Alternativas consideradas
1. **La API Agenda pide cada nombre a la API Clínica:** N llamadas HTTP por listado y dependencia en cada lectura.
2. **Guardar el nombre en la cita:** duplica datos que pueden cambiar.

## Consecuencias
- (+) Los listados de la API Agenda no dependen de la API Clínica.
- (−) Con más de 100 mascotas algunas se muestran como "Mascota #id". Mejora futura: endpoint `GET /mascotas?ids=1,2,3`.
