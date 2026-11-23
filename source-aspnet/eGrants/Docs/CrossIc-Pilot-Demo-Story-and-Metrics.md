# Cross-IC Pilot: Demo Story + Success Metrics Sign-off

## Purpose
This artifact formalizes the remaining Milestone 2 improvement:
- explicit deployment/demo story
- simple success metrics with sign-off status

---

## Pilot Use Case (Selected)
Allow controlled cross-IC access for non-NCI detail users in `eGrants`:
- FOIA / detailee PD => cross-IC read-only (Program baseline)
- detailee specialist => cross-IC specialist-equivalent access

---

## Demo Story (Leadership-Ready, 10–15 min)

### 1) Problem (1 min)
- Legacy IC-coupled logic caused non-NCI detail users to fail open/download/upload scenarios.
- Local/eRA links could break when viewer IC differed from authority context.

### 2) AI-assisted multi-agent workflow (2–3 min)
- Analyst: requirements + AC decomposition
- Implementation: app/session/controller/repository/SQL changes
- Test: unit + integration + DB contract checks
- Reviewer/security: server-side mutation guards
- Documentation: rollout scripts/checklists

### 3) What changed (3–4 min)
- Session model: `effective_role`, `access_scope`, `authority_ic`, `home_ic`
- Config-driven persona mapping (`AccessControl`)
- Server-side write guard enforcement for read-only personas
- DB contract updates for cross-IC parameters + URL authority fix
- Consolidated SQL change/rollback scripts

### 4) Live walk-through (3–4 min)
- Sign in as read-only mapped user => can view/open cross-IC docs, cannot mutate
- Sign in as specialist mapped user => can upload/update where specialist is allowed
- Confirm local/eRA links open without viewer-IC host breakage

### 5) Value + replication (1–2 min)
- Reduces one-off hardcoding and manual workarounds
- Introduces repeatable pattern for future cross-IC/detailee access requests
- Reusable for other role-scope access exceptions

---

## Success Metrics (Simple, AC-Aligned)

### Functional metrics
1. Cross-IC read-open success (FOIA/PD)
   - Target: 100% pass on AC1/AC2 scenarios in UAT set
2. Read-only mutation blocking
   - Target: 100% blocked on AC3 including direct requests
3. Specialist cross-IC upload success
   - Target: 100% pass on AC4/AC5 UAT scenarios
4. Role boundary enforcement
   - Target: 100% deny on AC6 out-of-scope files

### Delivery metrics
5. Build and targeted test health
   - Target: build green + targeted cross-IC tests passing
6. Rollback readiness
   - Target: one change script + one rollback script verified in non-prod

---

## Sign-off Table
| Metric | Owner | Status | Evidence |
|---|---|---|---|
| AC1/AC2 pass | UAT/POC | Pending | UAT run notes |
| AC3 pass | UAT/Sec reviewer | Pending | direct request test results |
| AC4/AC5 pass | UAT/POC | Pending | upload/open cross-user evidence |
| AC6 pass | UAT/Sec reviewer | Pending | denied access evidence |
| Build + tests | Dev | Complete | latest local run logs |
| SQL change/rollback dry run | DBA/Dev | Pending | execution checklist |

---

## Positioning Recommendation
Position this demo as:
- **Delivery acceleration** (primary)
- **Engineering enablement** (secondary)
- Innovation (supporting)

This aligns best with practical impact and reuse potential.
