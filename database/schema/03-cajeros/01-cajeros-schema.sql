-- ===================================================
-- Esquema 03: Operaciones de Ventanilla y Cajeros
-- ===================================================

CREATE SCHEMA IF NOT EXISTS cajeros_bc;

CREATE TABLE IF NOT EXISTS cajeros_bc.agencias (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(20) UNIQUE NOT NULL,
    nombre VARCHAR(120) NOT NULL,
    direccion TEXT NOT NULL,
    telefono VARCHAR(20),
    ciudad VARCHAR(80) NOT NULL,
    estado VARCHAR(20) DEFAULT 'ACTIVA'
);

CREATE TABLE IF NOT EXISTS cajeros_bc.ventanillas (
    id SERIAL PRIMARY KEY,
    agencia_id INT NOT NULL REFERENCES cajeros_bc.agencias(id),
    numero_ventanilla INT NOT NULL,
    tipo VARCHAR(50) DEFAULT 'GENERAL', -- GENERAL, PREFERENCIAL, EMPRESARIAL
    estado VARCHAR(20) DEFAULT 'DISPONIBLE',
    UNIQUE (agencia_id, numero_ventanilla)
);

CREATE TABLE IF NOT EXISTS cajeros_bc.sesiones_caja (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ventanilla_id INT NOT NULL REFERENCES cajeros_bc.ventanillas(id),
    cajero_id UUID NOT NULL, -- Referencia lógica a auth_bc.usuarios
    fecha_apertura TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    fecha_cierre TIMESTAMP WITH TIME ZONE,
    saldo_inicial NUMERIC(14, 2) NOT NULL,
    saldo_final_teorico NUMERIC(14, 2),
    saldo_final_declarado NUMERIC(14, 2),
    diferencia NUMERIC(14, 2) DEFAULT 0,
    estado VARCHAR(20) DEFAULT 'ABIERTA', -- ABIERTA, ARQUEADA, CERRADA, CUADRADA, DESCUADRADA
    observaciones TEXT
);

CREATE TABLE IF NOT EXISTS cajeros_bc.arqueo_denominaciones (
    id SERIAL PRIMARY KEY,
    sesion_caja_id UUID NOT NULL REFERENCES cajeros_bc.sesiones_caja(id) ON DELETE CASCADE,
    tipo_dinero VARCHAR(10) NOT NULL, -- BILLETE, MONEDA
    denominacion NUMERIC(6, 2) NOT NULL, -- 100.00, 50.00, 20.00, 10.00, 5.00, 1.00, 0.50, 0.25, 0.10, 0.05, 0.01
    cantidad INT NOT NULL,
    subtotal NUMERIC(14, 2) GENERATED ALWAYS AS (denominacion * cantidad) STORED
);
