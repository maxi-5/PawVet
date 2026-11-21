# ADR-001 — Una base de datos por API, sin claves foráneas entre bases

- **Estado:** Aceptada (Hito 2, Sprint 1)
- **Decisores:** Maximiliano Cuevas (PO), Vicente Bravo (SM)

## Contexto
La pauta exige al menos dos APIs independientes. En el Trabajo 2 todas las tablas (incluidas Cita, Atención, Vacuna y Vacunación) estaban en una sola base. Si las dos APIs compartieran la base, cualquier cambio de esquema de una rompería a la otra y no serían realmente independientes.

## Decisión
Cada API es dueña de sus datos:
- **API Clínica e Identidad** → `PawVetClinicaDB` (usuarios, roles, tutores, mascotas, veterinarios, especies, razas).
- **API Agenda y Atención** → `PawVetAgendaDB` (citas, atenciones, vacunas, vacunaciones).

Las columnas `id_mascota` e `id_veterinario` de la base de agenda son **referencias lógicas**: no hay clave foránea entre bases y su existencia se verifica por HTTP antes de escribir (ADR-003). En Docker, un solo contenedor de SQL Server aloja las dos bases, cada una con su propia cadena de conexión.

## Alternativas consideradas
1. **Base compartida:** más simple, pero acopla las APIs y contradice el objetivo de servicios independientes.
2. **Copiar mascotas y veterinarios a la base de agenda:** evita la llamada HTTP, pero obliga a sincronizar datos y genera inconsistencias.

## Consecuencias
- (+) Las APIs se despliegan, migran y prueban por separado (cada una tiene sus migraciones de EF Core).
- (−) No hay integridad referencial entre bases: si se elimina una mascota en la API Clínica, sus citas quedan con un id huérfano. Se acepta para este alcance y se documenta como limitación.
- (−) La API Agenda depende de la disponibilidad de la API Clínica para agendar (mitigado con el 503 de ADR-003).
