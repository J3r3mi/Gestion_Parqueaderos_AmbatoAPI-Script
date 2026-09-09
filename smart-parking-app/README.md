# Smart Parking Ambato - Frontend (Ionic/Angular)

## 1. Requisitos
- Node.js 18+ (tienes v22, perfecto)
- VS Code
- Backend `SmartParking.Api` corriendo (ver su propio README)

## 2. Instalar dependencias
Este proyecto se generó a mano (sin conexión a internet en el entorno donde lo construí), así que
el primer paso es dejar que npm resuelva todo:

```bash
cd smart-parking-app
npm install
```

Si prefieres usar el CLI de Ionic (opcional, da algunos comandos extra como `ionic serve`):
```bash
npm install -g @ionic/cli
```

## 3. Conectar con el backend
Abre `src/environments/environment.ts` y ajusta el puerto al que te muestre Visual Studio
cuando ejecutes el backend (aparece en la consola al hacer F5, algo como `https://localhost:7100`):

```ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7100/api'
};
```

**Importante:** el backend ya tiene CORS configurado para aceptar `http://localhost:8100`
(puerto por defecto de este proyecto) y `http://localhost:4200`. Si usas otro puerto, agrégalo
en `Program.cs` del backend, sección `AddCors`.

## 4. Ejecutar
```bash
npm start
```
Esto corre `ng serve` en `http://localhost:8100`. Abre esa URL en el navegador — verás la pantalla
de login.

## 5. Qué ya funciona (resumen completo)
- **Login real** (`LoginPage`): contra `POST /api/auth/login`, JWT persistente en `localStorage`,
  `authInterceptor` lo agrega a cada request, `authGuard`/`rolAdministradorGuard` protegen rutas
  por rol. Redirección post-login: conductor → `/home`, administrador → `/dashboard`,
  operador → `/validar-acceso`.
- **Mapa + ruta óptima** (`HomePage`): ver sección 6.
- **Reserva + QR** (`ReservarPage`, `MisReservasPage`): elegir plaza libre, ver tarifa, confirmar
  (maneja el `409 Conflict` del locking pesimista), generar QR en el cliente con la librería
  `qrcode`, listar/cancelar reservas propias.
- **Validación de acceso con cámara real** (`ValidarAccesoPage`): escáner QR con
  `@zxing/ngx-scanner`, modo Entrada/Salida, cobro automático al salir.
- **Dashboard del administrador** (`DashboardPage`): ver sección 8.

## 6. Mapa Leaflet + ruta óptima (HomePage)
- Al entrar a `/home`, `ngAfterViewInit` inicializa el mapa Leaflet centrado en Ambato.
- Trae parqueaderos reales desde `GET /api/parqueaderos` (marcador + popup con cupos libres).
- Sigue el GPS del conductor **en vivo** (`watchPosition`) con un círculo azul.
- Al tocar un marcador se abre un panel inferior con:
  - Segmento para elegir **A\*** o **Dijkstra**
  - Botón "Ver ruta óptima" → llama a `GET /api/rutas` y dibuja el resultado
  - Distancia, tiempo estimado y `nodosExplorados` (útil para comparar A* vs Dijkstra en la tesis)
  - Botón "Reservar plaza" → navega a `/reservar/:id`
- **La ruta se dibuja tramo por tramo** (no una sola línea): los tramos `esCalleSaturada`
  (Cevallos/Bolívar/Sucre) se pintan en rojo, el resto en azul — visualmente se ve qué evita
  el algoritmo, no solo un número.
- Cambiar el segmento A*/Dijkstra **recalcula y redibuja en vivo** sobre el mismo trayecto.

**Nota sobre los íconos de Leaflet:** apuntan al CDN de unpkg en vez de a `node_modules`, porque
las rutas relativas del CSS de Leaflet no resuelven bien con el bundler de Angular CLI.

**Nota sobre permisos de ubicación:** en `localhost:8100` el GPS funciona sin HTTPS. Para
Capacitor vas a necesitar permisos nativos (`AndroidManifest.xml` / `Info.plist`).

## 7. Escáner de QR del Operador (ValidarAccesoPage)
Cámara real con `@zxing/ngx-scanner`, sin simulación:
- Segmento Entrada/Salida cambia el endpoint llamado al escanear
- En modo Salida se elige antes el método de pago
- El escáner se **pausa automáticamente** mientras se espera la respuesta del backend, para no
  procesar el mismo QR dos veces
- Hay que tocar "Escanear siguiente vehículo" para reactivar la cámara (decisión deliberada)

**Requisitos de permisos:**
- Navegador en `localhost:8100`: pide permiso de cámara, funciona sin HTTPS
- Producción web: requiere HTTPS
- Capacitor: `android.permission.CAMERA` / `NSCameraUsageDescription`

## 8. Dashboard del Administrador
Consume los 5 endpoints del backend en paralelo (`forkJoin`):
- Tarjetas KPI (la de Cupos Libres se pinta en rojo si llega a 0)
- Recaudación del día
- Gráfico de ocupación horaria hecho a mano con `<div>`/CSS (sin librería de charts)
- Lista de alertas con ícono/color según tipo
- Auto-refresco cada 30s (polling, no hay WebSockets/SignalR) + botón de refresco manual

## 9. Gestión de Personal (GestionUsuariosPage)
Accesible desde el ícono de personas en el toolbar del Dashboard. Permite:
- Listar administradores y operadores (`GET /api/usuarios`)
- Crear nuevos (`POST /api/auth/registrar-staff`, protegido — solo un admin logueado puede hacerlo)
- Activar/desactivar cuentas con un `ion-toggle` + confirmación (`PUT /api/usuarios/{id}/estado`) —
  es soft-delete, nunca se borra el registro de la base de datos

**Importante — arranque del sistema:** el primer administrador NO puede crearse desde esta
pantalla (necesitas estar logueado como admin para llegar aquí). Usa el usuario demo del
`schema.sql` del backend y actualízale la contraseña siguiendo la sección 5 de su README.

## 10. Qué falta
- Envío real de correo para recuperación de contraseña (hoy es modo demo, ver `RecuperarPasswordPage`)
- Generar el APK/IPA final firmado para distribución (ver `MOBILE-SETUP.md`, sección 7)

## 11. Empaquetado móvil (Capacitor) — guía completa en MOBILE-SETUP.md
Ya está preparado en el código: `capacitor.config.ts`, scripts de npm (`cap:sync`,
`cap:android`, `cap:ios`), y `HomePage` detecta automáticamente si corre nativo
(`Capacitor.isNativePlatform()`) para usar `@capacitor/geolocation` en vez de la API web.

**Este entorno no tiene internet ni Android Studio/Xcode**, así que los comandos que generan
los proyectos nativos (`npx cap add android/ios`) tienes que correrlos tú. Todos los pasos
exactos — incluyendo los permisos nativos obligatorios de cámara/GPS que Capacitor NO agrega
solo, y cómo conectar con el backend desde un dispositivo real — están en
**`MOBILE-SETUP.md`**, en la raíz de este proyecto.
