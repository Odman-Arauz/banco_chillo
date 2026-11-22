import { Router } from 'express';
import { autorizar } from '../../middlewares/autenticacion.js';
import { ErrorHttp } from '../../middlewares/errores.js';
import { actualizarPlantillaEsquema, crearPlantillaEsquema, idEsquema } from './plantillas.esquemas.js';

const ROLES_ADMIN = ['ROLE_SUPERADMIN', 'ROLE_ADMIN'];

/**
 * CRUD de plantillas de notificación — EJEMPLO DE REFERENCIA Node.js (Partes 4 a 9 y 11 a 14 del práctico).
 * Express 5 propaga automáticamente los errores de funciones async al manejador global.
 */
export function crearRutasPlantillas(repositorio) {
  const rutas = Router();

  const buscarOFallar = async (idCrudo) => {
    const id = idEsquema.parse(idCrudo);
    const plantilla = await repositorio.obtenerPorId(id);
    if (!plantilla) throw new ErrorHttp(404, 'Recurso no encontrado', `Plantilla con identificador '${id}' no existe.`);
    return plantilla;
  };

  // GET /api/v1/notificaciones/plantillas?incluirInactivas=true
  rutas.get('/', async (req, res) => {
    res.json(await repositorio.listar({ incluirInactivas: req.query.incluirInactivas === 'true' }));
  });

  // GET /api/v1/notificaciones/plantillas/:id
  rutas.get('/:id', async (req, res) => {
    res.json(await buscarOFallar(req.params.id));
  });

  // POST /api/v1/notificaciones/plantillas
  rutas.post('/', autorizar(...ROLES_ADMIN), async (req, res) => {
    const datos = crearPlantillaEsquema.parse(req.body);
    const creada = await repositorio.crear(datos);
    res.status(201).location(`${req.baseUrl}/${creada.id}`).json(creada);
  });

  // PUT /api/v1/notificaciones/plantillas/:id
  rutas.put('/:id', autorizar(...ROLES_ADMIN), async (req, res) => {
    const { id } = await buscarOFallar(req.params.id);
    const datos = actualizarPlantillaEsquema.parse(req.body);
    res.json(await repositorio.actualizar(id, datos));
  });

  // DELETE /api/v1/notificaciones/plantillas/:id  → baja lógica (activa = false)
  rutas.delete('/:id', autorizar(...ROLES_ADMIN), async (req, res) => {
    const { id } = await buscarOFallar(req.params.id);
    await repositorio.desactivar(id);
    res.status(204).end();
  });

  return rutas;
}
