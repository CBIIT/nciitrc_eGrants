# Credential Disclosure — Remediation Runbook

**Incident date:** 2026-07-28
**Source:** NIH responsible-disclosure notice (public GitHub exposure)
**Repo:** https://github.com/CBIIT/nciitrc_eGrants
**Reported file:** `HelpAndRestore/Web.config.txt` @ commit `2fe1da11` (lines 24, 38)

---

## 1. What was exposed

The report named one file, but the same secrets are duplicated across ~25 files
and have been in git history since **2019-08-27**. Two additional PRODUCTION
credentials were found that the report did **not** flag.

| Secret (redacted here) | Placeholder | Account | Scope | In report? |
|---|---|---|---|---|
| DB password `Jo0ne…!` | `{DB_PASSWORD}` | `egrantsuser` | **dev AND prod** data sources (`MSSQLEGRANTSP`) | yes (dev only) |
| Cert password `g@…3` | `{CERT_PASSWORD}` | `EGRANTS-WEB-*.pfx` | dev | yes |
| DB password `Day…!ng` | `{PROD_DB_PASSWORD}` | `AllWebUSER` | **PROD** (`ncidb-p133-v\egrants_prod`) | **NO** |
| DB password `Jus…4!` | `{PROD_READ_DB_PASSWORD}` | `egrantsuser_read` | **PROD** (`ncidbprd`) | **NO** |

> The `egrantsuser` credential the report treats as "dev" is also used against a
> **production** data source (`source-vbscripts/config.csv`, `add_supp_prod.vbs`).
> Treat it as a production credential for rotation-priority purposes.

---

## 2. Remediation status

- [x] **Working tree sanitized** — all 4 secrets replaced with placeholders (commit `6fe327e2` on `dev`, not yet pushed).
- [x] **.gitignore hardened** — `*.pfx`, `*.pem`, `*.key`, `.env`, `secrets.json` etc.
- [ ] **Credential rotation** — DBA / infra action, see §3. **HIGHEST PRIORITY.**
- [ ] **History rewrite + force-push** — see §4. Requires coordinated maintenance window.
- [ ] **Push `dev`** — after rotation, or as part of the rewrite.

> Deleting/replacing the secrets in a new commit does NOT remove them from history.
> The URLs NIH cited pin an old SHA and remain readable until history is rewritten
> AND GitHub's cached views expire. **Rotation is the only thing that immediately
> neutralizes the leak** — do it first, independent of any git work.

---

## 3. Credential rotation (do this FIRST — no code dependency)

Rotate in priority order. Each is an action in SQL Server / the cert store, not in this repo.

1. **PROD `AllWebUSER`** (`ncidb-p133-v\egrants_prod`) — change password.
2. **PROD `egrantsuser_read`** (`ncidbprd`) — change password.
3. **`egrantsuser`** (used dev + prod) — change password.
4. **Dev web cert** `EGRANTS-WEB-DEV_NCI_NIH_GOV.pfx` — re-issue and/or change the .pfx password.

After each rotation, update the value in the deploy secret store (Jenkins credentials
/ `$env:db_password`, `$env:cert_password`) — NOT in the repo. The `.config` files now
carry only `{DB_PASSWORD}` / `{CERT_PASSWORD}` placeholders and are populated at deploy
time (see `JenkinsUpdates/*.txt`).

Verify old passwords no longer authenticate before closing the incident.

---

## 4. History rewrite (destructive — schedule a maintenance window)

Rewrites SHAs across all 1,332 commits and ~200 branches. **Every developer and every
Jenkins workspace must re-clone afterward.** Do NOT run against your working clone —
use a fresh mirror.

### Preconditions
- Rotation (§3) complete, or at minimum scheduled — the rewrite does not replace rotation.
- All developers notified; freeze merges during the window.
- Inventory open PRs (they reference old SHAs and will break).
- `git filter-repo` installed (`git filter-repo --version`).

### Procedure
```bash
# 1. Fresh mirror (all refs), NOT your working clone
git clone --mirror https://github.com/CBIIT/nciitrc_eGrants nciitrc_eGrants-mirror.git
cd nciitrc_eGrants-mirror.git

# 2. Rewrite every blob on every ref using the replacement expressions
git filter-repo --replace-text /path/to/security/incident-2026-07-28/replacements.txt

# 3. Verify the secrets are gone from ALL history
git grep -I -n -e 'Jo0ne62017' -e '{CERT_PASSWORD}' -e 'DayofSpr' -e 'Justice424' $(git rev-list --all) && echo "STILL PRESENT — STOP" || echo "clean"

# 4. filter-repo removes the remote; re-add and force-push everything
git remote add origin https://github.com/CBIIT/nciitrc_eGrants
git push --force --all origin
git push --force --tags origin
```

### After force-push — coordination
- **All developers:** re-clone fresh. Salvaging an old clone risks re-introducing the
  secret on the next push. Stash/back up unpushed work first; re-apply by hand
  (cherry-pick will not map cleanly across rewritten SHAs).
- **Jenkins / build machines:** wipe workspace and re-clone ("Wipe out repository and
  force clone" / clean checkout). Update any pipeline or deploy step that pins a
  specific commit SHA (the `JenkinsUpdates/*Deploy.txt` / `Backup/*` files reference SHAs).
- **Open PRs:** recreate/rebase onto rewritten history.
- **GitHub cached blobs:** the old SHA-pinned URLs may remain viewable for a while via
  GitHub's cache and any forks. If immediate purge is required, open a GitHub Support
  request to drop cached views and check for forks. Rotation (§3) is what makes residual
  cached copies harmless.

---

## 5. Files sanitized (commit 6fe327e2)

25 source files + `.gitignore`. Full list in the commit. Categories:
- `source-aspnet/egrants_new/*.config` (the deploy template + local variants)
- `source-aspnet/eGrants/appsettings.json` (certPass)
- `source-vbscripts/*.vbs`, `*.csv`
- `EmailHandling/*/config.csv`
- `HelpAndRestore/Web.config.txt`, `web.base.config.txt` (the reported copies)
