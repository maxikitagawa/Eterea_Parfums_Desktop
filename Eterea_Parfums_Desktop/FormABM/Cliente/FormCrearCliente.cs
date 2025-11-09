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
    public partial class FormCrearCliente : Form
    {
        // ====== Fuentes de datos completas (para autocompletar sin perder lista) ======
        private List<Pais> _paises = new List<Pais>();
        private List<Provincia> _provincias = new List<Provincia>();
        private List<Localidad> _localidades = new List<Localidad>();
        private List<Calle> _calles = new List<Calle>();

        // --- Color de tu diseño ---
        private static readonly Color Rose = Color.FromArgb(195, 156, 164);


        // ====== Flags / debounce ======
        private bool _suspendComboEvents = false;
        private Timer _debPais, _debProv, _debLoc;
        private const int DebounceMs = 180;

        // ====== Reglas de contraseña (idéntica a Empleado) ======
        // 8+ chars, al menos 1 MAYÚSCULA, 1 letra, 1 dígito y 1 especial del set dado
        private static readonly Regex _rxClave =
            new Regex(@"^(?=.*[A-Z])(?=.*[a-zA-Z])(?=.*\d)(?=.*[!¡""#\$%&/\(\)=\?¿]).{8,}$");

        // ====== Reglas piso/depto ======
        // Todo dígitos (0–3) o todo letras MAYÚSCULAS (0–3) para permitir tecleo y luego validar final 1..3
        private static readonly Regex _rxPisoDpto_Permisivo = new Regex(@"^(?:\d{0,3}|[A-Z]{0,3})$");
        private static readonly Regex _rxPisoDpto_Final = new Regex(@"^(?:\d{1,3}|[A-Z]{1,3})$");
        private string _lastValidPiso = "";
        private string _lastValidDepto = "";

        // ===== Helpers de validación =====
        private static bool SoloLetras(string s) =>
            Regex.IsMatch(s ?? "", @"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü]+$");

        private static bool LetrasYPunto(string s) =>
            Regex.IsMatch(s ?? "", @"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s\.]+$");

        private static bool AlfanumericoRazonSocial(string s) =>
            Regex.IsMatch(s ?? "", @"^[A-Za-z0-9ÁÉÍÓÚáéíóúÑñÜü\s\.\-&/()']+$");

        // ===== KeyPress dinámicos según DNI/CUIT =====

        // DNI: Nombre/Apellido → solo letras y espacios
        private void Nombre_KeyPress_DNI(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            if (!(char.IsLetter(e.KeyChar) || char.IsWhiteSpace(e.KeyChar))) e.Handled = true;
        }

        private void Apellido_KeyPress_DNI(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            if (!(char.IsLetter(e.KeyChar) || char.IsWhiteSpace(e.KeyChar))) e.Handled = true;
        }

        // CUIT: Razón social → letras, números y . - & / ( ) '
        private void Nombre_KeyPress_CUIT_RazonSocial(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            bool ok = char.IsLetterOrDigit(e.KeyChar) || char.IsWhiteSpace(e.KeyChar)
                      || ".-&/()'".Contains(e.KeyChar);
            if (!ok) e.Handled = true;
        }

        // CUIT: Tipo → solo letras, espacios y punto "."
        private void Tipo_KeyPress_CUIT(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            bool ok = char.IsLetter(e.KeyChar) || char.IsWhiteSpace(e.KeyChar) || e.KeyChar == '.';
            if (!ok) e.Handled = true;
        }

        public FormCrearCliente()
        {
            InitializeComponent();

            // ====== Ocultar etiquetas de error ======
            OcultarErrores();

            // ====== INPUT MASKS / LÍMITES ======
            // Usuario
            txt_usuario.MaxLength = 45;

            // Clave (validación fuerte al enviar)
            txt_clave.MaxLength = 100;

            // Nombre / Apellido
            txt_nombre.MaxLength = 45;
            txt_apellido.MaxLength = 45;
           

            // DNI/CUIT: solo números, máx 11
            txt_dni.MaxLength = 11;
            txt_dni.KeyPress += SoloDigitos_KeyPress;
            txt_dni.TextChanged += txt_dni_TextChanged;

            // Celular: solo números, máx 13
            txt_celular.MaxLength = 13;
            txt_celular.KeyPress += SoloDigitos_KeyPress;

            // Código Postal: solo números, exactamente 4
            txt_cp.MaxLength = 4;
            txt_cp.KeyPress += SoloDigitos_KeyPress;

            // Número de calle: solo números, máx 6
            txt_num_calle.MaxLength = 6;
            txt_num_calle.KeyPress += SoloDigitos_KeyPress;

            // Piso / Depto: no mezclar, máx 3, forzar mayúsculas
            txt_piso.MaxLength = 3;
            txt_depto.MaxLength = 3;
            txt_piso.CharacterCasing = CharacterCasing.Upper;
            txt_depto.CharacterCasing = CharacterCasing.Upper;
            txt_piso.KeyPress += PisoDpto_KeyPress_ModoExclusivo;
            txt_depto.KeyPress += PisoDpto_KeyPress_ModoExclusivo;
            txt_piso.TextChanged += Piso_TextChanged_RebotarInvalido;
            txt_depto.TextChanged += Depto_TextChanged_RebotarInvalido;
            _lastValidPiso = txt_piso.Text;
            _lastValidDepto = txt_depto.Text;

            HookComboHideError(combo_pais, lbl_paisE);
            HookComboHideError(combo_provincia, lbl_provinciaE);
            HookComboHideError(combo_localidad, lbl_localidadE);
            HookComboHideError(combo_calle, lbl_calleE);
            HookComboHideError(combo_activo, lbl_activoE);
            HookComboHideError(combo_con_iva, lbl_cond_ivaE);
            HookDateHideError(dateTime_nac, lbl_nacE);

            // Comentarios: máx 60; letras/números/espacios y hasta 2 paréntesis
            richTextBox_comentario.MaxLength = 60;
            richTextBox_comentario.KeyPress += Comentarios_KeyPress;

            // Fecha de nacimiento: iniciar vacío
            dateTime_nac.Format = DateTimePickerFormat.Custom;
            dateTime_nac.CustomFormat = " ";
            dateTime_nac.ValueChanged += dateTime_nac_ValueChanged;

            // Que el label de error para "Clave" ajuste su alto automáticamente y haga wrap por ancho máx
            lbl_claveE.AutoSize = true;
            lbl_claveE.MaximumSize = new Size(300, 0); // ajustamos 300 al ancho
            lbl_claveE.AutoEllipsis = false;           // no cortar con "..."
            lbl_claveE.UseCompatibleTextRendering = true; // opcional para mejor tipografía

            // Ocultar errores al modificar campos (UX)
            txt_usuario.TextChanged += (s, e) => lbl_usuarioE.Hide();
            txt_clave.TextChanged += (s, e) => lbl_claveE.Hide();
            txt_dni.TextChanged += (s, e) => lbl_dniE.Hide();
            txt_nombre.TextChanged += (s, e) => lbl_nombreE.Hide();
            txt_apellido.TextChanged += (s, e) => lbl_apellidoE.Hide();
            txt_celular.TextChanged += (s, e) => lbl_celularE.Hide();
            txt_email.TextChanged += (s, e) => lbl_emailE.Hide();
            txt_cp.TextChanged += (s, e) => lbl_cpE.Hide();
            txt_num_calle.TextChanged += (s, e) => lbl_num_calleE.Hide();
            txt_piso.TextChanged += (s, e) => lbl_pisoE.Hide();
            txt_depto.TextChanged += (s, e) => lbl_deptoE.Hide();
            richTextBox_comentario.TextChanged += (s, e) => lbl_comentariosE.Hide();


            // ====== COMBOS: estilo y listas ======
            combo_activo.Items.Clear();
            combo_activo.Items.Add("Activo");
            combo_activo.Items.Add("Inactivo");
            combo_activo.DrawMode = DrawMode.OwnerDrawFixed;
            combo_activo.DrawItem += comboBoxdiseño_DrawItem;
            combo_activo.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_con_iva.Items.Clear();
            combo_con_iva.DrawMode = DrawMode.OwnerDrawFixed;
            combo_con_iva.DrawItem += comboBoxdiseño_DrawItem;
            combo_con_iva.DropDownStyle = ComboBoxStyle.DropDownList;
            combo_con_iva.Enabled = false; // se habilita con CUIT válido

            // País / Provincia / Localidad / Calle con escritura y autocompletar
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

            // Cargar países
            _paises = PaisControlador.getAll() ?? new List<Pais>();
            var nombresPaises = _paises.Where(p => p.id != 1).Select(p => p.nombre).ToList();
            CargarComboConAutoComplete(combo_pais, nombresPaises);

            // Deshabilitar dependientes
            HabilitarCombo(combo_provincia, false);
            HabilitarCombo(combo_localidad, false);
            HabilitarCombo(combo_calle, false);

            // Timers de debounce
            _debPais = new Timer { Interval = DebounceMs };
            _debProv = new Timer { Interval = DebounceMs };
            _debLoc = new Timer { Interval = DebounceMs };
            _debPais.Tick += (s, e) => { _debPais.Stop(); HandlePaisTextSettled(); };
            _debProv.Tick += (s, e) => { _debProv.Stop(); HandleProvinciaTextSettled(); };
            _debLoc.Tick += (s, e) => { _debLoc.Stop(); HandleLocalidadTextSettled(); };

            // Wrappers livianos: reinician timer si el usuario escribe
            combo_pais.TextChanged += combo_pais_TextChanged_Debounced;
            combo_provincia.TextChanged += combo_provincia_TextChanged_Debounced;
            combo_localidad.TextChanged += combo_localidad_TextChanged_Debounced;

            // Botones
            btn_crear_cliente.Click += btn_crear_cliente_Click;
            button2.Click += button1_Click; // cerrar


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

        // =================== Lógica “settled” (idéntica a Empleado) ===================
        private void HandlePaisTextSettled()
        {
            if (_suspendComboEvents) return;

            var texto = combo_pais.Text?.Trim() ?? "";
            var paisSel = _paises.FirstOrDefault(p =>
                string.Equals(p.nombre, texto, StringComparison.OrdinalIgnoreCase));

            _suspendComboEvents = true;
            try
            {
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

        // =================== Helpers de combos ===================
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
                combo.Text = currentText; // restaurar sin disparar dependencias
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

 
       

        // =================== Botón crear ===================
        private void btn_crear_cliente_Click(object sender, EventArgs e)
        {
            if (validarDatosCliente(out string _))
            {
                crear();
            }
            else
            {
                // Podés enfocar el primer control con error si querés (como en Empleado)
                EnfocarPrimerControlConError();
            }
        }

        private void crear()
        {
            bool activo = (combo_activo.SelectedItem?.ToString() == "Activo");
            string rol = "cliente";

            var pais = PaisControlador.getByName(combo_pais.Text);
            var provincia = ProvinciaControlador.getByName(combo_provincia.Text);
            var ciudad = LocalidadControlador.getByName(combo_localidad.Text);
            var calle = CalleControlador.getByName(combo_calle.Text);

            // Si usás PasswordHelper, reemplazá txt_clave.Text por PasswordHelper.CrearHash(txt_clave.Text)
            var cliente = new Cliente(
                0,
                txt_usuario.Text.Trim(),
                txt_clave.Text,                      // ← hashear en controlador si corresponde
                txt_nombre.Text.Trim(),
                txt_apellido.Text.Trim(),
                int.Parse(txt_dni.Text),
                combo_con_iva.SelectedItem?.ToString() ?? "Consumidor Final",
                DateTime.Parse(dateTime_nac.Text),
                txt_celular.Text.Trim(),
                txt_email.Text.Trim(),
                pais,
                provincia,
                ciudad,
                int.Parse(txt_cp.Text),
                calle,
                int.Parse(txt_num_calle.Text),
                txt_piso.Text.Trim(),
                txt_depto.Text.Trim(),
                richTextBox_comentario.Text.Trim(),
                activo,
                rol
            );

            if (ClienteControlador.crearCliente(cliente))
                this.DialogResult = DialogResult.OK;
        }

        // =================== Validaciones ===================
        private bool validarDatosCliente(out string errorMsg)
        {
            errorMsg = string.Empty;
            OcultarErrores();

            // Usuario
            if (string.IsNullOrWhiteSpace(txt_usuario.Text) || txt_usuario.Text.Length < 3 || txt_usuario.Text.Length > 45)
            {
                lbl_usuarioE.Text = "El usuario debe tener entre 3 y 45 caracteres.";
                lbl_usuarioE.Show(); errorMsg += lbl_usuarioE.Text + Environment.NewLine;
            }
            else if (ClienteControlador.ExisteUsuario(txt_usuario.Text.Trim()))
            {
                lbl_usuarioE.Text = "Ya existe un cliente con ese nombre de usuario.";
                lbl_usuarioE.Show(); errorMsg += lbl_usuarioE.Text + Environment.NewLine;
            }

            // Clave (fuerte como Empleado)
            if (string.IsNullOrWhiteSpace(txt_clave.Text) || !_rxClave.IsMatch(txt_clave.Text))
            {
                lbl_claveE.Text = "La clave debe tener 8+ caracteres, incluir mayúsculas, letras, números y 1 especial (!,¡,\",#,$,%,&,/,(,),=,?,¿).";
                lbl_claveE.Show(); errorMsg += lbl_claveE.Text + Environment.NewLine;
            }

            // --- Nombre / Razón social ---
            bool esCUIT = (txt_dni.Text.Trim().Length == 11);
            var nombre = txt_nombre.Text?.Trim() ?? "";
            var apellidoOTipo = txt_apellido.Text?.Trim() ?? "";

            // Nombre/Razón social: requerimiento + longitud
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 2 || nombre.Length > 45)
            {
                lbl_nombreE.Text = esCUIT
                    ? "Debe ingresar la razón social (entre 2 y 45 caracteres)."
                    : "El nombre debe tener entre 2 y 45 caracteres.";
                lbl_nombreE.Show();
                errorMsg += lbl_nombreE.Text + Environment.NewLine;
            }
            else
            {
                if (esCUIT)
                {
                    // Razón social: letras, números y . - & / ( ) '
                    if (!AlfanumericoRazonSocial(nombre))
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
                    // DNI: solo letras y espacios
                    if (!SoloLetras(nombre))
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

            // --- Apellido / Tipo ---
            if (string.IsNullOrWhiteSpace(apellidoOTipo) || apellidoOTipo.Length < 2 || apellidoOTipo.Length > 45)
            {
                lbl_apellidoE.Text = esCUIT
                    ? "Debe ingresar el tipo de sociedad (entre 2 y 45 caracteres)."
                    : "El apellido debe tener entre 2 y 45 caracteres.";
                lbl_apellidoE.Show();
                errorMsg += lbl_apellidoE.Text + Environment.NewLine;
            }
            else
            {
                if (esCUIT)
                {
                    // Tipo: solo letras, espacios y punto (.)
                    if (!LetrasYPunto(apellidoOTipo))
                    {
                        lbl_apellidoE.Text = "El tipo solo puede tener letras, espacios y punto (.).";
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
                    // DNI: solo letras y espacios
                    if (!SoloLetras(apellidoOTipo))
                    {
                        lbl_apellidoE.Text = "El apellido solo puede contener letras.";
                        lbl_apellidoE.Show();
                        errorMsg += lbl_apellidoE.Text + Environment.NewLine;
                    }
                    else
                    {
                        lbl_apellidoE.Visible = false;
                    }
                }
            }


            // DNI/CUIT
            if (string.IsNullOrWhiteSpace(txt_dni.Text) || !txt_dni.Text.All(char.IsDigit))
            {
                lbl_dniE.Text = "Debe ingresar solo dígitos.";
                lbl_dniE.Show(); errorMsg += lbl_dniE.Text + Environment.NewLine;
            }
            else if (txt_dni.Text.Length != 8 && txt_dni.Text.Length != 11)
            {
                lbl_dniE.Text = "El documento debe tener 8 (DNI) o 11 (CUIT) dígitos.";
                lbl_dniE.Show(); errorMsg += lbl_dniE.Text + Environment.NewLine;
            }
            else
            {
                if (txt_dni.Text.Length == 11 && !CuitValido(txt_dni.Text))
                {
                    lbl_dniE.Text = "CUIT inválido (falló verificación).";
                    lbl_dniE.Show(); errorMsg += lbl_dniE.Text + Environment.NewLine;
                }

                var idExistente = ClienteControlador.BuscarIdPorDni(txt_dni.Text);
                if (idExistente != null)
                {
                    lbl_dniE.Text = "Ya existe un cliente con ese documento.";
                    lbl_dniE.Show(); errorMsg += lbl_dniE.Text + Environment.NewLine;
                }
            }

            // Condición IVA
            if (combo_con_iva.Enabled && combo_con_iva.SelectedItem == null)
            {
                lbl_cond_ivaE.Text = "Debe seleccionar una condición de IVA.";
                lbl_cond_ivaE.Show(); errorMsg += lbl_cond_ivaE.Text + Environment.NewLine;
            }

            // Fecha de nacimiento
            if (!DateTime.TryParse(dateTime_nac.Text, out DateTime fecha))
            {
                lbl_nacE.Text = "Debe ingresar una fecha de nacimiento válida.";
                lbl_nacE.Show(); errorMsg += lbl_nacE.Text + Environment.NewLine;
            }
            else
            {
                DateTime hoy = DateTime.Today;
                if (fecha > hoy)
                {
                    lbl_nacE.Text = "La fecha de nacimiento no puede ser futura.";
                    lbl_nacE.Show(); errorMsg += lbl_nacE.Text + Environment.NewLine;
                }
                else
                {
                    int edad = hoy.Year - fecha.Year;
                    if (fecha.Date > hoy.AddYears(-edad)) edad--;
                    if (edad < 18)
                    {
                        lbl_nacE.Text = "El cliente debe tener al menos 18 años.";
                        lbl_nacE.Show(); errorMsg += lbl_nacE.Text + Environment.NewLine;
                    }
                }
            }

            // Celular
            if (string.IsNullOrWhiteSpace(txt_celular.Text) ||
                !txt_celular.Text.All(char.IsDigit) ||
                txt_celular.Text.Length < 10 || txt_celular.Text.Length > 13)
            {
                lbl_celularE.Text = "El celular debe ser numérico y tener entre 10 y 13 dígitos.";
                lbl_celularE.Show(); errorMsg += lbl_celularE.Text + Environment.NewLine;
            }

            // Email
            if (string.IsNullOrWhiteSpace(txt_email.Text) ||
                !Regex.IsMatch(txt_email.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                lbl_emailE.Text = "Debe ingresar un correo electrónico válido.";
                lbl_emailE.Show(); errorMsg += lbl_emailE.Text + Environment.NewLine;
            }
            else if (ClienteControlador.ExisteEmail(txt_email.Text.Trim()))
            {
                lbl_emailE.Text = "Ya existe un cliente registrado con ese correo electrónico.";
                lbl_emailE.Show(); errorMsg += lbl_emailE.Text + Environment.NewLine;
            }

            // CP: exactamente 4
            if (txt_cp.Text.Length != 4 || !txt_cp.Text.All(char.IsDigit))
            {
                lbl_cpE.Text = "El código postal debe tener 4 dígitos numéricos.";
                lbl_cpE.Show(); errorMsg += lbl_cpE.Text + Environment.NewLine;
            }

            // País / Provincia / Localidad / Calle
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

            // Número de calle
            if (string.IsNullOrWhiteSpace(txt_num_calle.Text) || !txt_num_calle.Text.All(char.IsDigit))
            {
                lbl_num_calleE.Text = "El número de calle debe ser numérico.";
                lbl_num_calleE.Show(); errorMsg += lbl_num_calleE.Text + Environment.NewLine;
            }

            // Piso / Depto (opcionales) – no mezclar y 1..3 del mismo tipo
            if (!string.IsNullOrWhiteSpace(txt_piso.Text) && !_rxPisoDpto_Final.IsMatch(txt_piso.Text))
            {
                lbl_pisoE.Text = "Piso: ingrese 1–3 dígitos (ej. 34) o 1–3 letras (ej. PB), sin mezclar.";
                lbl_pisoE.Show(); errorMsg += lbl_pisoE.Text + Environment.NewLine;
            }
            if (!string.IsNullOrWhiteSpace(txt_depto.Text) && !_rxPisoDpto_Final.IsMatch(txt_depto.Text))
            {
                lbl_deptoE.Text = "Departamento: 1–3 dígitos o 1–3 letras, sin mezclar.";
                lbl_deptoE.Show(); errorMsg += lbl_deptoE.Text + Environment.NewLine;
            }

            // Activo
            if (combo_activo.SelectedItem == null)
            {
                lbl_activoE.Text = "Debe seleccionar el estado activo/inactivo.";
                lbl_activoE.Show(); errorMsg += lbl_activoE.Text + Environment.NewLine;
            }

            return string.IsNullOrEmpty(errorMsg);
        }

        private void EnfocarPrimerControlConError()
        {
            var pares = new (Label label, Control control)[]
            {
                (lbl_usuarioE, txt_usuario),
                (lbl_claveE, txt_clave),
                (lbl_nombreE, txt_nombre),
                (lbl_apellidoE, txt_apellido),
                (lbl_dniE, txt_dni),
                (lbl_cond_ivaE, combo_con_iva),
                (lbl_nacE, dateTime_nac),
                (lbl_celularE, txt_celular),
                (lbl_emailE, txt_email),
                (lbl_paisE, combo_pais),
                (lbl_provinciaE, combo_provincia),
                (lbl_localidadE, combo_localidad),
                (lbl_cpE, txt_cp),
                (lbl_calleE, combo_calle),
                (lbl_num_calleE, txt_num_calle),
                (lbl_pisoE, txt_piso),
                (lbl_deptoE, txt_depto),
                (lbl_comentariosE, richTextBox_comentario),
                (lbl_activoE, combo_activo)
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

        // ====== Diseño combos (tu estilo) ======
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

        // ====== CUIT ======
        private bool CuitValido(string cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit) || cuit.Length != 11 || !long.TryParse(cuit, out _))
                return false;

            int[] pesos = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int suma = 0;

            for (int i = 0; i < 10; i++)
                suma += (cuit[i] - '0') * pesos[i];

            int resto = suma % 11;
            int digitoVerificador = resto == 0 ? 0 : (resto == 1 ? 9 : 11 - resto);

            return digitoVerificador == (cuit[10] - '0');
        }

        // ====== Util ======
        private void OcultarErrores()
        {
            lbl_usuarioE.Hide();
            lbl_claveE.Hide();
            lbl_nombreE.Hide();
            lbl_apellidoE.Hide();
            lbl_dniE.Hide();
            lbl_cond_ivaE.Hide();
            lbl_celularE.Hide();
            lbl_emailE.Hide();
            lbl_cpE.Hide();
            lbl_num_calleE.Hide();
            lbl_pisoE.Hide();
            lbl_deptoE.Hide();
            lbl_comentariosE.Hide();
            lbl_nacE.Hide();
            lbl_paisE.Hide();
            lbl_provinciaE.Hide();
            lbl_localidadE.Hide();
            lbl_calleE.Hide();
            lbl_activoE.Hide();
        }

        // --- IVA según documento ---
        private void ActualizarOpcionesIVAporDocumento(string doc)
        {
            combo_con_iva.BeginUpdate();
            try
            {
                combo_con_iva.Items.Clear();

                if (string.IsNullOrWhiteSpace(doc) || !doc.All(char.IsDigit))
                {
                    combo_con_iva.Enabled = false;
                    return;
                }

                if (doc.Length == 8)
                {
                    // DNI → tres opciones y habilitado para elegir
                    combo_con_iva.Items.AddRange(new object[]
                    {
                "Consumidor Final",
                "Exento",
                "Monotributista"
                    });
                    combo_con_iva.SelectedIndex = 0;
                    combo_con_iva.Enabled = true;
                }
                else if (doc.Length == 11)
                {
                    // CUIT → solo RI; si es inválido, deshabilitado
                    if (!CuitValido(doc))
                    {
                        combo_con_iva.Enabled = false;
                        return;
                    }

                    combo_con_iva.Items.Add("Responsable Inscripto");
                    combo_con_iva.SelectedIndex = 0;
                    combo_con_iva.Enabled = false; // lo dejo fijo; si preferís, ponelo en true
                }
                else
                {
                    // Longitud intermedia → nada
                    combo_con_iva.Enabled = false;
                }
            }
            finally
            {
                combo_con_iva.EndUpdate();
            }
        }

        private void ActualizarEtiquetasYMascarasPorDocumento(string doc)
        {
            // Evitar acumular handlers
            txt_nombre.KeyPress -= Nombre_KeyPress_DNI;
            txt_apellido.KeyPress -= Apellido_KeyPress_DNI;
            txt_nombre.KeyPress -= Nombre_KeyPress_CUIT_RazonSocial;
            txt_apellido.KeyPress -= Tipo_KeyPress_CUIT;

            if (doc?.Length == 11)
            {
                // MODO CUIT (aunque no pase la verificación, mostramos estos rótulos)
                lbl_nombre.Text = "Razón social";
                lbl_apellido.Text = "Tipo";

                // Máscaras: razón social y tipo
                txt_nombre.KeyPress += Nombre_KeyPress_CUIT_RazonSocial; // letras, números y . - & / ( ) '
                txt_apellido.KeyPress += Tipo_KeyPress_CUIT;             // letras, espacios y .
            }
            else if (doc?.Length == 8)
            {
                // MODO DNI
                lbl_nombre.Text = "Nombre";
                lbl_apellido.Text = "Apellido";

                // Máscaras: solo letras y espacios
                txt_nombre.KeyPress += Nombre_KeyPress_DNI;
                txt_apellido.KeyPress += Apellido_KeyPress_DNI;
            }
            else
            {
                // Longitud intermedia o vacío → por defecto
                lbl_nombre.Text = "Nombre";
                lbl_apellido.Text = "Apellido";
                // (sin máscaras especiales)
            }
        }

        private void txt_dni_TextChanged(object sender, EventArgs e)
        {
            // Filtra no-dígitos y limita a 11
            if (!txt_dni.Text.All(char.IsDigit))
            {
                txt_dni.Text = new string(txt_dni.Text.Where(char.IsDigit).ToArray());
                txt_dni.SelectionStart = txt_dni.Text.Length;
            }
            if (txt_dni.Text.Length > 11)
            {
                txt_dni.Text = txt_dni.Text.Substring(0, 11);
                txt_dni.SelectionStart = txt_dni.Text.Length;
            }

            string doc = txt_dni.Text.Trim();

            // 1) Actualizar etiquetas + máscaras (Nombre/Apellido vs Razón social/Tipo)
            ActualizarEtiquetasYMascarasPorDocumento(doc);

            // 2) Actualizar opciones del IVA (lo que ya agregamos antes)
            ActualizarOpcionesIVAporDocumento(doc);

            // 3) Mensaje de CUIT inválido (opcional, solo feedback visual)
            if (doc.Length == 11 && !CuitValido(doc))
            {
                lbl_dniE.Text = "CUIT inválido (falló verificación).";
                lbl_dniE.Show();
            }
            else
            {
                lbl_dniE.Hide();
            }
        }



        private void HookComboHideError(ComboBox combo, Label errorLabel)
        {
            if (combo == null || errorLabel == null) return;

            // Oculte al elegir un ítem con el mouse/teclado
            combo.SelectionChangeCommitted += (s, e) => errorLabel.Hide();

            // También oculte si el texto coincide con algún ítem (para combos autocompletable)
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

        private void HookDateHideError(DateTimePicker dtp, Label errorLabel)
        {
            if (dtp == null || errorLabel == null) return;

            dtp.ValueChanged += (s, e) =>
            {
                errorLabel.Hide();
                // Si querés mostrar la fecha al elegirla:
                dtp.Format = DateTimePickerFormat.Short;
            };
        }

        



    }
}
