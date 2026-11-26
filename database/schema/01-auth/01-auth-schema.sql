-- ===================================================
-- Esquema 01: Autenticación, Roles y Seguridad (Auth)
-- ===================================================

CREATE SCHEMA IF NOT EXISTS auth_bc;

-- 1. Tabla de Roles del Sistema
CREATE TABLE IF NOT EXISTS auth_bc.roles (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) UNIQUE NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT,
    creado_en TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 2. Tabla de Usuarios y Cuentas Bancarias
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
    estado VARCHAR(20) DEFAULT 'ACTIVO' CHECK (estado IN ('ACTIVO', 'BLOQUEADO', 'INACTIVO')),
    intentos_fallidos INT DEFAULT 0,
    bloqueado_hasta TIMESTAMP WITH TIME ZONE,
    ultimo_acceso TIMESTAMP WITH TIME ZONE,
    creado_en TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    actualizado_en TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 3. Tabla Relacional de Asignación de Roles (N:M)
CREATE TABLE IF NOT EXISTS auth_bc.usuario_roles (
    usuario_id UUID NOT NULL REFERENCES auth_bc.usuarios(id) ON DELETE CASCADE,
    rol_id INT NOT NULL REFERENCES auth_bc.roles(id) ON DELETE CASCADE,
    PRIMARY KEY (usuario_id, rol_id)
);

-- 4. Tabla de Tokens de Refresco (Sesiones persistentes)
CREATE TABLE IF NOT EXISTS auth_bc.refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    usuario_id UUID NOT NULL REFERENCES auth_bc.usuarios(id) ON DELETE CASCADE,
    token_hash VARCHAR(128) UNIQUE NOT NULL, -- Resumen SHA-256 (nunca en texto plano)
    expira_en TIMESTAMP WITH TIME ZONE NOT NULL,
    revocado_en TIMESTAMP WITH TIME ZONE,
    ip_origen VARCHAR(45),
    agente_usuario TEXT,
    creado_en TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 5. Tabla Bitácora de Intentos de Acceso (Append-only)
CREATE TABLE IF NOT EXISTS auth_bc.intentos_acceso (
    id BIGSERIAL PRIMARY KEY,
    usuario_id UUID REFERENCES auth_bc.usuarios(id) ON DELETE SET NULL,
    identificacion_ingresada VARCHAR(13) NOT NULL,
    exitoso BOOLEAN NOT NULL,
    motivo VARCHAR(40) NOT NULL, -- OK, CLAVE_INCORRECTA, USUARIO_BLOQUEADO, USUARIO_INEXISTENTE, etc.
    ip_origen VARCHAR(45),
    creado_en TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índices de Rendimiento y Búsqueda
CREATE INDEX IF NOT EXISTS idx_intentos_acceso_usuario_creado 
    ON auth_bc.intentos_acceso(usuario_id, creado_en DESC);

CREATE INDEX IF NOT EXISTS idx_refresh_tokens_usuario 
    ON auth_bc.refresh_tokens(usuario_id);

-- Función y Trigger para Garantizar Inmutabilidad (Append-only)
CREATE OR REPLACE FUNCTION auth_bc.impedir_modificacion()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'La tabla %.% es inmutable (append-only): no se permite %',
        TG_TABLE_SCHEMA, TG_TABLE_NAME, TG_OP;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_intentos_acceso_inmutables ON auth_bc.intentos_acceso;
CREATE TRIGGER trg_intentos_acceso_inmutables
    BEFORE UPDATE OR DELETE ON auth_bc.intentos_acceso
    FOR EACH STATEMENT EXECUTE FUNCTION auth_bc.impedir_modificacion();
