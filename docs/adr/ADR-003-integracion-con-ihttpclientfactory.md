# ADR-003 — Integración entre servicios con IHttpClientFactory, timeouts y manejo de fallos

- **Estado:** Aceptada (Hito 2, Sprint 1)
- **Decisores:** Maximiliano Cuevas (PO), Vicente Bravo (SM)

## Contexto
Para agendar una cita la API Agenda debe comprobar que la mascota y el veterinario existen (viven en la API Clínica, ADR-001) y que el día no es feriado (API pública Nager.Date). Si una dependencia está lenta o caída, la API Agenda no debe quedar colgada ni caerse (RNF-05).

## Decisión
- **Clientes HTTP tipados** registrados con `AddHttpClient<IClinicaClient, ClinicaClient>` y `AddHttpClient<IFeriadosClient, FeriadosClient>`. IHttpClientFactory administra las conexiones (evita agotar sockets) y permite reemplazar los clientes por versiones falsas en las pruebas.
- **API Clínica:** timeout de **5 s**, sin reintentos. Mascota y veterinario se consultan **en paralelo** (`Task.WhenAll`). Resultado de cada consulta: `Encontrado`, `NoEncontrado` (404 → la API Agenda responde **400**) o `NoDisponible` (timeout, error de red o 5xx → **503** con ProblemDetails). La cita no se guarda.
- **API de feriados:** timeout de **3 s** y **caché de 24 h** por año (`IMemoryCache`). Si falla, **se continúa** sin validar el feriado y se registra una advertencia en el log: el feriado es una regla *Could* (RF-12) y no debe impedir agendar.
- Las URL base se configuran con `Servicios__ApiClinica` y `Servicios__ApiFeriados` (en Docker: `http://api-clinica:8080/`).

## Alternativas consideradas
1. **Reintentos con Polly:** mejora la tolerancia a fallos breves, pero triplica el tiempo de espera del usuario en una caída real; se deja como mejora futura.
2. **`new HttpClient()` en cada llamada:** agota sockets y no permite inyectar dobles de prueba.

## Consecuencias
- (+) La API Agenda responde siempre en un tiempo acotado (máx. ~8 s) y con un error comprensible.
- (+) Las pruebas automatizadas simulan la API Clínica caída (`FakeClinicaClient`, id 500) y verifican el 503.
- (−) Si la API de feriados está caída, se podría agendar en un feriado; queda registrado en el log.
