using Eterea_Parfums_Desktop.Controladores;
using System;
using System.IO;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormEliminarPromo : Form
    {
        private int promoId; // ID de la promoción
        private string promoNombre; // Nombre de la promoción
        public FormEliminarPromo(int idPromo, string nombrePromo)
        {
            InitializeComponent();

            promoId = idPromo;
            promoNombre = nombrePromo;

            // Mostrar el nombre de la promoción en una etiqueta o cuadro de texto
            lbl_nombre_promo_seleccionada.Text = nombrePromo;
        }


        private void btn_eliminar_promo_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(
                "¿Está seguro de que desea desactivar esta promoción?\n\n" +
                "Se marcará como inactiva y sus fechas se ajustarán automáticamente.",
                "Confirmar desactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    // ✅ Eliminación lógica (UPDATE: fechas + activo=false)
                    bool resultado = PromoControlador.eliminarPromo(promoId);

                    if (resultado)
                    {
                        MessageBox.Show(
                            "La promoción fue desactivada con éxito.",
                            "Éxito",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        this.DialogResult = DialogResult.OK; //para que Promos_UC recargue
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Ocurrió un error al desactivar la promoción: " + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void btn_x_cerrar_ventana_eliminar_Click(object sender, EventArgs e)
        {
            this.Close(); // Cierra el formulario
        }
    }



}
