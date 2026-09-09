import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'ec.ambato.smartparking',
  appName: 'Smart Parking Ambato',
  webDir: 'www', // debe coincidir con outputPath en angular.json (ya configurado así)
  server: {
    // Durante desarrollo con "live reload" contra el backend local, Android exige
    // tráfico en texto plano (cleartext) para conectar a http://localhost. En producción,
    // el backend real debe servir HTTPS y esta bandera debe quitarse.
    androidScheme: 'https',
    cleartext: true
  }
};

export default config;
