$url = "https://1drv.ws/u/c/306e8eddd430ea6d/IQBLuEPR_OQaQZ_VhD2ZthUnAWNhf87VgCH-cFFXOq16kH0?e=dmj67r"
$base64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($url))
$encoded = "u!" + $base64.TrimEnd('=').Replace('/','_').Replace('+','-')
$directUrl = "https://api.onedrive.com/v1.0/shares/$encoded/root/content"
Write-Host "Direct download URL:"
Write-Host ""
Write-Host $directUrl
