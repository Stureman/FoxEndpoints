# 📦 Creating and Publishing FoxEndpoints NuGet Package

## ✅ Project Configuration Complete

Your `FoxEndpoints.csproj` is now configured with:

### Multi-Targeting
- ✅ **Targets**: .NET 9.0 and .NET 10.0
- ✅ **Language**: Latest C# version (13 for .NET 9, 14 for .NET 10)
- ✅ **Frameworks**: `net9.0` and `net10.0`

### NuGet Package Metadata
- ✅ **PackageId**: FoxEndpoints
- ✅ **Version**: 1.0.0
- ✅ **Authors**: Richard Truedsson
- ✅ **Description**: Complete with feature list
- ✅ **Tags**: aspnetcore, minimal-api, endpoints, web-api, fastendpoints
- ✅ **License**: MIT
- ✅ **README**: Included in package
- ✅ **Symbols**: Enabled (.snupkg for debugging)

## 🔨 Building the Package

### Step 1: Build and Verify

Build the project for both frameworks:

```bash
cd /Users/richardtruedsson/RiderProjects/FoxEndpoints
dotnet build FoxEndpoints/FoxEndpoints.csproj
```

This will create builds in:
- `FoxEndpoints/bin/Debug/net9.0/FoxEndpoints.dll`
- `FoxEndpoints/bin/Debug/net10.0/FoxEndpoints.dll`

### Step 2: Create the NuGet Package

Create the `.nupkg` file:

```bash
dotnet pack FoxEndpoints/FoxEndpoints.csproj --configuration Release
```

This creates:
- `FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.nupkg` (main package)
- `FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.snupkg` (symbols package)

### Step 3: Inspect the Package (Optional)

Check what's inside:

```bash
# Extract and view contents
unzip -l FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.nupkg
```

Or use NuGet Package Explorer (GUI tool):
```bash
# Install globally (if not already installed)
dotnet tool install -g NuGetPackageExplorer

# Open package
nuget-explorer FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.nupkg
```

## 📤 Publishing to NuGet.org

### Prerequisites

