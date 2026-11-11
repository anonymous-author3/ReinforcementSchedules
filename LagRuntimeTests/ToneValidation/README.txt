TONO DURANTE TIME OUT LAG

Listo para probar: bin\LagValidation\ReinforcementSchedules.exe
La fuente del proyecto tambien esta actualizada; volver a compilar desde Visual Studio genera la version normal.
Lag Schedules > Time Out (seconds) > Tono. La casilla inicia desmarcada.
Usa la duracion TO; no es el pulso corto del firmware. Se cancela al finalizar componente/sesion.
Los protocolos XML anteriores cargan con Tono desactivado.

Validacion: motor Lag 50192, oraculo 15049, editor/guardado 15, intervalos 599 mas dominio exhaustivo, referencias 40.
Sin hardware, sin red, sin datos de sesiones reales. Falta probar en una caja vacia.
No se reemplazaron Debug/Release ni se cargo firmware.
Respaldo: backups\timeout-tone-20261111-113833.

Compatibilidad TXT: el ensayo usa la cabecera heredada admitida por LoadLegacyConfiguration. Los snapshots actuales con mapa Arduino son registros de auditoria; su cabecera no es compatible con ese lector preexistente. Guardar/cargar los protocolos actuales en XML.
