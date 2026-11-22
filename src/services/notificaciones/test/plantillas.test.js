import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import jwt from 'jsonwebtoken';
import request from 'supertest';
import { crearApp } from '../src/app.js';

// Pruebas con el runner nativo de Node (node --test) y supertest. No necesitan base de datos:
// se inyecta un repositorio en memoria.

const config = {
  esProduccion: true,
  corsOrigenes: ['http://localhost:8081'],
  jwt: { secreto: 'test_secreto_de_pruebas_con_mas_de_32_caracteres', emisor: 'BancoChillo.Auth', audiencia: 'BancoChillo.Clientes' },
};

const tokenCon = (rol) =>
  jwt.sign({ sub: 'usuario-prueba', role: rol }, config.jwt.secreto, {
    issuer: config.jwt.emisor,
    audience: config.jwt.audiencia,
    expiresIn: '5m',
  });

const repositorioEnMemoria = () => {
  const datos = [];
  return {
    listar: async ({ incluirInactivas }) => datos.filter((p) => incluirInactivas || p.activa),
    obtenerPorId: async (id) => datos.find((p) => p.id === id) ?? null,
    crear: async (p) => {
      const nueva = { id: datos.length + 1, activa: true, ...p };
      datos.push(nueva);
      return nueva;
    },
    actualizar: async (id, cambios) => Object.assign(datos.find((p) => p.id === id), cambios),
    desactivar: async (id) => {
      datos.find((p) => p.id === id).activa = false;
      return true;
    },
  };
};

const plantillaValida = { codigo: 'DEPOSITO_PRUEBA', canal: 'EMAIL', asunto: 'Depósito', cuerpo: 'Depósito de {{monto}}' };

describe('CRUD /api/v1/notificaciones/plantillas', () => {
  let app;
  beforeEach(() => {
    app = crearApp({ config, repositorios: { plantillas: repositorioEnMemoria() } });
  });

  it('rechaza peticiones sin token con 401', async () => {
    const res = await request(app).get('/api/v1/notificaciones/plantillas');
    assert.equal(res.status, 401);
  });

  it('un cajero no puede crear plantillas (403)', async () => {
    const res = await request(app)
      .post('/api/v1/notificaciones/plantillas')
      .set('Authorization', `Bearer ${tokenCon('ROLE_CAJERO')}`)
      .send(plantillaValida);
    assert.equal(res.status, 403);
  });

  it('un administrador crea, consulta y desactiva una plantilla', async () => {
    const auth = `Bearer ${tokenCon('ROLE_ADMIN')}`;

    const creada = await request(app).post('/api/v1/notificaciones/plantillas').set('Authorization', auth).send(plantillaValida);
    assert.equal(creada.status, 201);

    const obtenida = await request(app).get(`/api/v1/notificaciones/plantillas/${creada.body.id}`).set('Authorization', auth);
    assert.equal(obtenida.body.codigo, 'DEPOSITO_PRUEBA');

    const borrada = await request(app).delete(`/api/v1/notificaciones/plantillas/${creada.body.id}`).set('Authorization', auth);
    assert.equal(borrada.status, 204);

    const activas = await request(app).get('/api/v1/notificaciones/plantillas').set('Authorization', auth);
    assert.equal(activas.body.length, 0);
  });

  it('valida los datos de entrada y responde 400 con el detalle por campo', async () => {
    const res = await request(app)
      .post('/api/v1/notificaciones/plantillas')
      .set('Authorization', `Bearer ${tokenCon('ROLE_ADMIN')}`)
      .send({ ...plantillaValida, canal: 'FAX', codigo: 'minusculas' });
    assert.equal(res.status, 400);
    assert.ok(res.body.errores.canal);
    assert.ok(res.body.errores.codigo);
  });

  it('responde 404 en formato ProblemDetails si la plantilla no existe', async () => {
    const res = await request(app)
      .get('/api/v1/notificaciones/plantillas/999')
      .set('Authorization', `Bearer ${tokenCon('ROLE_ADMIN')}`);
    assert.equal(res.status, 404);
    assert.equal(res.body.status, 404);
  });
});
