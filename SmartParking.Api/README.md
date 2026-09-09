# Smart Parking Ambato - Backend (ASP.NET Core Web API)

## 1. Requisitos
- Visual Studio 2022 (con carga de trabajo "ASP.NET y desarrollo web")
- .NET 8 SDK
- MySQL Server corriendo en local (ya deberías tener `smart_parking` creada con `schema.sql`)

## 2. Configurar la conexión y los secretos — YA NO van en appsettings.json

`appsettings.json` ahora solo tiene placeholders (`"SET_VIA_USER_SECRETS_OR_ENV_VAR"`) — a propósito,
para que nunca termines subiendo una contraseña real o la clave JWT a un repositorio de código.

**En desarrollo (tu máquina, Visual Studio):** usa `dotnet user-secrets` — guarda los valores
fuera del proyecto, en tu perfil de usuario, y el `.csproj` ya trae el `UserSecretsId` configurado:

```bash
cd SmartParking.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=smart_parking;User=root;Password=TU_PASSWORD_REAL;TreatTinyAsBoolean=true;"
dotnet user-secrets set "Jwt:Key" "una-clave-larga-y-aleatoria-de-al-menos-32-caracteres"
```

Verifica que se guardaron con `dotnet user-secrets list`. ASP.NET Core los carga automáticamente
en modo Development — no necesitas tocar nada más en el código.

**En producción (servidor real):** usa variables de entorno, con doble guion bajo `__` en vez
de `:` para anidar secciones:

```bash
export ConnectionStrings__DefaultConnection="Server=...;Password=...;"
export Jwt__Key="clave-de-produccion-distinta-a-la-de-desarrollo"
```

Estas variables tienen prioridad automática sobre `appsettings.json` — es el comportamiento
estándar de configuración en capas de ASP.NET Core, no hay que configurar nada extra.

**Si corres el proyecto sin haber hecho ninguno de los dos pasos anteriores, va a fallar al
arrancar** (no puede conectarse a MySQL con un placeholder como contraseña) — es intencional:
mejor que falle ruidosamente al inicio que arrancar con un secreto vacío en silencio.

## 3. Restaurar paquetes y ejecutar
En Visual Studio: clic derecho en la solución → "Restaurar paquetes NuGet", luego F5 (o `dotnet run` desde terminal). Se abrirá Swagger en `https://localhost:xxxx/swagger`.

## 4. Diferencia clave con el mockup de React que subiste
En el prototipo de React, el login "funcionaba" contra un array en memoria y el JWT era un string simulado. Aquí:
- `AuthService.LoginAsync` consulta `usuarios` en MySQL de verdad y valida la contraseña con **BCrypt real** (`BCrypt.Net.BCrypt.Verify`).
- `JwtService` firma el token con HMAC-SHA256 usando tu `Jwt:Key` — Program.cs configura `JwtBearer` para **validar esa firma en cada request protegido**, algo que el mockup no podía hacer porque no existía servidor.

## 5. Los usuarios demo de `schema.sql` tienen un hash placeholder
El `INSERT` de `usuarios` en el script SQL trae `$2a$11$PLACEHOLDER_HASH`, que **no es un hash válido** — no vas a poder loguearte con esos registros tal cual. Dos formas de arreglarlo:

**Opción A - Registrar un conductor real vía API:**
Llama a `POST /api/auth/registro` desde Swagger con un correo/cédula nuevos; el endpoint genera el hash BCrypt correctamente y ya puedes loguearte con `POST /api/auth/login`.

**Opción B - Generar el hash para actualizar admin/operador manualmente:**
Crea un proyecto de consola temporal (o usa la ventana "C# Interactive" de Visual Studio) con este snippet, y pega el resultado en un `UPDATE usuarios SET password_hash = '...' WHERE correo = '...'`:
```csharp
using BCrypt.Net;
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Demo1234"));
```

## 7. Gestión de personal (Administrador/Operador)
- `POST /api/auth/registrar-staff` `[Authorize(Roles="administrador")]` — un admin ya logueado
  crea otras cuentas de administrador u operador. La API rechaza explícitamente `rol=conductor`
  aquí (para eso está `/api/auth/registro`, que sí es público).
