[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Split-Path -Parent $scriptDirectory
$projectPath = Join-Path $repositoryRoot 'src\CodexUsageTrayLite\CodexUsageTrayLite.csproj'
$constantsPath = Join-Path $repositoryRoot 'src\CodexUsageTrayLite\AppConstants.cs'
$solutionPath = Join-Path $repositoryRoot 'CodexUsageTrayLite.sln'
$templatePath = Join-Path $repositoryRoot 'packaging\README-local.txt.in'
$templateChinesePath = Join-Path $repositoryRoot 'packaging\README-local.zh-CN.txt.in'
$licensePath = Join-Path $repositoryRoot 'LICENSE'
$changeLogPath = Join-Path $repositoryRoot 'CHANGELOG.md'
$changeLogChinesePath = Join-Path $repositoryRoot 'CHANGELOG.zh-CN.md'
$releaseNotesPath = Join-Path $repositoryRoot 'RELEASE_NOTES.md'
$releaseNotesChinesePath = Join-Path $repositoryRoot 'RELEASE_NOTES.zh-CN.md'
$dotnetPath = Join-Path $repositoryRoot '.tools\dotnet\dotnet.exe'
$packageCache = Join-Path $repositoryRoot '.tools\.nuget\packages'
$dotnetHome = Join-Path $repositoryRoot '.tools\dotnet-home'
$releaseOutput = Join-Path $repositoryRoot 'src\CodexUsageTrayLite\bin\x64\Release\net48'
$testExecutable = Join-Path $repositoryRoot 'tests\CodexUsageTrayLite.Tests\bin\x64\Release\net48\CodexUsageTrayLite.Tests.exe'
$packageOutput = Join-Path $repositoryRoot 'artifacts\packages'

foreach ($requiredPath in @($projectPath, $constantsPath, $solutionPath, $templatePath, $templateChinesePath, $licensePath, $changeLogPath, $changeLogChinesePath, $releaseNotesPath, $releaseNotesChinesePath, $dotnetPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required file is missing: $requiredPath"
    }
}

[xml]$projectXml = Get-Content -LiteralPath $projectPath -Raw
$versionNode = $projectXml.SelectSingleNode('/Project/PropertyGroup/InformationalVersion')
if ($null -eq $versionNode -or [string]::IsNullOrWhiteSpace($versionNode.InnerText)) {
    throw 'InformationalVersion is missing from CodexUsageTrayLite.csproj.'
}
$version = $versionNode.InnerText.Trim()
if ($version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?$') {
    throw "InformationalVersion is not package-safe: $version"
}
if ($version -match '(?i)(?:^|[.-])local(?:$|[.-])') {
    throw "New package versions must not contain the local suffix: $version"
}

$constantsText = Get-Content -LiteralPath $constantsPath -Raw
$escapedVersion = [Regex]::Escape($version)
if ($constantsText -notmatch ('Version\s*=\s*"' + $escapedVersion + '"')) {
    throw "AppConstants.Version does not match InformationalVersion '$version'."
}

$changeLog = Get-Content -LiteralPath $changeLogPath -Raw
$changeLogChinese = Get-Content -LiteralPath $changeLogChinesePath -Raw
$releaseNotes = Get-Content -LiteralPath $releaseNotesPath -Raw
$releaseNotesChinese = Get-Content -LiteralPath $releaseNotesChinesePath -Raw
$currentChangeHeading = '(?m)^## ' + [Regex]::Escape($version) + ' - '
$currentReleaseHeading = '(?m)^# What''s new in ' + [Regex]::Escape($version) + '\s*$'
$currentReleaseChineseHeading = '(?m)^# ' + [Regex]::Escape($version) + ' 本版更新\s*$'
if ($changeLog -notmatch $currentChangeHeading -or $changeLogChinese -notmatch $currentChangeHeading) {
    throw "Both changelogs must begin their current history with version '$version' and no local suffix."
}
if ($releaseNotes -notmatch $currentReleaseHeading -or $releaseNotesChinese -notmatch $currentReleaseChineseHeading) {
    throw "Both release notes must identify current version '$version' and no local suffix."
}
foreach ($history in @($changeLog, $changeLogChinese)) {
    foreach ($requiredMarker in @('## 0.1.0-local - ', '## 0.2.15-local - ', '## 0.2.16 - ', '## 0.2.17 - ')) {
        if (-not $history.Contains($requiredMarker)) {
            throw "The complete changelog history is missing required marker '$requiredMarker'."
        }
    }
    if ($history.Contains('## 0.2.16-local - ') -or $history.Contains('## 0.2.17-local - ')) {
        throw 'Versions 0.2.16 and later must use public version headings without the local suffix.'
    }
}

$packageName = "CodexUsageTrayLite-v$version-win-x64"
if ($packageName -match '(?i)local') {
    throw "The package filename must not contain local: $packageName"
}
$zipPath = Join-Path $packageOutput ($packageName + '.zip')
$zipHashPath = $zipPath + '.sha256'
if ((Test-Path -LiteralPath $zipPath) -or (Test-Path -LiteralPath $zipHashPath)) {
    throw "Package version '$version' already exists. Increase the project version; existing packages are never overwritten."
}

$env:DOTNET_CLI_HOME = $dotnetHome
$env:NUGET_PACKAGES = $packageCache
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'

Write-Host "Restoring $version..."
& $dotnetPath restore $solutionPath --configfile (Join-Path $repositoryRoot 'NuGet.Config') --ignore-failed-sources
if ($LASTEXITCODE -ne 0) {
    throw "Restore failed with exit code $LASTEXITCODE."
}

Write-Host "Building $version..."
& $dotnetPath build $solutionPath -c Release -p:Platform=x64 --no-restore
if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $testExecutable -PathType Leaf)) {
    throw "Test executable was not produced: $testExecutable"
}
Write-Host 'Running automated tests...'
$testOutput = & $testExecutable 2>&1
$testExitCode = $LASTEXITCODE
$testOutput | ForEach-Object { Write-Host $_ }
if ($testExitCode -ne 0) {
    throw "Automated tests failed with exit code $testExitCode."
}
$passedLine = $testOutput | Where-Object { $_ -match '^Passed:\s+(\d+)\s*$' } | Select-Object -Last 1
if ($null -eq $passedLine -or $passedLine -notmatch '^Passed:\s+(\d+)\s*$') {
    throw 'The automated test pass count could not be verified.'
}
$testCount = $Matches[1]

