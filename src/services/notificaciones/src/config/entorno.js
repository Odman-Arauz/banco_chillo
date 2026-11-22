// Lee y valida las variables de entorno UNA sola vez al arrancar.
// Local: archivo .env (copia de .env.example). Render: variables del servicio.

const requerida = (nombre, valorPorDefecto) => {
  const valor = process.env[nombre] ?? valorPorDefecto;
  if (valor === undefined || valor === '') {
    throw new Error(`Falta la variable de entorno ${nombre}. Revisa .env.example y docs/guias/04-errores-frecuentes.md`);
  }
  return valor;
};

export const entorno = {
  esProduccion: process.env.NODE_ENV === 'production',
  puerto: Number(process.env.PUERTO ?? process.env.PORT ?? 5007),
  databaseUrl: requerida('DATABASE_URL'),
  jwt: {
    // Debe ser el MISMO secreto, emisor y audiencia que usan los servicios .NET (sección "Jwt").
    secreto: requerida('JWT_SECRETO'),
    emisor: process.env.JWT_EMISOR ?? 'BancoChillo.Auth',
    audiencia: process.env.JWT_AUDIENCIA ?? 'BancoChillo.Clientes',
  },
  corsOrigenes: (process.env.CORS_ORIGENES ?? 'http://localhost:8081,http://localhost:19006').split(','),
};
