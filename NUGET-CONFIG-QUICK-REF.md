# 🎯 NuGet.org Trusted Publishing - Quick Setup

## Configuration Values for NuGet.org

When you go to https://www.nuget.org/account/apikeys and click **"Add"** under "Trusted publishers", use these **EXACT** values:

```
┌─────────────────────────────────────────────────────────────┐
│  Package ID pattern:        FoxEndpoints                    │
│  Package owner:            [Your NuGet.org username]        │
│                                                             │
│  Repository owner:         Stureman                         │
│  Repository name:          FoxEndpoints                     │
│  Workflow filename:        publish.yml                      │
│                                                             │
│  Environment name:         [Leave empty]                    │
└─────────────────────────────────────────────────────────────┘
```

### Important Notes

⚠️ **Case-sensitive!** Make sure:
- `Stureman` (capital S, rest lowercase)
- `FoxEndpoints` (capital F and E)
- `publish.yml` (all lowercase)

✅ **Package owner**: Use YOUR NuGet.org username (the account you logged in with)

## After Configuration

1. **Commit workflow file:**
   ```bash
   git add .github/workflows/publish.yml
   git commit -m "Add NuGet publishing workflow"
   git push
   ```

2. **Create GitHub Release:**
   - Go to: https://github.com/Stureman/FoxEndpoints/releases/new
   - Tag: `v1.0.0`
   - Title: `FoxEndpoints 1.0.0`
   - Click "Publish release"

3. **Watch it publish:**
   - Go to: https://github.com/Stureman/FoxEndpoints/actions
   - Watch the workflow run ✅

4. **Check package:**
   - https://www.nuget.org/packages/FoxEndpoints

## That's It!

No API keys, no secrets, no hassle! 🎉