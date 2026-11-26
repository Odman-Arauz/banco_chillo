# Consolidación de Mockups — Banco Chillo (PWA)

> **Épica:** feat: interfaz pwa responsiva en react native para ventanilla y administracion #7  
> **Issue:** #28  
> **Responsable:** @S0yRai (Diseño y Frontend PWA)  
> **Guía de Estilo Oficial:** [guia-de-estilo.md](./guia-de-estilo.md)  
> **Proyecto en Figma (Prototipo Navegable):** [Enlace al Prototipo en Figma](https://www.figma.com/) *(actualizar con el enlace público)*  
> **Proyecto en Google Stitch:** [Enlace a Google Stitch](https://stitch.withgoogle.com/) *(actualizar con el enlace del proyecto)*  

---

## 1. Índice de Pantallas por Módulo

| Módulo | Pantallas Requeridas | Formato | Responsable | Estado | Carpeta de Archivos |
|---|---|---|---|---|---|
| **01 · Guía de Estilo** | Tokens, colores WCAG AA, tipografía, botones, inputs, cards | Lámina / PNG | @S0yRai | ✅ Completada | [`docs/mockups/`](./guia-de-estilo.md) |
| **02 · Administración (PWA)** | Layout roles, Dashboard, CRUD Agencias, Ventanillas, Modo Offline, Estados | Web 1440px + Móvil | @S0yRai | 🎨 En diseño | [`docs/mockups/admin/`](./admin/) |
| **03 · Autenticación** | Login, Verificación MFA (6 dígitos), Bloqueo de cuenta | App + Web | @Odman-Arauz | ⏳ Pendiente entrega (Vie 27) | [`docs/mockups/auth/`](./auth/) |
| **04 · Banca Personal (Socio)** | Posición consolidada, Detalle de movimientos, Transferencia (3 pasos) | App (Móvil) | @MathiasAlex2005 | ⏳ Pendiente entrega (Vie 27) | [`docs/mockups/banca-personal/`](./banca-personal/) |
| **05 · Cajeros y Ventanilla** | Apertura de caja, Depósito/Retiro, Arqueo y cierre inmutable | Web 1440px | @jostin1224jp / @MateoPupiales37 | ⏳ Pendiente entrega (Vie 27) | [`docs/mockups/cajeros/`](./cajeros/) |
| **06 · Créditos** | Simulador (francesa/alemana), Tabla de amortización | App + Web | @jostin1224jp | ⏳ Pendiente entrega (Vie 27) | [`docs/mockups/creditos/`](./creditos/) |
| **07 · Auditoría y Notificaciones** | Bitácora de eventos con filtros, Preferencias de notificación push/email | Web / App | @josuebj72-ai / @MateoPupiales37 | ⏳ Pendiente entrega (Vie 27) | [`docs/mockups/notificaciones/`](./notificaciones/) |

---

## 2. Galería de Pantallas Propias de Administración (PWA)

### 2.1 Layout General por Rol
- **Escritorio (Admin / Cajero):** Barra lateral izquierda (sidebar) colapsable con fondo `#1E293B`, opciones de navegación con iconos lineales, encabezado superior con perfil de usuario y estado del sistema.
- **Móvil (Socio):** Barra de navegación inferior (Bottom Navigation Bar) con 4 accesos directos (Inicio, Cuentas, Transferir, Perfil).

### 2.2 Dashboard de Administración
- Resumen ejecutivo con tarjetas métricas (KPIs): Agencias Activas, Ventanillas Operando, Transacciones del Día, Volumen Total Transaccionado ($ USD).
- Gráfico de transacciones por hora y listado de actividad reciente.

### 2.3 Gestión de Agencias (Campos reales de `AgenciaDto`)
- **Lista:** Tabla con columnas Código (`Codigo`), Nombre (`Nombre`), Dirección (`Direccion`), Teléfono (`Telefono`), Ciudad (`Ciudad`), Estado (`Estado`), Acciones.
- **Formulario Crear/Editar:** Modal / Drawer con validaciones en tiempo real para todos los campos de `AgenciaDto`.

### 2.4 Gestión de Ventanillas por Agencia
- Vista maestro-detalle: Selección de agencia y visualización de ventanillas asociadas (Ventanilla 1, Ventanilla 2, etc.), cajero asignado, estado (Abierta, Cerrada, En Pausa).

### 2.5 Modo Offline ("Sin Conexión")
- Detección de pérdida de red en la PWA.
- Banner informativo superior amarillo (`#F59E0B`), mensaje central con icono de desconexión, catálogo local disponible y bloqueo preventivo de operaciones que requieren autorización online.

### 2.6 Estados Genéricos del Sistema
- **Vacío (Empty State):** "No hay agencias registradas" con ilustración lineal y botón de acción principal.
- **Cargando (Loading State):** Skeleton loaders replicando la estructura de las tarjetas y tablas.
- **Error:** Tarjeta de error con código RFC 7807, descripción clara y botón "Reintentar".

---

## 3. Recorridos del Prototipo Interactivo en Figma

El prototipo cuenta con dos recorridos interactivos completos:

### Recorrido 1: Experiencia del Socio (Móvil)
1. **Inicio de sesión:** Ingreso de identificación y contraseña.
2. **Segundo factor (MFA):** Pantalla de ingreso de código de seguridad de 6 dígitos.
3. **Posición consolidada:** Visualización de cuentas de ahorro, saldo disponible y últimos movimientos.
4. **Transferencia bancaria:** Flujo de 3 pasos (Datos de destino → Monto y concepto → Confirmación y comprobante).

### Recorrido 2: Operatoria del Cajero (Web Escritorio 1440px)
1. **Inicio de sesión de cajero:** Validación de credenciales y asignación a ventanilla.
2. **Apertura de caja:** Conteo inicial de denominaciones (billetes y monedas) y confirmación de monto de apertura.
3. **Operación de depósito:** Registro de depósito en efectivo con desglose y generación de comprobante.
4. **Arqueo y Cierre de jornada:** Comparativa de saldo físico vs. saldo del sistema con indicadores de caja cuadrada (verde) o descuadre (rojo con desglose).
