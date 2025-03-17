# Configuration
$port = 8081

$baseDir = ""
# add prod check, unknown at this point
# D:\NCI Websites\egrants.nci.nih.gov\
if (Test-Path "D:\" -and "D:\NCI Websites\" -and "C:\Content\egrants.nci.nih.gov" ) {
    Write-Host "Running from PROD ..."
    $baseDir = "D:\NCI Websites\egrants.nci.nih.gov\DocPDFConversion"
} else if (Test-Path "D:\" -and "D:\Content"  ) {
    Write-Host "Running from non - PROD ..."
    if (Test-Path "D:\Content\egrants.nci.nih.gov\") {
        Write-Host "We seem to be running this on QA or Stage ..."
        $baseDir = "D:\Content\egrants.nci.nih.gov\DocPDFConversion"
    } else if (Test-Path "C:\Content\egrants.nci.nih.gov\DocPDFConversion") {
        Write-Host "We seem to be running this on dev ..."
        $baseDir = "D:\Content\egrants-web-dev.nci.nih.gov\DocPDFConversion"
    }
} else if (Test-Path "C:\" -and "C:\Content" -and "C:\Content\egrants.nci.nih.gov") {
    Write-Host "Found an equivalent location on C:/ to run from ..."
    $baseDir = "C:\Content\egrants.nci.nih.gov\DocPDFConversion"
} else {
    Write-Host "Didn't find any of the shared environment locales, so checking local dev location ..."
    if (Test-Path "C:\" -and "C:\Users" -and "C:\Users\hooverrl\" -and "C:\Users\hooverrl\Desktop" -and "C:\Users\hooverrl\Desktop\NCI" -and "C:\Users\hooverrl\Desktop\NCI\nciitrc_eGrants") {
        # rather than affirm every point on the way to my personal dev space, just try it ...
         $baseDir = "D:\Content\egrants-web-dev.nci.nih.gov\DocPDFConversion"
    }
}

# local dev : "C:\Users\hooverrl\Desktop\NCI\nciitrc_eGrants\source-aspnet\egrants_new\DocPDFConversion"
# server : "D:\Content\egrants-web-dev.nci.nih.gov\DocPDFConversion"
# $baseDir = "D:\Content\egrants-web-dev.nci.nih.gov\DocPDFConversion"

$outputDir = Join-Path -Path $baseDir -ChildPath "output"
$inputDir = Join-Path -Path $baseDir -ChildPath "input"

$libreOfficePath = "C:\Program Files\LibreOffice\program\soffice.exe"

$logDir = Join-Path -Path $baseDir -ChildPath "logs"
$logFile = Join-Path -Path $logDir -ChildPath "conversion.log"

# create directories if they don't exist (mlh : this is mostly for seamless deployment)

# Ensure output directory exists
if (!(Test-Path $baseDir)) {
    New-Item -Path $baseDir -ItemType Directory -Force | Out-Null
}

if (!(Test-Path $outputDir)) {
    New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
}

if (!(Test-Path $logDir)) {
    New-Item -Path $logDir -ItemType Directory -Force | Out-Null
}

if (!(Test-Path $inputDir)) {
    New-Item -Path $inputDir -ItemType Directory -Force | Out-Null
}


# Start HTTP Listener
$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://+:$port/")
$listener.Start()
Add-Content $logFile "$(Get-Date) - Server running on port $port"

Write-Host "Server running at http://localhost:$port/convert"

while ($true) {
    $context = $listener.GetContext()
    $request = $context.Request
    $response = $context.Response
	
    if ($request.Url.AbsolutePath -eq "/convert") {
        Add-Content $logFile "$(Get-Date) - Received conversion request"

		# example : "05d84643-39e7-480c-8085-6dcbdec97514.doc"
		$filenameBase = "$(New-Guid)"
		$filename = "$filenameBase.doc"
		Add-Content $logFile "filename : $filename"
			
		# example : "D:\Content\egrants-web-dev.nci.nih.gov\input" + "05d84643-39e7-480c-8085-6dcbdec97514.doc"
		$inputFile = Join-Path -Path $inputDir -ChildPath $filename
			
		$stream = $request.InputStream
		$fileStream = [System.IO.File]::Create($inputFile)	
		$stream.CopyTo($fileStream)

		$fileStream.Close()
		$stream.Close()

        # Verify input file exists
        if (Test-Path $inputFile) {
            Add-Content $logFile "$(Get-Date) - Converting $inputFile"

            # Run LibreOffice Conversion
            $convertCommand = "`"$libreOfficePath`" --headless --convert-to pdf --outdir `"$outputDir`" `"$inputFile`""
            Add-Content $logFile "$(Get-Date) - Executing: $convertCommand"

            $process = Start-Process -FilePath $libreOfficePath `
                -ArgumentList "--headless --convert-to pdf --outdir `"$outputDir`" `"$inputFile`"" `
                -WorkingDirectory "C:\Program Files\LibreOffice\program\" `
                -NoNewWindow -Wait -PassThru

			# example : "05d84643-39e7-480c-8085-6dcbdec97514.pdf"
			$outputFileName = "$filenameBase.pdf"
			$outputFile = Join-Path -Path $outputDir -ChildPath $outputFileName	

            Add-Content $logFile "$(Get-Date) - LibreOffice Exit Code: $($process.ExitCode)"
			Add-Content $logFile "Checking for output : $outputFile"

            # Check if PDF was created
            if (Test-Path $outputFile) {
                Add-Content $logFile "$(Get-Date) - Conversion successful: $outputFile"
                $pdfBytes = [System.IO.File]::ReadAllBytes($outputFile)

                # Send PDF as HTTP response
                $response.ContentType = "application/pdf"
                $response.ContentLength64 = $pdfBytes.Length
                $response.OutputStream.Write($pdfBytes, 0, $pdfBytes.Length)
            } else {
                Add-Content $logFile "$(Get-Date) - ERROR: Conversion failed"
                $response.StatusCode = 500
                $response.StatusDescription = "Conversion failed."
            }
			
			# clean up
			if (Test-Path $inputFile) {
				Remove-Item $inputFile -verbose
			}
			if (Test-Path $outputFile) {
				Remove-Item $outputFile -verbose
			}
        } else {
            Add-Content $logFile "$(Get-Date) - ERROR: Input file not found: $inputFile"
            $response.StatusCode = 500
            $response.StatusDescription = "Input file not found."
        }
    } else {
        $response.StatusCode = 404
        $errorMsg = [System.Text.Encoding]::UTF8.GetBytes("Not Found")
        $response.OutputStream.Write($errorMsg, 0, $errorMsg.Length)
    }

    $response.OutputStream.Close()
	

}

# Stop listener when script is terminated
$listener.Stop()