# ADR-0006: Aplicación de escritorio del cajero en WPF (.NET 10)

- **Estado:** Aceptado
- **Fecha:** 2026-11-22
- **Autores:** Docente / Tech Lead
- **Decisores:** Tech Lead

---

## 1. Contexto y Planteamiento del Problema

Las agencias necesitan una aplicación instalable (`.exe`) para las estaciones de ventanilla, además de la PWA.
Debe consumir los mismos microservicios (a través del Gateway) y ser simple de mantener por estudiantes.

---

## 2. Opciones Consideradas

- **Opción 1 — WPF (.NET 10):** nativa de Windows, MVVM maduro, se publica como un único `.exe`.
- **Opción 2 — Avalonia UI:** multiplataforma, sintaxis parecida a WPF, más curva de aprendizaje.
- **Opción 3 — WinForms:** la más sencilla, pero con apariencia anticuada y sin MVVM natural.

---

## 3. Decisión

Elegimos **WPF con MVVM (CommunityToolkit.Mvvm)** en `src/apps/escritorio-cajero/BancoChillo.Escritorio`.

- La app **nunca** se conecta a la base de datos: usa el API Gateway con JWT, igual que la PWA.
- Se publica como un único archivo autocontenido: `scripts/publicar-escritorio.ps1`.
- El pipeline `cd-escritorio.yml` adjunta el `.exe` a un GitHub Release al crear un tag `escritorio-v*`.
- El proyecto declara `EnableWindowsTargeting` para que la CI en Linux pueda compilarlo (no ejecutarlo).

---

## 4. Consecuencias

### Positivas
- Mismo lenguaje (C#) que el backend: los DTOs y las reglas se entienden igual.
- Experiencia de escritorio con teclado completo, ideal para ventanilla.

### Negativas / Trade-offs
- Solo Windows (las agencias usan Windows; los socios usan la PWA).
- El `.exe` autocontenido pesa alrededor de 130 MB. Aceptable para distribución interna.
