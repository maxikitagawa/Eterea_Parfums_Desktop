using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using Eterea_Parfums_Desktop.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormEditarEmpleado : Form
    {
        private string originalHashedPassword;
        private int id_editar;

        // ====== Fuentes para combos con búsqueda incremental ======
        private List<Pais> _paises = new List<Pais>();
        private List<Provincia> _provincias = new List<Provincia>();
        private List<Localidad> _localidades = new List<Localidad>();
        private List<Calle> _calles = new List<Calle>();
        private List<Sucursal> _sucursales = new List<Sucursal>();

        // ====== Flags / debounce ======
        private bool _suspendComboEvents = false;
        private Timer _debPais, _debProv, _debLoc;
        private const int DebounceMs = 180;

        // ====== Reglas piso/depto ======
        // Todo dígitos (1–3) o todo letras MAYÚSCULAS (1–3)
        private static readonly Regex _rxPisoDpto_Permisivo = new Regex(@"^(?:\d{0,3}|[A-Z]{0,3})$");
        private static readonly Regex _rxPisoDpto_Final = new Regex(@"^(?:\d{1,3}|[A-Z]{1,3})$");
        private string _lastValidPiso = "";
        private string _lastValidDepto = "";

        // ====== Password fuerte (opcional en edición; si se ingresa, debe cumplir) ======
        private static readonly Regex _rxClave =
            new Regex(@"^(?=.*[A-Z])(?=.*[a-zA-Z])(?=.*\d)(?=.*[!¡""#\$%&/\(\)=\?¿]).{8,}$");

        // =================== CTOR (edición) ===================
        public FormEditarEmpleado(Empleado empleado)
        {
            InitializeComponent();

            // Ocultar errores
            OcultarErrores();

            // ---- Input masks / límites ----
            txt_usuario.MaxLength = 45;

            txt_contraseña.MaxLength = 100; // opcional, si ingresa nueva
            // label de contraseña multilínea
            lbl_claveE.AutoSize = true;
            lbl_claveE.MaximumSize = new Size(300, 0);
            lbl_claveE.AutoEllipsis = false;
            lbl_claveE.UseCompatibleTextRendering = true;

            txt_nombre.MaxLength = 45;
            txt_apellido.MaxLength = 45;
            txt_nombre.KeyPress += SoloLetrasEspacios_KeyPress;
            txt_apellido.KeyPress += SoloLetrasEspacios_KeyPress;

            txt_dni.MaxLength = 8;
            txt_dni.KeyPress += SoloDigitos_KeyPress;

            txt_celular.MaxLength = 13; // 10–13 en validación final
            txt_celular.KeyPress += SoloDigitos_KeyPress;

            txt_cp.MaxLength = 4;
            txt_cp.KeyPress += SoloDigitos_KeyPress;

            txt_num_calle.MaxLength = 6;
            txt_num_calle.KeyPress += SoloDigitos_KeyPress;

            // Piso / Dpto: sin mezcla, mayúsculas, máx 3
            txt_piso.MaxLength = 3;
            txt_departamento.MaxLength = 3;
            txt_piso.CharacterCasing = CharacterCasing.Upper;
            txt_departamento.CharacterCasing = CharacterCasing.Upper;
            txt_piso.KeyPress += PisoDpto_KeyPress_ModoExclusivo;
            txt_departamento.KeyPress += PisoDpto_KeyPress_ModoExclusivo;
            txt_piso.TextChanged += Piso_TextChanged_RebotarInvalido;
            txt_departamento.TextChanged += Depto_TextChanged_RebotarInvalido;
            _lastValidPiso = txt_piso.Text;
            _lastValidDepto = txt_departamento.Text;

            // Sueldo
            txt_sueldo.MaxLength = 10;
            txt_sueldo.KeyPress += SoloDigitos_KeyPress;

            // Comentarios domicilio
            richTextBox_comentario.MaxLength = 60;
            richTextBox_comentario.KeyPress += Comentarios_KeyPress;

            // Fechas
            dateTime_nac.Format = DateTimePickerFormat.Short;
            dateTime_ing.Format = DateTimePickerFormat.Short;


            // TextBoxes / RichTextBox
            HookTextHideError(txt_usuario, lbl_usuarioE);
            HookTextHideError(txt_contraseña, lbl_claveE);
            HookTextHideError(txt_nombre, lbl_nombreE);
            HookTextHideError(txt_apellido, lbl_apellidoE);
            HookTextHideError(txt_dni, lbl_dniE);
            HookTextHideError(txt_celular, lbl_celularE);
            HookTextHideError(txt_e_mail, lbl_e_mailE);
            HookTextHideError(txt_sueldo, lbl_sueldoE);
            HookTextHideError(txt_cp, lbl_cpE);
            HookTextHideError(txt_num_calle, lbl_num_calleE);
            HookTextHideError(txt_piso, lbl_pisoE);
            HookTextHideError(txt_departamento, lbl_departamentoE);
            HookTextHideError(richTextBox_comentario, lbl_comentarios_domicilioE);

            // Combos (agregá los que uses en Empleado)
            HookComboHideError(combo_pais, lbl_paisE);
            HookComboHideError(combo_provincia, lbl_provinciaE);
            HookComboHideError(combo_localidad, lbl_localidadE);
            HookComboHideError(combo_calle, lbl_calleE);
            HookComboHideError(combo_activo, lbl_activoE);
            HookComboHideError(combo_sucursal, lbl_sucursalE);
            HookComboHideError(combo_rol, lbl_rolE);

            // DateTimePicker
            HookDateHideError(dateTime_nac, lbl_nacE);
            HookDateHideError(dateTime_ing, lbl_ingE);

            // ---- Combos (estilo) ----
            combo_activo.DrawMode = DrawMode.OwnerDrawFixed;
            combo_activo.DrawItem += comboBoxdiseño_DrawItem;
            combo_activo.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_rol.DrawMode = DrawMode.OwnerDrawFixed;
            combo_rol.DrawItem += comboBoxdiseño_DrawItem;
            combo_rol.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_sucursal.DrawMode = DrawMode.OwnerDrawFixed;
            combo_sucursal.DrawItem += comboBoxdiseño_DrawItem;
            combo_sucursal.DropDownStyle = ComboBoxStyle.DropDownList;

            // País/Provincia/Localidad/Calle autocompletable
            // 1) Preparar combos editables y dueño de dibujo
            UiTheme.PrepareAutocompleteCombo(combo_pais, UiTheme.DrawItemRose);
            UiTheme.PrepareAutocompleteCombo(combo_provincia, UiTheme.DrawItemRose);
            UiTheme.PrepareAutocompleteCombo(combo_localidad, UiTheme.DrawItemRose);
            UiTheme.PrepareAutocompleteCombo(combo_calle, UiTheme.DrawItemRose);

            // 2) Aplicar tema al área editable
            UiTheme.ApplyEditableComboTheme(combo_pais);
            UiTheme.ApplyEditableComboTheme(combo_provincia);
            UiTheme.ApplyEditableComboTheme(combo_localidad);
            UiTheme.ApplyEditableComboTheme(combo_calle);

            // 3) Borde temático al foco
            UiTheme.AttachFocusBorder(combo_pais);
            UiTheme.AttachFocusBorder(combo_provincia);
            UiTheme.AttachFocusBorder(combo_localidad);
            UiTheme.AttachFocusBorder(combo_calle);

            // ---- Cargar listas base ----
            _sucursales = SucursalControlador.getAll() ?? new List<Sucursal>();
            combo_sucursal.Items.Clear();
            foreach (var s in _sucursales.Where(s => s.id != 0 && s.id != 3))
                combo_sucursal.Items.Add(s.nombre);

            _paises = PaisControlador.getAll() ?? new List<Pais>();
            var nombresPaises = _paises.Where(p => p.id != 1).Select(p => p.nombre).ToList();
            CargarComboConAutoComplete(combo_pais, nombresPaises);

            // Deshabilitar dependientes al inicio
            HabilitarCombo(combo_provincia, false);
            HabilitarCombo(combo_localidad, false);
            HabilitarCombo(combo_calle, false);

            // ---- Debounce timers ----
            _debPais = new Timer { Interval = DebounceMs };
            _debProv = new Timer { Interval = DebounceMs };
            _debLoc = new Timer { Interval = DebounceMs };

            _debPais.Tick += (s, e) => { _debPais.Stop(); HandlePaisTextSettled(); };
            _debProv.Tick += (s, e) => { _debProv.Stop(); HandleProvinciaTextSettled(); };
            _debLoc.Tick += (s, e) => { _debLoc.Stop(); HandleLocalidadTextSettled(); };

            combo_pais.TextChanged += combo_pais_TextChanged_Debounced;
            combo_provincia.TextChanged += combo_provincia_TextChanged_Debounced;
            combo_localidad.TextChanged += combo_localidad_TextChanged_Debounced;

            // ---- Poblar datos del empleado ----
            id_editar = empleado.id;
            originalHashedPassword = empleado.clave;

            txt_usuario.Text = empleado.usuario ?? "";
            txt_contraseña.Text = ""; // no mostramos la actual

            txt_nombre.Text = empleado.nombre ?? "";
            txt_apellido.Text = empleado.apellido ?? "";
            txt_dni.Text = empleado.dni.ToString();
            dateTime_nac.Value = empleado.fecha_nacimiento;
            txt_celular.Text = empleado.celular ?? "";
            txt_e_mail.Text = empleado.e_mail ?? "";

            // Activo
            combo_activo.Items.Clear();
            combo_activo.Items.Add("Activo");
            combo_activo.Items.Add("Inactivo");
            combo_activo.SelectedItem = (empleado.activo ? "Activo" : "Inactivo");


            // Rol
            combo_rol.Items.Clear();
            combo_rol.Items.Add("admin");
            combo_rol.Items.Add("vendedor");
            combo_rol.SelectedItem = empleado.rol == "admin" ? "admin" : "vendedor";

            // Sucursal
            combo_sucursal.SelectedItem = empleado.sucursal_id?.nombre;

            // Dirección dependiente con handlers “settled”
            _suspendComboEvents = true;
            try
            {
                combo_pais.Text = empleado.pais_id?.nombre ?? "";
            }
            finally { _suspendComboEvents = false; }
            HandlePaisTextSettled();

            _suspendComboEvents = true;
            try
            {
                combo_provincia.Text = empleado.provincia_id?.nombre ?? "";
            }
            finally { _suspendComboEvents = false; }
            HandleProvinciaTextSettled();

            _suspendComboEvents = true;
            try
            {
                combo_localidad.Text = empleado.localidad_id?.nombre ?? "";
            }
            finally { _suspendComboEvents = false; }
            HandleLocalidadTextSettled();

            _suspendComboEvents = true;
            try
            {
                combo_calle.Text = empleado.calle_id?.nombre ?? "";
            }
            finally { _suspendComboEvents = false; }

            txt_cp.Text = empleado.codigo_postal.ToString();
            txt_num_calle.Text = empleado.numeracion_calle.ToString();
            txt_piso.Text = (empleado.piso ?? "").ToString();
            txt_departamento.Text = (empleado.departamento ?? "").ToString();
            richTextBox_comentario.Text = empleado.comentarios_domicilio ?? "";

            // Ingreso / sueldo
            dateTime_ing.Value = empleado.fecha_ingreso;
            txt_sueldo.Text = empleado.sueldo.ToString();

            // UI
            label1.Text = "Editar Empleado";
            btn_crear.Text = "Guardar cambios";

            // Botones
            btn_crear.Click += btn_crear_Click;

            //se habilita el helper para validar ingreso de texto en los combos
            new ComboPrefixGuard(combo_pais);
            new ComboPrefixGuard(combo_provincia);
            new ComboPrefixGuard(combo_localidad);
            new ComboPrefixGuard(combo_calle);
        }

        // =================== Debounce wrappers ===================
        private void combo_pais_TextChanged_Debounced(object sender, EventArgs e)
        {
            if (_suspendComboEvents) return;
            if (!combo_pais.Focused) return;
            RestartDebounce(_debPais);
        }
        private void combo_provincia_TextChanged_Debounced(object sender, EventArgs e)
        {
            if (_suspendComboEvents) return;
            if (!combo_provincia.Focused) return;
            RestartDebounce(_debProv);
        }
        private void combo_localidad_TextChanged_Debounced(object sender, EventArgs e)
        {
            if (_suspendComboEvents) return;
            if (!combo_localidad.Focused) return;
            RestartDebounce(_debLoc);
        }
        private void RestartDebounce(Timer t) { t.Stop(); t.Start(); }

        // =================== Lógica “settled” ===================
        private void HandlePaisTextSettled()
        {
            if (_suspendComboEvents) return;

            var texto = combo_pais.Text?.Trim() ?? "";
            var paisSel = _paises.FirstOrDefault(p =>
                string.Equals(p.nombre, texto, StringComparison.OrdinalIgnoreCase));

            _suspendComboEvents = true;
            try
            {
                // reset dependientes
                ResetCombo(combo_provincia, false, limpiarTexto: true);
                ResetCombo(combo_localidad, false, limpiarTexto: true);
                ResetCombo(combo_calle, false, limpiarTexto: true);

                if (paisSel == null) return;

                _provincias = ProvinciaControlador.getProvinciasPorPaisId(paisSel.id) ?? new List<Provincia>();
                ResetCombo(combo_provincia, true, limpiarTexto: true);
                CargarComboConAutoComplete(combo_provincia, _provincias.Select(x => x.nombre).ToList());
            }
            finally { _suspendComboEvents = false; }
        }

        private void HandleProvinciaTextSettled()
        {
            if (_suspendComboEvents) return;

            var texto = combo_provincia.Text?.Trim() ?? "";
            var provSel = _provincias.FirstOrDefault(p =>
                string.Equals(p.nombre, texto, StringComparison.OrdinalIgnoreCase));

            _suspendComboEvents = true;
            try
            {
                ResetCombo(combo_localidad, false, limpiarTexto: true);
                ResetCombo(combo_calle, false, limpiarTexto: true);

                if (provSel == null) return;

                _localidades = LocalidadControlador.getLocalidadesPorProvinciaId(provSel.id) ?? new List<Localidad>();
                ResetCombo(combo_localidad, true, limpiarTexto: true);
                CargarComboConAutoComplete(combo_localidad, _localidades.Select(x => x.nombre).ToList());
            }
            finally { _suspendComboEvents = false; }
        }

        private void HandleLocalidadTextSettled()
        {
            if (_suspendComboEvents) return;

            var texto = combo_localidad.Text?.Trim() ?? "";
            var locSel = _localidades.FirstOrDefault(l =>
                string.Equals(l.nombre, texto, StringComparison.OrdinalIgnoreCase));

            _suspendComboEvents = true;
            try
            {
                ResetCombo(combo_calle, false, limpiarTexto: true);

                if (locSel == null) return;

                _calles = CalleControlador.getCallesPorLocalidadId(locSel.id) ?? new List<Calle>();
                ResetCombo(combo_calle, true, limpiarTexto: true);
                CargarComboConAutoComplete(combo_calle, _calles.Select(x => x.nombre).ToList());
            }
            finally { _suspendComboEvents = false; }
        }

        // =================== Botón guardar ===================
        private void btn_crear_Click(object sender, EventArgs e)
        {
            if (validarDatosEmpleado(out string _))
                editar();
            else
                EnfocarPrimerControlConError();
        }

        // =================== Editar (persistencia) ===================
        private void editar()
        {
            bool activo = (combo_activo.SelectedItem?.ToString() == "Activo");
            string rol = (combo_rol.SelectedItem?.ToString() == "admin") ? "admin" : "vendedor";

            var pais = PaisControlador.getByName(combo_pais.Text);
            var provincia = ProvinciaControlador.getByName(combo_provincia.Text);
            var localidad = LocalidadControlador.getByName(combo_localidad.Text);
            var calle = CalleControlador.getByName(combo_calle.Text);
            var sucursal = SucursalControlador.getByName(combo_sucursal.SelectedItem?.ToString());

            // Password: si cambió, ya pasó por validación; la hasheamos
            string claveParaGuardar = originalHashedPassword;
            if (!string.IsNullOrWhiteSpace(txt_contraseña.Text))
                claveParaGuardar = PasswordHelper.CrearHash(txt_contraseña.Text);

            var empleado = new Empleado(
                id_editar,
                txt_usuario.Text.Trim(),
                claveParaGuardar,
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

            if (EmpleadoControlador.editarEmpleado(empleado))
                this.DialogResult = DialogResult.OK;
        }

        // =================== Validaciones ===================
        private bool validarDatosEmpleado(out string errorMsg)
        {
            errorMsg = string.Empty;
            OcultarErrores();

            // Usuario
            if (string.IsNullOrWhiteSpace(txt_usuario.Text) ||
                txt_usuario.Text.Length < 3 || txt_usuario.Text.Length > 45)
            {
                lbl_usuarioE.Text = "El usuario debe tener entre 3 y 45 caracteres.";
                lbl_usuarioE.Show(); errorMsg += lbl_usuarioE.Text + Environment.NewLine;
            }
            else if (EmpleadoControlador.ExisteUsuarioEnOtroEmpleado(txt_usuario.Text.Trim(), id_editar))
            {
                lbl_usuarioE.Text = "Ya existe un empleado con ese nombre de usuario.";
                lbl_usuarioE.Show(); errorMsg += lbl_usuarioE.Text + Environment.NewLine;
            }

            // Clave: opcional. Si se ingresó, debe cumplir regex fuerte.
            if (!string.IsNullOrWhiteSpace(txt_contraseña.Text) && !_rxClave.IsMatch(txt_contraseña.Text))
            {
                lbl_claveE.Text = "La clave debe tener 8+ caracteres, incluir mayúsculas, letras, números y 1 especial (!,¡,\",#,$,%,&,/,(,),=,?,¿).";
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

            // DNI (exacto 8)
            if (txt_dni.Text.Length != 8 || !txt_dni.Text.All(char.IsDigit))
            {
                lbl_dniE.Text = "El DNI debe tener exactamente 8 dígitos numéricos.";
                lbl_dniE.Show(); errorMsg += lbl_dniE.Text + Environment.NewLine;
            }
            else
            {
                var otroId = EmpleadoControlador.BuscarIdPorDni(txt_dni.Text);
                if (otroId != null && otroId != id_editar)
                {
                    lbl_dniE.Text = "Ya existe otro empleado con ese DNI.";
                    lbl_dniE.Show(); errorMsg += lbl_dniE.Text + Environment.NewLine;
                }
            }

            // Fechas
            if (!DateTime.TryParse(dateTime_ing.Text, out DateTime fechaIng) || fechaIng > DateTime.Today)
            {
                lbl_ingE.Text = "La fecha de ingreso debe ser válida y no futura.";
                lbl_ingE.Show(); errorMsg += lbl_ingE.Text + Environment.NewLine;
            }
            if (!DateTime.TryParse(dateTime_nac.Text, out DateTime fechaNac) ||
                fechaNac > DateTime.Today || CalcularEdad(fechaNac) < 18)
            {
                lbl_nacE.Text = "La fecha de nacimiento debe ser válida, no futura y el empleado debe ser mayor de 18 años.";
                lbl_nacE.Show(); errorMsg += lbl_nacE.Text + Environment.NewLine;
            }

            // Celular (10–13)
            if (string.IsNullOrWhiteSpace(txt_celular.Text) ||
                !txt_celular.Text.All(char.IsDigit) ||
                txt_celular.Text.Length < 10 || txt_celular.Text.Length > 13)
            {
                lbl_celularE.Text = "El celular debe ser numérico y tener entre 10 y 13 dígitos.";
                lbl_celularE.Show(); errorMsg += lbl_celularE.Text + Environment.NewLine;
            }

            // Email
            if (string.IsNullOrWhiteSpace(txt_e_mail.Text) ||
                !Regex.IsMatch(txt_e_mail.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                lbl_e_mailE.Text = "Debe ingresar un correo electrónico válido.";
                lbl_e_mailE.Show(); errorMsg += lbl_e_mailE.Text + Environment.NewLine;
            }
            else if (EmpleadoControlador.ExisteEmailEnOtroEmpleado(txt_e_mail.Text.Trim(), id_editar))
            {
                lbl_e_mailE.Text = "Ya existe un empleado con ese correo electrónico.";
                lbl_e_mailE.Show(); errorMsg += lbl_e_mailE.Text + Environment.NewLine;
            }

            // CP 4 dígitos
            if (txt_cp.Text.Length != 4 || !txt_cp.Text.All(char.IsDigit))
            {
                lbl_cpE.Text = "El código postal debe tener 4 dígitos numéricos.";
                lbl_cpE.Show(); errorMsg += lbl_cpE.Text + Environment.NewLine;
            }

            // Número de calle numérico
            if (string.IsNullOrWhiteSpace(txt_num_calle.Text) || !txt_num_calle.Text.All(char.IsDigit))
            {
                lbl_num_calleE.Text = "El número de calle debe ser numérico.";
                lbl_num_calleE.Show(); errorMsg += lbl_num_calleE.Text + Environment.NewLine;
            }

            // Piso / Dpto (opcionales) – no mezclar y 1..3 del mismo tipo
            if (!string.IsNullOrWhiteSpace(txt_piso.Text) && !_rxPisoDpto_Final.IsMatch(txt_piso.Text))
            {
                lbl_pisoE.Text = "Piso: ingrese 1–3 dígitos (ej. 34) o 1–3 letras (ej. PB), sin mezclar.";
                lbl_pisoE.Show(); errorMsg += lbl_pisoE.Text + Environment.NewLine;
            }
            if (!string.IsNullOrWhiteSpace(txt_departamento.Text) && !_rxPisoDpto_Final.IsMatch(txt_departamento.Text))
            {
                lbl_departamentoE.Text = "Departamento: 1–3 dígitos o 1–3 letras, sin mezclar.";
                lbl_departamentoE.Show(); errorMsg += lbl_departamentoE.Text + Environment.NewLine;
            }

            // Comentarios (máx 60, letras/números/espacios y hasta 2 paréntesis)
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

            // Sucursal
            if (combo_sucursal.SelectedItem == null)
            {
                lbl_sucursalE.Text = "Debe seleccionar una sucursal.";
                lbl_sucursalE.Show(); errorMsg += lbl_sucursalE.Text + Environment.NewLine;
            }

            // Sueldo ≥ 500000
            if (!int.TryParse(txt_sueldo.Text, out int sueldo) || sueldo < 500000)
            {
                lbl_sueldoE.Text = "El sueldo debe ser numérico y mayor o igual a 500000.";
                lbl_sueldoE.Show(); errorMsg += lbl_sueldoE.Text + Environment.NewLine;
            }

            // Activo / Rol
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

        private void EnfocarPrimerControlConError()
        {
            var pares = new (Label label, Control control)[]
            {
                (lbl_usuarioE, txt_usuario),
                (lbl_claveE, txt_contraseña),
                (lbl_nombreE, txt_nombre),
                (lbl_apellidoE, txt_apellido),
                (lbl_dniE, txt_dni),
                (lbl_nacE, dateTime_nac),
                (lbl_celularE, txt_celular),
                (lbl_e_mailE, txt_e_mail),
                (lbl_paisE, combo_pais),
                (lbl_provinciaE, combo_provincia),
                (lbl_localidadE, combo_localidad),
                (lbl_cpE, txt_cp),
                (lbl_calleE, combo_calle),
                (lbl_num_calleE, txt_num_calle),
                (lbl_pisoE, txt_piso),
                (lbl_departamentoE, txt_departamento),
                (lbl_comentarios_domicilioE, richTextBox_comentario),
                (lbl_sucursalE, combo_sucursal),
                (lbl_ingE, dateTime_ing),
                (lbl_sueldoE, txt_sueldo),
                (lbl_activoE, combo_activo),
                (lbl_rolE, combo_rol)
            };

            foreach (var p in pares)
            {
                if (p.label.Visible)
                {
                    p.control.Focus();
                    break;
                }
            }
        }

        // =================== Helpers de UI / combos ===================
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

        // Oculta el label de error cuando cambia el texto (TextBox, RichTextBox, MaskedTextBox)
        private void HookTextHideError(TextBoxBase tb, Label errorLabel)
        {
            if (tb == null || errorLabel == null) return;
            tb.TextChanged += (s, e) => errorLabel.Hide();
        }

        // Oculta el label de error al seleccionar un ítem en el combo o cuando el texto coincide con un ítem (auto-complete)
        private void HookComboHideError(ComboBox combo, Label errorLabel)
        {
            if (combo == null || errorLabel == null) return;

            combo.SelectionChangeCommitted += (s, e) => errorLabel.Hide();

            combo.TextChanged += (s, e) =>
            {
                var txt = combo.Text?.Trim() ?? "";
                if (string.IsNullOrEmpty(txt)) return;

                bool match = combo.Items
                    .Cast<object>()
                    .Select(i => i?.ToString() ?? "")
                    .Any(it => string.Equals(it, txt, StringComparison.OrdinalIgnoreCase));

                if (match) errorLabel.Hide();
            };
        }

        // Oculta el label de error al elegir fecha
        private void HookDateHideError(DateTimePicker dtp, Label errorLabel)
        {
            if (dtp == null || errorLabel == null) return;
            dtp.ValueChanged += (s, e) =>
            {
                errorLabel.Hide();
                dtp.Format = DateTimePickerFormat.Short;
            };
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
            _suspendComboEvents = true;
            string currentText = combo.Text;

            combo.BeginUpdate();
            try
            {
                combo.Items.Clear();
                foreach (var v in valores) combo.Items.Add(v);

                var ac = new AutoCompleteStringCollection();
                ac.AddRange(valores.ToArray());
                combo.AutoCompleteCustomSource = ac;
            }
            finally
            {
                combo.EndUpdate();
                combo.Text = currentText;
                _suspendComboEvents = false;
            }
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

        private void ResetCombo(ComboBox combo, bool habilitar, bool limpiarTexto = true)
        {
            _suspendComboEvents = true;
            try
            {
                combo.BeginUpdate();
                combo.Items.Clear();
                combo.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            }
            finally
            {
                combo.EndUpdate();
                if (limpiarTexto) combo.Text = string.Empty;
                combo.Enabled = habilitar;
                _suspendComboEvents = false;
            }
        }

        // =================== Restrictores / formato ===================
        private void SoloDigitos_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            if (!char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        private void SoloLetrasEspacios_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            if (!(char.IsLetter(e.KeyChar) || char.IsWhiteSpace(e.KeyChar))) e.Handled = true;
        }

        private void Comentarios_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            bool permitido = char.IsLetterOrDigit(e.KeyChar) || char.IsWhiteSpace(e.KeyChar) || e.KeyChar == '(' || e.KeyChar == ')';
            if (!permitido) { e.Handled = true; return; }

            if (e.KeyChar == '(' || e.KeyChar == ')')
            {
                var t = richTextBox_comentario.Text ?? "";
                int conteo = t.Count(c => c == '(' || c == ')');
                if (conteo >= 2) e.Handled = true;
            }
        }

        // Piso/Dpto: no mezclar, forzar mayúsculas y máx 3
        private void PisoDpto_KeyPress_ModoExclusivo(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            var tb = (TextBox)sender;
            string proposed = ProposedTextAfterKeyPress(tb, e.KeyChar).ToUpperInvariant();

            if (!_rxPisoDpto_Permisivo.IsMatch(proposed))
            {
                e.Handled = true;
                return;
            }
            if (char.IsLetter(e.KeyChar))
                e.KeyChar = char.ToUpperInvariant(e.KeyChar);
        }

        private void Piso_TextChanged_RebotarInvalido(object sender, EventArgs e)
            => RebotarSiInvalido((TextBox)sender, ref _lastValidPiso);

        private void Depto_TextChanged_RebotarInvalido(object sender, EventArgs e)
            => RebotarSiInvalido((TextBox)sender, ref _lastValidDepto);

        private void RebotarSiInvalido(TextBox tb, ref string lastValid)
        {
            if (_rxPisoDpto_Permisivo.IsMatch(tb.Text))
            {
                lastValid = tb.Text.ToUpperInvariant();
            }
            else
            {
                int caret = tb.SelectionStart - 1;
                tb.Text = lastValid;
                tb.SelectionStart = Math.Max(0, Math.Min(caret, tb.Text.Length));
            }
        }

        private string ProposedTextAfterKeyPress(TextBox tb, char ch)
        {
            if (char.IsControl(ch)) return tb.Text;
            int start = tb.SelectionStart;
            int len = tb.SelectionLength;

            string baseText = tb.Text;
            if (len > 0) baseText = baseText.Remove(start, len);

            return baseText.Insert(start, ch.ToString());
        }

        // =================== Estilo combos ===================
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

        private static int CalcularEdad(DateTime nacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - nacimiento.Year;
            if (nacimiento.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        private static bool ComentarioFormatoOk(string texto)
        {
            foreach (var c in texto)
                if (!(char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '(' || c == ')'))
                    return false;

            int par = texto.Count(c => c == '(' || c == ')');
            return par <= 2;
        }

        private void button1_Click(object sender, EventArgs e) => this.Close();
    }
}
