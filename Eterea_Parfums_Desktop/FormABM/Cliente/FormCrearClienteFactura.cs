using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.ControlesDeUsuario;
using Eterea_Parfums_Desktop.Modelos;
using PdfSharp.Quality;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormCrearClienteFactura : Form
    {
        public List<Pais> paises;
        public List<Provincia> provincias;
        public List<Localidad> localidades;
        public List<Calle> calles;

        private long _dni;

        private static bool SoloLetras(string s) =>
            Regex.IsMatch(s ?? "", @"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü]+$");

        private static bool AlfanumericoRazonSocial(string s) =>
            Regex.IsMatch(s ?? "", @"^[A-Za-z0-9ÁÉÍÓÚáéíóúÑñÜü\s\.\-&/()']+$");

        private static bool LetrasYPunto(string s) =>
    Regex.IsMatch(s ?? "", @"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s\.]+$");

        public Cliente ClienteCreado { get; private set; }

        public FormCrearClienteFactura(long dni)
        {
            InitializeComponent();

            _dni = dni;
            this.Load += FormCrearClienteFactura_Load; // ✅ SUSCRIPCIÓN AL EVENTO

            txt_nombre.TextChanged += (s, e) => lbl_nombreE.Hide();
            txt_apellido.TextChanged += (s, e) => lbl_apellidoE.Hide();
            txt_email.TextChanged += (s, e) => lbl_emailE.Hide();



        }
        private void FormCrearClienteFactura_Load(object sender, EventArgs e)
        {
            lbl_nombreE.Hide();
            lbl_apellidoE.Hide();
            lbl_dniE.Hide();
            lbl_cond_ivaE.Hide();
            lbl_emailE.Hide();

            bool esCUIT = _dni.ToString().Length == 11;
            bool esDNI = _dni.ToString().Length == 8;

            if (esCUIT)
            {
                lbl_dni.Text = "CUIT";
                lbl_nombre.Text = "Empresa";
                lbl_apellido.Text = "Tipo";

                lbl_nombreE.Text = "Debe ingresar la razón social.";
                lbl_apellidoE.Text = "Debe ingresar el tipo de sociedad.";
            }
            else if (esDNI)
            {
                lbl_dni.Text = "DNI";
                lbl_nombre.Text = "Nombre";
                lbl_apellido.Text = "Apellido";

                lbl_nombreE.Text = "Debe ingresar un nombre.";
                lbl_apellidoE.Text = "Debe ingresar un apellido.";
            }

            txt_dni.Text = _dni.ToString();
            txt_dni.ReadOnly = true;

            CargarOpcionesIVA(esDNI, esCUIT);

            combo_con_iva.DrawMode = DrawMode.OwnerDrawFixed;
            combo_con_iva.DrawItem += comboBoxdiseño_DrawItem;
            combo_con_iva.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void CargarOpcionesIVA(bool esDNI, bool esCUIT)
        {
            combo_con_iva.Items.Clear();

            if (esDNI)
            {
                combo_con_iva.Items.Add("Consumidor Final");
                combo_con_iva.Items.Add("Exento");
                combo_con_iva.Items.Add("Monotributista");
                combo_con_iva.SelectedIndex = 0; // por defecto CF
            }
            else if (esCUIT)
            {
                combo_con_iva.Items.Add("Responsable Inscripto");
                combo_con_iva.SelectedIndex = 0; // única opción
            }


        }


        private void Txt_dni_TextChanged(object sender, EventArgs e)
        {
            string texto = txt_dni.Text.Trim();

            if (texto.Length == 8)
            {
                lbl_dni.Text = "DNI";
            }
            else if (texto.Length == 11)
            {
                lbl_dni.Text = "CUIT";
            }
            else
            {
                lbl_dni.Text = "Documento";
            }
        }


        private void btn_crear_cliente_Click(object sender, EventArgs e)
        {
            bool clienteValidado = validarCliente(out string errorMsg);
            if (clienteValidado)
            {
                string usuario = txt_dni.Text; // ✅ usar DNI o CUIT como usuario
                string clavePorDefecto = txt_dni.Text;

                // ✅ Hashear la clave
                string claveHasheada = PasswordHelper.CrearHash(clavePorDefecto);

                string rol = "cliente";
                Pais pais = PaisControlador.getByName("SIN DATO");
                Provincia provincia = ProvinciaControlador.getByName("SIN DATO");
                Localidad localidad = LocalidadControlador.getByName("SIN DATO");
                Calle calle = CalleControlador.getByName("SIN DATO");
                DateTime fecha = new DateTime(1900, 1, 1);


                Cliente cliente = new Cliente(0, usuario, claveHasheada, txt_nombre.Text, txt_apellido.Text,
                    long.Parse(txt_dni.Text), combo_con_iva.Text, fecha, "0", txt_email.Text,
                    pais, provincia, localidad, 0, calle, 0,
                    "0", "0", " ",
                     true, rol);

                if (ClienteControlador.crearCliente(cliente))
                {
                    this.ClienteCreado = cliente;            // devolver el cliente
                    this.DialogResult = DialogResult.OK;     // informar OK al padre
                    this.Close();
                }
            }
        }
        private bool validarCliente(out string errorMsg)
        {
            errorMsg = "";
            limpiarMensajesError();

            bool esCUIT = (_dni.ToString().Length == 11);

            // --- NOMBRE / RAZÓN SOCIAL ---
            if (string.IsNullOrWhiteSpace(txt_nombre.Text))
            {
                lbl_nombreE.Text = esCUIT ? "Debe ingresar la razón social." : "Debe ingresar un nombre.";
                lbl_nombreE.Show();
                errorMsg += lbl_nombreE.Text + Environment.NewLine;
            }
            else
            {
                if (esCUIT)
                {
                    // Razón social: letras, números y . - & / ( ) '
                    if (!AlfanumericoRazonSocial(txt_nombre.Text))
                    {
                        lbl_nombreE.Text = "La razón social solo puede tener letras, números y . - & / ( ) '.";
                        lbl_nombreE.Show();
                        errorMsg += lbl_nombreE.Text + Environment.NewLine;
                    }
                    else
                    {
                        lbl_nombreE.Visible = false;
                    }
                }
                else
                {
                    // DNI: solo letras, máx 45
                    if (!SoloLetras(txt_nombre.Text))
                    {
                        lbl_nombreE.Text = "El nombre solo puede contener letras.";
                        lbl_nombreE.Show();
                        errorMsg += lbl_nombreE.Text + Environment.NewLine;
                    }
                    else
                    {
                        lbl_nombreE.Visible = false;
                    }
                }
            }

            // --- APELLIDO / TIPO ---
            if (string.IsNullOrWhiteSpace(txt_apellido.Text))
            {
                lbl_apellidoE.Text = esCUIT ? "Debe ingresar el tipo de sociedad." : "Debe ingresar un apellido.";
                lbl_apellidoE.Show();
                errorMsg += lbl_apellidoE.Text + Environment.NewLine;
            }
            else
            {
                if (esCUIT)
                {
                    // Tipo: SOLO letras, espacios y punto ".", máx 45
                    if (!LetrasYPunto(txt_apellido.Text) || txt_apellido.Text.Length > 45)
                    {
                        lbl_apellidoE.Text = txt_apellido.Text.Length > 45
                            ? "No puede tener más de 45 caracteres."
                            : "El tipo solo puede tener letras, espacios y punto.";
                        lbl_apellidoE.Show();
                        errorMsg += lbl_apellidoE.Text + Environment.NewLine;
                    }
                    else
                    {
                        lbl_apellidoE.Visible = false;
                    }
                }
                else
                {
                    // DNI: solo letras y espacios, máx 45
                    if (!SoloLetras(txt_apellido.Text) || txt_apellido.Text.Length > 45)
                    {
                        lbl_apellidoE.Text = !SoloLetras(txt_apellido.Text)
                            ? "El apellido solo puede contener letras."
                            : "No puede tener más de 45 caracteres.";
                        lbl_apellidoE.Show();
                        errorMsg += lbl_apellidoE.Text + Environment.NewLine;
                    }
                    else
                    {
                        lbl_apellidoE.Visible = false;
                    }
                }
            }

            // --- EMAIL: requerido + formato + longitud ---
            if (string.IsNullOrWhiteSpace(txt_email.Text))
            {
                lbl_emailE.Text = "Debes ingresar una dirección de correo electrónico.";
                lbl_emailE.Show();
                errorMsg += lbl_emailE.Text + Environment.NewLine;
            }
            else if (!Regex.IsMatch(txt_email.Text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$") || txt_email.Text.Length > 80)
            {
                if (!Regex.IsMatch(txt_email.Text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                    errorMsg += "Debes ingresar una dirección de correo electrónico válida." + Environment.NewLine;
                if (txt_email.Text.Length > 80)
                    errorMsg += "La dirección de correo electrónico no puede tener más de 80 caracteres." + Environment.NewLine;

                lbl_emailE.Text = "Email inválido.";
                lbl_emailE.Show();
            }
            else
            {
                // Unicidad en BD
                var emailNormalizado = txt_email.Text.Trim().ToLowerInvariant();
                bool existe = ClienteControlador.ExisteEmail(emailNormalizado); // bool

                if (existe)
                {
                    lbl_emailE.Text = "Este email ya está registrado. Ingresá otro.";
                    lbl_emailE.Show();
                    errorMsg += "El email ingresado ya se encuentra registrado. Ingresa otro." + Environment.NewLine;
                }
                else
                {
                    lbl_emailE.Visible = false;
                }
            }

            // --- CONDICIÓN IVA ---
            if (combo_con_iva.SelectedItem == null)
            {
                lbl_cond_ivaE.Text = "Debe seleccionar una condición frente al IVA.";
                lbl_cond_ivaE.Show();
                errorMsg += lbl_cond_ivaE.Text + Environment.NewLine;
            }
            else
            {
                lbl_cond_ivaE.Visible = false;
            }

            // --- LIMPIEZA VISUAL SI NO HAY ERRORES ---
            if (string.IsNullOrEmpty(errorMsg))
            {
                lbl_nombreE.Visible = false;
                lbl_apellidoE.Visible = false;
                lbl_emailE.Visible = false;
                lbl_cond_ivaE.Visible = false;
            }

            return string.IsNullOrEmpty(errorMsg);
        }


        private void limpiarMensajesError()
        {


        }

        private void button2_Click(object sender, EventArgs e)
        {          
            this.Close();
        }

        //Diseño del combo box
        private void comboBoxdiseño_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            // Obtener el ComboBox y el texto del ítem actual
            ComboBox combo = sender as ComboBox;
            string text = combo.Items[e.Index].ToString();

            // Definir colores personalizados
            Color backgroundColor;
            Color textColor;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                // Color cuando el ítem está seleccionado
                backgroundColor = Color.FromArgb(195, 156, 164);
                textColor = Color.White;
            }
            else
            {
                // Color cuando el ítem NO está seleccionado
                backgroundColor = Color.FromArgb(250, 236, 239); // Color personalizado
                textColor = Color.FromArgb(195, 156, 164);
            }

            // Pintar el fondo del ítem
            using (SolidBrush brush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Dibujar el texto
            TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, textColor, TextFormatFlags.Left);

            // Evitar problemas visuales
            e.DrawFocusRectangle();
        }

    }
}
