# Guía de Contribución — Banco Chillo

Bienvenido al repositorio oficial de **Banco Chillo**. Para mantener la integridad, seguridad y calidad de software financiero, todos los colaboradores y agentes deben adherirse a las siguientes directrices.

---

## 1. Flujo Git (GitHub Flow Adaptado)

El repositorio opera con dos ramas troncales protegidas:

1. **`main`**: Refleja el estado estable y desplegable en producción.
   - Requiere revisión obligatoria de código (Code Review).
   - Requiere paso exitoso de checks de CI (Build, Tests, Gitleaks).
   - No admite `git push --force`.
2. **`develop`**: Rama donde convergen las características del sprint en curso.

### Ciclo de vida de una contribución:
1. Tomar o asignar un Issue/Historia de Usuario desde el tablero de GitHub.
2. Crear una rama desde `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feat/12-modulo-cajero-arqueo
   ```
3. Implementar cambios siguiendo Clean Architecture y pruebas unitarias.
4. Ejecutar validaciones locales:
   ```bash
   dotnet test
   dotnet format --verify-no-changes
   ```
5. Realizar commits bajo la convención **Conventional Commits**:
   - `feat: ...`
   - `fix: ...`
   - `docs: ...`
   - `test: ...`
   - `refactor: ...`
6. Abrir un Pull Request apuntando a `develop` enlazando el Issue con la cláusula `Closes #12`.

---

## 2. Estándares de Código

### Backend (.NET)
- Cada microservicio está estrictamente dividido en `Domain`, `Application`, `Infrastructure` y `Api`.
- **Domain** no tiene dependencias de librerías externas ni de Entity Framework.
- Los casos de uso se implementan como **Commands** o **Queries** procesados por handlers de MediatR.
- Validación de entrada con **FluentValidation**.

### Frontend PWA (React Native / Expo Web)
- TypeScript con tipado estricto (`noImplicitAny: true`).
- Cumplimiento de accesibilidad **WCAG 2.2 AA** (contraste, foco de navegación por teclado, soporte de lectores de pantalla).
- Separación de responsabilidades: componentes de presentación desacoplados de la lógica de servicios y estado.

---

## 3. Seguridad Financiera
- Todo endpoint sensible requiere token JWT y validación de claims de rol.
- Las operaciones financieras de ventanilla exigen confirmación del cajero con validación de saldo en caja.
- Ningún dato sensible (contraseñas, PINs, llaves criptográficas) se loguea en consola ni en Seq/OpenTelemetry.
