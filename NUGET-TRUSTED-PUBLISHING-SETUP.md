# 🔐 NuGet.org Trusted Publishing Configuration

## ✅ Step-by-Step Setup Guide

### Step 1: Configure Trusted Publisher on NuGet.org

1. **Go to NuGet.org and sign in**
   - URL: https://www.nuget.org/
   - Sign in with your account

2. **Navigate to API Keys**
   - Click your username (top right)
   - Select **"API Keys"**
   - Or go directly to: https://www.nuget.org/account/apikeys

3. **Click "Add" under "Trusted publishers for publishing packages"**

4. **Fill in the form with EXACTLY these values:**

   ```
   Package ID pattern:        FoxEndpoints
   Package owner:            [Your NuGet.org username]
   
   Repository owner:         Stureman
   Repository name:          FoxEndpoints
   Workflow filename:        publish.yml
   
   Environment name:         [Leave empty or use "production"]
   ```

   **Important Field Explanations:**

   - **Package ID pattern**: `FoxEndpoints`
     - Must match the `<PackageId>` in your .csproj
     - Use `FoxEndpoints*` if you want to publish multiple related packages

   - **Package owner**: Your NuGet.org username
     - This is YOUR username on NuGet.org

   - **Repository owner**: `Stureman`
     - This is your GitHub username
     - Must match exactly (case-sensitive)

   - **Repository name**: `FoxEndpoints`
     - Must match your GitHub repository name exactly

   - **Workflow filename**: `publish.yml`
     - Must match the filename in `.github/workflows/publish.yml`
     - Do NOT include the path, just the filename

   - **Environment name**: (Optional)
     - Leave empty for simple setup
     - Or use `production` if you want extra protection (requires creating a GitHub environment)

5. **Click "Create"**

### Step 2: Verify GitHub Actions Workflow

The workflow has been created at:
```
.github/workflows/publish.yml
```

**Key features:**
- ✅ Triggers on GitHub Releases
- ✅ Manual trigger available via workflow_dispatch
- ✅ Builds for .NET 9 and .NET 10
- ✅ Runs tests before publishing
- ✅ Uses Trusted Publishing (no API key needed!)
- ✅ Skips duplicates (safe to re-run)

### Step 3: Commit and Push

```bash
cd /Users/richardtruedsson/RiderProjects/FoxEndpoints

git add .github/workflows/publish.yml
git commit -m "Add GitHub Actions workflow for NuGet publishing with Trusted Publishing"
git push origin main
```

### Step 4: Create Your First Release

**Option A: Via GitHub Web UI (Recommended)**

1. Go to: https://github.com/Stureman/FoxEndpoints
2. Click **"Releases"** (right sidebar)
3. Click **"Create a new release"**
4. Fill in:
   - **Choose a tag**: `v1.0.0` (create new tag)
   - **Release title**: `FoxEndpoints 1.0.0`
   - **Description**: 
     ```
     ## 🎉 Initial Release
     
     FoxEndpoints is a lightweight, minimal API endpoint framework for ASP.NET Core.
     
     ### Features
     - Natural IResult-based response handling
     - Support for .NET 9 and .NET 10
     - Global authorization support
     - Type-safe Send methods
     - Inspired by FastEndpoints
     
     ### Installation
     `dotnet add package FoxEndpoints`
     ```
5. Click **"Publish release"**
6. Watch the **Actions** tab - your package will publish automatically! 🚀

**Option B: Via Command Line**

```bash
# Create and push a tag
git tag v1.0.0
git push origin v1.0.0

# Then go to GitHub and create the release from the tag
```

### Step 5: Manual Trigger (Optional)

If you want to test without creating a release:

1. Go to **Actions** tab: https://github.com/Stureman/FoxEndpoints/actions
2. Select **"Publish to NuGet"** workflow
3. Click **"Run workflow"**
4. Select branch: `main`
5. Click **"Run workflow"**

