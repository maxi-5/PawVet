# Sprint 2 — Implementación

| | |
| --- | --- |
| Semana | Semana 3 |
| Hito asociado | Hito 3: Implementación |
| **Objetivo** | Tener las dos APIs funcionando con JWT compartido, integración entre servicios y el cliente React con los flujos de cada rol |
| Entregable | Código en `api-clinica/`, `api-agenda/`, `cliente-react/`, `docker-compose.yml` |

## Planificación

| Ítem | Responsable | Terminado |
| --- | --- | --- |
| B-05 Tutores y mascotas (RUT, raza de la especie, microchip único) | Vicente | Sí |
| B-04 Login y emisión del JWT con `id_veterinario` | Vicente | Sí |
| B-06 Validación del JWT en la API Agenda | Maximiliano | Sí |
| B-07 Citas con estados (`Cita.Confirmar`, `Cancelar`, …) y topes (`SeTopaCon`) | Maximiliano | Sí |
| B-08 `ClinicaClient` con IHttpClientFactory, timeout 5 s → 503 | Maximiliano | Sí |
| B-09 Cliente React: login, sesión expirada, mascotas, citas | Ambos | Sí |
| B-14 Usuarios, veterinarios y catálogos en el cliente | Vicente | Sí |
| B-16 docker-compose y `.env.example` | Maximiliano | Sí |
| B-18 Rediseño del cliente con ayuda de botones | Ambos | Sí |
| Adelantados del Sprint 3: B-10 agenda del veterinario y atención, B-11 feriados, B-15 vacunaciones | Maximiliano | Sí |

Cambios de contrato del sprint: ver `docs/openapi/CHANGELOG.md` (v1.0.1).

## Revisión
- **Fecha:** Por completar
- **Qué se demostró:** Por completar (sugerido: login de cada rol, agendar con tope 409, feriado 400, API Clínica detenida → 503, atención y vacuna, token vencido)
- **Ítems no terminados y a qué sprint pasan:** Por completar
- **Retroalimentación del docente:** Por completar

## Retrospectiva
- **Qué funcionó bien:** el JWT emitido por la API Clínica se validó sin problemas en la API Agenda; los clientes falsos (`FakeClinicaClient`, `FakeFeriadosClient`) permitieron probar el 503 y el feriado sin depender de servicios externos; docker compose levanta todo con un comando.
- **Qué no funcionó:**
  - Una primera versión del código se alejó del diseño del Hito 2 (otras entidades y otra división de APIs) y hubo que rehacerla.
  - En el computador del laboratorio aparecieron errores de entorno: paquete con vulnerabilidad (NU1902), referencia duplicada a Swashbuckle al copiar el `.csproj` (NU1504), variables de entorno que Visual Studio no leía por no reiniciarlo y falta de permiso para crear bases en SQL Express (`CREATE DATABASE permission denied`).
  - La primera interfaz no explicaba qué hacía cada botón.
- **Mejora acordada para el Sprint 3:** revisar cada ítem contra los hitos antes de programar; usar LocalDB por defecto en `configurar-variables.bat`; escribir una guía de ejecución paso a paso; rediseñar el cliente con ayuda en cada pantalla.
- **¿Se aplicó la mejora del Sprint 1?** Sí: los cambios de contrato quedaron en el CHANGELOG (v1.0.1) y los nombres se fijaron (PawVet, `PawVetClinicaDB`, `PawVetAgendaDB`, Issuer `PawVet.ApiClinica`).

## Evidencia
- `evidencia/sprint-2-inicio.png`, `evidencia/sprint-2-cierre.png`
- Pull requests: Por completar (enlaces)
