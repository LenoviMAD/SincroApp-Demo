using System.Drawing;
using System.Windows.Forms;

namespace SincroDatosApp
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlBanner    = new Panel();
            picLogo      = new PictureBox();
            lblSubtitulo = new Label();
            lblUsuario   = new Label();
            txtUsuario   = new TextBox();
            lblClave     = new Label();
            txtClave     = new TextBox();
            btnLogin     = new Button();
            lblError     = new Label();
            pnlBanner.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            SuspendLayout();

            // pnlBanner
            pnlBanner.BackColor = Color.FromArgb(15, 52, 96);
            pnlBanner.Controls.Add(picLogo);
            pnlBanner.Dock = DockStyle.Top;
            pnlBanner.Height = 130;
            pnlBanner.Name = "pnlBanner";

            // picLogo
            picLogo.BackColor = Color.Transparent;
            picLogo.Location = new Point(90, 15);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(200, 100);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabStop = false;

            // lblSubtitulo
            lblSubtitulo.Font = new Font("Segoe UI", 9F);
            lblSubtitulo.ForeColor = Color.FromArgb(120, 120, 150);
            lblSubtitulo.Location = new Point(20, 146);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Size = new Size(340, 20);
            lblSubtitulo.TabIndex = 0;
            lblSubtitulo.Text = "Sistema de Ventas Móvil";
            lblSubtitulo.TextAlign = ContentAlignment.MiddleCenter;

            // lblUsuario
            lblUsuario.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUsuario.ForeColor = Color.FromArgb(40, 40, 60);
            lblUsuario.Location = new Point(40, 184);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(300, 18);
            lblUsuario.TabIndex = 1;
            lblUsuario.Text = "Usuario";

            // txtUsuario
            txtUsuario.BorderStyle = BorderStyle.FixedSingle;
            txtUsuario.Font = new Font("Segoe UI", 11F);
            txtUsuario.Location = new Point(40, 204);
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Size = new Size(300, 28);
            txtUsuario.TabIndex = 2;

            // lblClave
            lblClave.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblClave.ForeColor = Color.FromArgb(40, 40, 60);
            lblClave.Location = new Point(40, 250);
            lblClave.Name = "lblClave";
            lblClave.Size = new Size(300, 18);
            lblClave.TabIndex = 3;
            lblClave.Text = "Contraseña";

            // txtClave
            txtClave.BorderStyle = BorderStyle.FixedSingle;
            txtClave.Font = new Font("Segoe UI", 11F);
            txtClave.Location = new Point(40, 270);
            txtClave.Name = "txtClave";
            txtClave.PasswordChar = '●';
            txtClave.Size = new Size(300, 28);
            txtClave.TabIndex = 4;

            // btnLogin
            btnLogin.BackColor = Color.FromArgb(15, 52, 96);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(40, 326);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(300, 42);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "Ingresar";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;

            // lblError
            lblError.Font = new Font("Segoe UI", 9F);
            lblError.ForeColor = Color.FromArgb(210, 40, 40);
            lblError.Location = new Point(30, 378);
            lblError.Name = "lblError";
            lblError.Size = new Size(320, 40);
            lblError.TabIndex = 6;
            lblError.Text = "";
            lblError.TextAlign = ContentAlignment.MiddleCenter;
            lblError.Visible = false;

            // LoginForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(380, 430);
            Controls.Add(lblError);
            Controls.Add(btnLogin);
            Controls.Add(txtClave);
            Controls.Add(lblClave);
            Controls.Add(txtUsuario);
            Controls.Add(lblUsuario);
            Controls.Add(lblSubtitulo);
            Controls.Add(pnlBanner);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SincroApp Demo — Acceso";

            pnlBanner.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Panel pnlBanner;
        private PictureBox picLogo;
        private Label lblSubtitulo;
        private Label lblUsuario;
        private TextBox txtUsuario;
        private Label lblClave;
        private TextBox txtClave;
        private Button btnLogin;
        private Label lblError;
    }
}
