-- ===================================================
-- Semillas: Roles y Catálogos Financieros Iniciales
-- ===================================================

INSERT INTO auth_bc.roles (codigo, nombre, descripcion) VALUES
('ROLE_SUPERADMIN', 'Super Administrador', 'Acceso total a la configuración y auditoría'),
('ROLE_ADMIN', 'Administrador de Agencia', 'Gestión de personal administrativo y agencias'),
('ROLE_SUPERVISOR_CAJA', 'Supervisor de Ventanilla', 'Autorización de retiros altos y visado de arqueos'),
('ROLE_CAJERO', 'Cajero de Ventanilla', 'Operación diaria de caja, depósitos y retiros'),
('ROLE_OFICIAL_CREDITO', 'Oficial de Crédito', 'Evaluación y aprobación de préstamos'),
('ROLE_SOCIO', 'Socio / Cliente', 'Acceso a banca personal web y móvil')
ON CONFLICT (codigo) DO NOTHING;

INSERT INTO cajeros_bc.agencias (codigo, nombre, direccion, ciudad) VALUES
('AG-001', 'Agencia Matriz Los Chillos', 'Av. General Rumiñahui y San Luis', 'Sangolquí'),
('AG-002', 'Agencia Valle Centro', 'Av. Ilaló y González Suárez', 'Tumbaco')
ON CONFLICT (codigo) DO NOTHING;
