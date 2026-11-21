# PawVet — Trabajo 3 Construcción de Software

Sistema de gestión para una clínica veterinaria: tutores, mascotas, agenda de citas, atenciones y vacunas, con acceso por rol. Dos APIs RESTful en **ASP.NET Core (.NET 10, controladores) + Entity Framework Core + SQL Server**, y un cliente web en **React**.

Equipo: **Maximiliano Cuevas** (Product Owner) y **Vicente Bravo** (Scrum Master). Docente: Enso Guidotti.
Diseño: `docs/hitos/Hito1-Caso-y-requerimientos.docx` y `docs/hitos/Hito2-Diseno.docx`.

## Arquitectura

| Componente | Puerto | Responsabilidad | Base de datos |
| --- | --- | --- | --- |
| `api-clinica` — API Clínica e Identidad | 5001 | Login (**emite el JWT**), usuarios, roles, tutores, mascotas, veterinarios, especies, razas | `PawVetClinicaDB` |
| `api-agenda` — API Agenda y Atención | 5002 | Citas, atenciones, vacunaciones (**valida el JWT**); consume la API Clínica y la API de feriados | `PawVetAgendaDB` |
| `cliente-react` | 5173 (dev) · 3000 (Docker) | Interfaz web por rol | — |

La API Agenda verifica mascota y veterinario en la API Clínica con un cliente HTTP tipado (IHttpClientFactory, timeout 5 s → 503) y consulta los feriados de Chile en Nager.Date (timeout 3 s, caché 24 h). Ver `docs/adr/`.

## Estructura

```
PawVet-Trabajo3/
├── api-clinica/  api-clinica.Tests/     → API Clínica e Identidad + pruebas xUnit
├── api-agenda/   api-agenda.Tests/      → API Agenda y Atención + pruebas xUnit
├── cliente-react/                       → React + Vite (Dockerfile con nginx)
├── docs/
│   ├── hitos/          → Hito 1 y Hito 2 entregados
│   ├── requerimientos/ → ERS (v2)
│   ├── diagramas/      → fuentes .mmd/.puml y png/ (casos de uso, clases, 2 ER, contexto, contenedores, despliegue, 3 secuencias, estados)
│   ├── openapi/        → api-clinica.v1.yaml, api-agenda.v1.yaml, CHANGELOG.md
│   ├── adr/            → ADR-001 a ADR-004
│   ├── trazabilidad/   → matriz requerimiento → HU → CU → endpoint → pantalla → prueba
│   ├── pruebas/        → colección Postman y guía del reporte de la suite
│   ├── gestion/        → backlog, sprints, tablero, flujo Git
│   ├── guia/           → guía paso a paso para ejecutar y mostrar el sistema (Word)
│   ├── capturas/       → capturas del cliente
│   └── informe/        → informe técnico (Word y PDF)
├── docker-compose.yml  .env.example     → sistema completo con un comando
├── PawVet.slnx                          → solución de Visual Studio
└── configurar-variables.bat  iniciar.bat → ejecución local sin Docker
```

## Requisitos
- Visual Studio 2026 (o 2022 17.14+) con el **SDK de .NET 10**, o el SDK solo.
- SQL Server: **LocalDB** (viene con Visual Studio) o SQL Server Express. Con Docker no hace falta.
- **Node.js 20+** para el cliente.
- Opcional: **Docker Desktop** para la opción A.

## Paso 0 (una sola vez por el equipo): crear las migraciones
El repositorio debe incluir la carpeta `Migrations` de cada API. Si todavía no existe, ejecútalo en la *Consola del Administrador de paquetes* de Visual Studio (después del paso B1) y sube las carpetas generadas:

```powershell
Add-Migration Inicial -Project PawVetClinica -StartupProject PawVetClinica
Add-Migration Inicial -Project PawVetAgenda -StartupProject PawVetAgenda
```

Cada API aplica sus migraciones sola al iniciar (crea la base, los roles, especies, razas, vacunas y el usuario administrador).

## Opción A — Un solo comando con Docker

```powershell
copy .env.example .env      # y completa MSSQL_SA_PASSWORD, Jwt__Key (32+ caracteres) y Admin__Password
docker compose up --build
```

| Componente | Dirección |
| --- | --- |
| Cliente web | http://localhost:3000 |
| API Clínica (Swagger) | http://localhost:5001/swagger |
| API Agenda (Swagger) | http://localhost:5002/swagger |

