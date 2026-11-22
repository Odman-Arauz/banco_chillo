import { GATEWAY_URL, verificarGateway } from '../salud';

// Ejemplo de prueba unitaria con Jest: se reemplaza fetch por una función falsa.
describe('verificarGateway', () => {
  it('quita /api/v1 de la URL base para llegar a /health', () => {
    expect(GATEWAY_URL.endsWith('/api/v1')).toBe(false);
  });

  it('devuelve "en-linea" cuando /health responde 200', async () => {
    const fetchFalso = jest.fn().mockResolvedValue({ ok: true });
    await expect(verificarGateway(fetchFalso)).resolves.toBe('en-linea');
    expect(fetchFalso).toHaveBeenCalledWith(`${GATEWAY_URL}/health`);
  });

  it('devuelve "sin-conexion" cuando la red falla', async () => {
    const fetchFalso = jest.fn().mockRejectedValue(new Error('red caída'));
    await expect(verificarGateway(fetchFalso)).resolves.toBe('sin-conexion');
  });
});