## 📋 Configuration Checklist

### On NuGet.org

- [ ] Signed in to NuGet.org
- [ ] Navigated to API Keys page
- [ ] Added Trusted Publisher with:
  - [ ] Package ID pattern: `FoxEndpoints`
  - [ ] Repository owner: `Stureman`
  - [ ] Repository name: `FoxEndpoints`
  - [ ] Workflow filename: `publish.yml`
- [ ] Clicked "Create"

### In GitHub Repository

- [ ] `.github/workflows/publish.yml` created ✅
- [ ] Workflow file committed and pushed
- [ ] Repository is public (or you have GitHub Actions enabled for private repos)

### Project Configuration

- [ ] `FoxEndpoints.csproj` has correct metadata ✅
- [ ] Version is `1.0.0` ✅
- [ ] Repository URLs point to `Stureman/FoxEndpoints` ✅
- [ ] README.md exists ✅

## 🎯 What Happens When You Publish

1. You create a GitHub Release with tag `v1.0.0`
2. GitHub Actions workflow triggers
3. Workflow:
   - ✅ Checks out your code
   - ✅ Installs .NET 9 and 10
   - ✅ Restores dependencies
   - ✅ Builds for both frameworks
   - ✅ Runs all tests
   - ✅ Creates NuGet package
   - ✅ Authenticates with NuGet.org via OIDC (no API key!)
   - ✅ Publishes package
4. Package appears on NuGet.org within minutes! 🎉

## 🔍 Monitoring

### Check Workflow Status

Go to: https://github.com/Stureman/FoxEndpoints/actions

You'll see:
- ✅ Green checkmark = Success
- ❌ Red X = Failed (click to see logs)
- 🟡 Yellow dot = Running

### Check Package on NuGet.org

After successful publish:
- Package URL: https://www.nuget.org/packages/FoxEndpoints
- May take a few minutes to be indexed and searchable

## ⚠️ Troubleshooting

### Error: "Trusted publisher not configured"

**Solution**: Verify on NuGet.org that:
- Repository owner is exactly `Stureman` (case-sensitive)
- Repository name is exactly `FoxEndpoints`
- Workflow filename is exactly `publish.yml`

### Error: "Package already exists"

**Solution**: Update version in `FoxEndpoints.csproj`:
```xml
<Version>1.0.1</Version>
```

### Error: "Tests failed"

**Solution**: The workflow runs tests before publishing. Fix failing tests:
```bash
dotnet test
```

### Workflow doesn't trigger

**Solution**: Make sure:
- Workflow file is in `.github/workflows/` directory
- You created a **Release**, not just a tag
- Repository has Actions enabled (Settings → Actions)

## 🔄 Publishing Future Versions

1. **Update version** in `FoxEndpoints.csproj`:
   ```xml
   <Version>1.1.0</Version>
   ```

2. **Commit and push**:
   ```bash
   git add FoxEndpoints/FoxEndpoints.csproj
   git commit -m "Bump version to 1.1.0"
   git push
   ```

3. **Create new release** on GitHub:
   - Tag: `v1.1.0`
   - Title: `FoxEndpoints 1.1.0`
   - Add release notes

4. **Done!** Package publishes automatically ✅

## 🎉 Summary

**NuGet.org Configuration:**
```
Package ID pattern:   FoxEndpoints
Repository owner:     Stureman
Repository name:      FoxEndpoints
Workflow filename:    publish.yml
Environment:          (leave empty)
```

**Workflow file:** ✅ Created at `.github/workflows/publish.yml`

**Next steps:**
1. Configure Trusted Publisher on NuGet.org (use values above)
2. Commit and push the workflow file
3. Create a GitHub Release with tag `v1.0.0`
4. Watch your package publish automatically! 🚀

---

**No API keys needed! No secrets to manage! Just create a release and let GitHub Actions do the work!** 🎉