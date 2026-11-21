# Backlog priorizado

Prioridad con MoSCoW. Estado al cierre del desarrollo del Sprint 2.

| ID | Ítem | HU / RF | Prioridad | Sprint | Responsable | Estado |
| --- | --- | --- | --- | --- | --- | --- |
| B-01 | Diagramas de casos de uso, clases, ER, arquitectura, despliegue, secuencia y estados | — | Must | 1 | Ambos | Hecho (Hito 2) |
| B-02 | Contratos OpenAPI de ambas APIs (diseño primero) | — | Must | 1 | Ambos | Hecho (Hito 2) · v1.0.1 en Sprint 2 |
| B-03 | Estructura del monorepo, tablero y ramas | — | Must | 1 | Vicente (SM) | Hecho |
| B-04 | Login con cuerpo JSON que emite el JWT compartido | HU-01 / RF-01 | Must | 1 | Vicente | Hecho |
| B-05 | CRUD de tutores y mascotas con validaciones (RUT, raza/especie, microchip) | HU-02 / RF-03 a RF-06 | Must | 2 | Vicente | Hecho |
| B-06 | Validación del JWT en la API Agenda (llave compartida) | RNF-01 | Must | 2 | Maximiliano | Hecho |
| B-07 | CRUD de citas con estados y control de topes | HU-03 / RF-07, RF-08 | Must | 2 | Maximiliano | Hecho |
| B-08 | Cliente HTTP tipado hacia la API Clínica con timeout y fallos | RNF-05 | Must | 2 | Maximiliano | Hecho |
| B-09 | Cliente React: login, sesión expirada, mascotas y citas | HU-01, HU-07 | Must | 2 | Ambos | Hecho |
| B-10 | Agenda del veterinario y registro de atención | HU-04, HU-05 | Should | 3 | Maximiliano | Adelantado en Sprint 2 |
| B-11 | Integración con la API de feriados | RF-12 | Could | 3 | Maximiliano | Adelantado en Sprint 2 |
| B-12 | Pruebas de integración y colección de pruebas | — | Must | 3 | Ambos | Escritas · falta ejecutar y guardar reportes |
| B-13 | Informe técnico, matriz de trazabilidad, ADR y README | — | Must | 3 | Ambos | Borrador listo · falta sección 8 con datos reales |

Ítems agregados durante el desarrollo:

| ID | Ítem | Origen | Prioridad | Sprint | Estado |
| --- | --- | --- | --- | --- | --- |
| B-14 | Gestión de usuarios, veterinarios y catálogos en el cliente (Administrador) | HU-06 / RF-02, CU-03 | Should | 2 | Hecho |
| B-15 | Vacunaciones y vacunas por vencer | RF-11, RF-13 | Should/Could | 2 | Hecho |
| B-16 | docker-compose con 4 contenedores y `.env.example` | Hito 2, Despliegue | Must | 2 | Hecho |
| B-17 | RF-14 Recordatorios por correo | Hito 1 | Won't | — | Fuera de esta versión |
| B-18 | Rediseño del cliente: menú lateral con descripciones y ayuda "¿Qué hace cada botón?" en cada pantalla | Revisión del Sprint 2 (usabilidad, RNF-06) | Should | 2 | Hecho |
| B-19 | Guía de ejecución paso a paso (Word) | Preparación de la defensa | Should | 3 | Hecho |
| B-20 | Corregir fecha/lote de una vacunación (`PUT /vacunaciones/{id}`) y registrar vacunas con fecha anterior desde el historial | Revisión del Sprint 3 (RF-11, RF-13) | Should | 3 | Hecho |