- `GET /api/usuarios` / `PUT /api/usuarios/{id}/estado` — listar y activar/desactivar personal
  (soft-delete: nunca se borra el registro, se marca `activo=false`). Un admin no puede
  desactivarse a sí mismo.

**Problema del "huevo y la gallina":** como `registrar-staff` requiere estar autenticado como
administrador, **el primer administrador del sistema no puede crearse por la API** — necesitas
uno ya existente en la base para arrancar la cadena. Dos opciones:
1. Usa el usuario admin demo del `schema.sql` (correo `admin@parqueo.ec`) y actualízale el
   `password_hash` con el snippet de BCrypt de la sección 5 de este README.
2. O inserta un `INSERT INTO usuarios` manual con un hash generado igual que en la opción 1.

## 9. Rate limiting (protección contra fuerza bruta)
Usa el rate limiter **nativo de .NET 8** (`Microsoft.AspNetCore.RateLimiting`), sin paquete NuGet
extra:
- `POST /api/auth/login`: máximo **5 intentos por minuto por IP**. El 6to responde
  `429 Too Many Requests` de inmediato (sin cola de espera).
- `POST /api/auth/solicitar-recuperacion`: máximo **3 solicitudes cada 5 minutos por IP** — más
  estricto, porque pedir tokens de recuperación repetidamente es una forma barata de acosar a
  un usuario (le llegarían múltiples "correos" de recuperación que no pidió).

Esto limita por dirección IP, no por cuenta — así que sigue funcionando aunque el atacante
pruebe con distintos correos/cédulas desde la misma máquina.

## 10. Qué falta (próximos módulos)
- `[Authorize(Roles = "administrador")]` en los endpoints que correspondan (revisar cobertura completa)

## 7. RoutingService (Dijkstra / A*) — decisión de diseño importante

**Limitación documentada, no oculta:** no existe un dataset abierto con coordenadas precisas
de intersecciones para Ambato (lo verifiqué buscando explícitamente). Por eso el grafo vial en
`Services/Routing/GrafoVialAmbatoSeed.cs` usa coordenadas **aproximadas**, ubicadas a mano sobre
los cruces conocidos de Cevallos, Bolívar y Sucre con las transversales del centro — no un
extracto real de OpenStreetMap. Esto está comentado explícitamente en el código para que puedas
citarlo como limitación reconocida en la tesis, no como un error.

**Cómo funciona:**
- `GET /api/rutas?origenLat=..&origenLng=..&parqueaderoId=..&algoritmo=astar|dijkstra`
- `GET /api/rutas/comparar?origenLat=..&origenLng=..&parqueaderoId=..` devuelve **ambos**
  algoritmos en una sola respuesta — pensado directamente para el capítulo de tu tesis que
  compara Dijkstra vs. A* (incluye `nodosExplorados` de cada uno, así puedes mostrar
  empíricamente que A* explora menos nodos gracias a la heurística).
- La posición GPS del conductor y las coordenadas del parqueadero (que sí vienen de MySQL,
  reales) se conectan dinámicamente a los 2 nodos más cercanos del grafo por distancia en
  línea recta (fórmula de Haversine) — así el sistema funciona sin importar dónde esté
  parado el conductor, no solo desde nodos predefinidos.
- Las calles Cevallos/Bolívar/Sucre están marcadas `EsCalleSaturada = true` y reciben una
  penalización de tiempo (x2.5 en hora pico: 7-9h, 12-14h, 17-20h; x1.3 fuera de pico) — así
  el algoritmo las evita cuando conviene, sin necesitar datos de tráfico en vivo.

**Para llevarlo a producción real** (con internet disponible en el servidor):
1. Usar Overpass API para extraer las vías reales de Ambato (query de ejemplo en el comentario
   de `GrafoVialAmbatoSeed.cs`)
2. Convertir cada `way` de OSM en nodos/aristas reales
3. Reemplazar datos de tráfico simulados por una fuente real si consigues acceso a una
   (Google Distance Matrix API, TomTom Traffic API, etc.) — ninguna es gratuita a gran escala,
   otro punto válido para la sección de limitaciones/trabajo futuro de la tesis
