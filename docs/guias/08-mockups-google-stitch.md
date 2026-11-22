# 08 · Mockups con Google Stitch

**Google Stitch** (<https://stitch.withgoogle.com>) es una herramienta de Google Labs que genera pantallas de
aplicaciones (móvil o web) a partir de una descripción en texto o de un boceto. Luego se pueden exportar a
**Figma** o a **código HTML/CSS**. Es gratuita con una cuenta de Google, con un límite mensual de generaciones:
**no gastes intentos con prompts vagos**.

## 1. Entrar

1. Abre <https://stitch.withgoogle.com> con tu cuenta de Google (personal o institucional).
2. Elige el tipo de diseño: **App** (móvil, para la banca del socio) o **Web** (escritorio, para cajero y administración).

## 2. Identidad visual común (OBLIGATORIA para todo el grupo)

Para que las pantallas de los 7 parezcan de **un solo banco**, todos usan estos tokens
(los mismos de `src/apps/pwa-banca/src/shared/design-tokens/colors.ts`):

| Token | Color | Uso |
|---|---|---|
| Primario | `#1E3A8A` | Encabezados, botones principales |
| Secundario | `#0284C7` | Enlaces, acciones secundarias |
| Acento | `#F59E0B` | Destacados, alertas suaves |
| Éxito | `#10B981` | Caja cuadrada, operación exitosa |
| Peligro | `#EF4444` | Descuadre, errores |
| Fondo | `#F8FAFC` | Fondo de pantallas |
| Texto | `#0F172A` / `#475569` | Principal / secundario |

Tipografía: **Inter** (o Roboto). Bordes redondeados: 12 px. Idioma: **español**. Moneda: **USD ($)**.

## 3. Prompt base (cópialo y cambia solo la última parte)

```text
Diseña una pantalla para "Banco Chillo", una cooperativa de ahorro y crédito ecuatoriana.
Estilo: moderno, limpio, confiable, alto contraste (WCAG AA), en español, moneda USD.
Colores: primario #1E3A8A, secundario #0284C7, acento #F59E0B, éxito #10B981, error #EF4444, fondo #F8FAFC.
Tipografía Inter, bordes redondeados de 12 px, iconos lineales.
Usa solo datos ficticios evidentes (ej. "Socio Demo", cuenta ****1234).
Pantalla: [DESCRIBE AQUÍ LA PANTALLA, sus campos y botones]
```

Ejemplo de la última parte:

```text
Pantalla: Apertura de caja del cajero. Encabezado con agencia y número de ventanilla.
Tabla de denominaciones (billetes 100, 50, 20, 10, 5, 1 y monedas 1, 0.50, 0.25, 0.10, 0.05, 0.01)
con columnas Denominación, Cantidad (campo numérico) y Subtotal. Total general grande abajo.
Botón primario "Abrir caja" y botón secundario "Cancelar". Versión escritorio 1440 px.
```

## 4. Iterar

- Pide cambios concretos por chat: *"mueve el total a la derecha"*, *"agrega un estado de error en el campo cantidad"*.
- Mantén **una sola conversación por módulo** para que conserve el estilo.
- Genera también los estados: **vacío, cargando, error, éxito**.

## 5. Exportar y entregar

1. Selecciona las pantallas → **Export / Copy to Figma** (pégalas en el archivo Figma del grupo) o descarga las imágenes PNG.
2. Guarda las PNG en el repositorio:
   ```
   docs/mockups/<modulo>/<nn>-<nombre-pantalla>.png
   ej. docs/mockups/cajeros/01-apertura-caja.png
   ```
3. Agrega en `docs/mockups/<modulo>/README.md` el **prompt usado** por cada pantalla y el enlace al proyecto de Stitch.
4. Opcional: exporta el **código HTML** como referencia visual para la PWA. No se copia tal cual: la PWA es React Native.

## 6. Pantallas que debe entregar el grupo (Semana 5)

| Módulo | Pantallas mínimas | Formato | Responsable |
|---|---|---|---|
| Auth | Login, verificación MFA (código de 6 dígitos), bloqueo de cuenta | App + Web | Auth |
| Banca personal (socio) | Posición consolidada, detalle de cuenta con movimientos, transferencia (3 pasos) | App | Cuentas / Transacciones |
| Cajero | Apertura de caja, depósito/retiro, arqueo y cierre (verde cuadrado / rojo descuadre) | Web | Cajeros |
| Créditos | Simulador (francesa/alemana) con tabla de amortización | App + Web | Créditos |
| Administración | Dashboard, CRUD de agencias y ventanillas | Web | Gateway / PWA |
| Auditoría / Notificaciones | Bitácora de eventos con filtros, preferencias de notificación del socio | Web / App | Auditoría / Notificaciones |

La responsable de la PWA **consolida** todo en `docs/mockups/README.md` (índice con miniaturas) y revisa que se respete la identidad visual.

## 7. Qué presentar como grupo

1. El **flujo navegable** (en Figma: *Prototype*, conecta botones entre pantallas) del recorrido del socio y del cajero.
2. Una lámina con la **guía de estilo** (colores, tipografía, botones, campos).
3. Capturas en el informe (Parte 3 y 4 de la [guía 12](12-informe-practica.md)).
