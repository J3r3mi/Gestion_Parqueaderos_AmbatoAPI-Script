# Empaquetado móvil con Capacitor

Este entorno donde construí el proyecto **no tiene internet ni Android Studio/Xcode**, así que
no pude ejecutar `npx cap add android/ios` yo mismo (esos comandos descargan plantillas nativas
completas de proyecto Gradle/Xcode). Lo que sí dejé listo:

- `capacitor.config.ts` — configuración principal, ya apuntando a `webDir: 'www'`
  (coincide con el `outputPath` de `angular.json`)
- Scripts de npm (`cap:sync`, `cap:android`, `cap:ios`) en `package.json`
- `HomePage` ya detecta si corre nativo (`Capacitor.isNativePlatform()`) y usa
  `@capacitor/geolocation` en vez de la API web del navegador cuando es necesario
- El escáner QR (`@zxing/ngx-scanner`) funciona sin cambios dentro del WebView nativo —
  usa `getUserMedia`, que Capacitor soporta igual que un navegador normal

**Lo que tienes que correr tú** (requiere internet + Android Studio para Android, o
una Mac con Xcode para iOS):

## 1. Instalar dependencias y agregar las plataformas
```bash
cd smart-parking-app
npm install
npx cap add android
npx cap add ios      # solo si tienes Mac
```

## 2. Compilar Angular y sincronizar con los proyectos nativos
```bash
npm run cap:sync
```
Esto corre `ng build --configuration production` y luego `npx cap sync`, que copia el build
de Angular dentro de `android/app/src/main/assets/public` (y el equivalente en iOS).

## 3. Agregar permisos nativos — PASO OBLIGATORIO
Capacitor NO agrega permisos automáticamente; sin esto, el GPS y la cámara van a fallar
silenciosamente en el dispositivo aunque funcionen perfecto en el navegador.

### Android — edita `android/app/src/main/AndroidManifest.xml`
Agrega estas líneas dentro de `<manifest>`, antes de `<application>`:
```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

### iOS — edita `ios/App/App/Info.plist`
Agrega estas claves dentro del `<dict>` principal:
```xml
<key>NSCameraUsageDescription</key>
<string>Necesitamos la cámara para que el operador escanee el código QR de acceso.</string>
<key>NSLocationWhenInUseUsageDescription</key>
<string>Necesitamos tu ubicación para mostrarte en el mapa y calcular la ruta al parqueadero.</string>
```

## 4. Abrir y correr en el IDE nativo
```bash
npm run cap:android   # abre Android Studio
npm run cap:ios       # abre Xcode (solo Mac)
```
Desde ahí conectas un dispositivo/emulador y le das Run — igual que cualquier app nativa.

## 5. Conectar con el backend desde el dispositivo real
`https://localhost:7100` **no existe** desde el punto de vista de un celular — `localhost` en
un dispositivo apunta al propio dispositivo, no a tu computadora. Dos opciones:

- **Emulador Android:** usa `10.0.2.2` en vez de `localhost` (túnel especial del emulador hacia
  tu máquina host). Cambia `environment.ts`:
  ```ts
  apiUrl: 'https://10.0.2.2:7100/api'
  ```
- **Dispositivo físico:** usa la IP local de tu computadora en la red WiFi (ej. `192.168.1.50`),
  y asegúrate de que el backend escuche en `0.0.0.0`, no solo `localhost`
  (en Visual Studio: `launchSettings.json` → cambiar `applicationUrl` o correr con
  `dotnet run --urls "http://0.0.0.0:5000"`). También agrega esa IP a la política CORS en
  `Program.cs` del backend.

## 6. Cada vez que cambies código Angular
```bash
npm run cap:sync
```
Repite este comando después de cada cambio para que el proyecto nativo tenga el build actualizado
— Capacitor no hace watch automático como `ng serve`.

## 7. Generar el APK final (para entregar/instalar directamente, sin Android Studio corriendo)
Desde Android Studio: `Build → Generate Signed Bundle / APK`. Para la tesis, un APK sin firmar
("debug") alcanza para la demo — firmarlo formalmente solo es necesario si vas a publicarlo en
Play Store.
