param(
	[parameter(Mandatory=$false)]
    [string]$dllConfigPath = ".\SmartScript\specflow.actions.json",
    [parameter(Mandatory=$true)]
    [string]$browser
	)


(Get-Content $dllConfigPath | Out-String) -replace "browser": "(.*)`"","browser": "(.*)`""$env"" | out-file -encoding ascii $dllConfigPath 

Write-Output "Browser updated to $browser"