# Push RoadChangerAddIn to GitHub (GeoGarmeux Spatial Industries)

The repo is prepped — `.gitignore` (keeps `bin/`, `obj/`, `.vs/`, and any `*.pfx` **out**)
and `README.md` are in place. Run the rest on your own machine, where your GitHub login is.

> **Org name:** GitHub org logins can't contain spaces. "GeoGarmeux Spatial Industries"
> is likely the slug **`GeoGarmeux-Spatial-Industries`** (confirm the exact spelling on
> github.com). The org must already exist — if not, create it first at
> <https://github.com/organizations/plan>. Replace `<ORG>` below with the real slug.

---

## Option A — GitHub CLI (fastest)

Requires the GitHub CLI (`winget install GitHub.cli`) and a one-time `gh auth login`.

```powershell
cd "C:\Users\chris\Documents\ArcGIS\AddIns\ArcGISPro\RoadChangerAddIn"

git init -b main
git add .
git commit -m "Initial commit: RoadChangerAddIn (TDS reclassify buttons + deploy kit)"

# creates the repo IN the org and pushes in one step
gh repo create "<ORG>/RoadChangerAddIn" --private --source=. --remote=origin --push
```

Done — the repo is live and pushed.

---

## Option B — Website + git (no CLI)

1. On github.com, go to your org → **New repository** → name it `RoadChangerAddIn`,
   set Private, and **don't** add a README/.gitignore (you already have them).
2. Then:

```powershell
cd "C:\Users\chris\Documents\ArcGIS\AddIns\ArcGISPro\RoadChangerAddIn"

git init -b main
git add .
git commit -m "Initial commit: RoadChangerAddIn (TDS reclassify buttons + deploy kit)"
git remote add origin https://github.com/<ORG>/RoadChangerAddIn.git
git push -u origin main
```

---

## Sharing / wiring up

- **Add collaborators / teams:** Org repo → Settings → Collaborators and teams.
- **Protect main:** Settings → Branches → add a rule for `main` (require PR review).
- The signing key (`Deploy/*.pfx`) is gitignored on purpose — **never** commit it. The
  public `*.cer` and the PowerShell scripts are safe to share and will be included.

## Sanity check before the first push

```powershell
git status            # confirm bin/ obj/ .vs/ and *.pfx are NOT listed
git ls-files | findstr /i ".pfx"   # should return nothing
```

If anything secret shows up, fix `.gitignore` before committing.
