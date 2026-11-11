# Corrección de FI y VI

Fecha: 10 de noviembre de 2026. Proyecto: ReinforcementSchedules_review.

## Resultado

- FI ya no añade un segundo. Por ejemplo, FI 30 programa Timer.Interval = 30000 ms, no 31000 ms.
- VI conserva los diez valores Fleshler-Hoffman como Double en segundos hasta convertir cada uno al milisegundo más cercano. Se eliminó el segundo añadido y el redondeo previo a segundos enteros.
- La selección aleatoria uniforme y eliminación de cada elemento mantienen muestreo sin reemplazo: cada bloque completo contiene una vez cada uno de los diez valores. Cada entrada y cámara mantiene su propia lista. La lista se reinicia al comenzar cada presentación del componente, como antes.
- La disponibilidad no entrega automáticamente el reforzador: la primera respuesta elegible posterior lo obtiene, conservando la contingencia FI/VI.
- Se agregaron validaciones en el editor, importación TXT/XML y configuración de sesión para rechazar valores no representables antes de iniciar. El importador TXT ya no redondea decimales FI/VI silenciosamente.

## Precisión y límites

Los valores de entrada FI/VI continúan siendo segundos enteros, como en el diseño original. No se cambió el tipo Integer() guardado ni el formato XML v4. FI admite 1 a 2147483 s. VI admite una media de 1 a 650243 s para que incluso el mayor valor de su lista quepa en el temporizador.

La conversión al temporizador admite 1 a 2147483647 ms y redondea una sola vez al ms más cercano (mitades hacia arriba). El error de cuantización individual es como máximo 0.5 ms. Esto no es una afirmación sobre la precisión física del sistema.

Para VI 30, la lista programada, ordenada aquí sólo para inspección, es:
1553, 4893, 8653, 12953, 17977, 24017, 31597, 41791, 57489, 99078 ms.
Su media matemática antes de cuantizar es 30 s. Su media programada tras cuantizar es 30000.1 ms. La media de un bloque incompleto o de una sesión truncada no tiene por qué coincidir con la media solicitada.

Windows Forms Timer depende de la cola de mensajes y del planificador de Windows. Un valor configurado en milisegundos no constituye una garantía de resolución o latencia física. En cinco observaciones locales de FI 1000 ms se obtuvieron 1025.223, 1034.051, 1008.565, 1017.779 y 1006.331 ms. Son observaciones de software sin hardware, no una calibración ni un límite de error general. USB, firmware, respuesta y entrega pueden añadir latencia; deben medirse aparte.

## Compatibilidad y alcance

Se mantiene la lectura de configuraciones previas válidas, el almacenamiento de enteros, clonación y XML v4. Las pruebas de compatibilidad existentes para versiones anteriores siguen pasando. Una configuración histórica con valor nominal FI 30 ahora programa 30 s en lugar del antiguo 31 s: la compatibilidad del archivo no conserva el error temporal anterior. No mezclar esta versión con registros anteriores sin identificar qué ejecutable produjo cada sesión.

No se modificaron FT/VT, razón, Lag, firmware, datos experimentales, nube ni las carpetas de ejecutables Debug, Release o LagValidation. Las regresiones verifican que las funciones existentes de Lag y almacenamiento se conservan. Esta corrección no constituye una validación temporal de FT/VT.

## Pruebas finalizadas

- Compilación Release .NET Framework 4.7.2: correcta, sin errores ni avisos mostrados.
- FI: prueba exhaustiva de todos los 2147483 valores enteros admitidos, conversión exacta.
- VI: prueba exhaustiva de 650243 medias, 6502430 intervalos, comparados con un oráculo independiente basado en integrales de intervalos equiprobables de una distribución exponencial. Se comprobaron media y cuantización.
- 599 aserciones de frontera e integración: respuesta antes/después de disponibilidad, rearme FI, seis entradas VI, 30 bloques por entrada, cámaras independientes, reposición de listas, 10000 ciclos aleatorios, límites, rechazo de XML inválido y de textos decimales no admitidos, editor, clonación y XML.
- Regresión Lag runtime: 50171 aserciones.
- Regresión independiente de Lag y XML: 15049 aserciones.
- Regresión de almacenamiento/UI: 82 aserciones.
- Regresión visual del editor: 31 comprobaciones.
- Revisión independiente de diferencias y algoritmos: sin hallazgos accionables.

No se inició ninguna sesión con hardware ni se usaron datos experimentales reales. Los logs completos están en IntervalTests.

## Ejecutable y respaldo

Ejecutar la nueva versión desde:
C:\ReinforcementSchedules_review\bin\IntervalValidation\ReinforcementSchedules.exe

SHA256 del ejecutable:
11D30A0693F2CB5A90C09EA22BBE81F53100E8DC4BAECC79B0454C772AE9CF14

El tamaño Release puede ser menor que el de LagValidation Debug porque esta compilación utiliza optimización; incluye las funciones actuales de Lag, feedback y reporte de secuencias.

Respaldo previo (206 archivos; manifiesto BACKUP_SHA256.json):
C:\ReinforcementSchedules_before_interval_fix_20261110_141733

Los cambios están en IntervalTests/interval-fix.patch. El código corregido está en el proyecto original indicado. Para repetir la validación, usar MSBuild de Visual Studio con salida de aplicación bin\IntervalValidation y salida intermedia obj\IntervalValidation, compilar IntervalTests/IntervalTests.vbproj y ejecutar IntervalTests/bin/ReinforcementSchedules.StorageTests.exe desde la raíz del proyecto. Las dos carpetas de regresión dentro de IntervalTests apuntan a esa misma compilación.
