[CmdletBinding()]
param([Parameter(Mandatory = $true)][ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version)

# Called by package-release.ps1 after bilingual version/history validation.
# No restore or build is permitted in this promotion path.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = Split-Path -Parent $PSScriptRoot
$packages = Join-Path $root 'artifacts\packages'
$name = "CodexUsageTrayLite-v$Version-win-x64"
$candidate = Join-Path $packages "CodexUsageTrayLite-v$Version-local-win-x64.zip"
$output = Join-Path $packages ($name + '.zip')
if ((Test-Path -LiteralPath $output) -or (Test-Path -LiteralPath ($output + '.sha256'))) {
    throw "Formal package already exists: $output"
}
$allowed = @(
    'CHANGELOG.md', 'CHANGELOG.zh-CN.md', 'CodexUsageTrayLite.exe',
    'CodexUsageTrayLite.exe.config', 'LICENSE.txt', 'THIRD_PARTY_NOTICES.md',
    'third-party/codex-usage-monitor-MIT.txt', 'third-party/codex-usage-widget-MIT.txt',
    'third-party/WebView2-LICENSE.txt', 'third-party/WebView2-NOTICE.txt',
    'Microsoft.Web.WebView2.Core.dll', 'README.txt', 'README.zh-CN.txt',
    'RELEASE_NOTES.md', 'RELEASE_NOTES.zh-CN.md',
    'ja-JP/CodexUsageTrayLite.resources.dll', 'ko-KR/CodexUsageTrayLite.resources.dll',
    'runtimes/win-x64/native/WebView2Loader.dll', 'SHA256SUMS.txt', 'WebView2Loader.dll',
    'zh-CN/CodexUsageTrayLite.resources.dll', 'zh-TW/CodexUsageTrayLite.resources.dll'
)
$documents = @('README.txt', 'README.zh-CN.txt', 'CHANGELOG.md', 'CHANGELOG.zh-CN.md', 'RELEASE_NOTES.md', 'RELEASE_NOTES.zh-CN.md')
$runtimeFiles = @($allowed | Where-Object { $_ -match '\.(exe|dll|config)$' })
function Read-ArchiveText($entry) {
    $reader = [IO.StreamReader]::new($entry.Open())
    try { $reader.ReadToEnd() } finally { $reader.Dispose() }
}
function Get-EntryHash($entry) {
    $stream = $entry.Open()
    $sha = [Security.Cryptography.SHA256]::Create()
    try { [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '').ToLowerInvariant() }
    finally { $stream.Dispose(); $sha.Dispose() }
}
function Assert-Archive($path, $expectedHashes) {
    $archive = [IO.Compression.ZipFile]::OpenRead($path)
    try {
        $files = @($archive.Entries | Where-Object { -not $_.FullName.Replace('\', '/').EndsWith('/') })
        $names = @($files | ForEach-Object { $_.FullName.Replace('\', '/') })
        if ($names.Count -ne $allowed.Count -or (Compare-Object ($allowed | Sort-Object) ($names | Sort-Object))) {
            throw 'Archive does not match the exact package allowlist.'
        }
        $entries = @{}
        foreach ($entry in $files) { $entries.Add($entry.FullName.Replace('\', '/'), $entry) }
        $manifest = Read-ArchiveText $entries['SHA256SUMS.txt']
        $hashes = @{}
        foreach ($line in ($manifest -split '\r?\n' | Where-Object { $_.Trim() })) {
            if ($line -cnotmatch '^([a-f0-9]{64})  (.+)$') { throw 'Malformed internal manifest.' }
            $hash = $Matches[1]; $file = $Matches[2]
            if (-not $entries.ContainsKey($file) -or $file -eq 'SHA256SUMS.txt' -or $hashes.ContainsKey($file)) { throw 'Unexpected or duplicate manifest entry.' }
            if ((Get-EntryHash $entries[$file]) -cne $hash) { throw "Internal hash mismatch: $file" }
            $hashes.Add($file, $hash)
        }
        if ($hashes.Count -ne ($allowed.Count - 1)) { throw 'Incomplete internal manifest.' }
        if ($null -ne $expectedHashes) {
            foreach ($file in $expectedHashes.Keys) {
                if ($file -notin $documents -and $hashes[$file] -cne $expectedHashes[$file]) {
                    throw "Promotion changed an immutable payload file: $file"
                }
            }
        }
        return $hashes
    } finally { $archive.Dispose() }
}

$candidateHash = (Get-FileHash -LiteralPath $candidate -Algorithm SHA256).Hash.ToLowerInvariant()
$sidecar = (Get-Content -LiteralPath ($candidate + '.sha256') -Raw).Trim()
if ($sidecar -cne ($candidateHash + '  ' + [IO.Path]::GetFileName($candidate))) { throw 'Candidate sidecar mismatch.' }
$candidateHashes = Assert-Archive $candidate $null
$testDirectory = Join-Path $root 'tests\CodexUsageTrayLite.Tests\bin\x64\Release\net48'
$buildDirectory = Join-Path $root 'src\CodexUsageTrayLite\bin\x64\Release\net48'
foreach ($file in $runtimeFiles) {
    foreach ($directory in @($testDirectory, $buildDirectory)) {
        $path = Join-Path $directory $file
        if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $candidateHashes[$file]) {
            throw "Test/build binary differs from candidate; do not rebuild during promotion: $file"
        }
    }
}
$fileVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $buildDirectory 'CodexUsageTrayLite.exe')).FileVersion
if ($fileVersion -ne "$Version.0") { throw "Candidate version does not match $Version." }
Write-Host 'Re-running existing tests against candidate-identical binaries; no build.'
$testOutput = & (Join-Path $testDirectory 'CodexUsageTrayLite.Tests.exe') 2>&1
$testExitCode = $LASTEXITCODE
$testOutput | ForEach-Object { Write-Host $_ }
if ($testExitCode -ne 0) { throw 'Candidate tests failed.' }
$passed = $testOutput | Where-Object { $_ -match '^Passed:\s+(\d+)\s*$' } | Select-Object -Last 1
if ($null -eq $passed -or $passed -notmatch '^Passed:\s+(\d+)\s*$') { throw 'Cannot verify test count.' }
$testCount = $Matches[1]
$temporary = Join-Path $packages ('.promotion-' + [Guid]::NewGuid().ToString('N'))
$stage = Join-Path $temporary 'payload'
try {
    New-Item -ItemType Directory -Path $stage -Force | Out-Null
    $archive = [IO.Compression.ZipFile]::OpenRead($candidate)
    try {
        foreach ($entry in $archive.Entries) {
            $relative = $entry.FullName.Replace('\', '/')
            if ($relative.EndsWith('/')) { continue }
            if ($relative -notin $allowed) { throw 'Unexpected candidate path.' }
            $dest = Join-Path $stage $relative
            New-Item -ItemType Directory -Path (Split-Path -Parent $dest) -Force | Out-Null
            [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $dest, $false)
        }
    } finally { $archive.Dispose() }
    foreach ($file in @('CHANGELOG.md','CHANGELOG.zh-CN.md','RELEASE_NOTES.md','RELEASE_NOTES.zh-CN.md')) {
        Copy-Item -LiteralPath (Join-Path $root $file) -Destination (Join-Path $stage $file)
    }
    foreach ($language in @('', '.zh-CN')) {
        $text = Get-Content -LiteralPath (Join-Path $root ('packaging\README-local' + $language + '.txt.in')) -Raw
        $text = $text.Replace('{{VERSION}}', $Version).Replace('{{PACKAGE_VERSION}}', $Version)
        $text = $text.Replace('{{SOURCE_URL}}', "https://github.com/zjwww/Codex-Usage-Tray-Lite/tree/v$Version")
        $text = $text.Replace('{{BASELINE_TAG}}', 'v2.0.0-preview.7').Replace('{{BASELINE_COMMIT}}', '36e9679164dcd7e5ef23d1f35822664785fad01f').Replace('{{TEST_COUNT}}', $testCount)
        if ($text -match '\{\{[A-Z0-9_]+\}\}') { throw 'Unresolved package guide token.' }
        [IO.File]::WriteAllText((Join-Path $stage ('README' + $language + '.txt')), $text, [Text.UTF8Encoding]::new($false))
    }
    $lines = foreach ($file in ($allowed | Where-Object { $_ -ne 'SHA256SUMS.txt' } | Sort-Object)) {
        $hash = (Get-FileHash -LiteralPath (Join-Path $stage $file) -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $file"
    }
    [IO.File]::WriteAllLines((Join-Path $stage 'SHA256SUMS.txt'), $lines, [Text.UTF8Encoding]::new($false))
    $tempZip = Join-Path $temporary ($name + '.zip')
    Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $tempZip -CompressionLevel Optimal
    $formalHashes = Assert-Archive $tempZip $candidateHashes
    if ((Get-FileHash -LiteralPath $candidate -Algorithm SHA256).Hash.ToLowerInvariant() -cne $candidateHash) { throw 'Candidate changed during promotion.' }
    $hash = (Get-FileHash -LiteralPath $tempZip -Algorithm SHA256).Hash.ToLowerInvariant()
    [IO.File]::WriteAllText(($tempZip + '.sha256'), "$hash  $name.zip`r`n", [Text.UTF8Encoding]::new($false))
    # File.Move refuses to replace an existing path, including a concurrent writer.
    [IO.File]::Move($tempZip, $output)
    [IO.File]::Move(($tempZip + '.sha256'), ($output + '.sha256'))
    Write-Host "Package created: $output"
    Write-Host "SHA-256: $hash"
    Write-Host "EXE SHA-256 preserved: $($formalHashes['CodexUsageTrayLite.exe'])"
} finally {
    $boundary = [IO.Path]::GetFullPath($packages).TrimEnd('\') + '\'
    $resolved = [IO.Path]::GetFullPath($temporary).TrimEnd('\') + '\'
    if (-not $resolved.StartsWith($boundary, [StringComparison]::OrdinalIgnoreCase)) { throw 'Temporary path escaped package directory.' }
    if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary -Recurse -Force }
}
