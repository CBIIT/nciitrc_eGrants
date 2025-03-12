# Configuration
$port = 8081
#$inputFile = "D:\Content\egrants-web-dev.nci.nih.gov\test.doc"

# local dev : "C:\Users\hooverrl\Desktop\NCI\nciitrc_eGrants\source-aspnet\egrants_new\DocPDFConversion"
# server : "D:\Content\egrants-web-dev.nci.nih.gov\DocPDFConversion"
$baseDir = "C:\Users\hooverrl\Desktop\NCI\nciitrc_eGrants\source-aspnet\egrants_new\DocPDFConversion"
#$baseDir = "D:\Content\egrants-web-dev.nci.nih.gov\DocPDFConversion"

# $outputDir = "D:\Content\egrants-web-dev.nci.nih.gov\output"
$outputDir = Join-Path -Path $baseDir -ChildPath "output"
$inputDir = Join-Path -Path $baseDir -ChildPath "input"

#$outputFile = "$outputDir\test.pdf"
$libreOfficePath = "C:\Program Files\LibreOffice\program\soffice.exe"

#$logFile = "D:\Content\egrants-web-dev.nci.nih.gov\conversion.log"
$logDir = Join-Path -Path $baseDir -ChildPath "logs"
$logFile = Join-Path -Path $logDir -ChildPath "conversion.log"
#$inputDir = "D:\Content\egrants-web-dev.nci.nih.gov\input"

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


		# from web
			#$filename = "received_file.dat"
			
			# example : "05d84643-39e7-480c-8085-6dcbdec97514.doc"
			$filenameBase = "$(New-Guid)"
			$filename = "$filenameBase.doc"
			Add-Content $logFile "filename : $filename"
			#$filepath = Join-Path -Path $PSScriptRoot -ChildPath $filename
			
			# example : "D:\Content\egrants-web-dev.nci.nih.gov\input" + "05d84643-39e7-480c-8085-6dcbdec97514.doc"
			$inputFile = Join-Path -Path $inputDir -ChildPath $filename
			
			$stream = $request.InputStream
			#$reader = New-Object System.IO.StreamReader($inputStream)
			#$fileContent = $reader.ReadToEnd()
			$fileStream = [System.IO.File]::Create($inputFile)	
			$stream.CopyTo($fileStream)

			$fileStream.Close()
			$stream.Close()
		# end from web

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