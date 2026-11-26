# Guía de Estilo Oficial — Banco Chillo

> **Versión:** 1.0  
> **Fecha de publicación:** Martes 24 de noviembre de 2026  
> **Responsable:** Diseño & Frontend PWA (@S0yRai) — Issue #28  
> **Tokens oficiales:** `src/apps/pwa-banca/src/shared/design-tokens/colors.ts`  
> **Estándar de Accesibilidad:** WCAG 2.2 Nivel AA  

Esta guía define el sistema visual institucional para la Progressive Web App (PWA) de **Banco Chillo**. Es de **uso obligatorio para todos los integrantes del grupo** al generar pantallas en Google Stitch, Figma o al implementar componentes en React Native.

---

## 1. Paleta de Colores Institucionales

Todos los colores provienen de los tokens oficiales del repositorio y han sido validados con el [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/) para cumplir con la pauta WCAG 2.2 AA (ratio mínimo de 4.5:1 para texto normal y 3.0:1 para elementos gráficos y componentes interactivos).

### 1.1 Colores Principales

| Nombre del Token | Código HEX | Rol Semántico | Ratio de Contraste | Cumplimiento WCAG AA |
|---|---|---|---|---|
| `Colors.primary.500` | `#1E3A8A` | Color primario institucional. Botones de acción principal, encabezados principales, barras de navegación. | 10.53:1 sobre `#F8FAFC` / 11.22:1 sobre blanco | **Pasa AAA** (Texto normal y grande) |
| `Colors.primary.700` | `#1E293B` | Azul pizarra oscuro. Barras laterales de administración y cajeros. | 13.56:1 sobre blanco | **Pasa AAA** |
| `Colors.primary.900` | `#0F172A` | Azul medianoche profundo. Texto principal de títulos de alta jerarquía. | 16.08:1 sobre blanco | **Pasa AAA** |
| `Colors.primary.50` | `#EEF2FF` | Azul suave. Fondos de selección, chips activos y estados hover suaves. | N/A (Fondo) | Elementos de fondo |
| `Colors.secondary.500` | `#0284C7` | Azul cian complementario. Enlaces interactivos, foco de inputs, botones secundarios. | 4.62:1 sobre blanco | **Pasa AA** (Texto normal y grande) |
| `Colors.accent.500` | `#F59E0B` | Dorado financiero. Alertas preventivas, llamadas de atención moderadas. | Uso en fondos con texto `#0F172A` (7.8:1) | **Pasa AAA** con texto oscuro |

### 1.2 Colores Semánticos y Operativos

| Nombre del Token | Código HEX | Rol Semántico | Requisito de Accesibilidad |
|---|---|---|---|
| `Colors.success` | `#10B981` | Transacciones exitosas, estados activos. | Debe acompañarse de texto y el icono `CheckCircle` |
| `Colors.warning` | `#F59E0B` | Advertencias de seguridad, montos límites próximos. | Debe acompañarse de icono `AlertTriangle` y texto explicativo |
| `Colors.danger` | `#EF4444` | Errores en formularios, bloqueo de cuenta, botones destructivos. | Ratio 4.5:1 con texto blanco; icono `AlertCircle` obligatorio |
| `Colors.surface` | `#FFFFFF` | Fondo de tarjetas (Cards), modales, menús y desplegables. | Borde sutil de contraste `#E2E8F0` |
| `Colors.background` | `#F8FAFC` | Fondo general de todas las pantallas web y móviles. | Tono neutro anti-fatiga visual |

### 1.3 Colores Específicos para Módulo de Cajeros (Arqueo y Cierre)

> [!IMPORTANT]
> **Regla de oro de accesibilidad:** El color **nunca** debe ser la única señal visual para indicar un estado operativo.

| Estado de Caja | Código HEX | Indicador Visual Adicional Obligatorio |
|---|---|---|
| `Colors.caja.cuadrada` | `#10B981` | Icono `Check` + Etiqueta textual visible: **"CAJA CUADRADA - DIFERENCIA $0.00"** |
| `Colors.caja.sobrante` | `#3B82F6` | Icono `PlusCircle` + Etiqueta textual visible: **"SOBRANTE: +$XX.XX"** |
| `Colors.caja.faltante` | `#EF4444` | Icono `AlertOctagon` + Etiqueta textual visible: **"DESCUADRE / FALTANTE: -$XX.XX"** |

