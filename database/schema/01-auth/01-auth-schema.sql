-- ===================================================
-- Esquema 01: Autenticación, Roles y Seguridad (Auth)
-- ===================================================

CREATE SCHEMA IF NOT EXISTS auth_bc;

CREATE TABLE IF NOT EXISTS auth_bc.roles (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) UNIQUE NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT,
    creado_en TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS auth_bc.usuarios (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    identificacion VARCHAR(13) UNIQUE NOT NULL, -- Cédula o RUC
    nombres VARCHAR(150) NOT NULL,
    apellidos VARCHAR(150) NOT NULL,
    correo VARCHAR(150) UNIQUE NOT NULL,
    telefono VARCHAR(20),
    password_hash VARCHAR(255) NOT NULL,
    mfa_habilitado BOOLEAN DEFAULT FALSE,
    mfa_secreto VARCHAR(100),
    estado VARCHAR(20) DEFAULT 'ACTIVO', -- ACTIVO, BLOQUEADO, INACTIVO
    intentos_fallidos INT DEFAULT 0,
    ultimo_acceso TIMESTAMP WITH TIME ZONE,
    creado_en TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    actualizado_en TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS auth_bc.usuario_roles (
    usuario_id UUID NOT NULL REFERENCES auth_bc.usuarios(id) ON DELETE CASCADE,
    rol_id INT NOT NULL REFERENCES auth_bc.roles(id) ON DELETE CASCADE,
    PRIMARY KEY (usuario_id, rol_id)
);
