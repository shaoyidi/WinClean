$url = "https://1drv.ms/u/c/306e8eddd430ea6d/IQDmt9mlOSb3SIxrqMNdZCUCAcXPlp3YKfVdP3KdVeTGrR4?e=kJz4c4"
$base64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($url))
$encoded = "u!" + $base64.TrimEnd('=').Replace('/','_').Replace('+','-')
$directUrl = "https://api.onedrive.com/v1.0/shares/$encoded/root/content"
Write-Host "Direct download URL:"
Write-Host $directUrl
