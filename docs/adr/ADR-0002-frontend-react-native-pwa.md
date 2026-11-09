# ADR-0002: Frontend Unificado PWA con React Native (Expo Web)

- **Estado:** Aceptado
- **Fecha:** 2026-11-09
- **Autores:** Frontend Lead, Arquitecto de Software
- **Decisores:** Tech Lead

---

## 1. Contexto y Planteamiento del Problema

Se necesita proveer una aplicación accesible tanto desde navegadores web (para estaciones de trabajo de personal administrativo y cajeros en agencias) como desde dispositivos móviles (para socios/clientes). Mantener dos codebases independientes (Web y Móvil) duplicaría los esfuerzos en un plazo de 17 semanas.

---

## 2. Decisión

Se adopta **React Native con soporte web (Expo / React Native for Web)** configurado como **Progressive Web App (PWA)**:
1. **Base de Código Única**: Componentes reutilizables adaptados para escritorio (teclado extendido de cajero) y móviles (gestos táctiles y responsive).
2. **Capacidades PWA**: Instalable en Windows/Linux/macOS de agencias bancarias y en Android/iOS de clientes.
3. **Multi-Rol**: Separación estricta de rutas protegidas mediante Guards para los portales `admin-portal` (Cajeros, Supervisores) y `banca-personal` (Socios).

---

## 3. Consecuencias

### Positivas
- 100% de reutilización de lógica de negocio, clientes API, formateadores y validadores.
- Rendimiento nativo en móviles y estándar web moderno en navegadores de oficina.
- Soporte offline controlado para catálogos y colas de sincronización seguras.
