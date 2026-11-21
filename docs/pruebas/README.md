# Evidencia de pruebas

La pauta pide dos evidencias: el **reporte de ejecución de la suite automatizada** y la **colección de pruebas de las APIs**.

## 1. Suite automatizada (xUnit)

Pruebas de integración que levantan cada API completa en memoria (`WebApplicationFactory`: middleware, JWT, roles, controladores, repositorios) con base de datos InMemory, más pruebas unitarias de las reglas del diagrama de clases.

| Proyecto | Métodos | Qué cubre |
| --- | --- | --- |
| `api-clinica.Tests` | 31 | Login y claim `id_veterinario`, usuarios (401, 403, 409, 400), tutores (RUT inválido/repetido, 403, 409 con mascotas), mascotas (raza de otra especie, microchip, fecha futura, edad), veterinarios, especies y razas |
| `api-agenda.Tests` | 45 (49 casos) | Citas (201, 401, 403, fecha pasada, mascota inexistente, **API Clínica caída → 503**, **feriado → 400**, **tope → 409**, transiciones de estado), mi agenda, atenciones (403 otro veterinario, 409), vacunaciones (próxima dosis, otra especie, 503, corrección de fecha), por vencer; unitarias de `Cita`, `Vacunacion` |

En `api-agenda.Tests` la API Clínica y la API de feriados se reemplazan por `FakeClinicaClient` (mascotas 1 y 2, veterinarios 1 y 2, id **500 = API caída**) y `FakeFeriadosClient`. Los tokens se generan con la misma llave, emisor y audiencia que usa la API Clínica.

**Ejecutar:** Visual Studio → *Prueba → Ejecutar todas las pruebas*, o desde la carpeta raíz:

```powershell
dotnet test PawVet.slnx --logger html --logger trx --results-directory docs/pruebas/resultados
```

Se genera un reporte HTML por cada proyecto de pruebas (Clínica y Agenda) en `docs/pruebas/resultados/`; esos archivos son los que se entregan. No se usa un nombre fijo porque el segundo proyecto sobrescribiría el reporte del primero.

## 2. Colección de Postman

`PawVet.postman_collection.json`: **41 solicitudes** en 3 carpetas, cada una verifica el código de estado (y el formato `application/problem+json` en los errores). Las variables (`tokenAdmin`, `idMascota`, `idCita`, …) se encadenan solas; los RUT y correos se generan en cada ejecución, así que se puede ejecutar varias veces.

**Opción rápida, sin instalar Postman (Newman):** con las dos APIs en ejecución, desde la carpeta raíz:

```powershell
npx -y newman run docs\pruebas\PawVet.postman_collection.json --reporters cli,json --reporter-json-export docs\pruebas\resultados\postman-resultados.json
```

**Con la aplicación Postman:**

1. Deja las dos APIs en ejecución (5001 y 5002).
2. Postman → **Import** → selecciona el archivo.
3. Clic derecho en la colección → **Run collection** → *Run*.
4. **Export Results** y guarda el archivo en `docs/pruebas/resultados/`.

Cubre: login por rol, 401 sin token y con token alterado, 403 por rol, validaciones 400, conflictos 409, tope de horario, feriado real (25 de diciembre, contra Nager.Date), transiciones de estado, atención y vacunación.

## 3. Prueba manual de la integración caída (RNF-05)

Detén la API Clínica y agenda una cita desde el cliente o Swagger: la API Agenda responde **503** "La API Clinica no respondio" en a lo más 5 segundos y no guarda la cita. Con la API Clínica de nuevo arriba, la misma solicitud responde 201.

## Estado
**21-11-2026: colección de Postman 41/41 solicitudes y 70/70 verificaciones aprobadas** (Newman), resultado en `resultados/postman-resultados.json`.

**21-11-2026: 80 de 80 pruebas aprobadas** (31 Clínica + 49 casos Agenda). En la primera ejecución falló `GetCita_Inexistente_Devuelve404ConProblemDetails`: `[Produces("application/json")]` forzaba el tipo de contenido de los errores; se quitó de los controladores y la suite quedó en verde.
