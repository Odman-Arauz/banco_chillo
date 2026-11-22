import jwt from 'jsonwebtoken';
import { ErrorHttp } from './errores.js';

/**
 * Valida el token "Authorization: Bearer <jwt>" emitido por el microservicio Auth.
 * Deja el usuario en req.usuario = { id, identificacion, roles }.
 */
export const autenticar = (opcionesJwt) => (req, _res, next) => {
  const [esquema, token] = (req.headers.authorization ?? '').split(' ');
  if (esquema !== 'Bearer' || !token) {
    return next(new ErrorHttp(401, 'No autenticado', 'Falta el token Bearer.'));
  }

  try {
    const datos = jwt.verify(token, opcionesJwt.secreto, {
      issuer: opcionesJwt.emisor,
      audience: opcionesJwt.audiencia,
      algorithms: ['HS256'],
    });
    // El servicio Auth (.NET) emite los roles en el claim "role" (texto o arreglo).
    const roles = [].concat(datos.role ?? []);
    req.usuario = { id: datos.sub, identificacion: datos.identificacion, roles };
    return next();
  } catch {
    return next(new ErrorHttp(401, 'No autenticado', 'Token inválido o expirado.'));
  }
};

/** Exige que el usuario tenga al menos uno de los roles indicados. */
export const autorizar = (...rolesPermitidos) => (req, _res, next) => {
  const tieneRol = req.usuario?.roles.some((r) => rolesPermitidos.includes(r));
  return tieneRol ? next() : next(new ErrorHttp(403, 'Prohibido', 'Su rol no permite esta operación.'));
};
