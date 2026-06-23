param(
    [switch]$Stage
)

# Sync repo-local skills across tool-specific discovery paths.
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$pairs = @(
    @{
        Source = Join-Path $repoRoot ".agents\skills\ncp-admin-grill-me\SKILL.md"
        Target = Join-Path $repoRoot ".cursor\skills\ncp-admin-grill-me\SKILL.md"
    }
)

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

foreach ($pair in $pairs) {
    if (-not (Test-Path $pair.Source)) {
        throw "Missing source skill file: $($pair.Source)"
    }

    $targetDir = Split-Path $pair.Target -Parent
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir | Out-Null
    }

    $content = [System.IO.File]::ReadAllText($pair.Source)
    $normalized = [System.Text.RegularExpressions.Regex]::Replace($content, "\r?\n", "`r`n")
    [System.IO.File]::WriteAllText($pair.Target, $normalized, $utf8NoBom)

    if ($Stage) {
        git add -- $pair.Target
    }

    Write-Host "Synced $($pair.Target.Substring($repoRoot.Path.Length + 1))"
}
