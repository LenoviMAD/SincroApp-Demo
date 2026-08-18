using EntidadesMultieempresas;
using SincroDatosApp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SincroDatosApp
{
    public partial class Form1 : Form
    {
        private AppConfig _config;

        private string WatchFolder => _config.PathArchivoTx;
        private string DownloadFolder => _config.DownloadFolder;

        private LoaderOverlay _loader;

        // 1) Duración (siempre lee config)
        private TimeSpan DuracionSubida => TimeSpan.FromSeconds(_config.IntervaloSubidaSegundos);
        private TimeSpan DuracionDescarga => TimeSpan.FromSeconds(_config.IntervaloDescargaSegundos);

        // 2) Restante (estado mutable)
        private TimeSpan _restanteSubida;
        private TimeSpan _restanteDescarga;

        private int    _empresaID;
        private string _token = "";
        private int EmpresaID => _empresaID;

        private bool _allowClose = false;

        private static string ConfigPath =>
         Path.Combine(AppContext.BaseDirectory, "appconfig.json");


        private ToolStripMenuItem mnuDetener;
        private ToolStripMenuItem mnuSalir;
        private ToolStripMenuItem mnuAbrir;

        public void LogInfo(string msg) => Log($"[INFO] {msg}");
        public void LogWarn(string msg) => Log($"[WARN] {msg}");
        public void LogError(string msg) => Log($"[ERROR] {msg}");

        public Form1()
        {
            InitializeComponent();
            LoadConfig();
            CargarIconos();
        }

        private void CargarIconos()
        {
            string icoPath = Path.Combine(AppContext.BaseDirectory, "vendixIco.ico");
            if (File.Exists(icoPath))
            {
                Icon vendixIcon = new Icon(icoPath);
                this.Icon = vendixIcon;
                notifyIcon1.Icon = vendixIcon;
            }

            string logoPath = Path.Combine(AppContext.BaseDirectory, "vendixLogo.png");
            if (File.Exists(logoPath) && picLogoMain != null)
                picLogoMain.Image = Image.FromFile(logoPath);
        }


        private async void Form1_Load(object sender, EventArgs e)
        {
            EnsureFolders();

            ConfigurarTray();


            nudSubirArchivos.Value = Math.Max(nudSubirArchivos.Minimum,
            Math.Min(nudSubirArchivos.Maximum, _config.IntervaloSubidaSegundos));

            nudDescarga.Value = Math.Max(nudDescarga.Minimum,
            Math.Min(nudDescarga.Maximum, _config.IntervaloDescargaSegundos));

            _loader = new LoaderOverlay(this);

            lblOrigen.Text = WatchFolder;
            lblProcesados.Text = DownloadFolder;

            // Timer 1s (countdown)
            tmrSubida.Interval = 1000;
            tmrDescarga.Interval = 1000;

            // Inicializar restantes con config actual
            _restanteSubida = DuracionSubida;
            _restanteDescarga = DuracionDescarga;

            LogInfo("Aplicación iniciada.");

            RefrescarLabels();

            // Mostrar diálogo de login antes de iniciar timers
            using LoginForm login = new();
            if (login.ShowDialog(this) != DialogResult.OK)
            {
                SalirApp();
                return;
            }
            _empresaID = login.EmpresaID;
            _token     = login.Token;
            LogInfo($"Login OK. EmpresaID={_empresaID}");

            IniciarSubida();
            IniciarDescarga();

            // Cargar archivos en TreeView
            CargarArchivosEnTreeView(WatchFolder);




            // Eventos
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;

            // Arrancar minimizado al tray
            EnviarAlTray();




        }

        private HttpClient CreateHttpClient(Uri? baseAddress = null)
        {
            HttpClient http = new();
            if (!string.IsNullOrEmpty(_token))
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            if (baseAddress != null)
                http.BaseAddress = baseAddress;
            return http;
        }

        // Re-login silencioso usando las credenciales del config (sin UI).
        // Se llama automáticamente cuando un endpoint devuelve 401 (token expirado).
        private async Task<bool> SilentLoginAsync()
        {
            string apiUrl = _config?.ApiUrl?.Trim() ?? "";
            if (string.IsNullOrEmpty(apiUrl)) return false;

            string endpoint = apiUrl.TrimEnd('/') + "/api/auth/login";
            try
            {
                var body = new { usuario = _config!.ApiUsuario, clave = _config.ApiClave };
                string json = JsonSerializer.Serialize(body);
                using HttpClient http = new();
                using StringContent content = new(json, Encoding.UTF8, "application/json");
                HttpResponseMessage resp = await http.PostAsync(endpoint, content);
                if (!resp.IsSuccessStatusCode) return false;

                string respJson = await resp.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(respJson);
                if (!doc.RootElement.TryGetProperty("empresaID", out JsonElement idProp)) return false;
                if (!doc.RootElement.TryGetProperty("token",     out JsonElement tkProp)) return false;

                _empresaID = idProp.GetInt32();
                _token     = tkProp.GetString() ?? "";
                LogInfo("Token renovado automáticamente.");
                return true;
            }
            catch { return false; }
        }

        private void EnviarAlTray()
        {
            this.Hide();
            this.ShowInTaskbar = false; // no aparece en la barra normal
            this.WindowState = FormWindowState.Minimized;

            //notifyIcon1.BalloonTipTitle = "SincroApp Demo";
            //notifyIcon1.BalloonTipText = "Ejecutándose en segundo plano.";
            //notifyIcon1.ShowBalloonTip(1500);
        }

        private void MostrarVentana()
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void notifyIcon1_DoubleClick(object? sender, EventArgs e)
        {
            MostrarVentana();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                EnviarAlTray();
                return;
            }

            base.OnFormClosing(e);
        }

        private void SalirApp()
        {
            _allowClose = true;
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
            this.Close();
        }

        private void ConfigurarTray()
        {
            var menu = new ContextMenuStrip();

            mnuAbrir = new ToolStripMenuItem("Abrir");
            mnuSalir = new ToolStripMenuItem("Salir");

            menu.Items.AddRange(new ToolStripItem[] { mnuAbrir, new ToolStripSeparator(), mnuSalir });

            notifyIcon1.ContextMenuStrip = menu;
            notifyIcon1.Visible = true;

            mnuAbrir.Click += (s, e) => MostrarVentana();
            mnuSalir.Click += (s, e) => SalirApp();        // <- cierra de verdad
        }


        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Arranca minimizado y se queda en el taskbar
            //this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = true;
        }


        private void EnsureFolders()
        {
            try
            {
                CrearCarpetaSiNoExiste(_config.PathArchivoTx, "PathArchivoTx");
                CrearCarpetaSiNoExiste(_config.DownloadFolder, "DownloadFolder");
                CrearCarpetaSiNoExiste(_config.ProcessedFolder, "ProcessedFolder");
                CrearCarpetaSiNoExiste(_config.ErrorFolder, "ErrorFolder");
            }
            catch (Exception ex)
            {
                LogError($"No se pudieron crear las carpetas.\n\n{ex.Message}");

            }
        }

        private void CrearCarpetaSiNoExiste(string ruta, string nombreCampo)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new InvalidOperationException($"{nombreCampo} está vacío.");

            // Normaliza y crea
            ruta = Path.GetFullPath(ruta);
            Directory.CreateDirectory(ruta);
        }

        private void RefrescarCarpetaTxUI()
        {
            // Asegura que se ejecute en el hilo UI
            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefrescarCarpetaTxUI));
                return;
            }

            CargarArchivosEnTreeView(_config.PathArchivoTx);
        }

        private void tmrSubida_Tick(object? sender, EventArgs e)
        {
            if (_restanteSubida.TotalSeconds <= 0)
            {
                tmrSubida.Stop();
                _restanteSubida = TimeSpan.Zero;
                lblIntervaloSubida.Text = "00:00";
                cmdTransmitirTx_Click(sender, e); // Llama al método de transmisión
                // Acá podrías disparar la acción de subida:
                // _ = ProcessPendingFilesAsync();  // si corresponde
                return;
            }

            _restanteSubida = _restanteSubida - TimeSpan.FromSeconds(1);
            lblIntervaloSubida.Text = _restanteSubida.ToString(@"mm\:ss");
        }

        private void tmrDescarga_Tick(object? sender, EventArgs e)
        {
            if (_restanteDescarga.TotalSeconds <= 0)
            {
                tmrDescarga.Stop();
                _restanteDescarga = TimeSpan.Zero;
                lblIntervaloDescarga.Text = "00:00";
                btnDescargarPedido_Click(sender, e);
                // Acá podrías disparar la acción de descarga:
                // _ = ProcessPendingFilesAsync();  // o tu método de descarga
                return;
            }

            _restanteDescarga = _restanteDescarga - TimeSpan.FromSeconds(1);
            lblIntervaloDescarga.Text = _restanteDescarga.ToString(@"mm\:ss");
        }


        private void RefrescarLabels()
        {
            lblIntervaloSubida.Text = _restanteSubida.ToString(@"mm\:ss");
            lblIntervaloDescarga.Text = _restanteDescarga.ToString(@"mm\:ss");
        }

        // ====== CONTROL SUBIDA ======
        public void ConfigurarSubidaSegundos(int segundos)
        {
            if (segundos < 0) segundos = 0;

            // Guardar en config (si tu _config lo permite)
            _config.IntervaloSubidaSegundos = segundos;

            // Resetear restante al nuevo valor
            _restanteSubida = DuracionSubida;
            lblIntervaloSubida.Text = _restanteSubida.ToString(@"mm\:ss");
        }

        public void IniciarSubida()
        {
            // Siempre arranca con el valor actual del config
            _restanteSubida = DuracionSubida;
            lblIntervaloSubida.Text = _restanteSubida.ToString(@"mm\:ss");
            tmrSubida.Start();
        }

        public void PausarSubida() => tmrSubida.Stop();

        public void ResetSubida(bool arrancar = true)
        {
            tmrSubida.Stop();
            _restanteSubida = DuracionSubida;
            lblIntervaloSubida.Text = _restanteSubida.ToString(@"mm\:ss");

            if (arrancar)
                tmrSubida.Start();
        }

        // ====== CONTROL DESCARGA ======
        public void ConfigurarDescargaSegundos(int segundos)
        {
            if (segundos < 0) segundos = 0;

            _config.IntervaloDescargaSegundos = segundos;

            _restanteDescarga = DuracionDescarga;
            lblIntervaloDescarga.Text = _restanteDescarga.ToString(@"mm\:ss");
        }

        public void IniciarDescarga()
        {
            _restanteDescarga = DuracionDescarga;
            lblIntervaloDescarga.Text = _restanteDescarga.ToString(@"mm\:ss");
            tmrDescarga.Start();
        }

        public void PausarDescarga() => tmrDescarga.Stop();

        public void ResetDescarga(bool arrancar = true)
        {
            tmrDescarga.Stop();
            _restanteDescarga = DuracionDescarga;
            lblIntervaloDescarga.Text = _restanteDescarga.ToString(@"mm\:ss");

            if (arrancar)
                tmrDescarga.Start();
        }





        private void CargarArchivosEnTreeView(string carpeta)
        {
            if (string.IsNullOrWhiteSpace(carpeta) || !Directory.Exists(carpeta))
            {
                tvArchivos.Nodes.Clear();
                tvArchivos.Nodes.Add("Carpeta no válida");
                return;
            }

            tvArchivos.BeginUpdate();
            try
            {
                tvArchivos.Nodes.Clear();

                // Nodo raíz (carpeta)
                var root = new TreeNode(Path.GetFileName(carpeta))
                {
                    Tag = carpeta
                };

                // Archivos dentro de la carpeta
                var archivos = Directory.GetFiles(carpeta, "*.*", SearchOption.TopDirectoryOnly)
                                        .OrderBy(f => f);

                foreach (var file in archivos)
                {
                    var node = new TreeNode(Path.GetFileName(file))
                    {
                        Tag = file // guardo la ruta completa por si después querés abrirlo
                    };
                    root.Nodes.Add(node);
                }

                root.Expand();
                tvArchivos.Nodes.Add(root);
            }
            finally
            {
                tvArchivos.EndUpdate();
            }
        }


        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    _config = new AppConfig();
                    SaveConfig(); // crea el archivo por primera vez
                    return;
                }

                var json = File.ReadAllText(ConfigPath);
                _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                _config = new AppConfig();
            }
        }

        private void SaveConfig()
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(ConfigPath, json);
        }

        private void btnCambiarOrigen_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            fbd.Description = "Seleccionar carpeta de entrada (archivos a subir)";
            fbd.SelectedPath = WatchFolder;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                _config.PathArchivoTx = fbd.SelectedPath;
                Directory.CreateDirectory(_config.PathArchivoTx);
                SaveConfig();

                lblOrigen.Text = _config.PathArchivoTx;
            }
        }

        private void btnCambiarProcesados_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            fbd.Description = "Seleccionar carpeta de pedidos descargados";
            fbd.SelectedPath = DownloadFolder;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                _config.ProcessedFolder = fbd.SelectedPath;
                Directory.CreateDirectory(_config.ProcessedFolder);
                SaveConfig();

                lblProcesados.Text = _config.ProcessedFolder;
            }
        }

        public void Log(string msg)
        {
            if (txtLog.IsDisposed) return;

            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action(() => Log(msg)));
                return;
            }

            txtLog.AppendText($"{DateTime.Now:HH:mm:ss} {msg}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }





        private async void cmdTransmitirTx_Click(object sender, EventArgs e)
        {
            try
            {
                cmdTransmitirTx.Enabled = false; // evita doble click / doble ejecución

                var configuredPath = _config?.PathArchivoTx;
                if (string.IsNullOrWhiteSpace(configuredPath))
                {
                    LogError("PathArchivoTx no está configurado.");
                    return;
                }

                Directory.CreateDirectory(_config.ProcessedFolder);
                Directory.CreateDirectory(_config.ErrorFolder);

                // 1) Resolver lista de archivos
                List<string> filesToProcess = new();

                if (File.Exists(configuredPath))
                {
                    filesToProcess.Add(configuredPath);
                }
                else if (Directory.Exists(configuredPath))
                {
                    var csvs = Directory.GetFiles(configuredPath, "*.csv").ToList();
                    var jsons = Directory.GetFiles(configuredPath, "*.json").ToList();
                    filesToProcess = csvs.Concat(jsons).ToList();
                    if (filesToProcess.Count == 0)
                    {

                        LogInfo("No hay archivos para subir.");

                        ResetSubida(arrancar: true);
                        RefrescarCarpetaTxUI();
                        return;
                    }
                }
                else
                {
                    LogError($"PathArchivoTx configurado no es válido: {configuredPath}");
                    ResetSubida(arrancar: true);
                    RefrescarCarpetaTxUI();
                    return;
                }

                int ok = 0;
                int fail = 0;
                var detalles = new List<string>();

                foreach (var filePath in filesToProcess)
                {
                    if (!File.Exists(filePath)) continue;

                    //Recorro las filas y miro la extension.
                    (bool Ok, int Registros, string Mensaje) r;

                    var ext = Path.GetExtension(filePath).ToLowerInvariant();

                    if (ext == ".json")
                    {
                        var nameLower = (Path.GetFileNameWithoutExtension(filePath) ?? "").ToLowerInvariant();
                        bool esReparto  = nameLower.Contains("reparto") || nameLower.Contains("repartos");
                        bool esMaestros = nameLower.Contains("maestros");

                        if (esMaestros)
                            r = await ProcesarMaestrosJsonAsync(filePath);
                        else if (esReparto)
                            r = await ProcesarRepartoJsonAsync(filePath);
                        else
                        {
                            detalles.Add($"IGNORADO - {Path.GetFileName(filePath)} (nombre no reconocido)");
                            continue;
                        }
                    }
                    else
                        r = await ProcesarArchivoCsvAsync(filePath);

                    if (r.Ok)
                    {
                        ok++;
                        detalles.Add($"OK - {Path.GetFileName(filePath)} ({r.Registros} registros) - {r.Mensaje}");
                        TryMoveFile(filePath, _config.ProcessedFolder, out _);
                    }
                    else
                    {
                        fail++;
                        detalles.Add($"ERROR - {Path.GetFileName(filePath)} - {r.Mensaje}");
                        TryMoveFile(filePath, _config.ErrorFolder, out _);
                    }
                }

                // refrescar la vista de la carpeta TX (ya moviste archivos)
                RefrescarCarpetaTxUI();

                var resumen = $"Procesados: {filesToProcess.Count}\nOK: {ok}\nError: {fail}\n\n" + string.Join("\n", detalles);

                LogInfo(resumen);
            }
            catch (Exception ex)
            {
                LogError($"Error procesando el/los CSV: {ex.Message}");
            }
            finally
            {
                ResetSubida(arrancar: true); // <-- ahora reinicia y vuelve a descontar
                cmdTransmitirTx.Enabled = true;
            }
        }


        /// <summary>
        /// Mueve el archivo a una carpeta destino. Si ya existe un archivo con el mismo nombre,
        /// renombra agregando un timestamp.
        /// </summary>
        private bool TryMoveFile(string sourceFilePath, string destFolder, out string finalPath)
        {
            finalPath = "";

            try
            {
                if (!File.Exists(sourceFilePath))
                    return false;

                Directory.CreateDirectory(destFolder);

                var fileName = Path.GetFileName(sourceFilePath);
                var destPath = Path.Combine(destFolder, fileName);

                // Si existe, renombrar
                if (File.Exists(destPath))
                {
                    var name = Path.GetFileNameWithoutExtension(fileName);
                    var ext = Path.GetExtension(fileName);
                    var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                    destPath = Path.Combine(destFolder, $"{name}_{stamp}{ext}");
                }

                File.Move(sourceFilePath, destPath);
                finalPath = destPath;
                return true;
            }
            catch
            {
                // si querés loguear el error acá, se puede
                return false;
            }
        }


        private async Task<(bool Ok, int Registros, string Mensaje)> ProcesarArchivoCsvAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return (false, 0, "Archivo inválido o no existe.");

                // Determinar tipo por nombre
                var name = Path.GetFileNameWithoutExtension(filePath) ?? "";
                var nameLower = name.ToLowerInvariant();

                bool isProductos = nameLower.Contains("productos");
                bool isClientes  = nameLower.Contains("clientes");
                bool isStock     = nameLower.Contains("stock");
                bool isFleteros  = nameLower.Contains("fleteros");

                if (!isProductos && !isClientes && !isStock && !isFleteros)
                    return (false, 0, $"No pude determinar el tipo por el nombre: {name}. Debe contener Productos/Clientes/Stock.");

                // PRODUCTOS
                if (isProductos)
                {
                    List<ProductosMultiEmpresaItem> items = ParseCsvToProductos(filePath, EmpresaID);
                    if (items == null || items.Count == 0)
                        return (false, 0, "No se encontraron registros en el archivo.");

                    bool sent = await SendProductosAsync(items);
                    if (!sent)
                        return (false, items.Count, "Falló el envío (SendProductosAsync devolvió false).");

                    return (true, items.Count, "Envío OK");
                }

                // CLIENTES
                if (isClientes)
                {
                    List<ClientesMultiEmpresaItem> items = ParseCsvToClientes(filePath, EmpresaID);
                    if (items == null || items.Count == 0)
                        return (false, 0, "No se encontraron registros en el archivo.");

                    bool sent = await SendClientesAsync(items);
                    if (!sent)
                        return (false, items.Count, "Falló el envío (SendClientesAsync devolvió false).");

                    return (true, items.Count, "Envío OK");
                }

                // STOCK
                if (isStock)
                {
                    List<StockMultiempresaItem> items = ParseCsvToStock(filePath, EmpresaID);
                    if (items == null || items.Count == 0)
                        return (false, 0, "No se encontraron registros en el archivo.");

                    bool sent = await SendStockAsync(items);
                    if (!sent)
                        return (false, items.Count, "Falló el envío (SendStockAsync devolvió false).");

                    return (true, items.Count, "Envío OK");
                }

                // FLETEROS (deshabilitado temporalmente)
                //if (isFleteros)
                //{
                //    var items = ParseCsvToFleteros(filePath, EmpresaID);
                //    if (items == null || items.Count == 0)
                //        return (false, 0, "No se encontraron registros en el archivo.");
                //    var sent = await SendFleterosAsync(items);
                //    if (!sent)
                //        return (false, items.Count, "Falló el envío (SendFleterosAsync devolvió false).");
                //    return (true, items.Count, "Envío OK");
                //}

                return (false, 0, "Tipo no soportado.");
            }
            catch (Exception ex)
            {
                return (false, 0, ex.Message);
            }
        }
        private async Task<(bool Ok, int Registros, string Mensaje)> ProcesarRepartoJsonAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return (false, 0, "JSON vacío.");

                var reparto = JsonSerializer.Deserialize<RepartosDetalleMultiEmpresaExcelItem>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (reparto == null)
                    return (false, 0, "JSON inválido (no deserializa).");

                // validación mínima
                if (reparto.NroReparto <= 0) return (false, 0, "NroReparto inválido.");
                if (reparto.FelterosID <= 0) return (false, 0, "FelterosID inválido.");

                // mandar al API
                var ok = await SendRepartoAsync(reparto, EmpresaID);

                if (!ok)
                    return (false, 1, "Falló el envío (SendRepartoAsync devolvió false).");

                var ventas = reparto.VentasMultiEmpresa?.Count ?? 0;
                return (true, 1, $"Reparto OK. Ventas: {ventas}");
            }
            catch (Exception ex)
            {
                return (false, 0, $"Error JSON reparto: {ex.Message}");
            }
        }


        // MAPEO CSV clientes — formato compacto (14 cols)
        // 0:  ClientesID
        // 1:  Codigo
        // 2:  Nombre
        // 3:  Direccion
        // 4:  Localidad
        // 5:  Telefono
        // 6:  DiaDeVenta
        // 7:  PorcentajePercepcionIB
        // 8:  Latitud
        // 9:  Longitud
        // 10: ListasPreciosID
        // 11: TiposDocumentosID
        // 12: NumeroDocumento
        // 13: Baja
        private static List<ClientesMultiEmpresaItem> ParseCsvToClientes(string filePath, int empresaID)
        {
            string[] lines = File.ReadAllLines(filePath);
            if (lines == null || lines.Length == 0)
                return new List<ClientesMultiEmpresaItem>();

            var list = new List<ClientesMultiEmpresaItem>();

            for (int row = 1; row < lines.Length; row++)
            {
                string line = lines[row];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] fields = line.Split(';');
                for (int i = 0; i < fields.Length; i++)
                    fields[i] = fields[i].Trim();

                var ic = new ClientesMultiEmpresaItem { EmpresaID = empresaID };

                if (fields.Length > 0 && int.TryParse(fields[0], NumberStyles.Any, CultureInfo.InvariantCulture, out int clientesID))
                    ic.ClientesID = clientesID;

                if (fields.Length > 1) ic.Codigo    = fields[1];
                if (fields.Length > 2) ic.Nombre    = fields[2];
                if (fields.Length > 3) ic.Direccion = fields[3];
                if (fields.Length > 4) ic.Localidad = fields[4];
                if (fields.Length > 5) ic.Telefono  = fields[5];

                if (fields.Length > 6 && int.TryParse(fields[6], NumberStyles.Any, CultureInfo.InvariantCulture, out int diaVenta))
                    ic.DiaDeVenta = diaVenta;

                if (fields.Length > 7 && decimal.TryParse(fields[7], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal percepcion))
                    ic.PorcentajePercepcionIB = percepcion;

                if (fields.Length > 8 && double.TryParse(fields[8], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
                    ic.Latitud = lat;

                if (fields.Length > 9 && double.TryParse(fields[9], NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                    ic.Longitud = lon;

                if (fields.Length > 10 && int.TryParse(fields[10], NumberStyles.Any, CultureInfo.InvariantCulture, out int listaID))
                    ic.ListasPreciosID = listaID;

                if (fields.Length > 11 && int.TryParse(fields[11], NumberStyles.Any, CultureInfo.InvariantCulture, out int tipoDocID))
                    ic.TiposDocumentosID = tipoDocID;

                if (fields.Length > 12) ic.NumeroDocumento = fields[12];

                if (fields.Length > 13 && bool.TryParse(fields[13], out bool baja))
                    ic.Baja = baja;

                list.Add(ic);
            }

            return list;
        }
        // FLETEROS — deshabilitado temporalmente (pendiente de integración futura)
        //private static List<FleterosMultiEmpresaItem> ParseCsvToFleteros(string filePath, int empresaID)
        //{
        //    var lines = File.ReadAllLines(filePath);
        //    if (lines == null || lines.Length == 0)
        //        return new List<FleterosMultiEmpresaItem>();
        //    var list = new List<FleterosMultiEmpresaItem>();
        //    for (int row = 1; row < lines.Length; row++)
        //    {
        //        var line = lines[row];
        //        if (string.IsNullOrWhiteSpace(line)) continue;
        //        var fields = line.Split(';');
        //        for (int i = 0; i < fields.Length; i++) fields[i] = fields[i].Trim();
        //        var ic = new FleterosMultiEmpresaItem();
        //        if (fields.Length > 0) ic.Nombre = fields[0];
        //        if (fields.Length > 1) ic.Clave  = fields[1];
        //        list.Add(ic);
        //    }
        //    return list;
        //}


        private static List<StockMultiempresaItem> ParseCsvToStock(string filePath, int empresaID)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines == null || lines.Length == 0)
                return new List<StockMultiempresaItem>();

            var list = new List<StockMultiempresaItem>();

            // Asumimos que la primera línea es encabezado. Si no lo es, cambiar startRow a 0.
            var startRow = 1;
            for (int row = startRow; row < lines.Length; row++)
            {
                var line = lines[row];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var fields = line.Split(';');
                for (int i = 0; i < fields.Length; i++)
                    fields[i] = fields[i].Trim();

                var ic = new StockMultiempresaItem();
                // Mapear campos según ejemplo (ajustar índices según CSV real)
                // MAPEO CSV Stock: 0=ProductosID, 1=CodigoProducto, 2=StockUnidades
                if (fields.Length > 0) ic.ProductosID = Convert.ToInt32(fields[0]);
                if (fields.Length > 1) ic.CodigoProducto = fields[1];
                if (fields.Length > 2) ic.StockUnidades = fields[2];
                ic.EmpresaID = empresaID;

                list.Add(ic);
            }

            return list;
        }


        private static List<ProductosMultiEmpresaItem> ParseCsvToProductos(string filePath, int empresaID)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines == null || lines.Length == 0)
                return new List<ProductosMultiEmpresaItem>();

            var list = new List<ProductosMultiEmpresaItem>();

            // Asumimos que la primera línea es encabezado. Si no lo es, cambiar startRow a 0.
            var startRow = 1;
            for (int row = startRow; row < lines.Length; row++)
            {
                var line = lines[row];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var fields = line.Split(';');
                for (int i = 0; i < fields.Length; i++)
                    fields[i] = fields[i].Trim();

                var p = new ProductosMultiEmpresaItem();

                int tempInt;
                decimal tempDec;
                bool tempBool;

                p.ProductosMultiEmpresaID = 0;

                // MAPEO CSV — índices del ERP (formato compacto)
                // 0:  ProductosID
                // 1:  ProductosIDPadre
                // 2:  CodigoDeProducto
                // 3:  Nombre
                // 4:  StockEnUnidades
                // 5:  CantidadMultiplo
                // 6:  DesactivadoParaLaVenta
                // 7:  MarcasProductosID
                // 8:  FamiliaProductosID
                // 9:  StockPropio
                // 10: CategoriaComercialID
                // 11: UnidadesPorBulto
                // 12: PrecioProducto
                // 13: ImpuestosID
                // 14: Baja
                // 15: CategoriasProductosID
                // 16: SubCategoriasProductosID

                if (fields.Length > 0 && int.TryParse(fields[0], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.ProductosID = tempInt;

                if (fields.Length > 1 && int.TryParse(fields[1], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.ProductosIDPadre = tempInt;

                if (fields.Length > 2) p.CodigoDeProducto = fields[2];

                if (fields.Length > 3) p.Nombre = fields[3];

                if (fields.Length > 4 && int.TryParse(fields[4], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.StockEnUnidades = tempInt;

                if (fields.Length > 5 && int.TryParse(fields[5], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.CantidadMultiplo = tempInt;

                if (fields.Length > 6 && bool.TryParse(fields[6], out tempBool))
                    p.DesactivadoParaLaVenta = tempBool;

                if (fields.Length > 7 && int.TryParse(fields[7], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.MarcasProductosID = tempInt;

                if (fields.Length > 8 && int.TryParse(fields[8], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.FamiliaProductosID = tempInt;

                if (fields.Length > 9 && bool.TryParse(fields[9], out tempBool))
                    p.StockPropio = tempBool;

                if (fields.Length > 10 && int.TryParse(fields[10], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.CategoriasComercialesIDs.Add(tempInt);

                if (fields.Length > 11 && int.TryParse(fields[11], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.UnidadesPorBulto = tempInt;

                if (fields.Length > 12 && decimal.TryParse(fields[12], NumberStyles.Any, CultureInfo.InvariantCulture, out tempDec))
                    p.PrecioProducto = tempDec;

                if (fields.Length > 13 && int.TryParse(fields[13], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.ImpuestosID = tempInt;

                if (fields.Length > 14 && bool.TryParse(fields[14], out tempBool))
                    p.Baja = tempBool;

                if (fields.Length > 15 && int.TryParse(fields[15], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.CategoriasProductosID = tempInt;

                if (fields.Length > 16 && int.TryParse(fields[16], NumberStyles.Any, CultureInfo.InvariantCulture, out tempInt))
                    p.SubCategoriasProductosID = tempInt;

                p.EmpresaID = empresaID;

                // URL imagen generada internamente desde UrlImagenBase en appconfig.json
                string urlBase = ConfigManager.Instance.UrlImagenBase;
                int idParaUrl = (p.ProductosIDPadre > 0 ? p.ProductosIDPadre : p.ProductosID) ?? 0;
                p.UrlImagenWeb = $"{urlBase}{idParaUrl}_128.png";



                list.Add(p);
            }

            return list;
        }

        private async Task<bool> SendClientesAsync(List<ClientesMultiEmpresaItem> clientes)
        {
            if (clientes == null)
                return false;

            var apiUrl = _config?.ApiUrl?.Trim();
            if (string.IsNullOrEmpty(apiUrl))
            {
                LogWarn("ApiUrl no configurada en appconfig.json");
                return false;
            }

            var endpoint = apiUrl.TrimEnd('/') + "/api/sync/clientes";

            try
            {
                using var http = CreateHttpClient();
                var json = JsonSerializer.Serialize(clientes, new JsonSerializerOptions { WriteIndented = false });
                if (_config != null && _config.EnableLogging)
                {
                    try { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "last_payload.json"), json); } catch { }
                }

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await http.PostAsync(endpoint, content);
                if (resp.IsSuccessStatusCode)
                    return true;

                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized && await SilentLoginAsync())
                {
                    using var http2 = CreateHttpClient();
                    using var content2 = new StringContent(json, Encoding.UTF8, "application/json");
                    var resp2 = await http2.PostAsync(endpoint, content2);
                    if (resp2.IsSuccessStatusCode) return true;
                }

                var text = await resp.Content.ReadAsStringAsync();
                LogError($"Error API Clientes: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error enviando al API Clientes: {ex.Message}");
                return false;
            }
        }              

        private async Task<bool> SendProductosAsync(List<ProductosMultiEmpresaItem> productos)
        {
            if (productos == null)
                return false;

            var apiUrl = _config?.ApiUrl?.Trim();
            if (string.IsNullOrEmpty(apiUrl))
            {
                LogWarn("ApiUrl no configurada en appconfig.json");
                return false;
            }

            var endpoint = apiUrl.TrimEnd('/') + "/api/sync/productos";

            try
            {
                using var http = CreateHttpClient();
                var json = JsonSerializer.Serialize(productos, new JsonSerializerOptions { WriteIndented = false });

                if (_config != null && _config.EnableLogging)
                {
                    try { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "last_payload_productos.json"), json); }
                    catch { }
                }

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await http.PostAsync(endpoint, content);
                if (resp.IsSuccessStatusCode)
                    return true;

                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized && await SilentLoginAsync())
                {
                    using var http2 = CreateHttpClient();
                    using var content2 = new StringContent(json, Encoding.UTF8, "application/json");
                    var resp2 = await http2.PostAsync(endpoint, content2);
                    if (resp2.IsSuccessStatusCode) return true;
                }

                var text = await resp.Content.ReadAsStringAsync();
                LogError($"Error API: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error enviando al API Productos: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendStockAsync(List<StockMultiempresaItem> productos)
        {
            if (productos == null)
                return false;

            var apiUrl = _config?.ApiUrl?.Trim();
            if (string.IsNullOrEmpty(apiUrl))
            {
                LogWarn("ApiUrl no configurada en appconfig.json");
                return false;
            }

            var endpoint = apiUrl.TrimEnd('/') + "/api/sync/stock";

            try
            {
                using var http = CreateHttpClient();
                var json = JsonSerializer.Serialize(productos, new JsonSerializerOptions { WriteIndented = false });

                if (_config != null && _config.EnableLogging)
                {
                    try { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "last_payload_stock.json"), json); }
                    catch { }
                }

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await http.PostAsync(endpoint, content);
                if (resp.IsSuccessStatusCode)
                    return true;

                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized && await SilentLoginAsync())
                {
                    using var http2 = CreateHttpClient();
                    using var content2 = new StringContent(json, Encoding.UTF8, "application/json");
                    var resp2 = await http2.PostAsync(endpoint, content2);
                    if (resp2.IsSuccessStatusCode) return true;
                }

                var text = await resp.Content.ReadAsStringAsync();
                LogError($"Error API stock: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error enviando al API Stock: {ex.Message}");
                return false;
            }
        }
        private async Task<bool> SendRepartoAsync(RepartosDetalleMultiEmpresaExcelItem reparto, int empresaId)
        {
            var apiUrl = _config?.ApiUrl?.Trim();
            if (string.IsNullOrEmpty(apiUrl))
            {
                LogWarn("ApiUrl no configurada en appconfig.json");
                return false;
            }

            // endpoint base
            var endpoint = apiUrl.TrimEnd('/') + $"/repartos?empresaId={empresaId}";

            try
            {
                using var http = new HttpClient();

                var payload = JsonSerializer.Serialize(reparto, new JsonSerializerOptions { WriteIndented = false });

                if (_config.EnableLogging)
                {
                    try { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "last_payload_reparto.json"), payload); } catch { }
                }

                using var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var resp = await http.PostAsync(endpoint, content);

                if (resp.IsSuccessStatusCode)
                    return true;

                var text = await resp.Content.ReadAsStringAsync();
                LogError($"Error API Repartos: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error enviando reparto al API: {ex.Message}");
                return false;
            }
        }

        // ── MAESTROS ─────────────────────────────────────────────────────────────

        private async Task<(bool Ok, int Registros, string Mensaje)> ProcesarMaestrosJsonAsync(string filePath)
        {
            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return (false, 0, "JSON vacío.");

                MaestrosSyncItem maestros = JsonSerializer.Deserialize<MaestrosSyncItem>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (maestros == null)
                    return (false, 0, "JSON inválido.");

                maestros.EmpresaID = EmpresaID;

                bool ok = await SendMaestrosAsync(maestros);
                if (!ok)
                    return (false, 1, "Falló el envío (SendMaestrosAsync devolvió false).");

                int total = maestros.Impuestos.Count
                          + maestros.MarcasProductos.Count
                          + maestros.FamiliasProductos.Count
                          + maestros.Categorias.Count
                          + maestros.SubCategorias.Count
                          + maestros.CategoriasComerciales.Count
                          + maestros.ListasPrecios.Count
                          + maestros.TiposDocumentos.Count;

                return (true, total, "Maestros OK");
            }
            catch (Exception ex)
            {
                return (false, 0, $"Error JSON maestros: {ex.Message}");
            }
        }

        private async Task<bool> SendMaestrosAsync(MaestrosSyncItem maestros)
        {
            string apiUrl = _config?.ApiUrl?.Trim();
            if (string.IsNullOrEmpty(apiUrl))
            {
                LogWarn("ApiUrl no configurada en appconfig.json");
                return false;
            }

            string endpoint = apiUrl.TrimEnd('/') + "/api/sync/maestros";

            try
            {
                using HttpClient http = CreateHttpClient();
                string json = JsonSerializer.Serialize(maestros, new JsonSerializerOptions { WriteIndented = false });

                if (_config != null && _config.EnableLogging)
                {
                    try { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "last_payload_maestros.json"), json); } catch { }
                }

                using StringContent content = new(json, Encoding.UTF8, "application/json");
                HttpResponseMessage resp = await http.PostAsync(endpoint, content);

                if (resp.IsSuccessStatusCode) return true;

                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized && await SilentLoginAsync())
                {
                    using HttpClient http2 = CreateHttpClient();
                    using StringContent content2 = new(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage resp2 = await http2.PostAsync(endpoint, content2);
                    if (resp2.IsSuccessStatusCode) return true;
                }

                string text = await resp.Content.ReadAsStringAsync();
                LogError($"Error API maestros: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error enviando maestros: {ex.Message}");
                return false;
            }
        }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static List<PedidosJsonMultiempresaExcelItem> BuildRowsFromJson(string jsonPedido)
        {
            var pedido = JsonSerializer.Deserialize<PedidoJsonMultiempresaItem>(jsonPedido, JsonOpts);
            if (pedido == null) return new List<PedidosJsonMultiempresaExcelItem>();

            var rows = new List<PedidosJsonMultiempresaExcelItem>(pedido.LstProductos?.Count ?? 0);

            foreach (var pr in pedido.LstProductos)
            {
                var unidades = pr.Unidades != 0 ? pr.Unidades : pr.Cantidad;

                rows.Add(new PedidosJsonMultiempresaExcelItem
                {
                    VendedorID = pedido.VendedorID,
                    ClienteID = pedido.ClienteID,
                    //EmpresaID = pedido.EmpresaID,
                    ClienteCodigo = pedido.ClienteCodigo ?? "",

                    CodigoProducto = pr.CodigoProducto ?? "",
                    ProductoID = pr.ProductoID,

                    Unidades = unidades,
                    ListaDePrecioID = pr.ListaDePrecioID,
                    PrecioUnitarioFinal = pr.PrecioUnitarioFinal,
                    PrecioUnitarioNeto = pr.PrecioUnitarioNeto,

                    TotalProducto = pr.TotalProducto,
                    TotalPedido = pedido.TotalPedido
                });
            }
            return rows;
        }

        public class PedidoPendienteIndexItem
        {
            public int PedidosJsonMultiempresaID { get; set; }
            public DateTime HoraGuardada { get; set; }
        }

        private async void btnDescargarPedido_Click(object sender, EventArgs e)
        {
             btnDescargarPedido.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                int empresaId = EmpresaID;

                var apiUrl = _config?.ApiUrl?.Trim();
                if (string.IsNullOrEmpty(apiUrl))
                {
                    LogWarn("ApiUrl no configurada en appconfig.json");
                    ResetDescarga(arrancar: true);
                    return;
                }

                // Carpeta destino
                string carpeta = DownloadFolder;
                if (string.IsNullOrWhiteSpace(carpeta))
                    carpeta = Path.Combine(Application.StartupPath, "Descargados");

                if (!Path.IsPathRooted(carpeta))
                    carpeta = Path.Combine(Application.StartupPath, carpeta);

                Directory.CreateDirectory(carpeta);

                using var http = CreateHttpClient(new Uri(apiUrl));

                // 1) Traer TODOS los pedidos pendientes con su JSON (wrapper)
                using var resp = await http.GetAsync($"/pedidos/pendientes-wrapper/{empresaId}");
                if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    LogInfo("No hay pedidos pendientes para descargar.");
                    ResetDescarga(arrancar: true);
                    return;
                }

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    throw new Exception($"Error API wrapper {(int)resp.StatusCode}: {err}");
                }

                var json = await resp.Content.ReadAsStringAsync();
                var pedidos = JsonSerializer.Deserialize<List<PedidoJsonMultiempresaDb>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new();

                if (pedidos.Count == 0)
                {
                    LogInfo("No hay pedidos pendientes para descargar.");
                    ResetDescarga(arrancar: true);
                    return;
                }

                // 2) Generar 1 CSV por pedido (usando tu JsonToRows + PedidosCsvWriter)
                var idsOk = new List<int>();

                foreach (var p in pedidos)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(p.JsonPedido))
                        {
                            LogWarn($"Pedido {p.PedidosJsonMultiempresaID}: JsonPedido vacío.");
                            continue;
                        }

                        // IMPORTANTE: JsonToRows es STATIC (por eso se llama por tipo)
                        var rows = PedidosJsonMultiempresa.JsonToRows(p.JsonPedido);
                        if (rows == null || rows.Count == 0)
                        {
                            LogWarn($"Pedido {p.PedidosJsonMultiempresaID}: sin productos / sin rows.");
                            continue;
                        }

                        var csv = PedidosCsvWriter.ToCsv(rows, ';');

                        var fileName = $"Pedido_{p.PedidosJsonMultiempresaID}_Empresa{empresaId}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        var fullPath = Path.Combine(carpeta, fileName);

                        File.WriteAllText(fullPath, csv, new UTF8Encoding(false));

                        idsOk.Add(p.PedidosJsonMultiempresaID);

                        LogInfo($"Pedido generado: {fullPath} (HoraGuardada: {p.HoraGuardada:dd/MM/yyyy HH:mm:ss})");
                    }
                    catch (Exception ex1)
                    {
                        LogError($"Error procesando pedido {p.PedidosJsonMultiempresaID}: {ex1.Message}");
                    }
                }

                // 3) Marcar descargados (si tu flujo lo hace desde el cliente)
                // Si tu endpoint /descargar-csv-uno ya marcaba descargado, NO uses esto.
                if (idsOk.Count > 0)
                {
                    var body = new
                    {
                        EmpresaID = empresaId,
                        Ids = idsOk
                    };

                    var content = new StringContent(
                        JsonSerializer.Serialize(body),
                        Encoding.UTF8,
                        "application/json"
                    );

                    using var respMark = await http.PostAsync("/pedidos/marcar-descargados", content);

                    if (!respMark.IsSuccessStatusCode)
                    {
                        var err = await respMark.Content.ReadAsStringAsync();
                        LogWarn($"No se pudieron marcar descargados ({(int)respMark.StatusCode}): {err}");
                    }
                }
                else
                {
                    LogInfo("No se generó ningún archivo (no hay IDs OK para marcar).");
                }
            }
            catch (Exception ex)
            {
                LogError("Error descargando pedidos: " + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnDescargarPedido.Enabled = true;
                ResetDescarga(arrancar: true);
            }
        }

        public async Task<string> ExportarCsvDesdeItemsAsync(int empresaId)
        {
            var oPedidosJsonMultiempresa = new PedidosJsonMultiempresa();
            var items = oPedidosJsonMultiempresa.GetEmpresasExternaItems(empresaId);

            if (items == null || items.Count == 0)
                return "";

            var csv = PedidosCsvWriter.ToCsv(items, ';');

            // ✅ leer ruta desde appsettings.json
            var carpeta = AppConfig.GetProcessedFolder();

            // fallback si viene vacío
            if (string.IsNullOrWhiteSpace(carpeta))
                carpeta = Path.Combine(Application.StartupPath, "Descargados");

            Directory.CreateDirectory(carpeta);

            var fileName = string.Format("Pedidos_Empresa{0}_{1}.csv", empresaId, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            var fullPath = Path.Combine(carpeta, fileName);

            File.WriteAllText(fullPath, csv, Encoding.UTF8);

            return fullPath;
        }

        private void btnCambiarIntervaloSubida_Click(object sender, EventArgs e)
        {
            int segundos = Convert.ToInt32(nudSubirArchivos.Value);

            // guardar en config
            _config.IntervaloSubidaSegundos = segundos;

            // si estaba corriendo, lo dejamos corriendo
            bool estabaCorriendo = tmrSubida.Enabled;

            // reset al nuevo valor
            ResetSubida(arrancar: estabaCorriendo);

            Log($"[CONFIG] Intervalo Subida cambiado a {segundos}s");
        }

        private void btnCambiarIntervaloDescarga_Click(object sender, EventArgs e)
        {
            int segundos = Convert.ToInt32(nudDescarga.Value);

            _config.IntervaloDescargaSegundos = segundos;

            bool estabaCorriendo = tmrDescarga.Enabled;

            ResetDescarga(arrancar: estabaCorriendo);

            Log($"[CONFIG] Intervalo Descarga cambiado a {segundos}s");
        }
    }

    public class AppConfig
    {
        public string ApiUrl { get; set; }
        public string DownloadFolder { get; set; }
        public string ProcessedFolder { get; set; }
        public string ErrorFolder { get; set; }
        public string PathArchivoTx { get; set; } = @"c:\Integrador";
        public int IntervaloSubidaSegundos { get; set; } = 30;
        public int IntervaloDescargaSegundos { get; set; } = 30;
        public int TxInterval { get; set; } = 5000;
        public bool EnableLogging { get; set; } = true;
        public string ApiUsuario { get; set; } = "";
        public string ApiClave { get; set; } = "";
        public string UrlImagenBase { get; set; } = "https://cdn.empresademo.example/CatalogoEcom/";

        public static string GetProcessedFolder()
        {
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "appsettings.json"
            );

            if (!File.Exists(path))
                return "";

            var json = File.ReadAllText(path);

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("ProcessedFolder", out var prop))
                return prop.GetString() ?? "";

            return "";
        }

    }

}