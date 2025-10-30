using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormEditarEmpleado : Form
    {
        private string originalHashedPassword;

        /*  string situacion;

          int id_eliminar;
        */

        int id_editar;

        public List<Pais> paises;
        public List<Provincia> provincias;
        public List<Localidad> localidades;
        public List<Calle> calles;
        public List<Sucursal> sucursales;
        /*public FormEditarEmpleado()
        {
            InitializeComponent();


            lbl_usuarioE.Hide();
            lbl_claveE.Hide();
            lbl_nombreE.Hide();
            lbl_apellidoE.Hide();
            lbl_dniE.Hide();
            lbl_nacE.Hide();
            lbl_celularE.Hide();
            lbl_e_mailE.Hide();
            lbl_paisE.Hide();
            lbl_provinciaE.Hide();
            lbl_localidadE.Hide();
            lbl_cpE.Hide();
            lbl_calleE.Hide();
            lbl_num_calleE.Hide();
            lbl_pisoE.Hide();
            lbl_departamentoE.Hide();
            lbl_comentarios_domicilioE.Hide();
            lbl_sucursalE.Hide();
            lbl_ingE.Hide();
            lbl_sueldoE.Hide();
            lbl_activoE.Hide();
            lbl_rolE.Hide();

            paises = PaisControlador.getAll();
            combo_pais.Items.Clear();
            foreach (Pais pais in paises)
            {
                if (pais.id != 1)
                    combo_pais.Items.Add(pais.nombre.ToString());
            }

            provincias = ProvinciaControlador.getAll();
            combo_provincia.Items.Clear();
            foreach (Provincia provincia in provincias)
            {
                if (provincia.id != 1)
                    combo_provincia.Items.Add(provincia.nombre.ToString());
            }

            localidades = LocalidadControlador.getAll();
            combo_localidad.Items.Clear();
            foreach (Localidad localidad in localidades)
            {
                if (localidad.id != 1)
                    combo_localidad.Items.Add(localidad.nombre.ToString());
            }

            calles = CalleControlador.getAll();
            combo_calle.Items.Clear();
            foreach (Calle calle in calles)
            {
                if (calle.id != 1)
                    combo_calle.Items.Add(calle.nombre.ToString());
            }

            sucursales = SucursalControlador.getAll();
            combo_sucursal.Items.Clear();
            foreach (Sucursal sucursal in sucursales)
            {
                combo_sucursal.Items.Add(sucursal.nombre.ToString());
            }


            combo_activo.Items.Clear();
            combo_activo.Items.Add("Activo");
            combo_activo.Items.Add("Inactivo");

            combo_rol.Items.Clear();
            combo_rol.Items.Add("admin");
            combo_rol.Items.Add("vendedor");

            //    situacion = "Edicion";

        }*/

        // ------------------------------------------------------------------------------------------------------------------------
        //SOBRECARGAR EL CONSTRUCTOR PARA INICIAR EL FORM CON LA INFO CARGADA, PARA EDITAR
        public FormEditarEmpleado(Empleado empleado)
        {
            InitializeComponent();

            lbl_usuarioE.Hide();
            lbl_claveE.Hide();
            lbl_nombreE.Hide();
            lbl_apellidoE.Hide();
            lbl_dniE.Hide();
            lbl_nacE.Hide();
            lbl_celularE.Hide();
            lbl_e_mailE.Hide();
            lbl_paisE.Hide();
            lbl_provinciaE.Hide();
            lbl_localidadE.Hide();
            lbl_cpE.Hide();
            lbl_calleE.Hide();
            lbl_num_calleE.Hide();
            lbl_pisoE.Hide();
            lbl_departamentoE.Hide();
            lbl_comentarios_domicilioE.Hide();
            lbl_sucursalE.Hide();
            lbl_ingE.Hide();
            lbl_sueldoE.Hide();
            lbl_activoE.Hide();
            lbl_rolE.Hide();


            paises = PaisControlador.getAll();
            combo_pais.Items.Clear();
            foreach (Pais pais in paises)
            {
                if (pais.id != 1)
                    combo_pais.Items.Add(pais.nombre.ToString());

            }

            LimpiarCombo(combo_provincia);
            LimpiarCombo(combo_localidad);
            LimpiarCombo(combo_calle);


            combo_provincia.Items.Clear();

            if (provincias != null && provincias.Count > 0)
            {
                foreach (Provincia provincia in provincias)
                {
                    if (provincia.id != 1)
                        combo_provincia.Items.Add(provincia.nombre.ToString());
                }
            }
            else
            {
                combo_provincia.Items.Add(" ");
                combo_provincia.SelectedIndex = 0;
            }



            combo_localidad.Items.Clear();

            if (localidades != null && localidades.Count > 0)
            {
                foreach (Localidad localidad in localidades)
                {
                    if (localidad.id != 1)
                        combo_localidad.Items.Add(localidad.nombre.ToString());
                }
            }
            else
            {
                combo_localidad.Items.Add(" ");
                combo_localidad.SelectedIndex = 0;
            }



            combo_calle.Items.Clear();

            if (calles != null && calles.Count > 0)
            {
                foreach (Calle calle in calles)
                {
                    if (calle.id != 1)
                        combo_calle.Items.Add(calle.nombre);
                }
            }
            else
            {
                combo_calle.Items.Add(" ");
                combo_calle.SelectedIndex = 0;
            }


            sucursales = SucursalControlador.getAll();
            combo_sucursal.Items.Clear();
            foreach (Sucursal sucursal in sucursales)
            {
                if (sucursal.id != 0)              // ⬅️ filtra id 0
                    combo_sucursal.Items.Add(sucursal.nombre);
            }



            id_editar = empleado.id;

            txt_usuario.Text = empleado.usuario.ToString();
            /*txt_contraseña.Text = empleado.clave.ToString();*/
            //txt_contraseña.Hide();
            // Guardamos el hash actual para usarlo si el usuario no cambia la contraseña
            originalHashedPassword = empleado.clave;

            // No se muestra la contraseña actual en el campo
            txt_contraseña.Text = "";

            // Mostrar el hash por consola
            Console.WriteLine("Hash actual: " + empleado.clave);

            txt_nombre.Text = empleado.nombre.ToString();
            txt_apellido.Text = empleado.apellido.ToString();
            txt_dni.Text = empleado.dni.ToString();
            dateTime_nac.Text = empleado.fecha_nacimiento.ToString();
            txt_celular.Text = empleado.celular.ToString();
            txt_e_mail.Text = empleado.e_mail.ToString();
            combo_pais.SelectedItem = empleado.pais_id.nombre.ToString();

            // Forzar carga de provincias con SelectedIndexChanged simulado
            combo_pais_SelectedIndexChanged(combo_pais, EventArgs.Empty);
            combo_provincia.SelectedItem = empleado.provincia_id.nombre;

            // Forzar carga de localidades
            combo_provincia_SelectedIndexChanged(combo_provincia, EventArgs.Empty);
            combo_localidad.SelectedItem = empleado.localidad_id.nombre;

            txt_cp.Text = empleado.codigo_postal.ToString();

            // Forzá carga de calles
            combo_localidad_SelectedIndexChanged(combo_localidad, EventArgs.Empty);
            combo_calle.SelectedItem = empleado.calle_id.nombre;

            txt_num_calle.Text = empleado.numeracion_calle.ToString();
            txt_piso.Text = empleado.piso.ToString();
            txt_departamento.Text = empleado.departamento.ToString();
            richTextBox_comentario.Text = empleado.comentarios_domicilio.ToString();


            combo_sucursal.Text = empleado.sucursal_id.nombre.ToString();
            dateTime_ing.Text = empleado.fecha_ingreso.ToString();
            txt_sueldo.Text = empleado.sueldo.ToString();


            combo_activo.Items.Clear();
            combo_activo.Items.Add("Activo");
            combo_activo.Items.Add("Inactivo");

            if (empleado.activo == true)
            {
                combo_activo.SelectedItem = "Activo";
            }
            else
            {
                combo_activo.SelectedItem = "Inactivo";
            }

            combo_rol.Items.Clear();
            combo_rol.Items.Add("admin");
            combo_rol.Items.Add("vendedor");

            if (empleado.rol == "admin")
            {
                combo_rol.SelectedItem = "admin";
            }
            else
            {
                combo_rol.SelectedItem = "vendedor";
            }


            //   situacion = "Edicion";

            label1.Text = "Editar Vendedor";
            btn_crear.Text = "Editar";


            //Diseño del combo box
            combo_activo.DrawMode = DrawMode.OwnerDrawFixed;
            combo_activo.DrawItem += comboBoxdiseño_DrawItem;
            combo_activo.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_rol.DrawMode = DrawMode.OwnerDrawFixed;
            combo_rol.DrawItem += comboBoxdiseño_DrawItem;
            combo_rol.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_sucursal.DrawMode = DrawMode.OwnerDrawFixed;
            combo_sucursal.DrawItem += comboBoxdiseño_DrawItem;
            combo_sucursal.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_calle.DrawMode = DrawMode.OwnerDrawFixed;
            combo_calle.DrawItem += comboBoxdiseño_DrawItem;
            combo_calle.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_pais.DrawMode = DrawMode.OwnerDrawFixed;
            combo_pais.DrawItem += comboBoxdiseño_DrawItem;
            combo_pais.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_provincia.DrawMode = DrawMode.OwnerDrawFixed;
            combo_provincia.DrawItem += comboBoxdiseño_DrawItem;
            combo_provincia.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_localidad.DrawMode = DrawMode.OwnerDrawFixed;
            combo_localidad.DrawItem += comboBoxdiseño_DrawItem;
            combo_localidad.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_pais.SelectedIndexChanged += combo_pais_SelectedIndexChanged;
            combo_provincia.SelectedIndexChanged += combo_provincia_SelectedIndexChanged;
            combo_localidad.SelectedIndexChanged += combo_localidad_SelectedIndexChanged;

        }

        private void combo_pais_SelectedIndexChanged(object sender, EventArgs e)
        {
            LimpiarCombo(combo_provincia);
            LimpiarCombo(combo_localidad);
            LimpiarCombo(combo_calle);

            string nombrePais = combo_pais.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(nombrePais)) return;

            Pais paisSeleccionado = paises.FirstOrDefault(p => p.nombre == nombrePais);
            if (paisSeleccionado != null)
            {
                provincias = ProvinciaControlador.getProvinciasPorPaisId(paisSeleccionado.id);
                if (provincias.Any())
                {
                    combo_provincia.Items.Clear();
                    foreach (var provincia in provincias)
                        combo_provincia.Items.Add(provincia.nombre);
                }
                else
                {
                    LimpiarCombo(combo_provincia);
                }
            }
        }

        private void combo_provincia_SelectedIndexChanged(object sender, EventArgs e)
        {
            LimpiarCombo(combo_localidad);
            LimpiarCombo(combo_calle);

            string nombreProvincia = combo_provincia.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(nombreProvincia)) return;

            Provincia provinciaSeleccionada = provincias.FirstOrDefault(p => p.nombre == nombreProvincia);
            if (provinciaSeleccionada != null)
            {
                localidades = LocalidadControlador.getLocalidadesPorProvinciaId(provinciaSeleccionada.id);
                if (localidades.Any())
                {
                    combo_localidad.Items.Clear();
                    foreach (var localidad in localidades)
                        combo_localidad.Items.Add(localidad.nombre);
                }
                else
                {
                    LimpiarCombo(combo_localidad);
                }
            }
        }

        private void combo_localidad_SelectedIndexChanged(object sender, EventArgs e)
        {
            LimpiarCombo(combo_calle);

            string nombreLocalidad = combo_localidad.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(nombreLocalidad)) return;

            Localidad localidadSeleccionada = localidades.FirstOrDefault(l => l.nombre == nombreLocalidad);
            if (localidadSeleccionada != null)
            {
                calles = CalleControlador.getCallesPorLocalidadId(localidadSeleccionada.id);
                if (calles.Any())
                {
                    combo_calle.Items.Clear();
                    foreach (var calle in calles)
                        combo_calle.Items.Add(calle.nombre);
                }
                else
                {
                    LimpiarCombo(combo_calle);
                }
            }
        }




        private void LimpiarCombo(ComboBox combo)
        {
            combo.Items.Clear();
            combo.Items.Add(" ");
            combo.SelectedIndex = 0;
        }


        private void editar()
        {
            bool activo = true;
            if (combo_activo.SelectedItem.ToString() == "Inactivo")
            {
                activo = false;
            }

            String rol = "vendedor";
            if (combo_rol.SelectedItem.ToString() == "admin")
            {
                rol = "admin";
            }

            Pais pais = PaisControlador.getByName(combo_pais.SelectedItem.ToString());
            Provincia provincia = ProvinciaControlador.getByName(combo_provincia.SelectedItem.ToString());
            Localidad localidad = LocalidadControlador.getByName(combo_localidad.SelectedItem.ToString());
            Calle calle = CalleControlador.getByName(combo_calle.SelectedItem.ToString());
            Sucursal sucursal = SucursalControlador.getByName(combo_sucursal.SelectedItem.ToString());

            // Si el usuario ingresó una nueva contraseña, la hasheamos. Si no, usamos la existente.
            string nuevaClave;
            if (!string.IsNullOrEmpty(txt_contraseña.Text))
            {
                // Se ingresa la contraseña en texto plano y se genera su hash
                nuevaClave = PasswordHelper.CrearHash(txt_contraseña.Text);
            }
            else
            {
                // No se modificó la contraseña, se mantiene la original
                nuevaClave = originalHashedPassword;
            }

            Empleado empleado = new Empleado(
                id_editar,
                txt_usuario.Text,
                nuevaClave,
                txt_nombre.Text,
                txt_apellido.Text,
                int.Parse(txt_dni.Text),
                DateTime.Parse(dateTime_nac.Text),
                txt_celular.Text,
                txt_e_mail.Text,
                pais,
                provincia,
                localidad,
                int.Parse(txt_cp.Text),
                calle,
                int.Parse(txt_num_calle.Text),
                txt_piso.Text,
                txt_departamento.Text,
                richTextBox_comentario.Text,
                 sucursal,
                 DateTime.Parse(dateTime_ing.Text),
                 int.Parse(txt_sueldo.Text),
                 activo,
                 rol);

            if (EmpleadoControlador.editarEmpleado(empleado))
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private bool validarDatosEmpleado(out string errorMsg)
        {
            errorMsg = string.Empty;

            // Ocultar etiquetas de error
            lbl_usuarioE.Hide();
            lbl_claveE.Hide();
            lbl_nombreE.Hide();
            lbl_apellidoE.Hide();
            lbl_dniE.Hide();
            lbl_nacE.Hide();
            lbl_celularE.Hide();
            lbl_e_mailE.Hide();
            lbl_paisE.Hide();
            lbl_provinciaE.Hide();
            lbl_localidadE.Hide();
            lbl_cpE.Hide();
            lbl_calleE.Hide();
            lbl_num_calleE.Hide();
            lbl_pisoE.Hide();
            lbl_departamentoE.Hide();
            lbl_comentarios_domicilioE.Hide();
            lbl_sucursalE.Hide();
            lbl_ingE.Hide();
            lbl_sueldoE.Hide();
            lbl_activoE.Hide();
            lbl_rolE.Hide();

            // Validaciones individuales
            if (string.IsNullOrEmpty(txt_usuario.Text))
            {
                lbl_usuarioE.Text = "Debe indicar un nombre de usuario.";
                lbl_usuarioE.Show();
                errorMsg += lbl_usuarioE.Text + Environment.NewLine;
            }
            else if (EmpleadoControlador.ExisteUsuarioEnOtroEmpleado(txt_usuario.Text, id_editar))
            {
                lbl_usuarioE.Text = "Ya existe un empledo con ese nombre de usuario.";
                lbl_usuarioE.Show();
                errorMsg += lbl_usuarioE.Text + Environment.NewLine;
            }

            /*if (string.IsNullOrEmpty(txt_contraseña.Text))
            {
                lbl_claveE.Text = "Debe generar una contraseña.";
                lbl_claveE.Show();
                errorMsg += lbl_claveE.Text + Environment.NewLine;
            }*/

            if (string.IsNullOrEmpty(txt_nombre.Text))
            {
                lbl_nombreE.Text = "Debe ingresar el nombre del usuario.";
                lbl_nombreE.Show();
                errorMsg += lbl_nombreE.Text + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txt_apellido.Text))
            {
                lbl_apellidoE.Text = "Debe ingresar el apellido del usuario.";
                lbl_apellidoE.Show();
                errorMsg += lbl_apellidoE.Text + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txt_dni.Text))
            {
                lbl_dniE.Text = "Debe ingresar el número de DNI del usuario.";
                lbl_dniE.Show();
                errorMsg += lbl_dniE.Text + Environment.NewLine;
            }
            else if (txt_dni.Text.Length != 8 && txt_dni.Text.Length != 11)
            {
                lbl_dniE.Text = "El DNI tiene que ser de 8 u 11 dígitos.";
                lbl_dniE.Show();
                errorMsg += lbl_dniE.Text + Environment.NewLine;
            }
            else
            {
                var empleadoId = EmpleadoControlador.BuscarIdPorDni(txt_dni.Text);
                if (empleadoId != null && empleadoId != id_editar)
                {
                    lbl_dniE.Text = "Ya existe otro empleado con ese DNI.";
                    lbl_dniE.Show();
                    errorMsg += lbl_dniE.Text + Environment.NewLine;
                }
            }

            // Fecha de Ingreso
            if (!DateTime.TryParse(dateTime_ing.Text, out DateTime fecha))
            {
                lbl_ingE.Text = "Debe ingresar una fecha de ingreso válida.";
                lbl_ingE.Show();
                errorMsg += lbl_ingE.Text + Environment.NewLine;
            }
            else
            {
                DateTime hoy = DateTime.Today;

                if (fecha > hoy)
                {
                    lbl_ingE.Text = "La fecha de ingreso no puede ser futura.";
                    lbl_ingE.Show();
                    errorMsg += lbl_ingE.Text + Environment.NewLine;
                }

            }

            // Fecha de nacimiento
            if (!DateTime.TryParse(dateTime_nac.Text, out DateTime fechanac))
            {
                lbl_nacE.Text = "Debe ingresar una fecha de nacimiento válida.";
                lbl_nacE.Show();
                errorMsg += lbl_nacE.Text + Environment.NewLine;
            }
            else
            {
                DateTime hoy = DateTime.Today;

                if (fechanac > hoy)
                {
                    lbl_nacE.Text = "La fecha de nacimiento no puede ser futura.";
                    lbl_nacE.Show();
                    errorMsg += lbl_nacE.Text + Environment.NewLine;
                }
                else
                {
                    int edad = hoy.Year - fechanac.Year;
                    if (fecha.Date > hoy.AddYears(-edad)) edad--;

                    if (edad < 18)
                    {
                        lbl_nacE.Text = "El cliente debe tener al menos 18 años.";
                        lbl_nacE.Show();
                        errorMsg += lbl_nacE.Text + Environment.NewLine;
                    }
                }
            }

            if (string.IsNullOrEmpty(txt_celular.Text) || !int.TryParse(txt_celular.Text, out _))
            {
                lbl_celularE.Text = "Debe ingresar un número de celular válido.";
                lbl_celularE.Show();
                errorMsg += lbl_celularE.Text + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txt_e_mail.Text) || !txt_e_mail.Text.Contains("@"))
            {
                lbl_e_mailE.Text = "Debe ingresar un correo electrónico válido.";
                lbl_e_mailE.Show();
                errorMsg += lbl_e_mailE.Text + Environment.NewLine;
            }
            else if (EmpleadoControlador.ExisteEmailEnOtroEmpleado(txt_e_mail.Text, id_editar))
            {
                lbl_e_mailE.Text = "Ya existe un cliente con ese correo electrónico.";
                lbl_e_mailE.Show();
                errorMsg += lbl_e_mailE.Text + Environment.NewLine;
            }

            if (combo_pais.SelectedItem == null)
            {
                lbl_paisE.Text = "Debe seleccionar un país.";
                lbl_paisE.Show();
                errorMsg += lbl_paisE.Text + Environment.NewLine;
            }

            if (combo_provincia.SelectedItem == null)
            {
                lbl_provinciaE.Text = "Debe seleccionar una provincia.";
                lbl_provinciaE.Show();
                errorMsg += lbl_provinciaE.Text + Environment.NewLine;
            }

            if (combo_localidad.SelectedItem == null)
            {
                lbl_localidadE.Text = "Debe seleccionar una localidad.";
                lbl_localidadE.Show();
                errorMsg += lbl_localidadE.Text + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txt_cp.Text) || !int.TryParse(txt_cp.Text, out _))
            {
                lbl_cpE.Text = "Debe ingresar un código postal válido.";
                lbl_cpE.Show();
                errorMsg += lbl_cpE.Text + Environment.NewLine;
            }

            if (combo_calle.SelectedItem == null)
            {
                lbl_calleE.Text = "Debe seleccionar una calle.";
                lbl_calleE.Show();
                errorMsg += lbl_calleE.Text + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txt_num_calle.Text) || !int.TryParse(txt_num_calle.Text, out _))
            {
                lbl_num_calleE.Text = "Debe ingresar un número de calle válido.";
                lbl_num_calleE.Show();
                errorMsg += lbl_num_calleE.Text + Environment.NewLine;
            }

            if (combo_sucursal.SelectedItem == null)
            {
                lbl_sucursalE.Text = "Debe seleccionar una sucursal.";
                lbl_sucursalE.Show();
                errorMsg += lbl_sucursalE.Text + Environment.NewLine;
            }   

            if (string.IsNullOrEmpty(txt_sueldo.Text) || !int.TryParse(txt_sueldo.Text, out _))
            {
                lbl_sueldoE.Text = "Debe ingresar un sueldo válido.";
                lbl_sueldoE.Show();
                errorMsg += lbl_sueldoE.Text + Environment.NewLine;
            }

            if (combo_activo.SelectedItem == null)
            {
                lbl_activoE.Text = "Debe seleccionar el estado activo/inactivo.";
                lbl_activoE.Show();
                errorMsg += lbl_activoE.Text + Environment.NewLine;
            }

            if (combo_rol.SelectedItem == null)
            {
                lbl_rolE.Text = "Debe seleccionar un rol válido.";
                lbl_rolE.Show();
                errorMsg += lbl_rolE.Text + Environment.NewLine;
            }

            return string.IsNullOrEmpty(errorMsg);
        }

        private void FormEditarEmpleado_Load(object sender, EventArgs e)
        {

        }

        private void btn_crear_Click(object sender, EventArgs e)
        {
            string errorMsg;

            if (validarDatosEmpleado(out errorMsg))
            {
                editar();
            }
            else
            {

            }
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

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