1. **Create NuGet Account**: https://www.nuget.org/
2. **Get API Key**: 
   - Go to https://www.nuget.org/account/apikeys
   - Create new API key with "Push" permission
   - Copy the key (you'll only see it once!)

### Publishing Steps

#### Step 1: Push to NuGet.org

```bash
dotnet nuget push FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

Replace `YOUR_API_KEY` with your actual API key.

#### Step 2: Push Symbol Package (Optional but Recommended)

This enables debugging for consumers:

```bash
dotnet nuget push FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.snupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

#### Step 3: Verify Publication

- Check https://www.nuget.org/packages/FoxEndpoints
- It may take a few minutes to appear and be indexed

## 🏠 Publishing to a Private Feed

### Azure DevOps Artifacts

```bash
# Add the feed
dotnet nuget add source \
  "https://pkgs.dev.azure.com/YOUR_ORG/_packaging/YOUR_FEED/nuget/v3/index.json" \
  --name "AzureDevOps" \
  --username "YOUR_USERNAME" \
  --password "YOUR_PAT"

# Push package
dotnet nuget push FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.nupkg \
  --source "AzureDevOps" \
  --api-key az
```

### GitHub Packages

```bash
# Add the feed
dotnet nuget add source \
  --username YOUR_USERNAME \
  --password YOUR_GITHUB_TOKEN \
  --store-password-in-clear-text \
  --name github "https://nuget.pkg.github.com/YOUR_USERNAME/index.json"

# Push package
dotnet nuget push FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.nupkg \
  --source "github" \
  --api-key YOUR_GITHUB_TOKEN
```

### Local Feed (Testing)

```bash
# Create local feed directory
mkdir -p ~/nuget-local

# Push to local feed
dotnet nuget push FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.nupkg \
  --source ~/nuget-local
```

Then add the feed in consumer projects:
```bash
dotnet nuget add source ~/nuget-local --name "Local"
```

## 📝 Before Publishing Checklist

### Required Changes

Before publishing, update these placeholders in `FoxEndpoints.csproj`:

1. **RepositoryUrl**: Change from `https://github.com/yourusername/FoxEndpoints` to your actual GitHub URL
2. **PackageProjectUrl**: Same as above
3. **Authors**: Already set to "Richard Truedsson" ✅
4. **Version**: Currently 1.0.0 - update as needed

Example:
```xml
<RepositoryUrl>https://github.com/richardtruedsson/FoxEndpoints</RepositoryUrl>
<PackageProjectUrl>https://github.com/richardtruedsson/FoxEndpoints</PackageProjectUrl>
```

### Quality Checklist

- ✅ All tests passing: `dotnet test`
- ✅ No compilation errors: `dotnet build`
- ✅ README.md is complete and accurate
- ✅ Version number is correct (semantic versioning: MAJOR.MINOR.PATCH)
- ✅ License is appropriate (MIT is set)
- ✅ Repository URLs are correct
- ✅ Package tags are relevant

## 🔄 Version Management

### Semantic Versioning

Follow [SemVer](https://semver.org/):
- **MAJOR**: Breaking changes (e.g., 1.0.0 → 2.0.0)
- **MINOR**: New features, backward compatible (e.g., 1.0.0 → 1.1.0)
- **PATCH**: Bug fixes, backward compatible (e.g., 1.0.0 → 1.0.1)

### Updating Version

Edit `FoxEndpoints.csproj`:
```xml
<Version>1.1.0</Version>
```

Or use command line:
```bash
dotnet pack -p:Version=1.1.0
```

### Pre-release Versions

For beta/alpha releases:
```xml
<Version>1.1.0-beta1</Version>
<Version>2.0.0-rc1</Version>
```

## 📊 Package Contents

When you pack, the `.nupkg` will contain:

```
FoxEndpoints.1.0.0.nupkg
├── lib/
│   ├── net9.0/
│   │   └── FoxEndpoints.dll
│   └── net10.0/
│       └── FoxEndpoints.dll
├── README.md
└── FoxEndpoints.nuspec (auto-generated metadata)
```

## 🎯 Consuming the Package

Once published, consumers can install it:

```bash
# Install latest version
dotnet add package FoxEndpoints

# Install specific version
dotnet add package FoxEndpoints --version 1.0.0

# Install pre-release
dotnet add package FoxEndpoints --prerelease
```

Or in `.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="FoxEndpoints" Version="1.0.0" />
</ItemGroup>
```

### Framework Compatibility

- **.NET 9 projects** will automatically use the `net9.0` build
- **.NET 10 projects** will automatically use the `net10.0` build
- **.NET 8 and below** won't be able to use this package (by design)

## 🔐 API Key Management

### Store API Key Securely

Don't commit API keys! Use environment variables:

```bash
# Set environment variable
export NUGET_API_KEY="your-api-key-here"

# Use in commands
dotnet nuget push FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

Or use `dotnet nuget` config:
```bash
# Add API key to config (stored encrypted on Windows)
dotnet nuget setapikey YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## 🚀 Quick Reference

### Build for Release
```bash
dotnet build -c Release FoxEndpoints/FoxEndpoints.csproj
```

### Create Package
```bash
dotnet pack -c Release FoxEndpoints/FoxEndpoints.csproj
```

### Publish to NuGet
```bash
dotnet nuget push FoxEndpoints/bin/Release/FoxEndpoints.1.0.0.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

### Test Locally
```bash
# Pack
dotnet pack -c Release FoxEndpoints/FoxEndpoints.csproj

# Install in test project
cd ../TestProject
dotnet add package FoxEndpoints --source ../FoxEndpoints/bin/Release
```

## 📚 Additional Resources

- **NuGet Package Explorer**: https://github.com/NuGetPackageExplorer/NuGetPackageExplorer
- **NuGet Documentation**: https://docs.microsoft.com/en-us/nuget/
- **Semantic Versioning**: https://semver.org/
- **Creating Packages**: https://docs.microsoft.com/en-us/nuget/create-packages/creating-a-package-dotnet-cli

---

## ✅ Summary

Your FoxEndpoints project is now ready to be packaged and published! 

**Multi-targeting**: ✅ .NET 9 and .NET 10  
**NuGet metadata**: ✅ Complete  
**README included**: ✅ Yes  
**Symbols package**: ✅ Enabled  

Just update the repository URLs and you're ready to publish! 🎉