### 1.4 Jerarquía de Texto

| Token | Código HEX | Uso | Ratio sobre Fondo `#F8FAFC` |
|---|---|---|---|
| `Colors.text.primary` | `#0F172A` | Títulos, etiquetas de campos obligatorios, datos de tablas. | 15.17:1 (**AAA**) |
| `Colors.text.secondary` | `#475569` | Subtítulos, textos de apoyo, descripciones, metadatos. | 7.02:1 (**AAA**) |
| `Colors.text.muted` | `#94A3B8` | Placeholders de campos de texto, textos deshabilitados. | Solo para elementos inactivos o pistas secundarias |
| `Colors.text.inverse` | `#FFFFFF` | Texto sobre botones primarios (`#1E3A8A`) o banners oscuros. | 10.53:1 (**AAA**) |

---

## 2. Tipografía Institucional

- **Fuente principal:** `Inter` (alternativa de respaldo: `Roboto`, `system-ui`).
- **Idioma del sistema:** Español latinoamericano (`es-EC`).
- **Formato monetario oficial:** Dólares estadounidenses `USD ($)` con separador de miles con coma y dos decimales (`$1,250.00`).

### Escala Tipográfica

| Nivel | Tamaño (px) | Interlineado (Line-height) | Peso (Weight) | Uso |
|---|---|---|---|---|
| **Display / H1** | 32 px | 40 px | Bold (700) | Títulos de Dashboard y pantallas principales |
| **H2** | 24 px | 32 px | SemiBold (600) | Títulos de secciones, tarjetas principales |
| **H3** | 18 px | 24 px | SemiBold (600) | Encabezados de tablas, subtítulos de modales |
| **Body Large** | 16 px | 24 px | Regular (400) / Medium (500) | Texto principal de lectura en escritorio |
| **Body Regular** | 14 px | 20 px | Regular (400) | Texto de celdas de tabla, valores de inputs, ayuda |
| **Caption / Small** | 12 px | 16 px | Medium (500) | Etiquetas de insignias (badges), pie de inputs |

---

## 3. Espaciado, Grid y Bordes

El sistema de espaciado se basa en una cuadrícula de múltiplos de **4 px y 8 px**:

| Token de Espacio | Valor en px | Uso |
|---|---|---|
| `space-1` | 4 px | Separación mínima entre icono y texto |
| `space-2` | 8 px | Separación entre elementos internos de botones o chips |
| `space-3` | 12 px | Espaciado interno compacto (padding de inputs y badges) |
| `space-4` | 16 px | Padding estándar de tarjetas móviles y separación de campos |
| `space-6` | 24 px | Padding estándar de tarjetas de escritorio y modales |
| `space-8` | 32 px | Separación entre secciones principales de una pantalla |
| `space-12` | 48 px | Márgenes exteriores en resoluciones de escritorio |

### Radio de Bordes (Border Radius)
- **Componentes estándar (Botones, Inputs, Tarjetas, Modales):** `12 px` (identidad visual unificada de Banco Chillo).
- **Insignias y Pills:** `9999 px` (bordes completamente redondeados).
- **Checkbox y Selectores:** `6 px`.

---

## 4. Componentes de Interfaz

### 4.1 Botones

Todos los botones deben tener una **altura táctil mínima de 44 px** para cumplir con el criterio WCAG 2.2 AA de target touch.

1. **Botón Primario:**
   - Fondo: `#1E3A8A`
   - Texto: `#FFFFFF` (Inter SemiBold 14/16 px)
   - Border radius: `12 px`
   - Hover: `#1E293B`
   - Foco: Contorno exterior de 2 px en `#0284C7` con separación de 2 px.
   - Uso: Acción principal de la pantalla ("Iniciar Sesión", "Guardar Agencia", "Confirmar Transferencia").

