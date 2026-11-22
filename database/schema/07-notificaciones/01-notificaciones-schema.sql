-- ===================================================
-- Esquema 07: Notificaciones (microservicio Node.js + Express)
-- Base de referencia: tabla de plantillas usada por el CRUD de ejemplo.
-- El responsable del Issue de Notificaciones agrega aquí preferencias y bitácora de envíos.
-- ===================================================

CREATE SCHEMA IF NOT EXISTS notificaciones_bc;

CREATE TABLE IF NOT EXISTS notificaciones_bc.plantillas (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) UNIQUE NOT NULL,          -- ej. DEPOSITO_REALIZADO
    canal VARCHAR(10) NOT NULL CHECK (canal IN ('EMAIL', 'SMS', 'PUSH')),
    asunto VARCHAR(150) NOT NULL,
    cuerpo TEXT NOT NULL,                        -- admite variables {{monto}}, {{cuenta}}, {{fecha}}
    activa BOOLEAN NOT NULL DEFAULT TRUE,
    creado_en TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    actualizado_en TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO notificaciones_bc.plantillas (codigo, canal, asunto, cuerpo) VALUES
('DEPOSITO_REALIZADO', 'EMAIL', 'Depósito recibido',
 'Banco Chillo le informa: depósito de ${{monto}} en su cuenta terminada en {{cuenta}} el {{fecha}}.'),
('RETIRO_REALIZADO', 'SMS', 'Retiro realizado',
 'Banco Chillo: retiro de ${{monto}} en cuenta ***{{cuenta}} el {{fecha}}. Si no lo reconoce llame al 1800-000-000.')
ON CONFLICT (codigo) DO NOTHING;
