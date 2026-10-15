param(
    [string]$BaseUrl = "http://localhost:5171",
    [int]$MaxBodyLength = 200
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [string]$Body
    )

    $arguments = @{
        Uri             = "$BaseUrl$Path"
        Method          = $Method
        ContentType     = "application/json; charset=utf-8"
        UseBasicParsing = $true
    }

    if ($PSBoundParameters.ContainsKey("Body")) {
        $arguments.Body = [System.Text.Encoding]::UTF8.GetBytes($Body)
    }

    try {
        $response = Invoke-WebRequest @arguments
        return [pscustomobject]@{
            Status      = [int]$response.StatusCode
            ContentType = [string]$response.Headers["Content-Type"]
            Body        = $response.Content
        }
    }
    catch {
        $status = 0
        $contentType = ""
        $text = $_.Exception.Message
        $response = $_.Exception.Response

        if ($response) {
            $status = [int]$response.StatusCode

            if ($response.GetType().Name -eq "HttpResponseMessage") {
                $contentType = [string]$response.Content.Headers.ContentType
                $text = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            }
            else {
                $contentType = [string]$response.ContentType
                $reader = New-Object System.IO.StreamReader($response.GetResponseStream(), [System.Text.Encoding]::UTF8)
                $text = $reader.ReadToEnd()
                $reader.Dispose()
            }
        }

        if (-not $text) {
            $text = "(тело ответа пустое)"
        }

        return [pscustomobject]@{
            Status      = $status
            ContentType = $contentType
            Body        = $text
        }
    }
}

function Show-Result {
    param(
        [string]$Label,
        [pscustomobject]$Response
    )

    $body = $Response.Body.ToString().Trim()

    if ($Response.ContentType -like "text/html*") {
        $body = " статический файл отдан из wwwroot"
    }

    $whitespace = $body -replace '\s+', " "

    if ($whitespace.Length -gt $MaxBodyLength) {
        $whitespace = $whitespace.Substring(0, $MaxBodyLength) + "..."
    }

    $color = if ($Response.Status -ge 200 -and $Response.Status -lt 300) { "Green" } else { "Yellow" }

    Write-Host ("{0,-44} {1,-5}" -f $Label, $Response.Status) -ForegroundColor $color -NoNewline
    Write-Host (" [{0}]" -f ($Response.ContentType -replace "; charset=utf-8", "")) -ForegroundColor DarkGray
    Write-Host ("       {0}" -f $whitespace) -ForegroundColor Gray
}

Write-Host "Проверка API: $BaseUrl" -ForegroundColor Cyan

Show-Result "GET  /" (Invoke-Api "GET" "/")
Show-Result "GET  /index.html" (Invoke-Api "GET" "/index.html")
Show-Result "GET  /css/styles.css" (Invoke-Api "GET" "/css/styles.css")
Show-Result "GET  /api/hello" (Invoke-Api "GET" "/api/hello")
Show-Result "GET  /api/server-info" (Invoke-Api "GET" "/api/server-info")
Start-Sleep -Milliseconds 1100
Show-Result "GET  /api/server-info (повтор)" (Invoke-Api "GET" "/api/server-info")
Show-Result "GET  /api/sum?a=10&b=15" (Invoke-Api "GET" "/api/sum?a=10&b=15")
Show-Result "GET  /api/sum?a=-2.5&b=0.5" (Invoke-Api "GET" "/api/sum?a=-2.5&b=0.5")
Show-Result "GET  /api/sum?a=8&b=4" (Invoke-Api "GET" "/api/sum?a=8&b=4")
Show-Result "GET  /api/sum (без параметров)" (Invoke-Api "GET" "/api/sum")
Show-Result "GET  /api/sum?a=5 (один параметр)" (Invoke-Api "GET" "/api/sum?a=5")
Show-Result "GET  /api/divide?a=10&b=2" (Invoke-Api "GET" "/api/divide?a=10&b=2")
Show-Result "GET  /api/divide?a=10&b=0" (Invoke-Api "GET" "/api/divide?a=10&b=0")
Show-Result "GET  /api/divide?a=abc&b=2" (Invoke-Api "GET" "/api/divide?a=abc&b=2")
Show-Result "GET  /api/divide?a=7.5&b=2.5" (Invoke-Api "GET" "/api/divide?a=7.5&b=2.5")
Show-Result "POST /api/echo hello" (Invoke-Api "POST" "/api/echo" '{"text":"hello"}')
Show-Result "POST /api/echo пустая строка" (Invoke-Api "POST" "/api/echo" '{"text":""}')
Show-Result "POST /api/echo кириллица" (Invoke-Api "POST" "/api/echo" '{"text":"привет"}')
Show-Result "POST /api/echo без тела" (Invoke-Api "POST" "/api/echo")
Show-Result "GET  /api/tasks" (Invoke-Api "GET" "/api/tasks")
Show-Result "POST /api/tasks" (Invoke-Api "POST" "/api/tasks" '{"title":"Купить хлеб"}')
Show-Result "POST /api/tasks пустой title" (Invoke-Api "POST" "/api/tasks" '{"title":"   "}')
Show-Result "GET  /api/tasks (после добавления)" (Invoke-Api "GET" "/api/tasks")
Show-Result "PUT  /api/tasks/1 isDone=true" (Invoke-Api "PUT" "/api/tasks/1" '{"isDone":true}')
Show-Result "PUT  /api/tasks/1 isDone=false" (Invoke-Api "PUT" "/api/tasks/1" '{"isDone":false}')
Show-Result "PUT  /api/tasks/999" (Invoke-Api "PUT" "/api/tasks/999" '{"isDone":true}')
Show-Result "DELETE /api/tasks/2" (Invoke-Api "DELETE" "/api/tasks/2")
Show-Result "DELETE /api/tasks/999" (Invoke-Api "DELETE" "/api/tasks/999")
Show-Result "GET  /api/tasks (после удаления)" (Invoke-Api "GET" "/api/tasks")
Show-Result "GET  /api/unknown (404)" (Invoke-Api "GET" "/api/unknown")
Show-Result "GET  /api/tasks строкой id" (Invoke-Api "GET" "/api/tasks/abc")

Write-Host "Проверка завершена" -ForegroundColor Cyan