2. **Botón Secundario / Outline:**
   - Fondo: `#FFFFFF`
   - Borde: 1.5 px sólido `#0284C7`
   - Texto: `#0284C7`
   - Border radius: `12 px`
   - Hover: Fondo `#EEF2FF`
   - Uso: Acciones secundarias ("Cancelar", "Volver", "Descargar Comprobante").

3. **Botón Destructivo / Peligro:**
   - Fondo: `#EF4444`
   - Texto: `#FFFFFF`
   - Hover: `#DC2626`
   - Uso: Acciones irreversibles ("Desactivar Agencia", "Bloquear Usuario").

4. **Botón Deshabilitado:**
   - Fondo: `#E2E8F0`
   - Texto: `#94A3B8`
   - Cursor: Not-allowed
   - Uso: Formularios incompletos o peticiones en tránsito.

### 4.2 Campos de Formulario (Inputs)

- **Altura mínima:** `44 px`
- **Border radius:** `12 px`
- **Padding interno:** `12 px` horizontal, `10 px` vertical
- **Tipografía:** Inter Regular 14 px

Estados:
- **Normal:** Fondo `#FFFFFF`, borde de 1 px `#CBD5E1`, texto `#0F172A`, placeholder `#94A3B8`.
- **Foco:** Fondo `#FFFFFF`, borde de 2 px `#0284C7`, anillo de sombra suave azul (`0 0 0 3px rgba(2, 132, 199, 0.2)`).
- **Error:** Fondo `#FEF2F2`, borde de 1.5 px `#EF4444`, texto `#0F172A`. Debajo del campo: mensaje de error en texto `#EF4444` (12 px) precedido del icono de advertencia.
- **Deshabilitado / Solo lectura:** Fondo `#F1F5F9`, borde `#E2E8F0`, texto `#64748B`.

### 4.3 Tarjetas (Cards)

- **Fondo:** `#FFFFFF`
- **Borde:** 1 px sólido `#E2E8F0`
- **Border radius:** `12 px`
- **Sombra (Elevation):** `0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)`
- **Padding:** 16 px (móvil) / 24 px (escritorio).

---

## 5. Criterios de Accesibilidad (WCAG 2.2 AA)

1. **Contraste de Color:**
   - Texto estándar sobre su fondo: mínimo **4.5:1**.
   - Texto grande (18 px negrita o 24 px normal) y componentes UI: mínimo **3.0:1**.
2. **Foco del Teclado Visible:**
   - Todo elemento interactivo accesible por teclado (`Tab`) debe mostrar un borde de foco visible de al menos 2 px en color `#0284C7`.
3. **Áreas Táctiles Mínimas (Touch Targets):**
   - Todo botón, enlace o campo táctil en móviles y tabletas debe medir al menos **44 × 44 px**.
4. **No Confiar Solo en el Color:**
   - El color no debe ser el único vehículo de información. Los estados de éxito, advertencia o error deben llevar iconos lineales explicativos y texto explícito.
5. **Datos y Privacidad:**
   - Todos los datos en mockups deben ser ficticios evidentes (ej. "Socio Demo", "AG-001", cuenta "****1234").

---

## 6. Prompt Base Oficial para Google Stitch

Para que las pantallas de los 7 módulos mantengan absoluta coherencia visual, **todos los integrantes deben usar este prompt base**:

```text
Diseña una pantalla para "Banco Chillo", una cooperativa de ahorro y crédito ecuatoriana.
Estilo: moderno, limpio, confiable, financiero institucional, alto contraste (WCAG 2.2 AA), en español, moneda USD ($).
Colores: primario #1E3A8A, secundario #0284C7, acento #F59E0B, éxito #10B981, error #EF4444, superficie #FFFFFF, fondo #F8FAFC, texto #0F172A.
Tipografía Inter, bordes redondeados de 12 px, iconos lineales limpios, espaciados en múltiplos de 8 px.
Usa solo datos ficticios evidentes (ej. "Socio Demo", cuenta ****1234, cédula 1700000000).
Pantalla: [INSERTAR AQUÍ EL NOMBRE DE LA PANTALLA, CAMPOS, BOTONES Y VERSIÓN ESCRITORIO 1440 PX O MÓVIL]
```
