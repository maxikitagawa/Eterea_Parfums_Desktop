using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormCrearEmpleado : Form
    {
        // Fuentes de datos completas para filtrar sin perder la lista original
        private List<Pais> _paises = new List<Pais>();
        private List<Provincia> _provincias = new List<Provincia>();
        private List<Localidad> _localidades = new List<Localidad>();
        private List<Calle> _calles = new List<Calle>();
        private List<Sucursal> _sucursales = new List<Sucursal>();

        // RegEx contraseña: 8+ chars, al menos 1 mayúscula, 1 letra, 1 dígito y 1 especial del set dado
        private static readonly Regex _rxClave =
            new Regex(@"^(?=.*[A-Z])(?=.*[a-zA-Z])(?=.*\d)(?=.*[!¡""#\$%&/\(\)=\?¿]).{8,}$");

        public FormCrearEmpleado()
        {
            InitializeComponent();

            // Ocultar errores
            OcultarErrores();

            // ---------- INPUT MASKS / LIMITES ----------
            // Usuario
            txt_usuario.MaxLength = 45;

            // Clave (validación fuerte en submit)
            txt_contraseña.MaxLength = 100; // por si luego usás generadores

            // Nombre/Apellido: 2..45
            txt_nombre.MaxLength = 45;
            txt_apellido.MaxLength = 45;
            txt_nombre.KeyPress += SoloLetrasEspacios_KeyPress;
            txt_apellido.KeyPress += SoloLetrasEspacios_KeyPress;

            // DNI: exactamente 8 dígitos
            txt_dni.MaxLength = 8;
            txt_dni.KeyPress += SoloDigitos_KeyPress;

            // Celular: máximo 13 dígitos
            txt_celular.MaxLength = 13;
            txt_celular.KeyPress += SoloDigitos_KeyPress;

            // Código Postal: 4 dígitos
            txt_cp.MaxLength = 4;
            txt_cp.KeyPress += SoloDigitos_KeyPress;

            // Número de calle: solo números, hasta 6 dígitos
            txt_num_calle.MaxLength = 6;
            txt_num_calle.KeyPress += SoloDigitos_KeyPress;

            // Piso / Dpto: alfanumérico, sin especiales, hasta 3 chars
            txt_piso.MaxLength = 3;
            txt_departamento.MaxLength = 3;
            txt_piso.KeyPress += SoloAlfaNumerico_KeyPress;
            txt_departamento.KeyPress += SoloAlfaNumerico_KeyPress;

            // Sueldo: solo números, validación >= 500000 en submit
            txt_sueldo.MaxLength = 10;
            txt_sueldo.KeyPress += SoloDigitos_KeyPress;

            // Comentarios domicilio: letras, números, espacios y como máximo 2 paréntesis en total
            richTextBox_comentario.MaxLength = 60;
            richTextBox_comentario.KeyPress += Comentarios_KeyPress;

            // Fechas
            dateTime_nac.ValueChanged += dateTime_nac_ValueChanged;
            dateTime_nac.Format = DateTimePickerFormat.Short;
            dateTime_ing.Format = DateTimePickerFormat.Short;

            // ---------- COMBOS ----------
            // Estado, Rol, Sucursal (diseño como lo tenías)
            combo_activo.Items.Clear();
            combo_activo.Items.Add("Activo");
            combo_activo.Items.Add("Inactivo");
            combo_activo.DrawMode = DrawMode.OwnerDrawFixed;
            combo_activo.DrawItem += comboBoxdiseño_DrawItem;
            combo_activo.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_rol.Items.Clear();
            combo_rol.Items.Add("admin");
            combo_rol.Items.Add("vendedor");
            combo_rol.DrawMode = DrawMode.OwnerDrawFixed;
            combo_rol.DrawItem += comboBoxdiseño_DrawItem;
            combo_rol.DropDownStyle = ComboBoxStyle.DropDownList;

            // Sucursales (filtrando id 0)
            _sucursales = SucursalControlador.getAll() ?? new List<Sucursal>();
            combo_sucursal.DrawMode = DrawMode.OwnerDrawFixed;
            combo_sucursal.DrawItem += comboBoxdiseño_DrawItem;
            combo_sucursal.DropDownStyle = ComboBoxStyle.DropDownList;
            combo_sucursal.Items.Clear();
            foreach (var s in _sucursales.Where(s => s.id != 0))
                combo_sucursal.Items.Add(s.nombre);

            // País/Prov/Loc/Calle con escritura + autocompletar dependiente
            PrepararComboAutocompletable(combo_pais);
            PrepararComboAutocompletable(combo_provincia);
            PrepararComboAutocompletable(combo_localidad);
            PrepararComboAutocompletable(combo_calle);

            // Cargar Países
            _paises = PaisControlador.getAll() ?? new List<Pais>();
            var nombresPaises = _paises.Where(p => p.id != 1).Select(p => p.nombre).ToList();
            CargarComboConAutoComplete(combo_pais, nombresPaises);

            // Deshabilitar dependientes inicialmente
            HabilitarCombo(combo_provincia, false);
            HabilitarCombo(combo_localidad, false);
            HabilitarCombo(combo_calle, false);

            // Eventos de dependencia
            combo_pais.TextChanged += combo_pais_TextChanged;
            combo_provincia.TextChanged += combo_provincia_TextChanged;
            combo_localidad.TextChanged += combo_localidad_TextChanged;

            // Botones
            btn_crear.Click += btn_crear_Click_1;
            button1.Click += button1_Click;
        }

        // ====== Helpers de UI ======
        private void OcultarErrores()
        {
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
        }

        private void PrepararComboAutocompletable(ComboBox combo)
        {
            combo.DropDownStyle = ComboBoxStyle.DropDown; // permite escribir
            combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            combo.AutoCompleteSource = AutoCompleteSource.CustomSource;
            combo.DrawMode = DrawMode.OwnerDrawFixed;
            combo.DrawItem += comboBoxdiseño_DrawItem;
        }

        private void CargarComboConAutoComplete(ComboBox combo, List<string> valores)
        {
            combo.Items.Clear();
            foreach (var v in valores) combo.Items.Add(v);

            var ac = new AutoCompleteStringCollection();
            ac.AddRange(valores.ToArray());
            combo.AutoCompleteCustomSource = ac;
        }

        private void HabilitarCombo(ComboBox combo, bool habilitar)
        {
            combo.Enabled = habilitar;
            if (!habilitar)
            {
                combo.Text = string.Empty;
                combo.Items.Clear();
                combo.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            }
        }

        // ====== Dependencias de combos con filtrado incremental ======
        private void combo_pais_TextChanged(object sender, EventArgs e)
        {
            var texto = combo_pais.Text?.Trim() ?? "";
            var paisSel = _paises.FirstOrDefault(p => string.Equals(p.nombre, texto, StringComparison.OrdinalIgnoreCase));
            if (paisSel == null)
            {
                // Mientras escribe, sugerimos por prefijo
                var sugerencias = _paises
                    .Where(p => p.id != 1 && p.nombre.StartsWith(texto, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.nombre)
                    .ToList();
                CargarComboConAutoComplete(combo_pais, sugerencias.Any() ? sugerencias : _paises.Where(p => p.id != 1).Select(p => p.nombre).ToList());
                HabilitarCombo(combo_provincia, false);
                HabilitarCombo(combo_localidad, false);
                HabilitarCombo(combo_calle, false);
                return;
            }

            // País válido → cargar provincias y habilitar
            _provincias = ProvinciaControlador.getProvinciasPorPaisId(paisSel.id) ?? new List<Provincia>();
            CargarComboConAutoComplete(combo_provincia, _provincias.Select(x => x.nombre).ToList());
            HabilitarCombo(combo_provincia, true);
            HabilitarCombo(combo_localidad, false);
            HabilitarCombo(combo_calle, false);
        }

        private void combo_provincia_TextChanged(object sender, EventArgs e)
        {
            var texto = combo_provincia.Text?.Trim() ?? "";
            var provSel = _provincias.FirstOrDefault(p => string.Equals(p.nombre, texto, StringComparison.OrdinalIgnoreCase));
            if (provSel == null)
            {
                var sugerencias = _provincias
                    .Where(p => p.nombre.StartsWith(texto, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.nombre)
                    .ToList();
                CargarComboConAutoComplete(combo_provincia, sugerencias.Any() ? sugerencias : _provincias.Select(p => p.nombre).ToList());
                HabilitarCombo(combo_localidad, false);
                HabilitarCombo(combo_calle, false);
                return;
            }

            _localidades = LocalidadControlador.getLocalidadesPorProvinciaId(provSel.id) ?? new List<Localidad>();
            CargarComboConAutoComplete(combo_localidad, _localidades.Select(x => x.nombre).ToList());
            HabilitarCombo(combo_localidad, true);
            HabilitarCombo(combo_calle, false);
        }

        private void combo_localidad_TextChanged(object sender, EventArgs e)
        {
            var texto = combo_localidad.Text?.Trim() ?? "";
            var locSel = _localidades.FirstOrDefault(l => string.Equals(l.nombre, texto, StringComparison.OrdinalIgnoreCase));
            if (locSel == null)
            {
                var sugerencias = _localidades
                    .Where(l => l.nombre.StartsWith(texto, StringComparison.OrdinalIgnoreCase))
                    .Select(l => l.nombre)
                    .ToList();
                CargarComboConAutoComplete(combo_localidad, sugerencias.Any() ? sugerencias : _localidades.Select(l => l.nombre).ToList());
                HabilitarCombo(combo_calle, false);
                return;
            }

            _calles = CalleControlador.getCallesPorLocalidadId(locSel.id) ?? new List<Calle>();
            CargarComboConAutoComplete(combo_calle, _calles.Select(x => x.nombre).ToList());
            HabilitarCombo(combo_calle, true);
        }

        // ====== Botones ======
        private void btn_crear_Click_1(object sender, EventArgs e)
        {
            if (validarDatosEmpleado(out string errorMsg))
            {
                crear();
            }
            else
            {
                MessageBox.Show(errorMsg, "Datos inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void crear()
        {
            bool activo = (combo_activo.SelectedItem?.ToString() == "Activo");
            string rol = (combo_rol.SelectedItem?.ToString() == "admin") ? "admin" : "vendedor";

            var pais = PaisControlador.getByName(combo_pais.Text);
            var provincia = ProvinciaControlador.getByName(combo_provincia.Text);
            var localidad = LocalidadControlador.getByName(combo_localidad.Text);
            var calle = CalleControlador.getByName(combo_calle.Text);
            var sucursal = SucursalControlador.getByName(combo_sucursal.SelectedItem?.ToString());

            var empleado = new Empleado(
                0,
                txt_usuario.Text.Trim(),
                txt_contraseña.Text,                   // hashéalas en el controlador si corresponde
                txt_nombre.Text.Trim(),
                txt_apellido.Text.Trim(),
                int.Parse(txt_dni.Text),
                DateTime.Parse(dateTime_nac.Text),
                txt_celular.Text.Trim(),
                txt_e_mail.Text.Trim(),
                pais,
                provincia,
                localidad,
                int.Parse(txt_cp.Text),
                calle,
                int.Parse(txt_num_calle.Text),
                txt_piso.Text.Trim(),
                txt_departamento.Text.Trim(),
                richTextBox_comentario.Text.Trim(),
                sucursal,
                DateTime.Parse(dateTime_ing.Text),
                int.Parse(txt_sueldo.Text),
                activo,
                rol
            );

            if (EmpleadoControlador.crearEmpleado(empleado))
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        // ====== Validaciones de Submit ======
        private bool validarDatosEmpleado(out string errorMsg)
        {
            errorMsg = string.Empty;
            OcultarErrores();

            // Usuario
            if (string.IsNullOrWhiteSpace(txt_usuario.Text) || txt_usuario.Text.Length < 3 || txt_usuario.Text.Length > 45)
            {
                lbl_usuarioE.Text = "El usuario debe tener entre 3 y 45 caracteres.";
                lbl_usuarioE.Show(); errorMsg += lbl_usuarioE.Text + Environment.NewLine;
            }
            else if (EmpleadoControlador.ExisteUsuario(txt_usuario.Text.Trim()))
            {
                lbl_usuarioE.Text = "Ya existe un empleado con ese nombre de usuario.";
                lbl_usuarioE.Show(); errorMsg += lbl_usuarioE.Text + Environment.NewLine;
            }

            // Clave (fuerte)
            if (string.IsNullOrWhiteSpace(txt_contraseña.Text) || !_rxClave.IsMatch(txt_contraseña.Text))
            {
                lbl_claveE.Text = "La clave debe tener 8+ caracteres, incluir mayúsculas, letras, números y 1 especial (!, ¡, \", #, $, %, &, /, (, ), =, ?, ¿).";
                lbl_claveE.Show(); errorMsg += lbl_claveE.Text + Environment.NewLine;
            }

            // Nombre / Apellido
            if (txt_nombre.Text.Trim().Length < 2 || txt_nombre.Text.Trim().Length > 45)
            {
                lbl_nombreE.Text = "El nombre debe tener entre 2 y 45 caracteres.";
                lbl_nombreE.Show(); errorMsg += lbl_nombreE.Text + Environment.NewLine;
            }
            if (txt_apellido.Text.Trim().Length < 2 || txt_apellido.Text.Trim().Length > 45)
            {
                lbl_apellidoE.Text = "El apellido debe tener entre 2 y 45 caracteres.";
                lbl_apellidoE.Show(); errorMsg += lbl_apellidoE.Text + Environment.NewLine;
            }

            // DNI
            if (txt_dni.Text.Length != 8 || !txt_dni.Text.All(char.IsDigit))
            {
                lbl_dniE.Text = "El DNI debe tener exactamente 8 dígitos numéricos.";
                lbl_dniE.Show(); errorMsg += lbl_dniE.Text + Environment.NewLine;
            }
            else
            {
                var existente = EmpleadoControlador.BuscarIdPorDni(txt_dni.Text);
                if (existente != null)
                {
                    lbl_dniE.Text = "Ya existe un empleado con ese DNI.";
                    lbl_dniE.Show(); errorMsg += lbl_dniE.Text + Environment.NewLine;
                }
            }

            // Fechas
            if (!DateTime.TryParse(dateTime_ing.Text, out DateTime fechaIng) || fechaIng > DateTime.Today)
            {
                lbl_ingE.Text = "La fecha de ingreso debe ser válida y no futura.";
                lbl_ingE.Show(); errorMsg += lbl_ingE.Text + Environment.NewLine;
            }
            if (!DateTime.TryParse(dateTime_nac.Text, out DateTime fechaNac) || fechaNac > DateTime.Today || CalcularEdad(fechaNac) < 18)
            {
                lbl_nacE.Text = "La fecha de nacimiento debe ser válida, no futura y el empleado debe ser mayor de 18 años.";
                lbl_nacE.Show(); errorMsg += lbl_nacE.Text + Environment.NewLine;
            }

            // Celular
            if (string.IsNullOrWhiteSpace(txt_celular.Text) || !txt_celular.Text.All(char.IsDigit) || txt_celular.Text.Length > 13)
            {
                lbl_celularE.Text = "El celular debe ser numérico y no superar 13 dígitos.";
                lbl_celularE.Show(); errorMsg += lbl_celularE.Text + Environment.NewLine;
            }

            // Email
            if (string.IsNullOrWhiteSpace(txt_e_mail.Text) ||
                !Regex.IsMatch(txt_e_mail.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                lbl_e_mailE.Text = "Debe ingresar un correo electrónico válido.";
                lbl_e_mailE.Show(); errorMsg += lbl_e_mailE.Text + Environment.NewLine;
            }
            else if (EmpleadoControlador.ExisteEmail(txt_e_mail.Text.Trim()))
            {
                lbl_e_mailE.Text = "Ya existe un empleado registrado con ese correo electrónico.";
                lbl_e_mailE.Show(); errorMsg += lbl_e_mailE.Text + Environment.NewLine;
            }

            // CP
            if (txt_cp.Text.Length != 4 || !txt_cp.Text.All(char.IsDigit))
            {
                lbl_cpE.Text = "El código postal debe tener 4 dígitos numéricos.";
                lbl_cpE.Show(); errorMsg += lbl_cpE.Text + Environment.NewLine;
            }

            // Número de calle
            if (string.IsNullOrWhiteSpace(txt_num_calle.Text) || !txt_num_calle.Text.All(char.IsDigit))
            {
                lbl_num_calleE.Text = "El número de calle debe ser numérico.";
                lbl_num_calleE.Show(); errorMsg += lbl_num_calleE.Text + Environment.NewLine;
            }

            // Piso / Dpto (si cargan algo, validar formato)
            if (!string.IsNullOrWhiteSpace(txt_piso.Text) && !txt_piso.Text.All(char.IsLetterOrDigit))
            {
                lbl_pisoE.Text = "Piso: solo letras o números (máx. 3).";
                lbl_pisoE.Show(); errorMsg += lbl_pisoE.Text + Environment.NewLine;
            }
            if (!string.IsNullOrWhiteSpace(txt_departamento.Text) && !txt_departamento.Text.All(char.IsLetterOrDigit))
            {
                lbl_departamentoE.Text = "Departamento: solo letras o números (máx. 3).";
                lbl_departamentoE.Show(); errorMsg += lbl_departamentoE.Text + Environment.NewLine;
            }

            // Comentarios: solo letras/números/espacios y hasta 2 paréntesis en total
            var comentario = (richTextBox_comentario.Text ?? string.Empty).Trim();
            if (comentario.Length > 60 || !ComentarioFormatoOk(comentario))
            {
                lbl_comentarios_domicilioE.Text = "Comentarios: letras/números/espacios y hasta 2 paréntesis. Máx. 60 caracteres.";
                lbl_comentarios_domicilioE.Show(); errorMsg += lbl_comentarios_domicilioE.Text + Environment.NewLine;
            }

            // Combos dependientes
            if (string.IsNullOrWhiteSpace(combo_pais.Text))
            {
                lbl_paisE.Text = "Debe seleccionar un país.";
                lbl_paisE.Show(); errorMsg += lbl_paisE.Text + Environment.NewLine;
            }
            if (string.IsNullOrWhiteSpace(combo_provincia.Text))
            {
                lbl_provinciaE.Text = "Debe seleccionar una provincia.";
                lbl_provinciaE.Show(); errorMsg += lbl_provinciaE.Text + Environment.NewLine;
            }
            if (string.IsNullOrWhiteSpace(combo_localidad.Text))
            {
                lbl_localidadE.Text = "Debe seleccionar una localidad.";
                lbl_localidadE.Show(); errorMsg += lbl_localidadE.Text + Environment.NewLine;
            }
            if (string.IsNullOrWhiteSpace(combo_calle.Text))
            {
                lbl_calleE.Text = "Debe seleccionar una calle.";
                lbl_calleE.Show(); errorMsg += lbl_calleE.Text + Environment.NewLine;
            }

            if (combo_sucursal.SelectedItem == null)
            {
                lbl_sucursalE.Text = "Debe seleccionar una sucursal.";
                lbl_sucursalE.Show(); errorMsg += lbl_sucursalE.Text + Environment.NewLine;
            }

            // Sueldo >= 500000
            if (!int.TryParse(txt_sueldo.Text, out int sueldo) || sueldo < 500000)
            {
                lbl_sueldoE.Text = "El sueldo debe ser numérico y mayor o igual a 500000.";
                lbl_sueldoE.Show(); errorMsg += lbl_sueldoE.Text + Environment.NewLine;
            }

            if (combo_activo.SelectedItem == null)
            {
                lbl_activoE.Text = "Debe seleccionar el estado activo/inactivo.";
                lbl_activoE.Show(); errorMsg += lbl_activoE.Text + Environment.NewLine;
            }
            if (combo_rol.SelectedItem == null)
            {
                lbl_rolE.Text = "Debe seleccionar un rol.";
                lbl_rolE.Show(); errorMsg += lbl_rolE.Text + Environment.NewLine;
            }

            return string.IsNullOrEmpty(errorMsg);
        }

        private static int CalcularEdad(DateTime nacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - nacimiento.Year;
            if (nacimiento.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        private static bool ComentarioFormatoOk(string texto)
        {
            // Solo letras, números, espacios y paréntesis
            foreach (var c in texto)
            {
                if (!(char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '(' || c == ')'))
                    return false;
            }
            // Máximo 2 paréntesis (contando ambos)
            int par = texto.Count(c => c == '(' || c == ')');
            return par <= 2;
        }

        // ====== Restrictores de ingreso ======
        private void SoloDigitos_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir backspace
            if (char.IsControl(e.KeyChar)) return;

            // Solo dígitos
            if (!char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void SoloLetrasEspacios_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            if (!(char.IsLetter(e.KeyChar) || char.IsWhiteSpace(e.KeyChar)))
                e.Handled = true;
        }

        private void SoloAlfaNumerico_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            if (!char.IsLetterOrDigit(e.KeyChar))
                e.Handled = true;
        }

        private void Comentarios_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            // Permitimos letras, dígitos, espacio y paréntesis
            bool permitido = char.IsLetterOrDigit(e.KeyChar) || char.IsWhiteSpace(e.KeyChar) || e.KeyChar == '(' || e.KeyChar == ')';
            if (!permitido) { e.Handled = true; return; }

            // Controlar máximo 2 paréntesis en total
            if (e.KeyChar == '(' || e.KeyChar == ')')
            {
                var t = richTextBox_comentario.Text ?? "";
                int conteo = t.Count(c => c == '(' || c == ')');
                if (conteo >= 2) e.Handled = true;
            }
        }

        // ====== Diseño combos (tu estilo original) ======
        private void comboBoxdiseño_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ComboBox combo = sender as ComboBox;
            string text = combo.Items[e.Index].ToString();

            Color backgroundColor;
            Color textColor;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                backgroundColor = Color.FromArgb(195, 156, 164);
                textColor = Color.White;
            }
            else
            {
                backgroundColor = Color.FromArgb(250, 236, 239);
                textColor = Color.FromArgb(195, 156, 164);
            }

            using (SolidBrush brush = new SolidBrush(backgroundColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, textColor, TextFormatFlags.Left);

            e.DrawFocusRectangle();
        }

        private void button1_Click(object sender, EventArgs e) => this.Close();

        private void dateTime_nac_ValueChanged(object sender, EventArgs e)
        {
            dateTime_nac.Format = DateTimePickerFormat.Short;
        }
    }
}
