# ADR-0003: Flujo de Operaciones de Ventanilla y Arqueo Inmutable de Caja

- **Estado:** Aceptado
- **Fecha:** 2026-11-09
- **Autores:** Domain Architect, Lead Backend
- **Decisores:** Tech Lead, Oficial de Operaciones

---

## 1. Contexto

Las operaciones de ventanilla en instituciones bancarias y cooperativas exigen un estricto control de custodia de efectivo, prevención de fraudes y cuadre contable al centavo.

---

## 2. Decisión

El microservicio `BancoChillo.Cajeros` implementa un ciclo de vida estricto:
1. **Apertura de Jornada**:
   - Asignación de ventanilla física y registro de fondo inicial de sencillo entregado por la bóveda central.
   - Estado de caja: `ABIERTA`.
2. **Operaciones Transaccionales**:
   - Cada depósito, retiro o pago genera un movimiento atómico registrado con firma criptográfica de la sesión del cajero.
   - Verificación de límites de retiro y control de saldo físico en la gaveta del cajero.
3. **Arqueo y Cierre de Jornada**:
   - Desglose físico de efectivo por denominación (billetes de $100, $50, $20, $10, $5, $1 y monedas).
   - Comparación automática contra el saldo teórico del sistema contable.
   - En caso de descuadre (sobrante o faltante), se genera un reporte de incidencia que requiere visado y firma del Supervisor de Agencia.
   - Estado de caja: `CERRADA`.
4. **Inmutabilidad**:
   - Ningún registro de arqueo o movimiento de caja puede ser modificado ni eliminado. Las correcciones se realizan exclusivamente mediante transacciones de reverso auditadas.
