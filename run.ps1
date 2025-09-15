param (
    [string]$Command,
    [string]$Project = "src/Flaggi/Flaggi.csproj",
    [switch]$Push
)

function Show-Help {
    Write-Host "`n🚩 Available commands:" -ForegroundColor Cyan
    Write-Host "  get:version [-Project <Path>]         -> Get version from csproj"
    Write-Host "  tag:version [-Project <Path>] [-Push] -> Create git tag from version"
    Write-Host "`nExamples:" -ForegroundColor Yellow
    Write-Host "  ./run.ps1 get:version -Project src/Flaggi/Flaggi.csproj"
    Write-Host "  ./run.ps1 tag:version -Push"
}

function Get-VersionFromCsproj($csprojPath) {
    if (-not (Test-Path $csprojPath)) {
        Write-Host "❌ Project file not found: $csprojPath" -ForegroundColor Red
        exit 1
    }

    [xml]$xml = Get-Content $csprojPath
    # Search for the first non-empty Version node in any PropertyGroup
    $versionNode = $xml.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1 -ExpandProperty Version
    
    if (-not $versionNode) {
        Write-Host "❌ <Version> not found in $csprojPath" -ForegroundColor Red
        exit 1
    }

    $version = $versionNode.Trim()
    if (-not $version) {
        Write-Host "❌ Version is empty in $csprojPath" -ForegroundColor Red
        exit 1
    }

    return $version
}

switch -Regex ($Command) {
    "^get:version$" {
        $version = Get-VersionFromCsproj $Project
        Write-Host "📦 Version from ${Project}: ${version}" -ForegroundColor Green
    }

    "^tag:version$" {
        $version = Get-VersionFromCsproj $Project
        $tag = "v$version"

        Write-Host "🏷️ Creating git tag: ${tag}" -ForegroundColor Cyan
        git tag $tag

        if ($Push) {
            Write-Host "🚀 Pushing tag ${tag} to origin..." -ForegroundColor Green
            git push origin $tag
        }
        else {
            Write-Host "ℹ️ Use -Push to push tag to remote." -ForegroundColor Yellow
        }
    }

    "help" {
        Show-Help
    }

    default {
        Write-Host "❌ Invalid command." -ForegroundColor Red
        Show-Help
    }
}