New-Item -ItemType Directory -Path $packageOutput -Force | Out-Null
$temporaryRoot = Join-Path $packageOutput ('.package-temp-' + [Guid]::NewGuid().ToString('N'))
$stagingRoot = Join-Path $temporaryRoot $packageName
$temporaryZip = Join-Path $temporaryRoot ($packageName + '.zip')
$temporaryZipHash = $temporaryZip + '.sha256'
$packageRootFull = [IO.Path]::GetFullPath($packageOutput).TrimEnd('\') + '\'
$temporaryRootFull = [IO.Path]::GetFullPath($temporaryRoot).TrimEnd('\') + '\'
if (-not $temporaryRootFull.StartsWith($packageRootFull, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Temporary package path escaped the artifacts package directory.'
}

try {
    New-Item -ItemType Directory -Path (Join-Path $stagingRoot 'runtimes\win-x64\native') -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $stagingRoot 'ja-JP') -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $stagingRoot 'ko-KR') -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $stagingRoot 'zh-CN') -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $stagingRoot 'zh-TW') -Force | Out-Null

    $packageFiles = @(
        @{ Source = (Join-Path $releaseOutput 'CodexUsageTrayLite.exe'); Destination = (Join-Path $stagingRoot 'CodexUsageTrayLite.exe') },
        @{ Source = (Join-Path $releaseOutput 'CodexUsageTrayLite.exe.config'); Destination = (Join-Path $stagingRoot 'CodexUsageTrayLite.exe.config') },
        @{ Source = (Join-Path $releaseOutput 'Microsoft.Web.WebView2.Core.dll'); Destination = (Join-Path $stagingRoot 'Microsoft.Web.WebView2.Core.dll') },
        @{ Source = (Join-Path $releaseOutput 'WebView2Loader.dll'); Destination = (Join-Path $stagingRoot 'WebView2Loader.dll') },
        @{ Source = (Join-Path $releaseOutput 'runtimes\win-x64\native\WebView2Loader.dll'); Destination = (Join-Path $stagingRoot 'runtimes\win-x64\native\WebView2Loader.dll') },
        @{ Source = (Join-Path $releaseOutput 'ja-JP\CodexUsageTrayLite.resources.dll'); Destination = (Join-Path $stagingRoot 'ja-JP\CodexUsageTrayLite.resources.dll') },
        @{ Source = (Join-Path $releaseOutput 'ko-KR\CodexUsageTrayLite.resources.dll'); Destination = (Join-Path $stagingRoot 'ko-KR\CodexUsageTrayLite.resources.dll') },
        @{ Source = (Join-Path $releaseOutput 'zh-CN\CodexUsageTrayLite.resources.dll'); Destination = (Join-Path $stagingRoot 'zh-CN\CodexUsageTrayLite.resources.dll') },
        @{ Source = (Join-Path $releaseOutput 'zh-TW\CodexUsageTrayLite.resources.dll'); Destination = (Join-Path $stagingRoot 'zh-TW\CodexUsageTrayLite.resources.dll') }
    )
    foreach ($file in $packageFiles) {
        if (-not (Test-Path -LiteralPath $file.Source -PathType Leaf)) {
            throw "Required build output is missing: $($file.Source)"
        }
        Copy-Item -LiteralPath $file.Source -Destination $file.Destination
    }
    Copy-Item -LiteralPath $licensePath -Destination (Join-Path $stagingRoot 'LICENSE.txt')
    Copy-Item -LiteralPath (Join-Path $repositoryRoot 'THIRD_PARTY_NOTICES.md') -Destination (Join-Path $stagingRoot 'THIRD_PARTY_NOTICES.md')
    New-Item -ItemType Directory -Path (Join-Path $stagingRoot 'third-party') -Force | Out-Null
    foreach ($notice in @('codex-usage-monitor-MIT.txt', 'codex-usage-widget-MIT.txt', 'WebView2-LICENSE.txt', 'WebView2-NOTICE.txt')) {
        Copy-Item -LiteralPath (Join-Path $repositoryRoot ('third-party\' + $notice)) -Destination (Join-Path $stagingRoot ('third-party\' + $notice))
    }
    Copy-Item -LiteralPath $changeLogPath -Destination (Join-Path $stagingRoot 'CHANGELOG.md')
    Copy-Item -LiteralPath $changeLogChinesePath -Destination (Join-Path $stagingRoot 'CHANGELOG.zh-CN.md')
    Copy-Item -LiteralPath $releaseNotesPath -Destination (Join-Path $stagingRoot 'RELEASE_NOTES.md')
    Copy-Item -LiteralPath $releaseNotesChinesePath -Destination (Join-Path $stagingRoot 'RELEASE_NOTES.zh-CN.md')

    foreach ($readmeDefinition in @(
        @{ Template = $templatePath; Output = 'README.txt' },
        @{ Template = $templateChinesePath; Output = 'README.zh-CN.txt' }
    )) {
        $readme = Get-Content -LiteralPath $readmeDefinition.Template -Raw
        $readme = $readme.Replace('{{VERSION}}', $version)
        $readme = $readme.Replace('{{BASELINE_TAG}}', 'v2.0.0-preview.7')
        $readme = $readme.Replace('{{BASELINE_COMMIT}}', '36e9679164dcd7e5ef23d1f35822664785fad01f')
        $readme = $readme.Replace('{{TEST_COUNT}}', $testCount)
        if ($readme -match '\{\{[A-Z0-9_]+\}\}') {
            throw "Unresolved token in $($readmeDefinition.Template)."
        }
        $expectedSourceUrl = "https://github.com/zjwww/Codex-Usage-Tray-Lite/tree/v$version"
        if (-not $readme.Contains($expectedSourceUrl)) {
            throw "The package README source URL does not match v${version}: $($readmeDefinition.Template)"
        }
        [IO.File]::WriteAllText((Join-Path $stagingRoot $readmeDefinition.Output), $readme, [Text.UTF8Encoding]::new($false))
    }

    $manifestLines = Get-ChildItem -LiteralPath $stagingRoot -Recurse -File |
        Sort-Object FullName |
        ForEach-Object {
            $relativePath = $_.FullName.Substring($stagingRoot.Length + 1).Replace('\', '/')
            $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            "$hash  $relativePath"
        }
    [IO.File]::WriteAllLines((Join-Path $stagingRoot 'SHA256SUMS.txt'), $manifestLines, [Text.UTF8Encoding]::new($false))

    Compress-Archive -Path (Join-Path $stagingRoot '*') -DestinationPath $temporaryZip -CompressionLevel Optimal

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $expectedEntries = @(
        'CHANGELOG.md',
        'CHANGELOG.zh-CN.md',
        'CodexUsageTrayLite.exe',
        'CodexUsageTrayLite.exe.config',
        'LICENSE.txt',
        'THIRD_PARTY_NOTICES.md',
        'third-party/codex-usage-monitor-MIT.txt',
        'third-party/codex-usage-widget-MIT.txt',
        'third-party/WebView2-LICENSE.txt',
        'third-party/WebView2-NOTICE.txt',
        'Microsoft.Web.WebView2.Core.dll',
        'README.txt',
        'README.zh-CN.txt',
        'RELEASE_NOTES.md',
        'RELEASE_NOTES.zh-CN.md',
        'ja-JP/CodexUsageTrayLite.resources.dll',
        'ko-KR/CodexUsageTrayLite.resources.dll',
        'runtimes/win-x64/native/WebView2Loader.dll',
        'SHA256SUMS.txt',
        'WebView2Loader.dll',
        'zh-CN/CodexUsageTrayLite.resources.dll',
        'zh-TW/CodexUsageTrayLite.resources.dll'
    ) | Sort-Object
    $archive = [IO.Compression.ZipFile]::OpenRead($temporaryZip)
    try {
        $actualEntries = $archive.Entries |
            Where-Object { -not $_.FullName.Replace('\', '/').EndsWith('/') } |
            ForEach-Object { $_.FullName.Replace('\', '/') } |
            Sort-Object
    }
    finally {
        $archive.Dispose()
    }
    $entryDifference = Compare-Object -ReferenceObject $expectedEntries -DifferenceObject $actualEntries
    if ($null -ne $entryDifference) {
        throw "ZIP content audit failed:`n$($entryDifference | Out-String)"
    }

    $zipHash = (Get-FileHash -LiteralPath $temporaryZip -Algorithm SHA256).Hash.ToLowerInvariant()
    [IO.File]::WriteAllText($temporaryZipHash, "$zipHash  $($packageName).zip`r`n", [Text.UTF8Encoding]::new($false))

    Move-Item -LiteralPath $temporaryZip -Destination $zipPath
    Move-Item -LiteralPath $temporaryZipHash -Destination $zipHashPath

    Write-Host "Package created: $zipPath"
    Write-Host "SHA-256: $zipHash"
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) {
        $verifiedTemporaryRoot = [IO.Path]::GetFullPath($temporaryRoot).TrimEnd('\') + '\'
        if ($verifiedTemporaryRoot.StartsWith($packageRootFull, [StringComparison]::OrdinalIgnoreCase)) {
            Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
        }
    }
}
