param(
    [string]$BaseUrl = "https://localhost:44380",
    [string]$ValidUsername = "",
    [string]$ValidPassword = "",
    [string]$BruteForceUsername = "",
    [string]$BruteForceWrongPassword = "WrongPassword!123",
    [int]$BruteForceAttempts = 6,
    [int]$ChatBurstCount = 25
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-StatusCodeFromException {
    param([System.Exception]$Exception)

    if ($Exception -and $Exception.Response -and $Exception.Response.StatusCode) {
        return [int]$Exception.Response.StatusCode
    }

    if ($Exception -and $Exception.Exception -and $Exception.Exception.Response -and $Exception.Exception.Response.StatusCode) {
        return [int]$Exception.Exception.Response.StatusCode
    }

    return -1
}

function Get-BodyFromException {
    param([System.Exception]$Exception)

    $response = $null
    if ($Exception -and $Exception.Response) {
        $response = $Exception.Response
    }
    elseif ($Exception -and $Exception.Exception -and $Exception.Exception.Response) {
        $response = $Exception.Exception.Response
    }

    if (-not $response) {
        return ""
    }

    try {
        $stream = $response.GetResponseStream()
        if (-not $stream) {
            return ""
        }

        $reader = New-Object System.IO.StreamReader($stream)
        return $reader.ReadToEnd()
    }
    catch {
        return ""
    }
}

function Extract-AntiForgeryToken {
    param([string]$Html)
    $match = [regex]::Match($Html, 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"')
    if (-not $match.Success) {
        throw "Không tìm thấy __RequestVerificationToken trong form."
    }
    return $match.Groups[1].Value
}

function Invoke-Login {
    param(
        [Microsoft.PowerShell.Commands.WebRequestSession]$Session,
        [string]$Username,
        [string]$Password,
        [string]$ReturnUrl = "",
        [bool]$IncludeToken = $true
    )

    $loginUrl = "$BaseUrl/Authentication/Login"
    if (-not [string]::IsNullOrWhiteSpace($ReturnUrl)) {
        $loginUrl += "?returnUrl=$([Uri]::EscapeDataString($ReturnUrl))"
    }

    $page = Invoke-WebRequest -Uri $loginUrl -Method GET -WebSession $Session
    $form = @{
        Username = $Username
        Password = $Password
    }

    if ($IncludeToken) {
        $token = Extract-AntiForgeryToken -Html $page.Content
        $form["__RequestVerificationToken"] = $token
    }

    return Invoke-WebRequest -Uri $loginUrl -Method POST -Body $form -WebSession $Session -MaximumRedirection 10
}

Write-Host "==== SECURITY EVIDENCE RUN ===="
Write-Host "BaseUrl: $BaseUrl"
Write-Host ""

# 1) CSRF success/fail
if (-not [string]::IsNullOrWhiteSpace($ValidUsername) -and -not [string]::IsNullOrWhiteSpace($ValidPassword)) {
    Write-Host "[SEC-01A] CSRF normal login with valid token"
    try {
        $sessionSuccess = New-Object Microsoft.PowerShell.Commands.WebRequestSession
        $ok = Invoke-Login -Session $sessionSuccess -Username $ValidUsername -Password $ValidPassword -IncludeToken $true
        Write-Host "  Status: $($ok.StatusCode)"
        Write-Host "  FinalUrl: $($ok.BaseResponse.ResponseUri.AbsoluteUri)"
    }
    catch {
        Write-Host "  Failed unexpectedly. Status: $(Get-StatusCodeFromException $_)"
    }

    Write-Host "[SEC-01B] CSRF attack without anti-forgery token"
    try {
        $sessionNoToken = New-Object Microsoft.PowerShell.Commands.WebRequestSession
        $noToken = Invoke-Login -Session $sessionNoToken -Username $ValidUsername -Password $ValidPassword -IncludeToken $false
        Write-Host "  Unexpected pass. Status: $($noToken.StatusCode)"
    }
    catch {
        $status = Get-StatusCodeFromException $_
        Write-Host "  Blocked as expected. Status: $status"
    }
}
else {
    Write-Host "[SEC-01] Skipped (thiếu ValidUsername/ValidPassword)."
}

Write-Host ""

# 2) Brute-force lockout
if (-not [string]::IsNullOrWhiteSpace($BruteForceUsername)) {
    Write-Host "[SEC-02] Brute-force lockout"
    $sessionBrute = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    for ($i = 1; $i -le $BruteForceAttempts; $i++) {
        try {
            $resp = Invoke-Login -Session $sessionBrute -Username $BruteForceUsername -Password $BruteForceWrongPassword -IncludeToken $true
            Write-Host ("  Attempt {0}: HTTP {1}" -f $i, $resp.StatusCode)
        }
        catch {
            $status = Get-StatusCodeFromException $_
            $body = Get-BodyFromException $_
            $preview = if ($body.Length -gt 120) { $body.Substring(0, 120) + "..." } else { $body }
            Write-Host ("  Attempt {0}: HTTP {1} | {2}" -f $i, $status, $preview)
        }
    }
}
else {
    Write-Host "[SEC-02] Skipped (thiếu BruteForceUsername)."
}

Write-Host ""

# 3) Open redirect blocked
if (-not [string]::IsNullOrWhiteSpace($ValidUsername) -and -not [string]::IsNullOrWhiteSpace($ValidPassword)) {
    Write-Host "[SEC-03] Open redirect blocked"
    try {
        $sessionRedirect = New-Object Microsoft.PowerShell.Commands.WebRequestSession
        $redirectResult = Invoke-Login -Session $sessionRedirect -Username $ValidUsername -Password $ValidPassword -ReturnUrl "https://evil.com" -IncludeToken $true
        $finalUrl = $redirectResult.BaseResponse.ResponseUri.AbsoluteUri
        Write-Host "  FinalUrl: $finalUrl"
        if ($finalUrl -like "https://evil.com*") {
            Write-Host "  Unexpected: redirected to external URL."
        }
        else {
            Write-Host "  Passed: stayed on local domain."
        }
    }
    catch {
        Write-Host "  Failed. Status: $(Get-StatusCodeFromException $_)"
    }
}
else {
    Write-Host "[SEC-03] Skipped (thiếu ValidUsername/ValidPassword)."
}

Write-Host ""

# 4) Chatbot rate limit + sanitize
Write-Host "[SEC-04] Chatbot rate-limit burst"
$chatSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
for ($i = 1; $i -le $ChatBurstCount; $i++) {
    try {
        $r = Invoke-WebRequest -Uri "$BaseUrl/ChatBot/Ask" -Method POST -Body @{ message = "test $i" } -WebSession $chatSession
        Write-Host ("  Message {0}: HTTP {1}" -f $i, $r.StatusCode)
    }
    catch {
        $status = Get-StatusCodeFromException $_
        Write-Host ("  Message {0}: HTTP {1}" -f $i, $status)
    }
}

Write-Host "[SEC-05] Chatbot length validation"
$longMessage = ("A" * 501)
try {
    $longResp = Invoke-WebRequest -Uri "$BaseUrl/ChatBot/Ask" -Method POST -Body @{ message = $longMessage } -WebSession $chatSession
    Write-Host "  Unexpected pass. HTTP $($longResp.StatusCode)"
}
catch {
    Write-Host "  Blocked as expected. HTTP $(Get-StatusCodeFromException $_)"
}

Write-Host "[SEC-06] Chatbot empty/control chars validation"
try {
    $emptyResp = Invoke-WebRequest -Uri "$BaseUrl/ChatBot/Ask" -Method POST -Body @{ message = "`r`n`t" } -WebSession $chatSession
    Write-Host "  HTTP $($emptyResp.StatusCode)"
    Write-Host "  Body: $($emptyResp.Content)"
}
catch {
    Write-Host "  HTTP $(Get-StatusCodeFromException $_)"
}

Write-Host ""
Write-Host "Run completed. Mở App_Data/security-audit.log để chụp minh chứng log."
