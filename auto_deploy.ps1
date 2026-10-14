$source = "bin\Release\net35\ArfTattleHook.dll"
$dest = "C:\Users\IAGOXR\Desktop\Tattletail\Mods\ArfTattleHook.dll"

Write-Host "[AutoDeploy] Esperando a que el proceso 'tattletailWindows' se cierre si está abierto..."
while (Get-Process -Name "tattletailWindows" -ErrorAction SilentlyContinue) {
    Start-Sleep -Seconds 1
}

Start-Sleep -Milliseconds 500
try {
    Copy-Item -Path $source -Destination $dest -Force
    Write-Host "[AutoDeploy] ¡DLL copiada con éxito a Tattletail Mods\ArfTattleHook.dll!"
} catch {
    Write-Host "[AutoDeploy] Error al copiar: $_"
}
