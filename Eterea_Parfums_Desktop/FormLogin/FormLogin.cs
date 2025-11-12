using Eterea_Parfums_Desktop.Controladores;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            // Ruta segura (no importa si Ruta_Base termina o no con \)
            var fileName = "Diseño Logo2.png";
            var rutaCompletaImagen = Path.Combine(Program.Ruta_Base, fileName);

            if (File.Exists(rutaCompletaImagen))
            {
                // Liberamos el archivo y se evitan locks
                using (var fs = new FileStream(rutaCompletaImagen, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var img = Image.FromStream(fs);
                    // Si ya había una imagen, liberarla para evitar locks/mem leaks
                    var anterior = img_logo.Image;
                    img_logo.Image = (Image)img.Clone();
                    anterior?.Dispose();
                }
            }
            else
            {
                MessageBox.Show("No se encontró la imagen:\n" + rutaCompletaImagen,
                                "Archivo faltante", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


            lbl_error_user.Visible = false;
            lbl_error_pass.Visible = false;
            lbl_error_auth.Visible = false;

            
           
            // Suscribirse al evento Load (si no se ha hecho desde el diseñador)
            //this.Load += FormLogin_Load;

        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            txt_usuario.Focus(); // Asigna el foco a txt_usuario al cargar el formulario
        }

        /*private void FormLogin_Load(object sender, EventArgs e)
        {
            txt_usuario.Focus(); // Asigna el foco a txt_usuario al cargar el formulario
        }*/

        private void btn_login_Click(object sender, EventArgs e)
        {

            lbl_error_user.Visible = false;
            lbl_error_pass.Visible = false;
            lbl_error_auth.Visible = false;

            string usuario = txt_usuario.Text;
            string clave = txt_contraseña.Text;




            if (!validarCampos())
            {
                if (EmpleadoControlador.auth(usuario, clave))
                {
                    // Asignar el usuario logueado a la variable global
                    Program.logueado = EmpleadoControlador.obtenerEmpleadoPorUsuario(usuario);

                    // Obtener la referencia de FormStart y FormInicioAutoconsulta
                    //FormStart formStart = Application.OpenForms.OfType<FormStart>().FirstOrDefault();
                    FormInicioAutoconsulta formAutoconsulta = Application.OpenForms.OfType<FormInicioAutoconsulta>().FirstOrDefault();

                    // Ocultar FormInicioAutoconsulta correctamente
                    if (formAutoconsulta != null)
                    {
                        formAutoconsulta.Close();
                        //formAutoconsulta.SendToBack(); // Asegura que no reaparezca en primer plano
                    }
                    
                    // Determinar el formulario a abrir según el rol del usuario
                    Form nuevoFormulario = null;
                    if (Program.logueado.rol == "admin")
                    {
                        nuevoFormulario = new FormInicioAdministrador();
                    }
                    else if (Program.logueado.rol == "vendedor")
                    {
                        nuevoFormulario = new FormInicioVendedor();
                    }

                    if (nuevoFormulario != null) //&& formStart != null)
                    {
                        // Asegurar que FormStart sea el dueño del nuevo formulario
                        //nuevoFormulario.Owner = formStart;
                        nuevoFormulario.Show(); // Mostrar sin bloquear

                        // Forzar que el nuevo formulario esté al frente
                        nuevoFormulario.BringToFront();
                        nuevoFormulario.Activate(); // Asegura que reciba el foco
                        nuevoFormulario.TopMost = true;
                        //nuevoFormulario.TopMost = false; // Restaurar estado normal después

                        // Cerrar el formulario actual (login)
                        this.Close();


                    }
                }
                else
                {
                    lbl_error_auth.Text = "El usuario o la contraseña están mal ingresados.";
                    lbl_error_auth.Visible = true;
                }
            }

        }

        private bool validarCampos()
        {
            bool tieneError = false;
            //VALIDAR QUE AMBOS TEXTOS NO ESTEN VACIOS
            if (string.IsNullOrEmpty(txt_usuario.Text) & string.IsNullOrEmpty(txt_contraseña.Text))
            {
                lbl_error_user.Text = "Se debe ingresar un usuario.";
                lbl_error_user.Visible = true;
                lbl_error_pass.Text = "Se debe ingresar una contraseña.";
                lbl_error_pass.Visible = true;
                tieneError = true;
            }

            else if (string.IsNullOrEmpty(txt_usuario.Text))
            {
                lbl_error_user.Text = "Se debe ingresar un usuario.";
                lbl_error_user.Visible = true;
                tieneError = true;
            }
            else if (string.IsNullOrEmpty(txt_contraseña.Text))
            {
                lbl_error_pass.Text = "Se debe ingresar una contraseña.";
                lbl_error_pass.Visible = true;
                tieneError = true;
            }

            else
            {
                lbl_error_user.Visible = false;
            }
            return tieneError;

        }

        private async void close_Click(object sender, EventArgs e)
        {
            
            // Esperar un poco para que se procese el cambio de formulario
            //await Task.Delay(100);

            // Crear y mostrar el formulario de inicio de autoconsulta
            FormInicioAutoconsulta formInicioAutoconsulta = new FormInicioAutoconsulta();
            formInicioAutoconsulta.Show();
            formInicioAutoconsulta.WindowState = FormWindowState.Normal;
            formInicioAutoconsulta.TopMost = true;
            formInicioAutoconsulta.BringToFront();
            formInicioAutoconsulta.Focus();
            formInicioAutoconsulta.Activate();


            this.Hide();       // ocultamos el login para que no se note

            // ⚠️ Esperar brevemente para asegurarnos que el nuevo formulario se dibuje
            await Task.Delay(200); // Ajustá el tiempo si hace falta
            //Cerrar el formLogin
            this.Close();
        }


        /*private async void close_Click1(object sender, EventArgs e)
        {
            FormInicioAutoconsulta inicioAutoconsulta = null;
            FormStart formStart = null;

            // Esperar un poco para que se procese el cambio de formulario
            await Task.Delay(100);

            // Crear y mostrar el formulario de inicio de autoconsulta
            FormInicioAutoconsulta formInicioAutoconsulta = new FormInicioAutoconsulta();
            formInicioAutoconsulta.Show();
            formInicioAutoconsulta.WindowState = FormWindowState.Normal;
            formInicioAutoconsulta.TopMost = true;
            formInicioAutoconsulta.BringToFront();
            formInicioAutoconsulta.Activate();

            // Cerrar el FormLogin con un pequeño retraso para asegurar que el otro formulario se muestra bien
            Task.Delay(100).ContinueWith(_ => this.Close(), TaskScheduler.FromCurrentSynchronizationContext());
        }*/


    }
}
