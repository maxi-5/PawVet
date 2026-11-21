# Especificación de requerimientos (ERS) — PawVet

Versión 2 (Sprint 2). La versión 1 está en `docs/hitos/Hito1-Caso-y-requerimientos.docx` y fue validada por el docente sin observaciones. Cambios de esta versión: nombre del sistema PawVet (antes "VetClinica") y criterios de aceptación con el código HTTP esperado.

## 1. Caso de negocio
Sistema web para una clínica veterinaria pequeña (Mulchén) que centraliza tutores, mascotas, agenda de citas y ficha clínica, con acceso por rol. Resuelve horas duplicadas, pérdida del historial y olvido de vacunas; registra el microchip exigido por la Ley 21.020 sin duplicados.

**Fuera del alcance:** pagos, facturación, inventario y notificaciones.

## 2. Partes interesadas y roles
| Rol | Permisos |
| --- | --- |
| Administrador | Gestiona usuarios, veterinarios y catálogos; puede eliminar registros |
| Recepcionista | Registra tutores y mascotas; agenda, confirma y cancela citas |
| Veterinario | Consulta su agenda; registra atenciones y vacunaciones |
| Tutor (indirecto) | No usa el sistema; lo atiende la recepción |

## 3. Requerimientos funcionales (MoSCoW)
| ID | Requerimiento | Prioridad |
| --- | --- | --- |
| RF-01 | Iniciar sesión con email y contraseña y recibir un JWT válido por 60 minutos | Must |
| RF-02 | El administrador crea, lista y elimina usuarios asignándoles un rol | Must |
| RF-03 | Recepcionista y administrador crean, ven, editan y eliminan tutores | Must |
| RF-04 | Recepcionista y administrador crean, ven, editan y eliminan mascotas asociadas a tutor, especie y raza | Must |
| RF-05 | Rechazar una mascota cuya raza no pertenezca a la especie | Must |
| RF-06 | Rechazar un microchip repetido (15 dígitos, opcional) | Must |
| RF-07 | La recepcionista agenda, edita, confirma y cancela citas | Must |
| RF-08 | Impedir dos citas del mismo veterinario en el mismo horario | Must |
| RF-09 | El veterinario ve su agenda del día | Should |
| RF-10 | El veterinario registra la atención de una cita (diagnóstico, tratamiento, peso, temperatura) | Should |
| RF-11 | El veterinario registra vacunaciones con la fecha de la próxima dosis | Should |
| RF-12 | Impedir agendar en feriados nacionales | Could |
| RF-13 | Listar las vacunas por vencer en los próximos 30 días | Could |
| RF-14 | Recordatorios automáticos por correo al tutor | Won't (esta versión) |

## 4. Requerimientos no funcionales
| ID | Tipo | Requerimiento verificable | Cómo se verifica |
| --- | --- | --- | --- |
| RNF-01 | Seguridad | Todo excepto el login exige JWT; sin token 401, rol sin permiso 403 | Pruebas `*_SinToken_Devuelve401`, `*_Como*_Devuelve403` |
| RNF-02 | Seguridad | Contraseñas con BCrypt; ninguna respuesta incluye contraseñas | `UsuarioDto` sin password; `Usuario.VerificarPassword` |
| RNF-03 | Seguridad | Llave JWT y cadenas de conexión en variables de entorno, no en el repositorio | `appsettings.json` con valores vacíos; `.env` en `.gitignore` |
| RNF-04 | Rendimiento | Listados < 1 s con 1.000 registros | Paginación (máx. 100), índices; medición manual pendiente |
| RNF-05 | Disponibilidad | Si la API Clínica no responde en 5 s, la API Agenda responde 503 sin caerse | `PostCita_ConApiClinicaCaida_Devuelve503` |
| RNF-06 | Usabilidad | Indicador de carga, errores comprensibles en español y descripción de cada botón | Componentes `Cargando`, `Mensaje` y `Encabezado`, `http.js` |
| RNF-07 | Usabilidad | Al expirar la sesión, el cliente avisa y redirige al login | Evento `sesion-expirada` (Secuencia 3) |
| RNF-08 | Mantenibilidad | APIs versionadas por URL (`/api/v1`) y documentadas con OpenAPI | Asp.Versioning + Swagger; contratos en `docs/openapi` |

## 5. Historias de usuario
| HU | Historia | Criterios de aceptación |
| --- | --- | --- |
| HU-01 | Como usuario, quiero iniciar sesión para acceder a las funciones de mi rol (RF-01) | Credenciales correctas → token y menú del rol · contraseña incorrecta → "Credenciales incorrectas" (401) |
| HU-02 | Como recepcionista, quiero registrar una mascota asociada a su tutor (RF-04 a RF-06) | Datos válidos → 201 · raza de otra especie → 400 · microchip repetido → 409 |
| HU-03 | Como recepcionista, quiero agendar una cita (RF-07, RF-08, RF-12) | Horario libre → Agendada (201) · horario ocupado → 409 · feriado → 400 indicando el feriado |
| HU-04 | Como veterinario, quiero ver mis citas del día (RF-09) | Solo mis citas, ordenadas por hora |
| HU-05 | Como veterinario, quiero registrar el diagnóstico y tratamiento (RF-10) | Cita confirmada → Atendida y queda en el historial · recepcionista → 403 |
| HU-06 | Como administrador, quiero crear usuarios con su rol (RF-02) | Email nuevo → creado con contraseña encriptada · email repetido → 409 |
| HU-07 | Como usuario, quiero que el sistema me avise cuando la sesión expire (RNF-07) | Token vencido → "Tu sesión expiró" y redirección al login |

## 6. Casos de uso
CU-01 Iniciar sesión · CU-02 Gestionar usuarios · CU-03 Gestionar catálogos (veterinarios, especies, razas) · CU-04 Gestionar tutores · CU-05 Gestionar mascotas · CU-06 Agendar cita («include» verificar mascota y veterinario, verificar feriado) · CU-07 Confirmar/cancelar cita · CU-08 Ver agenda · CU-09 Registrar atención · CU-10 Registrar vacunación («extend» CU-09). Diagrama: `docs/diagramas/casos-de-uso.puml`.

## 7. Supuestos y restricciones
- Un solo local; acceso desde navegador. Una cita dura 30 minutos por defecto (15–120).
- APIs en ASP.NET Core con controladores, EF Core con SQL Server, cliente React, secretos en variables de entorno.

## 8. Validación con el cliente
El caso fue presentado en el Hito 1 y **validado por el docente sin observaciones**. Evidencia: Por completar (captura o correo de la validación en `docs/requerimientos/evidencia/`).
