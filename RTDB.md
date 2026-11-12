# Archivos locales y Firebase RTDB

La primera vez que se abre **SetUp** aparece una conexión opcional con los cuatro campos del proyecto de Unity: URL de RTDB, Web API key, correo y contraseña. Después de conectarse correctamente, las siguientes aperturas restauran la sesión automáticamente y omiten ese aviso. Cerrar la primera ventana permite trabajar exclusivamente con archivos locales. **Configure RTDB…** permite cambiar la conexión después.

Se recuerdan URL, API key y correo en `%LOCALAPPDATA%\ReinforcementSchedules\connection.json`. El token de renovación se guarda cifrado con la protección de Windows para el usuario actual, asociado a esa base y API key. La contraseña nunca se guarda. Los tokens se renuevan y actualizan automáticamente. Si Firebase invalida la sesión o Windows no puede descifrarla, se vuelve a pedir iniciar sesión; los otros campos permanecen rellenados.

Si se abre sin internet, se conserva la sesión guardada y se reintenta automáticamente, sin pedir los cuatro campos otra vez. Los nuevos archivos se encolan para la base y cuenta recordadas. **Use local only** cierra la sesión, elimina el token guardado y recuerda la elección de modo local en las siguientes aperturas. Para volver a usar RTDB se utiliza **Configure RTDB…**.

## Configuraciones

- **Load** conserva el selector local, admite JSON y mantiene la importación de XML y TXT anteriores.
- **Save** guarda primero el JSON en la carpeta elegida. Si hay una RTDB conectada, también encola una copia para esa base.
- **Load from RTDB…** muestra las configuraciones guardadas desde esta aplicación. Descarga y valida la configuración completa (JSON estructurado nuevo o XML antiguo) antes de aplicarlo. Conserva una copia local en `%LOCALAPPDATA%\ReinforcementSchedules\firebase\<destino>\configs` que también se puede abrir con **Load**.
- El nombre del archivo identifica la configuración en la base, sin distinguir mayúsculas. Guardar de nuevo `Setup.json` actualiza esa configuración; usar otro nombre crea otra entrada. Conviene usar nombres distintos para programas distintos, aunque estén en carpetas locales diferentes.

## Datos de sesión

Los archivos originales continúan en `C:\Data\Raw`, `C:\Data\Summary` y `C:\Data\Charts`. Al finalizar, detener o cerrar una sesión se encolan los registros, el resumen y la gráfica PNG disponible. Los registros y resúmenes se almacenan en RTDB como texto; las gráficas, como base64. El envío ocurre al cerrar los archivos, no por cada respuesta del animal.

El destino queda fijado al iniciar la sesión. Las sesiones iniciadas en modo local permanecen locales. Cambiar de base o cuenta no redirige sesiones anteriores. Los datos pendientes de otra conexión esperan hasta volver a iniciar sesión con la base y cuenta originales.

Las cargas se ejecutan en segundo plano. Si falla la red, los archivos y la cola sobreviven al cierre normal del programa. Se reintenta al conectar, al guardar y cada minuto; **Sync pending files** permite reintentar manualmente. Cerrar el programa no espera a la red: los envíos pendientes se recuperan en la siguiente conexión. La cola está en `%LOCALAPPDATA%\ReinforcementSchedules\firebase\<destino>\pending`. Los archivos de sesión referenciados por esa cola deben permanecer en su ubicación hasta que se confirme el envío.

El UID de cada sesión evita duplicar registros al reintentar una solicitud cuya respuesta se perdió. Si el proceso o equipo se interrumpe antes de finalizar una sesión, sus archivos locales pueden existir sin haberse encolado todavía.

## Preparar Firebase

1. Habilitar el proveedor **Email/Password** en Firebase Authentication y crear la cuenta del investigador.
2. Obtener la URL raíz de RTDB y la Web API key del mismo proyecto.
3. Autorizar esa cuenta para leer y escribir la rama `reinforcementSchedules`. La aplicación usa esta rama propia, por lo que puede coexistir con los datos de Unity.

Ejemplo de reglas para una sola cuenta autorizada. Sustituir el UID e incorporar esta rama a las reglas existentes de la base:

```json
{
  "rules": {
    "reinforcementSchedules": {
      ".read": "auth != null && auth.uid === 'RESEARCHER_UID'",
      ".write": "auth != null && auth.uid === 'RESEARCHER_UID'"
    }
  }
}
```

Estructura remota:

```text
reinforcementSchedules/
  configIndex/<hash del nombre>/     fileName, savedAtUtc, schemaVersion
  configs/<hash del nombre>/         metadatos, setup (objeto JSON) y arrayShapes
  sessions/<UID>/                    metadatos y files/{raw,summary,chart}
  connectionChecks/<UID temporal>/   prueba de acceso, eliminada al conectar
```

La configuración y su índice se actualizan juntos mediante una escritura atómica. Los archivos nuevos usan JSON con `FormatVersion: 6`. La RTDB usa `schemaVersion: 2`, con el objeto de configuración en `setup`, no XML como texto. `arrayShapes` conserva posiciones nulas y arreglos vacíos que RTDB omite. Los registros antiguos con `schemaVersion: 1` y `xml` siguen siendo legibles y no se reescriben automáticamente; las configuraciones de Unity tienen un esquema diferente y no se importan como programas de cajas.

Referencias: [Firebase Authentication REST](https://firebase.google.com/docs/reference/rest/auth), [sesiones de Firebase](https://firebase.google.com/docs/auth/admin/manage-sessions), [protección para el usuario de Windows](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.dataprotectionscope), [escrituras RTDB](https://firebase.google.com/docs/database/rest/save-data), [lecturas RTDB](https://firebase.google.com/docs/database/rest/retrieve-data).

## Compilar y verificar

Desde una consola de desarrollador de Visual Studio con .NET Framework 4.7.2:

```powershell
msbuild ReinforcementSchedules.vbproj /t:Build /p:Configuration=Debug
msbuild StorageTests\StorageTests.vbproj /t:Build /p:Configuration=Debug
.\StorageTests\bin\Debug\ReinforcementSchedules.StorageTests.exe
```

Las pruebas usan un transporte HTTP simulado y archivos temporales dentro de `obj\st`, sin acceder a Firebase ni al Arduino. Comprueban modo local, autenticación, permisos, renovación de tokens, recuperación de pendientes, reintentos idempotentes, aislamiento de destinos, JSON y XML inválidos y el diálogo de inicio. Se requiere una comprobación adicional con la cuenta y reglas reales de la RTDB que se vaya a utilizar.

## Compatibilidad JSON

Guardar siempre crea JSON legible con saltos de línea e indentación de dos espacios. Cargar reconoce el contenido JSON o XML, independientemente de la extensión, y mantiene el importador TXT anterior. Cambiar solo la extensión de un TXT no lo convierte. Para convertir un archivo antiguo, abrirlo y guardarlo como `.json`; el original se conserva si se elige un archivo nuevo.

El nombre completo identifica cada configuración remota: `Setup.xml` y `Setup.json` son entradas distintas. La migración no borra ni reemplaza registros remotos anteriores. Las versiones antiguas de Soft Schedules no podrán cargar los JSON nuevos.

Se conservan las unidades y los índices del modelo: `Components` usa índice 1 para el primer componente; los arreglos de entradas usan índice 0 para la entrada 1. Los tiempos y valores de programas no cambian al convertir. `SavedAtUtc` es metadato de guardado, serializado como fecha JSON de .NET (`/Date(...)/`). No es un tiempo experimental.
