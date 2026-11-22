import cors from 'cors';
import express from 'express';
import rateLimit from 'express-rate-limit';
import helmet from 'helmet';
import morgan from 'morgan';
import { autenticar } from './middlewares/autenticacion.js';
import { manejadorErrores, noEncontrado } from './middlewares/errores.js';
import { crearRutasPlantillas } from './modulos/plantillas/plantillas.rutas.js';

/**
 * Construye la aplicación Express SIN arrancar el servidor.
 * Recibe sus dependencias (configuración y repositorios) para poder probarla con repositorios falsos.
 */
export function crearApp({ config, repositorios, verificarBaseDatos = async () => true }) {
  const app = express();

  // ---- Middlewares de seguridad (Parte 14 del práctico) ----
  app.use(helmet());
  app.use(cors({ origin: config.corsOrigenes }));
  app.use(express.json({ limit: '100kb' }));
  app.use(rateLimit({ windowMs: 60_000, limit: 100, standardHeaders: 'draft-8', legacyHeaders: false }));
  if (!config.esProduccion) app.use(morgan('dev'));

  // ---- Rutas públicas ----
  app.get('/health', async (_req, res) => {
    const bdOk = await verificarBaseDatos().catch(() => false);
    res.status(bdOk ? 200 : 503).json({ estado: bdOk ? 'Healthy' : 'Unhealthy', servicio: 'notificaciones' });
  });

  // ---- Rutas protegidas con JWT ----
  app.use('/api/v1/notificaciones', autenticar(config.jwt));
  app.use('/api/v1/notificaciones/plantillas', crearRutasPlantillas(repositorios.plantillas));

  app.use(noEncontrado);
  app.use(manejadorErrores);

  return app;
}
