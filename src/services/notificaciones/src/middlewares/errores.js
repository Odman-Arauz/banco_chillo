import { ZodError } from 'zod';

/** Error con código HTTP. Lánzalo desde servicios/controladores: el manejador lo convierte en respuesta. */
export class ErrorHttp extends Error {
  constructor(estado, titulo, detalle, extras = {}) {
    super(detalle);
    this.estado = estado;
    this.titulo = titulo;
    this.extras = extras;
  }
}

export const noEncontrado = (req, _res, next) =>
  next(new ErrorHttp(404, 'Ruta no encontrada', `No existe ${req.method} ${req.originalUrl}`));

/**
 * Manejador global: responde SIEMPRE en formato RFC 7807 (ProblemDetails),
 * igual que los microservicios .NET, para que la PWA trate todos los errores igual.
 */
// Express reconoce un manejador de errores porque recibe 4 parámetros: no quites _next.
export const manejadorErrores = (err, req, res, _next) => {
  let error = err;

  if (err instanceof ZodError) {
    const errores = {};
    for (const issue of err.issues) {
      const campo = issue.path.join('.') || 'cuerpo';
      (errores[campo] ??= []).push(issue.message);
    }
    error = new ErrorHttp(400, 'Datos inválidos', 'Uno o más campos no son válidos.', { errores });
  } else if (err?.code === '23505') {
    // Violación de UNIQUE en PostgreSQL.
    error = new ErrorHttp(409, 'Conflicto', 'El registro ya existe.');
  } else if (!(err instanceof ErrorHttp)) {
    console.error('Error no controlado', err);
    error = new ErrorHttp(500, 'Error interno', 'Ocurrió un error inesperado. Intente nuevamente.');
  }

  res
    .status(error.estado)
    .type('application/problem+json')
    .json({ status: error.estado, title: error.titulo, detail: error.message, instance: req.originalUrl, ...error.extras });
};
