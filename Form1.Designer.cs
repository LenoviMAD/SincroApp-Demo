using System.Drawing;
using System.Windows.Forms;

namespace SincroDatosApp
{
    partial class Form1
    {
        /// <summary>
        ///  Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Limpiar los recursos que se estén usando.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        ///  Método necesario para admitir el Diseñador.  
        ///  No se puede modificar el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            cmdTransmitirTx = new Button();
            tmrSubida = new Timer(components);
            fbdArchivosSubida = new FolderBrowserDialog();
            btnCambiarProcesados = new Button();
            picLogoMain = new PictureBox();
            lblOrigen = new Label();
            lblProcesados = new Label();
            lblIntervaloSubida = new Label();
            nudDescarga = new NumericUpDown();
            btnCambiarOrigen = new Button();
            btnCambiarIntervaloDescarga = new Button();
            lblArchivos = new Label();
            label4 = new Label();
            btnDescargarPedido = new Button();
            flpLoader = new FlowLayoutPanel();
            progressLoader = new ProgressBar();
            lblLoader = new Label();
            tlpLoader = new TableLayoutPanel();
            tvArchivos = new TreeView();
            label6 = new Label();
            lblIntervaloDescarga = new Label();
            lblNombreIntervaloDescarga = new Label();
            lblNombreIntervaloSubida = new Label();
            tmrDescarga = new Timer(components);
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            tabPage3 = new TabPage();
            lblCambiarDescargaSubirArchivos = new Label();
            btnCambiarIntervaloSubida = new Button();
            nudSubirArchivos = new NumericUpDown();
            lblCambiarSegundosDescarga = new Label();
            tabPage4 = new TabPage();
            txtLog = new RichTextBox();
            notifyIcon1 = new NotifyIcon(components);
            ((System.ComponentModel.ISupportInitialize)picLogoMain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDescarga).BeginInit();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudSubirArchivos).BeginInit();
            tabPage4.SuspendLayout();
            SuspendLayout();
            // 
            // cmdTransmitirTx
            // 
            cmdTransmitirTx.Location = new Point(8, 6);
            cmdTransmitirTx.Name = "cmdTransmitirTx";
            cmdTransmitirTx.Size = new Size(138, 23);
            cmdTransmitirTx.TabIndex = 0;
            cmdTransmitirTx.Text = "Subir Datos Manual";
            cmdTransmitirTx.UseVisualStyleBackColor = true;
            cmdTransmitirTx.Click += cmdTransmitirTx_Click;
            // 
            // tmrSubida
            // 
            tmrSubida.Tick += tmrSubida_Tick;
            // 
            // btnCambiarProcesados
            // 
            btnCambiarProcesados.Location = new Point(8, 60);
            btnCambiarProcesados.Name = "btnCambiarProcesados";
            btnCambiarProcesados.Size = new Size(167, 23);
            btnCambiarProcesados.TabIndex = 3;
            btnCambiarProcesados.Text = "Cambiar Ruta Pedidos";
            btnCambiarProcesados.UseVisualStyleBackColor = true;
            btnCambiarProcesados.Click += btnCambiarProcesados_Click;
            //
            // picLogoMain
            //
            picLogoMain.BackColor = Color.Transparent;
            picLogoMain.Location = new Point(155, 270);
            picLogoMain.Name = "picLogoMain";
            picLogoMain.Size = new Size(180, 72);
            picLogoMain.SizeMode = PictureBoxSizeMode.Zoom;
            picLogoMain.TabStop = false;
            //
            // lblOrigen
            // 
            lblOrigen.AutoSize = true;
            lblOrigen.Location = new Point(8, 32);
            lblOrigen.Name = "lblOrigen";
            lblOrigen.Size = new Size(70, 15);
            lblOrigen.TabIndex = 8;
            lblOrigen.Text = "Ruta Origen";
            // 
            // lblProcesados
            // 
            lblProcesados.AutoSize = true;
            lblProcesados.Location = new Point(8, 86);
            lblProcesados.Name = "lblProcesados";
            lblProcesados.Size = new Size(94, 15);
            lblProcesados.TabIndex = 9;
            lblProcesados.Text = "Ruta Procesados";
            // 
            // lblIntervaloSubida
            // 
            lblIntervaloSubida.AutoSize = true;
            lblIntervaloSubida.Location = new Point(9, 65);
            lblIntervaloSubida.Name = "lblIntervaloSubida";
            lblIntervaloSubida.Size = new Size(34, 15);
            lblIntervaloSubida.TabIndex = 10;
            lblIntervaloSubida.Text = "00:00";
            // 
            // nudDescarga
            //
            nudDescarga.Location = new Point(8, 219);
            nudDescarga.Minimum = 5;
            nudDescarga.Maximum = 3600;
            nudDescarga.Name = "nudDescarga";
            nudDescarga.Size = new Size(53, 23);
            nudDescarga.TabIndex = 12;
            // 
            // btnCambiarOrigen
            // 
            btnCambiarOrigen.Location = new Point(8, 6);
            btnCambiarOrigen.Name = "btnCambiarOrigen";
            btnCambiarOrigen.Size = new Size(145, 23);
            btnCambiarOrigen.TabIndex = 13;
            btnCambiarOrigen.Text = "Cambiar Ruta de Origen";
            btnCambiarOrigen.UseVisualStyleBackColor = true;
            btnCambiarOrigen.Click += btnCambiarOrigen_Click;
            // 
            // btnCambiarIntervaloDescarga
            // 
            btnCambiarIntervaloDescarga.Location = new Point(6, 190);
            btnCambiarIntervaloDescarga.Name = "btnCambiarIntervaloDescarga";
            btnCambiarIntervaloDescarga.Size = new Size(70, 23);
            btnCambiarIntervaloDescarga.TabIndex = 14;
            btnCambiarIntervaloDescarga.Text = "Cambiar Intervalo";
            btnCambiarIntervaloDescarga.UseVisualStyleBackColor = true;
            btnCambiarIntervaloDescarga.Click += btnCambiarIntervaloDescarga_Click;
            // 
            // lblArchivos
            // 
            lblArchivos.AutoSize = true;
            lblArchivos.Location = new Point(8, 117);
            lblArchivos.Name = "lblArchivos";
            lblArchivos.Size = new Size(56, 15);
            lblArchivos.TabIndex = 15;
            lblArchivos.Text = "Archivos:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(43, 65);
            label4.Name = "label4";
            label4.Size = new Size(59, 15);
            label4.TabIndex = 16;
            label4.Text = "Segundos";
            // 
            // btnDescargarPedido
            // 
            btnDescargarPedido.Location = new Point(8, 6);
            btnDescargarPedido.Name = "btnDescargarPedido";
            btnDescargarPedido.Size = new Size(167, 23);
            btnDescargarPedido.TabIndex = 18;
            btnDescargarPedido.Text = "Descargar Pedidos Manual";
            btnDescargarPedido.UseVisualStyleBackColor = true;
            btnDescargarPedido.Click += btnDescargarPedido_Click;
            // 
            // flpLoader
            // 
            flpLoader.Location = new Point(0, 0);
            flpLoader.Name = "flpLoader";
            flpLoader.Size = new Size(200, 100);
            flpLoader.TabIndex = 0;
            // 
            // progressLoader
            // 
            progressLoader.Location = new Point(0, 0);
            progressLoader.Name = "progressLoader";
            progressLoader.Size = new Size(100, 23);
            progressLoader.TabIndex = 0;
            // 
            // lblLoader
            // 
            lblLoader.Location = new Point(0, 0);
            lblLoader.Name = "lblLoader";
            lblLoader.Size = new Size(100, 23);
            lblLoader.TabIndex = 0;
            // 
            // tlpLoader
            // 
            tlpLoader.Location = new Point(0, 0);
            tlpLoader.Name = "tlpLoader";
            tlpLoader.Size = new Size(200, 100);
            tlpLoader.TabIndex = 0;
            // 
            // tvArchivos
            // 
            tvArchivos.Location = new Point(8, 135);
            tvArchivos.Name = "tvArchivos";
            tvArchivos.Size = new Size(281, 202);
            tvArchivos.TabIndex = 19;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(42, 70);
            label6.Name = "label6";
            label6.Size = new Size(59, 15);
            label6.TabIndex = 21;
            label6.Text = "Segundos";
            // 
            // lblIntervaloDescarga
            // 
            lblIntervaloDescarga.AutoSize = true;
            lblIntervaloDescarga.Location = new Point(8, 70);
            lblIntervaloDescarga.Name = "lblIntervaloDescarga";
            lblIntervaloDescarga.Size = new Size(34, 15);
            lblIntervaloDescarga.TabIndex = 20;
            lblIntervaloDescarga.Text = "00:00";
            // 
            // lblNombreIntervaloDescarga
            // 
            lblNombreIntervaloDescarga.AutoSize = true;
            lblNombreIntervaloDescarga.Location = new Point(8, 48);
            lblNombreIntervaloDescarga.Name = "lblNombreIntervaloDescarga";
            lblNombreIntervaloDescarga.Size = new Size(74, 15);
            lblNombreIntervaloDescarga.TabIndex = 22;
            lblNombreIntervaloDescarga.Text = "Descarga en:";
            // 
            // lblNombreIntervaloSubida
            // 
            lblNombreIntervaloSubida.AutoSize = true;
            lblNombreIntervaloSubida.Location = new Point(8, 43);
            lblNombreIntervaloSubida.Name = "lblNombreIntervaloSubida";
            lblNombreIntervaloSubida.Size = new Size(103, 15);
            lblNombreIntervaloSubida.TabIndex = 23;
            lblNombreIntervaloSubida.Text = "Verifica Subida en:";
            // 
            // tmrDescarga
            // 
            tmrDescarga.Tick += tmrDescarga_Tick;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(539, 385);
            tabControl1.TabIndex = 24;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(btnDescargarPedido);
            tabPage1.Controls.Add(lblNombreIntervaloDescarga);
            tabPage1.Controls.Add(label6);
            tabPage1.Controls.Add(picLogoMain);
            tabPage1.Controls.Add(lblIntervaloDescarga);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(531, 357);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Bajada";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(tvArchivos);
            tabPage2.Controls.Add(lblArchivos);
            tabPage2.Controls.Add(lblNombreIntervaloSubida);
            tabPage2.Controls.Add(cmdTransmitirTx);
            tabPage2.Controls.Add(lblIntervaloSubida);
            tabPage2.Controls.Add(label4);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(531, 357);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Subida";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(lblCambiarDescargaSubirArchivos);
            tabPage3.Controls.Add(btnCambiarIntervaloSubida);
            tabPage3.Controls.Add(nudSubirArchivos);
            tabPage3.Controls.Add(lblCambiarSegundosDescarga);
            tabPage3.Controls.Add(btnCambiarOrigen);
            tabPage3.Controls.Add(btnCambiarProcesados);
            tabPage3.Controls.Add(lblProcesados);
            tabPage3.Controls.Add(lblOrigen);
            tabPage3.Controls.Add(btnCambiarIntervaloDescarga);
            tabPage3.Controls.Add(nudDescarga);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(531, 357);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Configuracion";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // lblCambiarDescargaSubirArchivos
            // 
            lblCambiarDescargaSubirArchivos.AutoSize = true;
            lblCambiarDescargaSubirArchivos.Location = new Point(65, 156);
            lblCambiarDescargaSubirArchivos.Name = "lblCambiarDescargaSubirArchivos";
            lblCambiarDescargaSubirArchivos.Size = new Size(164, 15);
            lblCambiarDescargaSubirArchivos.TabIndex = 18;
            lblCambiarDescargaSubirArchivos.Text = "Segundos para Subir Archivos";
            // 
            // btnCambiarIntervaloSubida
            // 
            btnCambiarIntervaloSubida.Location = new Point(8, 124);
            btnCambiarIntervaloSubida.Name = "btnCambiarIntervaloSubida";
            btnCambiarIntervaloSubida.Size = new Size(70, 23);
            btnCambiarIntervaloSubida.TabIndex = 17;
            btnCambiarIntervaloSubida.Text = "Cambiar Intervalo";
            btnCambiarIntervaloSubida.UseVisualStyleBackColor = true;
            btnCambiarIntervaloSubida.Click += btnCambiarIntervaloSubida_Click;
            // 
            // nudSubirArchivos
            //
            nudSubirArchivos.Location = new Point(10, 153);
            nudSubirArchivos.Minimum = 5;
            nudSubirArchivos.Maximum = 3600;
            nudSubirArchivos.Name = "nudSubirArchivos";
            nudSubirArchivos.Size = new Size(53, 23);
            nudSubirArchivos.TabIndex = 16;
            // 
            // lblCambiarSegundosDescarga
            // 
            lblCambiarSegundosDescarga.AutoSize = true;
            lblCambiarSegundosDescarga.Location = new Point(63, 222);
            lblCambiarSegundosDescarga.Name = "lblCambiarSegundosDescarga";
            lblCambiarSegundosDescarga.Size = new Size(140, 15);
            lblCambiarSegundosDescarga.TabIndex = 15;
            lblCambiarSegundosDescarga.Text = "Segundos para Descargar";
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(txtLog);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(531, 357);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Log";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            txtLog.Dock = DockStyle.Fill;
            txtLog.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtLog.Location = new Point(3, 3);
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.Size = new Size(525, 351);
            txtLog.TabIndex = 0;
            txtLog.Text = "";
            txtLog.WordWrap = false;
            // 
            // notifyIcon1
            //
            notifyIcon1.Text = "SincroApp Demo";
            notifyIcon1.Visible = true;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(539, 385);
            Controls.Add(tabControl1);
            Name = "Form1";
            Text = "SincroApp Demo — Sincronizador";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)nudDescarga).EndInit();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudSubirArchivos).EndInit();
            tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picLogoMain).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button cmdTransmitirTx;
        private Timer tmrSubida;
        private FolderBrowserDialog fbdArchivosSubida;
        private Button btnCambiarProcesados;
        private PictureBox picLogoMain;
        private Label lblOrigen;
        private Label lblProcesados;
        private Label lblIntervaloSubida;
        private NumericUpDown nudDescarga;
        private Button btnCambiarOrigen;
        private Button btnCambiarIntervaloDescarga;
        private Label lblArchivos;
        private Label label4;
        private Button btnDescargarPedido;
        private TableLayoutPanel tlpLoader;
        private FlowLayoutPanel flpLoader;
        private Label lblLoader;
        private ProgressBar progressLoader;
        private TreeView tvArchivos;
        private Label label6;
        private Label lblIntervaloDescarga;
        private Label lblNombreIntervaloDescarga;
        private Label lblNombreIntervaloSubida;
        private Timer tmrDescarga;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private RichTextBox txtLog;
        private NotifyIcon notifyIcon1;
        private Label lblCambiarDescargaSubirArchivos;
        private Button btnCambiarIntervaloSubida;
        private NumericUpDown nudSubirArchivos;
        private Label lblCambiarSegundosDescarga;
    }
}
