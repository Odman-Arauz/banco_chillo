# RN-001: Reglas de Negocio — Gestión de Cajeros y Ventanilla

---

## RN-001.1: Apertura de Caja
- Un cajero solo puede tener una caja en estado `ABIERTA` de manera simultánea en todo el sistema.
- La apertura requiere registrar el desglose del fondo inicial de sencillo entregado por bóveda.
- No se pueden realizar depósitos ni retiros si la caja no está en estado `ABIERTA`.

## RN-001.2: Retiros en Ventanilla
- Todo retiro exige validación de la identidad del titular (cédula de identidad ecuatoriana válida con algoritmo módulo 10).
- Si el monto del retiro supera el umbral de seguridad ($2,000 USD), el sistema exige autorización digital del Supervisor de Agencia.
- El monto a retirar no puede superar el saldo disponible de la cuenta del cliente ni el saldo físico disponible en la gaveta del cajero.

## RN-001.3: Depósitos en Efectivo
- El depósito se acredita de manera inmediata al saldo contable y disponible de la cuenta destino.
- Depósitos superiores a $5,000 USD requieren registrar la declaración de origen lícito de fondos (normativa UAFE).

## RN-001.4: Arqueo y Cierre Diario de Caja
- Al finalizar la jornada, el cajero debe contar físicamente su gaveta e ingresar la cantidad de billetes y monedas por denominación.
- El sistema compara `Saldo Físico Declarado` vs `Saldo Teórico del Sistema`.
- Si `Diferencia == 0`: Cierre balanceado automático.
- Si `Diferencia != 0`: Se registra el descuadre clasificado como `SOBRANTE` o `FALTANTE`, emitiendo una alerta inmediata a la gerencia de operaciones y exigiendo justificación obligatoria.
