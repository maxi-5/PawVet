# Matriz de trazabilidad

Requerimiento → historia → caso de uso → diseño → endpoint → pantalla → prueba automatizada.
**C** = API Clínica (5001, `api-clinica.Tests`), **A** = API Agenda (5002, `api-agenda.Tests`). Las rutas omiten el prefijo `/api/v1`.

| Req. | HU | CU | Diseño | Endpoint | Pantalla | Pruebas |
| --- | --- | --- | --- | --- | --- | --- |
| RF-01 | HU-01 | CU-01 | Secuencia 1, ADR-002 | C `POST /auth/login` | `Login.jsx` | `Login_ConCredencialesCorrectas_DevuelveTokenConRol`, `Login_ConPasswordIncorrecta_Devuelve401`, `Login_ConEmailSinFormato_Devuelve400`, `Login_DeVeterinarioVinculado_IncluyeIdVeterinarioEnElToken` |
| RF-02 | HU-06 | CU-02 | Clases (Usuario, Rol) | C `GET/POST /usuarios`, `GET/DELETE /usuarios/{id}`, `GET /roles` | `Usuarios.jsx` | `GetUsuarios_ComoRecepcionista_Devuelve403`, `PostUsuario_ConEmailRepetido_Devuelve409`, `PostUsuario_ConRolInexistente_Devuelve400`, `DeleteUsuario_ElPropioAdministrador_Devuelve409`, `DeleteUsuario_Existente_Devuelve204YLuego404` |
| RF-03 | HU-02 | CU-04 | ER PawVetClinicaDB (UQ_Tutor_Rut) | C `/tutores`, `/tutores/{id}` | `Tutores.jsx` | `PostTutor_ComoRecepcionista_Devuelve201`, `PostTutor_ComoVeterinario_Devuelve403`, `PostTutor_ConRutInvalido_Devuelve400`, `PostTutor_ConRutRepetido_Devuelve409`, `PutTutor_CambiaLosDatos_Devuelve204`, `DeleteTutor_ComoRecepcionista_Devuelve403`, `DeleteTutor_ConMascotas_Devuelve409` |
| RF-04 | HU-02 | CU-05 | Clases (`Mascota.CalcularEdad`) | C `/mascotas`, `/mascotas/{id}` | `Mascotas.jsx` | `PostMascota_Valida_Devuelve201ConEdadYNombres`, `PostMascota_ConTutorInexistente_Devuelve400`, `PostMascota_ConFechaDeNacimientoFutura_Devuelve400` |
| RF-05 | HU-02 | CU-05 | Clases (`Raza.PerteneceA`) | C `POST/PUT /mascotas` → 400 | `Mascotas.jsx` (razas filtradas por especie) | `PostMascota_ConRazaDeOtraEspecie_Devuelve400`, `GetRazas_PorEspecie_SoloTraeEsaEspecie` |
| RF-06 | HU-02 | CU-05 | ER (UQ_Mascota_Microchip filtrado) | C `POST/PUT /mascotas` → 409 | `Mascotas.jsx` | `PostMascota_ConMicrochipRepetido_Devuelve409`, `PostMascota_ConMicrochipDe10Digitos_Devuelve400` |
| CU-03 | — | CU-03 | Clases (Veterinario, Especie, Raza) | C `/veterinarios`, `/especies`, `/razas` | `Veterinarios.jsx`, `Catalogos.jsx` | `CatalogosTests` (8): 200 recepcionista, 403, 409 RUT, 400 usuario no veterinario, razas por especie, 400 especie inexistente, 409 especie con razas, 409 especie repetida |
| RF-07 | HU-03 | CU-06, CU-07 | Secuencia 2, Estados | A `POST/GET /citas`, `GET/PUT/DELETE /citas/{id}`, `PATCH /citas/{id}/estado` | `CitaForm.jsx`, `Citas.jsx` | `PostCita_Valida_Devuelve201EnEstadoAgendada`, `PostCita_ComoVeterinario_Devuelve403`, `PostCita_ConFechaPasada_Devuelve400`, `PatchEstado_Confirmar_Devuelve200`, `PatchEstado_ConfirmarUnaCitaCancelada_Devuelve409`, `PatchEstado_NoAsistioAntesDeLaHora_Devuelve409`, `PatchEstado_ConEstadoNoPermitido_Devuelve400`, `PutCita_Reprogramar_Devuelve204`, `PutCita_Cancelada_Devuelve409`, `DeleteCita_ComoRecepcionista_Devuelve403`, `DeleteCita_ComoAdministrador_Devuelve204`; unitarias `Confirmar_*`, `Cancelar_*`, `MarcarNoAsistio_*` |
| RF-08 | HU-03 | CU-06 | Clases (`Cita.SeTopaCon`), índice IX_Cita_Veterinario_Fecha | A `POST/PUT /citas` → 409 | `CitaForm.jsx` | `PostCita_QueSeTopaConOtraDelMismoVeterinario_Devuelve409`, `PostCita_MismoHorarioOtroVeterinario_Devuelve201`, `PostCita_EnHorarioDeUnaCitaCancelada_Devuelve201`, `SeTopaCon_DetectaSolapamientoDeHorarios` (5 casos), `SeTopaCon_CitaCancelada_NoBloqueaElHorario` |
| RF-09 | HU-04 | CU-08 | Contenedores (claim `id_veterinario`) | A `GET /citas/mi-agenda` | `MiAgenda.jsx` | `GetMiAgenda_SoloDevuelveLasCitasDelVeterinarioLogueado`, `GetMiAgenda_VeterinarioSinFicha_Devuelve403` |
| RF-10 | HU-05 | CU-09 | Estados (`Cita.MarcarAtendida`) | A `POST/GET /atenciones`, `GET /atenciones/{id}` | `Atencion.jsx`, `Historial.jsx` | `PostAtencion_SobreCitaConfirmada_Devuelve201YLaCitaQuedaAtendida`, `PostAtencion_SobreCitaAgendada_Devuelve409`, `PostAtencion_DeCitaDeOtroVeterinario_Devuelve403`, `PostAtencion_DosVecesLaMismaCita_Devuelve409`, `PostAtencion_ComoRecepcionista_Devuelve403`, `PostAtencion_ConTemperaturaFueraDeRango_Devuelve400`, `GetAtenciones_PorMascota_DevuelveElHistorial`, `MarcarAtendida_CitaAgendada_LanzaTransicionInvalida` |
| RF-11 | — | CU-10 | Clases (`Vacunacion.CalcularProximaDosis`) | A `POST/GET /vacunaciones`, `PUT /vacunaciones/{id}`, `GET /vacunas` | `Atencion.jsx`, `Historial.jsx` (Registrar vacuna, Corregir) | `PostVacunacion_Valida_CalculaLaProximaDosis`, `PutVacunacion_CambiaLaFecha_RecalculaLaProximaDosis`, `PutVacunacion_ComoRecepcionista_Devuelve403`, `PutVacunacion_Inexistente_Devuelve404`, `PostVacunacion_VacunaDeOtraEspecie_Devuelve400`, `PostVacunacion_ConFechaFutura_Devuelve400`, `PostVacunacion_ConApiClinicaCaida_Devuelve503`, `GetVacunaciones_SinIdMascota_Devuelve400`, `CalcularProximaDosis_SumaElIntervaloDeLaVacuna` |
| RF-12 | HU-03 | CU-06 «include» | Secuencia 2, Contexto, ADR-003 | A `POST /citas` → 400 (Nager.Date) | `CitaForm.jsx` | `PostCita_EnDiaFeriado_Devuelve400` (con `FakeFeriadosClient`); Postman "Agendar cita en feriado" (API real) |
| RF-13 | — | — | Clases (`Vacunacion.EstaPorVencer`) | A `GET /vacunaciones/por-vencer` | `PorVencer.jsx` | `GetPorVencer_IncluyeVacunaQueVenceEnLosProximosDias`, `EstaPorVencer_DentroDelPlazo_DevuelveTrue` |
| RF-14 | — | — | Fuera de esta versión (Won't) | — | — | — |

## Requerimientos no funcionales

| Req. | Diseño / implementación | Evidencia |
| --- | --- | --- |
| RNF-01 | `[Authorize(Roles = ...)]` en cada controlador; Secuencia 3 | `GetUsuarios_SinToken_Devuelve401`, `PostCita_SinToken_Devuelve401`, todas las pruebas `*_Devuelve403`; Postman "Token alterado" |
| RNF-02 | BCrypt en `Usuario.VerificarPassword` y `UsuariosController`; DTOs sin contraseña | `Login_*`; contrato `Usuario` sin password |
| RNF-03 | `Jwt__Key` y `ConnectionStrings__*` desde variables de entorno; `.env.example`, `.gitignore` | Revisión de `appsettings.json` (valores vacíos) |
| RNF-04 | Paginación (máx. 100), índices únicos y IX_Cita_Veterinario_Fecha | Medición manual: Por completar |
| RNF-05 | `ClinicaClient` con timeout 5 s → 503 (ADR-003) | `PostCita_ConApiClinicaCaida_Devuelve503`, `PostVacunacion_ConApiClinicaCaida_Devuelve503` |
| RNF-06 | `Cargando`, `Mensaje`, `http.js` (mensajes en español); `Encabezado` con "¿Qué hace cada botón?", descripciones en el menú y `title` en cada botón | Revisión manual del cliente; `docs/capturas/cliente-citas.png` |
| RNF-07 | Evento `sesion-expirada` en `http.js` → `AuthContext` → `/login` | Secuencia 3; revisión manual (token vencido) |
| RNF-08 | Asp.Versioning (URL), Swagger, contratos OpenAPI | `GetCita_Inexistente_Devuelve404ConProblemDetails`; Swagger en `/swagger` |
