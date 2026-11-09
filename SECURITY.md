# Política de Seguridad — Banco Chillo

En **Banco Chillo**, la seguridad de los activos financieros y la privacidad de la información personal de nuestros usuarios y funcionarios constituyen una prioridad crítica.

---

## 1. Reporte de Vulnerabilidades

Si descubres una vulnerabilidad potencial de seguridad en este proyecto:
- **NO** abras un Issue público en GitHub.
- Envía un reporte detallado a través de la pestaña de **Security Advisories** de GitHub del repositorio (`Security > Report a vulnerability`).
- Incluye:
  - Descripción técnica del vector de ataque o vulnerabilidad.
  - Pasos exactos para reproducir la falla (PoC con datos sintéticos).
  - Componentes o microservicios afectados.

---

## 2. Controles de Seguridad Obligatorios

1. **Cero Secretos en Repositorio**:
   - Todo commit se analiza localmente y en el pipeline de GitHub Actions mediante `gitleaks`.
2. **Protección de Datos Personales (LOPDP Ecuador)**:
   - Los datos de socios (cédulas, correos, teléfonos, direcciones) deben ser encriptados en reposo y anonimizados en entornos de desarrollo y pruebas.
3. **MFA y Sesiones**:
   - Las sesiones administrativas cuentan con un tiempo de expiración por inactividad de máximo 5 minutos.
   - Todo cambio de credencial o transacción sobre el umbral seguro exige verificación de segundo factor (TOTP / OTP).
4. **Idempotencia Transaccional**:
   - Todos los endpoints de débito, crédito o transferencia requieren un encabezado `Idempotency-Key` (UUIDv4) para prevenir transacciones duplicadas por fallos de red.
