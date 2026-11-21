# Registro de cambios de los contratos OpenAPI

Los contratos se diseñaron primero (Hito 2) y el código se construyó a partir de ellos (Hito 3). Todo cambio posterior al contrato se registra aquí, en el mismo pull request que modifica el código. Versionado: `MAYOR.MENOR.PARCHE` del documento; la versión de la URL (`/api/v1`) solo cambia si se rompe la compatibilidad.

## [1.1.0] — Sprint 3

### Agregado (sin romper compatibilidad)
- `PUT /vacunaciones/{id}` (API Agenda, solo Veterinario): corrige la fecha de aplicación o el lote de una vacunación y **recalcula la próxima dosis** con el intervalo de la vacuna. Responde 200 con la vacunación, 400 si la fecha es futura y 404 si no existe. Nuevo esquema `VacunacionEditar`.
- Motivo: en la revisión se detectó que una vacuna registrada con fecha equivocada dejaba mal calculada la próxima dosis y, por lo tanto, la lista de vacunas por vencer (RF-13). El cliente permite además registrar vacunas con una fecha anterior (vacunas puestas antes o en otra clínica).

## [1.0.1] — Sprint 2 (implementación)

Cambios detectados al comparar el Swagger generado con los `.yaml` del Hito 2.

### Cambiado (sin romper compatibilidad)
- **Nombre del sistema y de las bases.** El nombre de trabajo "VetClinica" del Hito 1 y 2 se reemplaza por **PawVet**, el nombre del proyecto del equipo. Las bases pasan de `VetClinicaDB` y `AgendaDB` a `PawVetClinicaDB` y `PawVetAgendaDB`. No cambia ninguna ruta ni esquema.
- **Emisor del token.** Se fija `Issuer = PawVet.ApiClinica` y `Audience = PawVet` (en el Hito 2 no estaban definidos). Las dos APIs los leen de `appsettings.json`.

### Agregado
- `DELETE /usuarios/{id}` (API Clínica): responde **409** si el Administrador intenta eliminar su propia cuenta. Motivo: evitar que el sistema quede sin administrador. Se agregó el código 409 a la operación.
- `GET /tutores` y `GET /mascotas`: parámetro opcional `q` para buscar por nombre/RUT o nombre/microchip, y `idTutor` en mascotas. Necesarios para el buscador del cliente.
- `GET /vacunaciones/por-vencer`: el parámetro `dias` se limita a 1–365 (fuera de rango se usa 30).

### Aclarado
- `PATCH /citas/{id}/estado` solo acepta `Confirmada`, `Cancelada` y `NoAsistio` (400 para otro valor). `Atendida` se alcanza únicamente con `POST /atenciones`, como indica el diagrama de estados.
- `GET /mascotas/{id}` y `GET /veterinarios/{id}` son los que usa la API Agenda para verificar la existencia (reenviando el token del usuario, ADR-002).

## [1.0.0] — Sprint 1 (Hito 2)

- Versión inicial de `api-clinica.v1.yaml` (29 operaciones) y `api-agenda.v1.yaml` (14 operaciones).
- Seguridad `bearerAuth`, roles en `x-roles`, errores `ProblemDetails` (RFC 9457).
- Cambios respecto de la API del Trabajo 2: login con cuerpo JSON, DTOs de entrada y salida, ProblemDetails, `GET /veterinarios` para todos los roles, y tablas de agenda movidas a la base de la API Agenda.