## Opción B — Local con Visual Studio

1. **Secretos:** doble clic en `configurar-variables.bat` (guarda `ConnectionStrings__ClinicaDB`, `ConnectionStrings__AgendaDB` con LocalDB y una `Jwt__Key` aleatoria compartida). **Cierra y vuelve a abrir Visual Studio.**
2. **Migraciones:** paso 0, si falta.
3. **Ejecutar:** doble clic en `iniciar.bat` (abre las dos APIs y el cliente en http://localhost:5173). Desde Visual Studio: *Configurar proyectos de inicio → Varios proyectos* → `PawVetClinica` y `PawVetAgenda`; y en una consola `cd cliente-react`, `npm install`, `npm run dev`.

Ninguna llave ni cadena de conexión está en el repositorio: `appsettings.json` las trae vacías y `.env` está en `.gitignore`.

## Interfaz
Menú lateral con una descripción bajo cada opción, encabezado en cada pantalla con su propósito y un recuadro plegable **"¿Qué hace cada botón?"**; cada botón muestra además su descripción al pasar el mouse. Guía completa de ejecución y recorrido por roles: `docs/guia/Guia-ejecucion-PawVet.docx`.

## Primer uso
1. Ingresa con el administrador inicial `admin@pawvet.cl` / `Admin123!` (o el `Admin__Password` del `.env`).
2. **Usuarios:** crea una recepcionista y un veterinario. **Veterinarios:** crea la ficha del veterinario y vincúlala a su usuario (así su token trae `id_veterinario`).
3. Como **recepcionista:** registra un tutor, su mascota y agenda una cita; confírmala. Prueba agendar en el mismo horario (409) o un 25 de diciembre (feriado, 400).
4. Como **veterinario:** *Mi agenda* → **Atender** → registra la atención y, si corresponde, la vacuna. Queda en el *Historial*.

## Roles

| Función | Administrador | Recepcionista | Veterinario |
| --- | --- | --- | --- |
| Usuarios, veterinarios, especies y razas | Gestiona | Consulta veterinarios | Consulta veterinarios |
| Tutores y mascotas | Gestiona y elimina | Crea y edita | Consulta |
| Citas | Gestiona y elimina | Agenda, edita, confirma, cancela | Solo su agenda |
| Atenciones | Consulta | — | Registra (solo sus citas) |
| Vacunaciones | Consulta | Consulta | Registra |

## Pruebas
- Suite xUnit (76 métodos, 80 casos): `dotnet test PawVet.slnx` — guía y comando del reporte en `docs/pruebas/README.md`.
- Colección Postman (41 solicitudes): `docs/pruebas/PawVet.postman_collection.json`.
- Archivos `.http` en cada API para probar desde Visual Studio.

## Variables de entorno

| Variable | Usada por | Descripción |
| --- | --- | --- |
| `ConnectionStrings__ClinicaDB` | api-clinica | Cadena de conexión a PawVetClinicaDB |
| `ConnectionStrings__AgendaDB` | api-agenda | Cadena de conexión a PawVetAgendaDB |
| `Jwt__Key` | ambas | Llave HS256 compartida (mín. 32 caracteres) |
| `Admin__Email`, `Admin__Password` | api-clinica | Administrador inicial (opcionales) |
| `Servicios__ApiClinica` | api-agenda | URL de la API Clínica (por defecto http://localhost:5001/) |
| `MSSQL_SA_PASSWORD` | docker compose | Clave de SQL Server en el contenedor |
| `VITE_API_CLINICA`, `VITE_API_AGENDA` | cliente | URL de las APIs (opcionales) |

## Problemas comunes
| Mensaje | Solución |
| --- | --- |
| `Falta la variable de entorno Jwt__Key` / `ConnectionStrings__...` | Ejecuta `configurar-variables.bat` y reinicia Visual Studio |
| `No hay migraciones. Ejecuta una vez: Add-Migration Inicial ...` | Paso 0 |
| `CREATE DATABASE permission denied` | Usa LocalDB (`(localdb)\MSSQLLocalDB`), como deja el `.bat` |
| La API Agenda responde 503 al agendar | La API Clínica no está en ejecución en el puerto 5001 |
| El cliente dice "No se pudo conectar con el servidor" | Revisa que las dos APIs estén en ejecución |
