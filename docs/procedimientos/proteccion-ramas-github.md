# Procedimiento: Activación de Protección de Ramas en GitHub

Dado que la cuenta colaboradora posee permisos de escritura (`WRITE`), las reglas de protección de rama y rulesets a nivel de repositorio requieren ser activadas por el propietario del repositorio (`Odman-Arauz`) en la interfaz web de GitHub.

---

## 1. Protección de la Rama `main` (Producción)

1. Ir a **Settings** > **Branches** (o **Rules** > **Rulesets**).
2. Hacer clic en **Add branch protection rule** (o **New ruleset**).
3. En **Branch name pattern**, escribir: `main`.
4. Habilitar las siguientes casillas:
   - ✅ **Require a pull request before merging**
     - Require approvals: `1`
     - Dismiss stale pull request approvals when new commits are pushed: `Checked`
     - Require review from Code Owners: `Optional`
     - Require conversation resolution before merging: `Checked`
   - ✅ **Require status checks to pass before merging**
     - Buscar y marcar: `CI Backend / build-and-test`, `CI Frontend PWA / build-pwa`, `Gitleaks / Escaneo de Secretos`.
   - ✅ **Do not allow bypassing the above settings** (Enforce admins).
   - ❌ **Do not allow force pushes**.
   - ❌ **Do not allow deletions**.
5. Guardar cambios.

---

## 2. Protección de la Rama `develop` (Integración)

1. En **Branches** > **Add branch protection rule**.
2. En **Branch name pattern**, escribir: `develop`.
3. Habilitar:
   - ✅ **Require a pull request before merging** (Require approvals: `1`).
   - ✅ **Require status checks to pass before merging** (`CI Backend`, `CI Frontend PWA`).
   - ❌ **Do not allow force pushes**.
   - ❌ **Do not allow deletions**.
4. Guardar cambios.

---

## 3. Ramas por Defecto
En **Settings** > **General** > **Default branch**:
- Mantener `develop` como rama por defecto para nuevas contribuciones y PRs diarios, o `main` como rama de despliegue estable.
