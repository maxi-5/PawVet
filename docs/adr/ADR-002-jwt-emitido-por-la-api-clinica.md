# ADR-002 — La API Clínica emite el JWT y la API Agenda lo valida con la misma llave

- **Estado:** Aceptada (Hito 2, Sprint 1)
- **Decisores:** Maximiliano Cuevas (PO), Vicente Bravo (SM)

## Contexto
Los usuarios inician sesión una sola vez y deben poder usar las dos APIs. La pauta exige que el JWT lo emita una API y lo valide otra, y que los secretos no estén en el repositorio.

## Decisión
- La **API Clínica** emite un JWT **HS256** de **60 minutos** en `POST /api/v1/auth/login` con los claims `sub` (email), `id_user`, `nombre`, `role` y, para veterinarios con ficha vinculada, `id_veterinario`.
- La **API Agenda** valida firma, emisor (`PawVet.ApiClinica`), audiencia (`PawVet`) y vencimiento (`ClockSkew = 0`) con la **misma llave**, leída de la variable de entorno `Jwt__Key` en ambas APIs. Si la llave no existe, la API no inicia y muestra un mensaje claro.
- Cuando la API Agenda llama a la API Clínica, **reenvía el token del usuario** (no usa una cuenta de servicio), así la API Clínica aplica sus propios permisos.
- El claim `id_veterinario` permite que `GET /citas/mi-agenda` y `POST /atenciones` sepan qué veterinario está logueado sin otra consulta. Para eso se agregó la columna `id_user` (única y opcional) en `VETERINARIO`.

## Alternativas consideradas
1. **RS256 (llave privada/pública):** solo la API Clínica podría firmar. Es más seguro, pero requiere gestionar certificados; queda como mejora futura.
2. **Servidor de identidad externo (IdentityServer, Auth0):** excede el alcance del curso.

## Consecuencias
- (+) Una sola sesión para todo el sistema; la API Agenda no necesita consultar usuarios.
- (−) Las dos APIs conocen la llave: si una se compromete, se pueden firmar tokens. Mitigación: la llave vive solo en variables de entorno / `.env` no versionado.
- (−) Un token sigue siendo válido hasta que vence aunque se elimine el usuario (máximo 60 minutos).
