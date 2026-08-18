using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SincroDatosApp
{
    public partial class LoginForm : Form
    {
        public int    EmpresaID { get; private set; }
        public string Token     { get; private set; } = "";

        public LoginForm()
        {
            InitializeComponent();
            CargarLogo();
            CargarIcono();

            // Enter en usuario → foco a clave; Enter en clave → login
            txtUsuario.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; txtClave.Focus(); } };
            txtClave.KeyDown   += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; btnLogin_Click(s, e); } };

            // Pre-cargar usuario si estaba guardado
            string usuario = ConfigManager.Instance.ApiUsuario;
            if (!string.IsNullOrWhiteSpace(usuario))
            {
                txtUsuario.Text = usuario;
                txtClave.Focus();
            }
        }

        private void CargarLogo()
        {
            string logoPath = Path.Combine(AppContext.BaseDirectory, "vendixLogo.png");
            if (File.Exists(logoPath))
            {
                picLogo.Image = Image.FromFile(logoPath);
                return;
            }

            // Fallback: texto si no existe el archivo
            Label lbl = new Label
            {
                Text      = "SincroApp Demo",
                Font      = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize  = true
            };
            pnlBanner.Controls.Add(lbl);
            lbl.Location = new Point((pnlBanner.Width - lbl.PreferredWidth) / 2, 38);
        }

        private void CargarIcono()
        {
            string icoPath = Path.Combine(AppContext.BaseDirectory, "vendixIco.ico");
            if (File.Exists(icoPath))
                this.Icon = new Icon(icoPath);
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string clave   = txtClave.Text;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
            {
                MostrarError("Ingrese usuario y contraseña.");
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text    = "Verificando...";
            lblError.Visible = false;

            try
            {
                (int empresaID, string token) = await LoginAsync(usuario, clave);
                EmpresaID    = empresaID;
                Token        = token;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (LoginException ex)
            {
                MostrarError(ex.Message);
            }
            catch
            {
                MostrarError("No se pudo conectar con el servidor.\nVerifique la red y la URL en appconfig.json.");
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text    = "Ingresar";
            }
        }

        private static async Task<(int EmpresaID, string Token)> LoginAsync(string usuario, string clave)
        {
            string apiUrl = ConfigManager.Instance.ApiUrl?.Trim() ?? "";
            if (string.IsNullOrEmpty(apiUrl))
                throw new LoginException("ApiUrl no configurada en appconfig.json.");

            string endpoint = apiUrl.TrimEnd('/') + "/api/auth/login";

            var bodyObj = new { usuario, clave };
            string json = JsonSerializer.Serialize(bodyObj);

            using HttpClient http = new();
            http.Timeout = TimeSpan.FromSeconds(10);
            using StringContent content = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage resp = await http.PostAsync(endpoint, content);

            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new LoginException("Usuario o contraseña incorrectos.");

            if (!resp.IsSuccessStatusCode)
                throw new LoginException($"Error del servidor ({(int)resp.StatusCode}).");

            string respJson = await resp.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(respJson);

            if (!doc.RootElement.TryGetProperty("empresaID", out JsonElement idProp))
                throw new LoginException("Respuesta inesperada del servidor.");
            if (!doc.RootElement.TryGetProperty("token", out JsonElement tokenProp))
                throw new LoginException("Respuesta no contiene token.");

            return (idProp.GetInt32(), tokenProp.GetString() ?? "");
        }

        private void MostrarError(string msg)
        {
            lblError.Text    = msg;
            lblError.Visible = true;
        }
    }

    public class LoginException : Exception
    {
        public LoginException(string message) : base(message) { }
    }
}
