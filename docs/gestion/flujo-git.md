# Flujo de trabajo con Git

- **Repositorio:** https://github.com/maxi-5/PawVet — monorepo en GitHub con `/docs`, `/api-clinica`, `/api-agenda`, `/cliente-react` (Hito 1).
- **Rama principal:** `main`, protegida: solo se integra por pull request con una aprobación.
- **Ramas:** `feature/<id>-<descripcion>` (ej. `feature/B-07-citas-estados`), `fix/<descripcion>`, `docs/<descripcion>`.
- **Commits:** en español e imperativo, con el ID del ítem: `B-07: agrega PATCH /citas/{id}/estado`.
- **Pull request:** descripción con qué cambia, cómo probarlo y el ítem que cierra (`Cierra #12`). Lo revisa **el otro integrante**. Si cambia un contrato, el PR incluye la entrada en `docs/openapi/CHANGELOG.md`.
- **Secretos:** `.env` está en `.gitignore`; solo se versiona `.env.example`. Antes de cada PR: `git diff --staged` para revisar que no haya llaves ni cadenas de conexión.

Ramas sugeridas para este sprint, en el orden en que se construyó el código:

| Rama | Responsable | Contenido |
| --- | --- | --- |
| `feature/B-04-login-jwt` | Vicente | AuthController, TokenService, DbSeeder |
| `feature/B-05-tutores-mascotas` | Vicente | Tutores, Mascotas, Validaciones (RUT), catálogos |
| `feature/B-06-agenda-base` | Maximiliano | Proyecto api-agenda, AgendaDbContext, validación JWT |
| `feature/B-07-citas` | Maximiliano | CitasController, reglas de Cita |
| `feature/B-08-integracion` | Maximiliano | ClinicaClient, FeriadosClient |
| `feature/B-09-cliente` | Ambos | cliente-react |
| `feature/B-12-pruebas` | Ambos | api-*.Tests y colección Postman |
| `docs/B-13-informe` | Ambos | docs/ |
