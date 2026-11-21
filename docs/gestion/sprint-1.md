# Sprint 1 — Diseño

| | |
| --- | --- |
| Semana | Semana 2 |
| Hito asociado | Hito 2: Diseño |
| **Objetivo** | Dejar diseñado el sistema (diagramas, contratos y estructura) para implementar sin ambigüedades |
| Entregable | `Hito2-Diseno.docx` (en `docs/hitos/`) |

## Planificación

| Ítem | Responsable | Terminado |
| --- | --- | --- |
| B-01 Diagramas (casos de uso, clases, ER, C4, despliegue, 3 secuencias, estados) | Ambos | Sí |
| B-02 Contratos OpenAPI v1.0.0 | Vicente (Clínica), Maximiliano (Agenda) | Sí |
| B-03 Monorepo, tablero y ramas | Vicente | Sí |
| B-04 Login con cuerpo JSON y claim id_veterinario (diseño) | Vicente | Sí |
| ADR-001 a ADR-003 | Ambos | Sí |

## Revisión
- **Fecha:** Por completar
- **Qué se demostró:** documento del Hito 2 con todos los diagramas como código, contratos y trazabilidad.
- **Retroalimentación del docente:** Por completar (el caso del Hito 1 quedó validado sin observaciones).

## Retrospectiva
- **Qué funcionó bien:** dejar los diagramas como código (Mermaid/PlantUML) en el repositorio permitió corregirlos rápido; escribir los contratos OpenAPI antes del código obligó a definir cada código de respuesta; el caso del Hito 1 quedó validado sin observaciones.
- **Qué no funcionó:** en los documentos usamos el nombre de trabajo "VetClinica" mientras el proyecto se llamaba PawVet, y no definimos el emisor (Issuer) ni la audiencia (Audience) del token.
- **Mejora acordada para el Sprint 2:** registrar todo cambio de contrato en `docs/openapi/CHANGELOG.md` en el mismo pull request y fijar los nombres (sistema, bases de datos, emisor del token) antes de programar.

## Evidencia
- `evidencia/sprint-1-inicio.png`, `evidencia/sprint-1-cierre.png`
- Pull requests: Por completar (enlaces)
