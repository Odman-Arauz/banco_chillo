import { crearApp } from './app.js';
import { entorno } from './config/entorno.js';
import { obtenerPool } from './db/pool.js';
import { crearRepositorioPlantillas } from './modulos/plantillas/plantillas.repositorio.js';

const pool = obtenerPool(entorno.databaseUrl);

const app = crearApp({
  config: entorno,
  repositorios: { plantillas: crearRepositorioPlantillas(pool) },
  verificarBaseDatos: async () => (await pool.query('SELECT 1')).rowCount === 1,
});

app.listen(entorno.puerto, () => {
  console.log(`Notificaciones escuchando en http://localhost:${entorno.puerto} (health: /health)`);
});
