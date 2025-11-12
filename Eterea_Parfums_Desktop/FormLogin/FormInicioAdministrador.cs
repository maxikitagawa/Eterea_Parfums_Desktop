using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.ControlesDeUsuario;
using Eterea_Parfums_Desktop.ControlesDeUsuario.AdministrarStock;
using Eterea_Parfums_Desktop.ControlesDeUsuario.GenerarInformes;
using Eterea_Parfums_Desktop.Helpers;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormInicioAdministrador : Form
    {
        // Declarar la instancia de ToolTip
        private ToolTip toolTip = new ToolTip();

        // Variable para almacenar el botón previamente seleccionado
        private Button botonAnterior;

        private Facturar_UC facturarUC;
    



        public FormInicioAdministrador()
        {
            InitializeComponent();

            // Configurar las imágenes
            var rutaLogo = Path.Combine(Program.Ruta_Base, "Diseño Logo2.png");
            if (File.Exists(rutaLogo))
            {
                using (var fs = new FileStream(rutaLogo, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var img = (Image)Image.FromStream(fs).Clone();
                    var old = img_logo.Image;
                    img_logo.Image = img;
                    old?.Dispose();
                }
            }
            else
            {
                MessageBox.Show("No se encontró: " + rutaLogo);
            }

            // --- Cerrar sesión ---
            var rutaCerrar = Path.Combine(Program.Ruta_Base, "CerrarSesion.png");
            if (File.Exists(rutaCerrar))
            {
                using (var fs = new FileStream(rutaCerrar, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var img = (Image)Image.FromStream(fs).Clone();
                    var old = btn_cerrar_sesion.Image;
                    btn_cerrar_sesion.Image = img;
                    old?.Dispose();
                }
            }
            else
            {
                MessageBox.Show("No se encontró: " + rutaCerrar);
            }

            // Configurar el saludo
            txt_saludo.Text = Program.logueado.nombre + " " + Program.logueado.apellido;

            // Configurar el ToolTip para el botón de cerrar sesión
            toolTip.SetToolTip(btn_cerrar_sesion, "Cerrar sesión");

            Gestionar_UC adminUC = new Gestionar_UC();
            addUserControl(adminUC);

            pictureBox3.Visible = false;
            pictureBox5.Visible = false;
            pictureBox10.Visible = false;

            // Cambiar el color de PictureBox
            pictureBox4.BackColor = Color.FromArgb(232, 186, 197);
            btn_gestionar.BackColor = Color.FromArgb(232, 186, 197);

            // Obtener y mostrar el nombre de la sucursal en el label
            txt_nombre_suc.Text = SucursalControlador.ObtenerNombreSucursalPorId(Program.sucursal);
        


        }

        private void btn_cerrar_sesion_Click(object sender, EventArgs e)
        {
            if (CajaManager.HayCajaAsignada())
            {
                MessageBox.Show(
                    "Hay una caja abierta en uso.\nDebes cerrarla antes de cerrar sesión.",
                    "Cierre de sesión bloqueado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            var confirm = MessageBox.Show("¿Estás seguro que deseas cerrar sesión?", "Cerrar sesión", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                // Limpiar datos de sesión
                Program.logueado = null;
                CajaManager.ResetCaja();

                // Mostrar el login encima del FormStart
                FormLogin formLogin = new FormLogin();
                formLogin.StartPosition = FormStartPosition.CenterScreen;
                formLogin.Show(); // Mostrar encima del FormStart



                // Cerrar este form (FormInicioAdministrador)
                this.Close();
            }
        }



        private void btn_gestionar_Click(object sender, EventArgs e)
        {
            Gestionar_UC adminUC = new Gestionar_UC();
            addUserControl(adminUC);

            CambiarColorBoton1((Button)sender);
        }

        private void btn_facturar_Click(object sender, EventArgs e)
        {
            CambiarColorBoton2((Button)sender);

            if (facturarUC == null || facturarUC.IsDisposed)
                facturarUC = new Facturar_UC();

            addUserControl(facturarUC);

            facturarUC.BringToFront();
            facturarUC.Visible = true;
        }



        private void btn_administrar_stock_Click(object sender, EventArgs e)
        {
            CambiarColorBoton3((Button)sender);

            // Verificamos si ya hay una caja asignada
            if (Program.sucursal != 0)
            {
                // Ya hay una sucursal asignada, abrir directamente el panel de stock
                AdministrarStock_UC administrarStockUC = new AdministrarStock_UC(Program.sucursal);

                addUserControl(administrarStockUC);
            }
            /*else
            {
                // No hay sucursal asignada, mostrar FormNumeroDeSucursal para elegirla
                FormNumeroDeSucursal formNumeroDeSucursal = new FormNumeroDeSucursal();
                formNumeroDeSucursal.ConfirmarNumeroSucursal += (s, nuevaSucursal) =>
                {
                    Program.sucursal = nuevaSucursal;
                    AdministrarStock_UC administrarStockUC = new AdministrarStock_UC(Program.sucursal);
                    addUserControl(administrarStockUC);
                };

                formNumeroDeSucursal.ShowDialog();
            }*/
        }    

        private void btn_generar_informes_Click(object sender, EventArgs e)
        {
            CambiarColorBoton4((Button)sender);

            GenerarInformes_UC informesDeVentas1UC = new GenerarInformes_UC();
            addUserControl(informesDeVentas1UC);           
        }


        public void addUserControl(UserControl uc)
        {
            uc.Dock = DockStyle.Fill;
            panel_admin.Controls.Clear();
            panel_admin.Controls.Add(uc);
            uc.BringToFront();
        }

        private void CambiarColorBoton1(Button botonSeleccionado)
        {
            // Restaurar el color del botón anterior si existe
            if (botonAnterior != null)
            {
                botonAnterior.BackColor = Color.FromArgb(232, 196, 206); // Restaurar color original
                pictureBox1.BackColor = Color.FromArgb(232, 196, 206);  // Restaurar color original de PictureBox1
                
            }

            // Cambiar el color del botón seleccionado
            botonSeleccionado.BackColor = Color.FromArgb(232, 186, 197);

            // Cambiar el color de PictureBox
            pictureBox4.BackColor = Color.FromArgb(232, 186, 197);
            pictureBox1.BackColor = Color.FromArgb(232, 196, 206);
            pictureBox8.BackColor = Color.FromArgb(232, 196, 206);
            pictureBox9.BackColor = Color.FromArgb(232, 196, 206);

            // Mostrar PictureBox
            pictureBox2.Visible = true;
            pictureBox3.Visible = false;
            pictureBox5.Visible = false;
            pictureBox10.Visible = false;

            // Guardar el botón actual como el anterior
            botonAnterior = botonSeleccionado;
        }

        private void CambiarColorBoton2(Button botonSeleccionado)
        {
            // Restaurar el color del botón anterior si existe
            if (botonAnterior != null)
            {
                botonAnterior.BackColor = Color.FromArgb(232, 196, 206); // Restaurar color original           
            }

            // Cambiar el color del botón seleccionado
            botonSeleccionado.BackColor = Color.FromArgb(232, 186, 197);

            btn_gestionar.BackColor = Color.FromArgb(232, 196, 206);

            // Cambiar el color de PictureBox
            pictureBox4.BackColor = Color.FromArgb(232, 196, 206);
            pictureBox1.BackColor = Color.FromArgb(232, 186, 197);
            pictureBox8.BackColor = Color.FromArgb(232, 196, 206);
            pictureBox9.BackColor = Color.FromArgb(232, 196, 206);


            // Mostrar PictureBox
            pictureBox2.Visible = false;
            pictureBox3.Visible = true;
            pictureBox5.Visible = false;
            pictureBox10.Visible = false;

            // Guardar el botón actual como el anterior
            botonAnterior = botonSeleccionado;
        }

        private void CambiarColorBoton3(Button botonSeleccionado)
        {
            // Restaurar el color del botón anterior si existe
            if (botonAnterior != null)
            {
                botonAnterior.BackColor = Color.FromArgb(232, 196, 206); // Restaurar color original           
            }

            // Cambiar el color del botón seleccionado
            botonSeleccionado.BackColor = Color.FromArgb(232, 186, 197);

            btn_gestionar.BackColor = Color.FromArgb(232, 196, 206);

            // Cambiar el color de PictureBox
            pictureBox4.BackColor = Color.FromArgb(232, 196, 206);
            pictureBox1.BackColor = Color.FromArgb(232, 196, 206);
            pictureBox8.BackColor = Color.FromArgb(232, 186, 197);
            pictureBox9.BackColor = Color.FromArgb(232, 196, 206);


            // Mostrar PictureBox
            pictureBox2.Visible = false;
            pictureBox3.Visible = false;
            pictureBox5.Visible = true;
            pictureBox10.Visible = false;

            // Guardar el botón actual como el anterior
            botonAnterior = botonSeleccionado;
        }

        private void CambiarColorBoton4(Button botonSeleccionado)
        {
            // Restaurar el color del botón anterior si existe
            if (botonAnterior != null)
            {
                botonAnterior.BackColor = Color.FromArgb(232, 196, 206); // Restaurar color original
            }

            // Cambiar el color del botón seleccionado
            botonSeleccionado.BackColor = Color.FromArgb(232, 186, 197);

            btn_gestionar.BackColor = Color.FromArgb(232, 196, 206);

            // Cambiar el color de PictureBox
            pictureBox4.BackColor = Color.FromArgb(232, 196, 206);
            pictureBox1.BackColor = Color.FromArgb(232, 196, 206);
            pictureBox8.BackColor = Color.FromArgb(232, 196, 206);
            pictureBox9.BackColor = Color.FromArgb(232, 186, 197);
            

            // Mostrar PictureBox
            pictureBox2.Visible = false;
            pictureBox3.Visible = false;
            pictureBox5.Visible = false;
            pictureBox10.Visible = true;

            // Guardar el botón actual como el anterior
            botonAnterior = botonSeleccionado;
        }

        public void MostrarFacturar()
        {
            if (facturarUC == null)
                facturarUC = new Facturar_UC();

            addUserControl(facturarUC);
        }


    }
}