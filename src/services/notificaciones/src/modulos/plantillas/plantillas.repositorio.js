// Acceso a datos con SQL parametrizado ($1, $2...). NUNCA concatenar valores del usuario en el SQL.

const COLUMNAS = 'id, codigo, canal, asunto, cuerpo, activa, creado_en AS "creadoEn", actualizado_en AS "actualizadoEn"';

export const crearRepositorioPlantillas = (pool) => ({
  async listar({ incluirInactivas }) {
    const { rows } = await pool.query(
      `SELECT ${COLUMNAS} FROM notificaciones_bc.plantillas
       WHERE ($1::boolean OR activa) ORDER BY codigo`,
      [incluirInactivas],
    );
    return rows;
  },

  async obtenerPorId(id) {
    const { rows } = await pool.query(`SELECT ${COLUMNAS} FROM notificaciones_bc.plantillas WHERE id = $1`, [id]);
    return rows[0] ?? null;
  },

  async crear({ codigo, canal, asunto, cuerpo }) {
    const { rows } = await pool.query(
      `INSERT INTO notificaciones_bc.plantillas (codigo, canal, asunto, cuerpo)
       VALUES ($1, $2, $3, $4) RETURNING ${COLUMNAS}`,
      [codigo, canal, asunto, cuerpo],
    );
    return rows[0];
  },

  async actualizar(id, { canal, asunto, cuerpo, activa }) {
    const { rows } = await pool.query(
      `UPDATE notificaciones_bc.plantillas
       SET canal = $2, asunto = $3, cuerpo = $4, activa = COALESCE($5, activa), actualizado_en = NOW()
       WHERE id = $1 RETURNING ${COLUMNAS}`,
      [id, canal, asunto, cuerpo, activa ?? null],
    );
    return rows[0] ?? null;
  },

  async desactivar(id) {
    const { rowCount } = await pool.query(
      'UPDATE notificaciones_bc.plantillas SET activa = FALSE, actualizado_en = NOW() WHERE id = $1',
      [id],
    );
    return rowCount > 0;
  },
});
