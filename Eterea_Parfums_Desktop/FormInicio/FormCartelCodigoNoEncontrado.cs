using Eterea_Parfums_Desktop.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormCartelCodigoNoEncontrado : Form
    {
        private PictureBox pictureBox2;
        private Button btnIngresarManual;
        private Button btnVolverEscanear;
        private Label label1;
        private PictureBox pictureBox1;
        private PictureBox pictureBox3;
        private Label lbl_pregunta;
        private PictureBox pictureBox4;
        private Form _formInvocador;

        private bool abrirIngresoManual = false;
        private bool volverAEscanear = false;



        public FormCartelCodigoNoEncontrado(Form formInvocador)
        {
            InitializeComponent();
            _formInvocador = formInvocador;

            this.FormClosed += FormCartelCodigoNoEncontrado_FormClosed;

        }


        public enum Resultado
        {
            IngresarManual,
            ReintentarEscaneo
        }

        public Resultado Eleccion { get; private set; }

        public FormCartelCodigoNoEncontrado()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /*private void btnIngresarManual_Click(object sender, EventArgs e)
        {
            Eleccion = Resultado.IngresarManual;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }*/

        private void btnReintentar_Click(object sender, EventArgs e)
        {
            Eleccion = Resultado.ReintentarEscaneo;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCartelCodigoNoEncontrado));
            this.lbl_pregunta = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnIngresarManual = new System.Windows.Forms.Button();
            this.btnVolverEscanear = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_pregunta
            // 
            this.lbl_pregunta.AutoSize = true;
            this.lbl_pregunta.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.lbl_pregunta.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_pregunta.Location = new System.Drawing.Point(32, 112);
            this.lbl_pregunta.Name = "lbl_pregunta";
            this.lbl_pregunta.Size = new System.Drawing.Size(529, 24);
            this.lbl_pregunta.TabIndex = 0;
            this.lbl_pregunta.Text = "¿Desea ingresar el código manualmente o volver a escanear?";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(177)))), ((int)(((byte)(184)))));
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox2.Location = new System.Drawing.Point(12, 29);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(579, 61);
            this.pictureBox2.TabIndex = 304;
            this.pictureBox2.TabStop = false;
            // 
            // btnIngresarManual
            // 
            this.btnIngresarManual.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(137)))), ((int)(((byte)(164)))));
            this.btnIngresarManual.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIngresarManual.Location = new System.Drawing.Point(36, 159);
            this.btnIngresarManual.Name = "btnIngresarManual";
            this.btnIngresarManual.Size = new System.Drawing.Size(204, 36);
            this.btnIngresarManual.TabIndex = 305;
            this.btnIngresarManual.Text = "Ingresar Manualmente";
            this.btnIngresarManual.UseVisualStyleBackColor = false;
            this.btnIngresarManual.Click += new System.EventHandler(this.btnIngresarManual_Click_1);
            // 
            // btnVolverEscanear
            // 
            this.btnVolverEscanear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(137)))), ((int)(((byte)(164)))));
            this.btnVolverEscanear.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVolverEscanear.Location = new System.Drawing.Point(357, 159);
            this.btnVolverEscanear.Name = "btnVolverEscanear";
            this.btnVolverEscanear.Size = new System.Drawing.Size(204, 36);
            this.btnVolverEscanear.TabIndex = 306;
            this.btnVolverEscanear.Text = "Volver a Escanear";
            this.btnVolverEscanear.UseVisualStyleBackColor = false;
            this.btnVolverEscanear.Click += new System.EventHandler(this.btnVolverEscanear_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(177)))), ((int)(((byte)(184)))));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(90, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(434, 29);
            this.label1.TabIndex = 307;
            this.label1.Text = "El codigo ingresado no fue encontrado.";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Location = new System.Drawing.Point(11, 94);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(579, 122);
            this.pictureBox1.TabIndex = 308;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(177)))), ((int)(((byte)(184)))));
            this.pictureBox3.ErrorImage = ((System.Drawing.Image)(resources.GetObject("pictureBox3.ErrorImage")));
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(48, 45);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(36, 32);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 309;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(225)))), ((int)(((byte)(230)))));
            this.pictureBox4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox4.Location = new System.Drawing.Point(3, 2);
            this.pictureBox4.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(595, 243);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox4.TabIndex = 310;
            this.pictureBox4.TabStop = false;
            // 
            // FormCartelCodigoNoEncontrado
            // 
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(602, 247);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnVolverEscanear);
            this.Controls.Add(this.btnIngresarManual);
            this.Controls.Add(this.lbl_pregunta);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pictureBox4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormCartelCodigoNoEncontrado";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }





        private void btnIngresarManual_Click_1(object sender, EventArgs e)
        {
            // Abrimos el ingreso manual como "hijo" de este form (Owner)
            var formIngreso = new FormIngresoCodigoManual(_formInvocador);
            formIngreso.Owner = this;               // << CLAVE: encadenar
            this.Hide();                            // ocultamos la pregunta mientras se ingresa

            ModalHelper.MostrarModalSinAgregarNuevoFondo(formIngreso);

            // cuando se cierra el manual, cerramos la pregunta
            this.Close();
        }

        private void btnVolverEscanear_Click_1(object sender, EventArgs e)
        {
            volverAEscanear = true;
            this.Close();

            // Volver a habilitar la UI de escaneo normal en FormInicioAutoconsulta
            var metodo = _formInvocador.GetType().GetMethod("ResetAutoConsulta");
            if (metodo != null)
            {
                metodo.Invoke(_formInvocador, null);
            }
        }

        private void FormCartelCodigoNoEncontrado_FormClosed(object sender, FormClosedEventArgs e)
        {
           
        }



    }
}
