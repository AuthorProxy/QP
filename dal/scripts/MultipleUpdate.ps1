$dir = Convert-Path .
$in = Join-Path $dir "\MultipleUpdate\*.sql"
$out = Join-Path $dir "\MultipleUpdate.sql"
Get-ChildItem $in | Sort Name | Get-Content | Set-Content -Encoding UTF8 -Path $out