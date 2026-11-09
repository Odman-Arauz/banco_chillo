## Tipo de Cambio

- [ ] `feat`: Nueva funcionalidad
- [ ] `fix`: Corrección de error
- [ ] `refactor`: Refactorización de código sin cambio de comportamiento
- [ ] `docs`: Modificación de documentación o ADRs
- [ ] `test`: Adición o mejora de pruebas
- [ ] `chore`: Mantenimiento, dependencias o configuración

## Descripción

Describe el propósito de este PR y el módulo afectado (ej. `BancoChillo.Cajeros`, `pwa-banca`).

Closes #(número de issue)

## Checklist de Verificación

- [ ] El código sigue la Clean Architecture (Domain -> Application -> Infrastructure -> Api).
- [ ] No contiene credenciales, secretos ni datos personales reales (validado por Gitleaks).
- [ ] Se incluyeron o actualizaron pruebas unitarias para los casos de uso implementados.
- [ ] Si aplica cambio arquitectónico, se incluye o actualiza un ADR en `docs/adr/`.
- [ ] Se ejecutaron pruebas locales (`dotnet test` o `npm test`) con resultado exitoso.
