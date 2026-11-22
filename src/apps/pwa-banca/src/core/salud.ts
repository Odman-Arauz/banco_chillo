// ====================================================
// Verificación de conectividad con el API Gateway
// ====================================================

import { API_BASE_URL } from './api';

export type EstadoGateway = 'verificando' | 'en-linea' | 'sin-conexion';

/** URL raíz del gateway (sin /api/v1), donde vive /health. */
export const GATEWAY_URL = API_BASE_URL.replace(/\/api\/v1\/?$/, '');

export async function verificarGateway(fetcher: typeof fetch = fetch): Promise<EstadoGateway> {
  try {
    const respuesta = await fetcher(`${GATEWAY_URL}/health`);
    return respuesta.ok ? 'en-linea' : 'sin-conexion';
  } catch {
    return 'sin-conexion';
  }
}
