using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.tool.xml;

namespace Eterea_Parfums_Desktop.ControlesDeUsuario.GenerarInformes
{
    public partial class GenerarInformes_UC : UserControl
    {
        int numeroSucursal;      


        public GenerarInformes_UC()
        {
            InitializeComponent();

            numeroSucursal = Program.sucursal;

            // Agregar opciones al comboBox
            combobox_informe.Items.Add("Informe de ventas");
            combobox_informe.Items.Add("Inventario");
            combobox_informe.Items.Add("Listar perfumes con bajo stock");
            combobox_informe.Items.Add("Informe de Historial de Caja");

            InformesDeVentas1_UC adminUC = new InformesDeVentas1_UC(numeroSucursal);
            addUserControl(adminUC);

            combobox_informe.DrawMode = DrawMode.OwnerDrawFixed;
            combobox_informe.DrawItem += comboBoxDiseño_DrawItem;
            combobox_informe.DropDownStyle = ComboBoxStyle.DropDownList;

            //Llenar combo de sucursales
            comboBox_cambiar_sucursal.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox_cambiar_sucursal.DrawItem += comboBoxDiseño_DrawItem;
            comboBox_cambiar_sucursal.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_cambiar_sucursal.SelectedIndexChanged += comboBox_cambiar_sucursal_SelectedIndexChanged; //Evento

            CargarComboSucursal();


            combobox_informe.SelectedIndex = 0; // Seleccionar el primer informe por defecto

            this.Cursor = Cursors.Default;
            this.UseWaitCursor = false;

            combobox_informe.SelectedIndexChanged += combobox_informe_SelectedIndexChanged;  //Evento que cumple la funcion del boton "Confirmar"
            
        }



        public void addUserControl(UserControl uc)
        {
            uc.Dock = DockStyle.Fill;
            panel_info_ventas.Controls.Clear();
            panel_info_ventas.Controls.Add(uc);
            uc.BringToFront();
        }



        private void CargarComboSucursal()
        {
            // Obtener sucursales y filtrar la que tiene id == 0
            var sucursales = SucursalControlador.getAll()
                .Where(s => s.id != 0 && s.id != 3)
                .ToList();

            comboBox_cambiar_sucursal.DataSource = null;
            comboBox_cambiar_sucursal.Items.Clear();

            comboBox_cambiar_sucursal.DisplayMember = "nombre";
            comboBox_cambiar_sucursal.ValueMember = "id";

            comboBox_cambiar_sucursal.DataSource = sucursales;

            // Seleccionar la sucursal actual por defecto
            var actual = sucursales.FirstOrDefault(s => s.id == Program.sucursal);
            if (actual != null)
            {
                comboBox_cambiar_sucursal.SelectedValue = actual.id;
                numeroSucursal = actual.id;
            }
        }


        private void comboBox_cambiar_sucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_cambiar_sucursal.SelectedItem is Sucursal sucursalSeleccionada)
            {
                numeroSucursal = sucursalSeleccionada.id;

                // Refrescar el informe actualmente seleccionado
                combobox_informe_SelectedIndexChanged(null, null);

                // Actualizar el label lbl_info si el control ya está instanciado
                if (panel_info_ventas.Controls.Count > 0)
                {
                    var uc = panel_info_ventas.Controls[0];
                    if (uc is InformesDeVentas1_UC informes1)
                        informes1.ActualizarSucursal(sucursalSeleccionada);
                    else if (uc is InformesDeVentas2_UC informes2)
                        informes2.ActualizarSucursal(sucursalSeleccionada);
                    else if (uc is ListadoPerfumesBajoStock_UC listado)
                        listado.ActualizarSucursal(sucursalSeleccionada);
                    else if (uc is HistorialDeCaja_UC historialUC)
                        historialUC.ActualizarSucursal(sucursalSeleccionada.id);
                }
            }
        }



        private void comboBoxDiseño_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            // Obtener el ComboBox y el texto del ítem actual
            ComboBox combo = sender as ComboBox;

            string text = combo.Items[e.Index] is Sucursal suc ? suc.nombre : combo.Items[e.Index].ToString();


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

        private void combobox_informe_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combobox_informe.SelectedIndex == 0)
            {
                InformesDeVentas1_UC adminUC = new InformesDeVentas1_UC(numeroSucursal);
                addUserControl(adminUC);
            }
            else if (combobox_informe.SelectedIndex == 1)
            {
                InformesDeVentas2_UC adminUC = new InformesDeVentas2_UC(numeroSucursal);
                addUserControl(adminUC);
            }
            else if (combobox_informe.SelectedIndex == 2)
            {
                ListadoPerfumesBajoStock_UC listadoUC = new ListadoPerfumesBajoStock_UC(numeroSucursal);
                addUserControl(listadoUC);
            }
            else if (combobox_informe.SelectedIndex == 3)
            {
                HistorialDeCaja_UC historialUC = new HistorialDeCaja_UC(numeroSucursal);
                addUserControl(historialUC);

                var sucursalActual = SucursalControlador.getAll().FirstOrDefault(s => s.id == numeroSucursal);
                if (sucursalActual != null)
                {
                    historialUC.Controls.Find("lbl_info", true).FirstOrDefault()?.GetType()
                        .GetProperty("Text")?.SetValue(
                            historialUC.Controls.Find("lbl_info", true).FirstOrDefault(),
                            sucursalActual.nombre);
                }
            }
        }




    }
}
