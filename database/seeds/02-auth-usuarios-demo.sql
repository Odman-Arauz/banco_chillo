-- =====================================================================
-- Semillas: Usuarios Ficticios de Prueba por Rol (auth_bc)
-- 
-- Datos estrictamente ficticios según Regla de Oro 1 de AGENTS.md.
-- Cédulas calculadas con algoritmo Módulo 10 ecuatoriano usando provincia de prueba '30'.
-- Contraseña unificada: 'Demo#2026'
-- Hash BCrypt: '$2a$12$8IFwAuRD1z4tChTG5lzowugGet7uxQntd9mbVJW/4CYTzOzadEe8S'
-- =====================================================================

INSERT INTO auth_bc.usuarios (
    identificacion,
    nombres,
    apellidos,
    correo,
    telefono,
    password_hash,
    mfa_habilitado,
    estado,
    intentos_fallidos
) VALUES
-- 1. Super Administrador
('3000000012', 'Superadmin Demo', 'Uno', 'superadmin@bancochillo.test', '0990000001', '$2a$12$8IFwAuRD1z4tChTG5lzowugGet7uxQntd9mbVJW/4CYTzOzadEe8S', FALSE, 'ACTIVO', 0),
-- 2. Administrador de Agencia
('3000000020', 'Admin Demo', 'Uno', 'admin@bancochillo.test', '0990000002', '$2a$12$8IFwAuRD1z4tChTG5lzowugGet7uxQntd9mbVJW/4CYTzOzadEe8S', FALSE, 'ACTIVO', 0),
-- 3. Supervisor de Ventanilla
('3000000038', 'Supervisor Demo', 'Uno', 'supervisor@bancochillo.test', '0990000003', '$2a$12$8IFwAuRD1z4tChTG5lzowugGet7uxQntd9mbVJW/4CYTzOzadEe8S', FALSE, 'ACTIVO', 0),
-- 4. Cajero de Ventanilla
('3000000046', 'Cajero Demo', 'Uno', 'cajero@bancochillo.test', '0990000004', '$2a$12$8IFwAuRD1z4tChTG5lzowugGet7uxQntd9mbVJW/4CYTzOzadEe8S', FALSE, 'ACTIVO', 0),
-- 5. Oficial de Crédito
('3000000053', 'Oficial Credito Demo', 'Uno', 'credito@bancochillo.test', '0990000005', '$2a$12$8IFwAuRD1z4tChTG5lzowugGet7uxQntd9mbVJW/4CYTzOzadEe8S', FALSE, 'ACTIVO', 0),
-- 6. Socio / Cliente
('3000000061', 'Socio Demo', 'Uno', 'socio@bancochillo.test', '0990000006', '$2a$12$8IFwAuRD1z4tChTG5lzowugGet7uxQntd9mbVJW/4CYTzOzadEe8S', FALSE, 'ACTIVO', 0),
-- 7. Auditor de Cumplimiento
('3000000079', 'Auditor Demo', 'Uno', 'auditor@bancochillo.test', '0990000007', '$2a$12$8IFwAuRD1z4tChTG5lzowugGet7uxQntd9mbVJW/4CYTzOzadEe8S', FALSE, 'ACTIVO', 0)
ON CONFLICT (identificacion) DO NOTHING;

-- Asignación de Roles correspondientes a cada usuario ficticio
INSERT INTO auth_bc.usuario_roles (usuario_id, rol_id)
SELECT u.id, r.id
FROM auth_bc.usuarios u
JOIN auth_bc.roles r ON (
    (u.identificacion = '3000000012' AND r.codigo = 'ROLE_SUPERADMIN') OR
    (u.identificacion = '3000000020' AND r.codigo = 'ROLE_ADMIN') OR
    (u.identificacion = '3000000038' AND r.codigo = 'ROLE_SUPERVISOR_CAJA') OR
    (u.identificacion = '3000000046' AND r.codigo = 'ROLE_CAJERO') OR
    (u.identificacion = '3000000053' AND r.codigo = 'ROLE_OFICIAL_CREDITO') OR
    (u.identificacion = '3000000061' AND r.codigo = 'ROLE_SOCIO') OR
    (u.identificacion = '3000000079' AND r.codigo = 'ROLE_AUDITOR')
)
ON CONFLICT (usuario_id, rol_id) DO NOTHING;
