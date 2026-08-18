# SincroApp

Aplicación de escritorio en **Windows Forms (.NET 8)** que sincroniza archivos CSV/JSON contra una API central: sube productos, clientes, stock y maestros, y baja pedidos pendientes. La pensé como una herramienta liviana que corre desatendida en la bandeja del sistema del cliente.

## Qué hace

`SincroApp` corre como una app de escritorio (normalmente minimizada) que:

- **Ciclo de subida (upload):** monitorea una carpeta local (`PathArchivoTx`) con un `System.Windows.Forms.Timer` configurable. Cuando aparece un CSV/JSON nuevo (productos, clientes, stock, maestros), lo parsea y lo sube por HTTP a la API. Si el envío fue bien mueve el archivo a `ProcessedFolder`; si falla, a `ErrorFolder`.
- **Ciclo de descarga (download):** con otro timer, llama periódicamente a la API para traer pedidos pendientes, genera un CSV por cada uno (`PedidosCsvWriter`) y lo deja en `DownloadFolder`.
- **Login:** `LoginForm` autentica contra `POST /api/auth/login` usando `ApiUsuario` / `ApiClave` de `appconfig.json`.
- Toda la configuración runtime vive en `appconfig.json` (leído/escrito por el singleton `ConfigManager`) — es una app WinForms clásica, sin capas formales, con toda la lógica concentrada en `Form1.cs`.

## Sobre el modelo de ejecución

No es un Windows Service registrado (no hereda de `ServiceBase` ni usa `BackgroundService`/`IHostedService` de .NET Generic Host) — es una app de escritorio que corre en el tray y usa `System.Windows.Forms.Timer` para disparar los ciclos de sincronización a intervalos configurables (`IntervaloSubidaSegundos`, `IntervaloDescargaSegundos`). El patrón que implementa —sincronización desatendida y periódica, con reintentos, carpetas de procesado/error, y logging— es el mismo, solo que empaquetado como app de escritorio en vez de servicio de Windows.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download) con soporte de Windows Desktop (`Microsoft.WindowsDesktop.App` — WinForms solo compila/corre en Windows).
- El proyecto hermano **`IntegradorArchivosApi-Demo`** corriendo en `http://localhost:5080` (esta app no tiene backend propio, todo lo que sincroniza va contra esa API).

## API a la que se conecta

Esta app no tiene base de datos ni backend propio. Todo el flujo (login, subida de CSV/JSON, descarga de pedidos pendientes) se hace vía HTTP contra:

- **`IntegradorArchivosApi-Demo`** → `http://localhost:5080` (ver [`../IntegradorArchivosApi-Demo`](../IntegradorArchivosApi-Demo)). Hay que levantarla **antes** de correr `SincroApp-Demo`, o el login y la sincronización van a fallar por connection refused.

## Credenciales

`appconfig.json` ya viene con estas credenciales precargadas (son las mismas que siembra `IntegradorArchivosApi-Demo`):

| Usuario | Clave |
|---|---|
| `demo` | `Demo123!` |

## Cómo ejecutar

1. Levantar primero **`IntegradorArchivosApi-Demo`**:
   ```bash
   cd ../IntegradorArchivosApi-Demo
   dotnet run
   ```
   Dejarla corriendo en `http://localhost:5080` (crea y siembra la base LocalDB sola, sin pasos manuales).
2. En otra terminal, restaurar y correr `SincroApp-Demo`:
   ```bash
   dotnet restore
   dotnet run --project SincroDatosApp.csproj
   ```
   (o abrir `SincroDatosApp.sln` en Visual Studio y correr con F5 — es una app WinForms, necesita Windows).
3. La app arranca minimizada en la bandeja del sistema. Abrir la ventana principal desde el ícono de la bandeja para ver el login y la configuración.
4. Loguearse con `demo` / `Demo123!` (ya viene precargado en `appconfig.json`, no hace falta tipearlo).
5. Probar el ciclo de subida: soltar un CSV/JSON de ejemplo (ver `csv-templates/`) en la carpeta configurada en `PathArchivoTx` y verificar que se sube a la API y se mueve a `ProcessedFolder`.

## Modelado de entidades

Los DTOs de sincronización (productos, clientes, stock, maestros, etc.) están modelados en `Entidades/EntidadesMultieempresas.cs`, dentro de este mismo repo — el proyecto compila y corre de forma completamente independiente, sin ninguna referencia externa.
