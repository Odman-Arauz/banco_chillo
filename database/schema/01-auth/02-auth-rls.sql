-- =====================================================================
-- Esquema 01: Seguridad a Nivel de Fila (RLS) en auth_bc
-- ADR-0005 y ADR-0007: Supabase y PostgreSQL
-- =====================================================================

ALTER TABLE auth_bc.roles ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth_bc.usuarios ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth_bc.usuario_roles ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth_bc.refresh_tokens ENABLE ROW LEVEL SECURITY;
ALTER TABLE auth_bc.intentos_acceso ENABLE ROW LEVEL SECURITY;
