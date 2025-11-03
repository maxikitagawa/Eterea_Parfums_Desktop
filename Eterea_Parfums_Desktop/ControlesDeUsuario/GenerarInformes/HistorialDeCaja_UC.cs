using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.DTOs;
using Eterea_Parfums_Desktop.Modelos;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Rectangle = System.Drawing.Rectangle;

namespace Eterea_Parfums_Desktop.ControlesDeUsuario.GenerarInformes
{
    public partial class HistorialDeCaja_UC : UserControl
    {
        private int numeroSucursal;
        private List<HistorialCajaDTO> historial;

        public HistorialDeCaja_UC(int numeroSucursal)
        {
            InitializeComponent();
            this.numeroSucursal = numeroSucursal;

            dataGridViewHistorialCaja.CellClick += dataGridViewHistorialCaja_CellClick;
            dataGridViewHistorialCaja.CellPainting += dataGridViewHistorialCaja_CellPainting;

            dataGridViewHistorialCaja.RowHeadersVisible = false;
            dataGridViewHistorialCaja.AutoGenerateColumns = false;
            dataGridViewHistorialCaja.DataSource = null;

            lbl_sin_datos.Visible = false;

            selector_dia.ValueChanged += selector_dia_ValueChanged;

            CrearColumnas();

            this.Cursor = Cursors.Default;
            this.UseWaitCursor = false;
        }

        private void selector_dia_ValueChanged(object sender, EventArgs e)
        {
            FiltrarPorFecha(selector_dia.Value.Date);
        }

        private void CrearColumnas()
        {
            dataGridViewHistorialCaja.Columns.Clear();

            dataGridViewHistorialCaja.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Caja N°",
                DataPropertyName = "NumeroCaja",
                ReadOnly = true
            });

            dataGridViewHistorialCaja.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Apertura",
                DataPropertyName = "FechaApertura",
                ReadOnly = true
            });

            dataGridViewHistorialCaja.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Cierre",
                DataPropertyName = "FechaCierre",
                ReadOnly = true
            });

            dataGridViewHistorialCaja.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Empleado",
                DataPropertyName = "Empleado",
                ReadOnly = true
            });

            DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn
            {
                HeaderText = "Datos Empleado",
                Text = "Ver Empleado",
                UseColumnTextForButtonValue = true,
                Name = "btnVerEmpleado"
            };
            dataGridViewHistorialCaja.Columns.Add(btnCol);
        }

        private void FiltrarPorFecha(DateTime fechaSeleccionada)
        {
            historial = HistorialCajaControlador.GetPorSucursal(numeroSucursal)
                .Where(h => h.FechaApertura.Date == fechaSeleccionada)
                .ToList();

            dataGridViewHistorialCaja.DataSource = null;
            dataGridViewHistorialCaja.DataSource = historial;

            dataGridViewHistorialCaja.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.Value != null)
                {
                    string columnName = dataGridViewHistorialCaja.Columns[e.ColumnIndex].DataPropertyName;

                    if ((columnName == "FechaApertura" || columnName == "FechaCierre") && e.Value is DateTime dt)
                    {
                        e.Value = dt.ToString("dd/MM/yyyy HH:mm") + " hs";
                        e.FormattingApplied = true;
                    }
                }
            };

        }

        private void dataGridViewHistorialCaja_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewHistorialCaja.Columns[e.ColumnIndex].Name == "btnVerEmpleado")
            {
                var usuario = historial[e.RowIndex].Usuario;
                FormEmpleado form = new FormEmpleado(usuario)
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    TopMost = true
                };
                form.ShowDialog();
                form.BringToFront();
                form.Focus();
            }
        }

        private void dataGridViewHistorialCaja_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridViewHistorialCaja.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                Rectangle buttonRect = e.CellBounds;
                buttonRect.Inflate(-2, -2);

                Color buttonColor = Color.FromArgb(228, 137, 164);
                Color textColor = Color.FromArgb(250, 236, 239);

                using (SolidBrush brush = new SolidBrush(buttonColor))
                {
                    e.Graphics.FillRectangle(brush, buttonRect);
                }

                TextRenderer.DrawText(e.Graphics, (string)e.Value, e.CellStyle.Font, buttonRect, textColor,
                                      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        public void ActualizarSucursal(int nuevaSucursal)
        {
            this.numeroSucursal = nuevaSucursal;
            var sucursal = SucursalControlador.getAll().FirstOrDefault(s => s.id == nuevaSucursal);
            lbl_info.Text = sucursal != null ? sucursal.nombre : "Sucursal desconocida";

            if (selector_dia.Value != null)
                FiltrarPorFecha(selector_dia.Value.Date);
        }

        // Dentro del evento del botón btn_exportar_pdf_Click para HistorialDeCaja_UC
        private void btn_exportar_pdf_Click(object sender, EventArgs e)
        {
            if (dataGridViewHistorialCaja.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Sucursal sucursal = SucursalControlador.getById(numeroSucursal);

            SaveFileDialog guardarPDF = new SaveFileDialog
            {
                FileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf",
                Filter = "Archivos PDF (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                AddExtension = true
            };

            if (guardarPDF.ShowDialog() == DialogResult.OK)
            {
                using (FileStream stream = new FileStream(guardarPDF.FileName, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4.Rotate(), 25, 25, 25, 25);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    // Logo
                    try
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(Properties.Resources.LogoEtereaFactura, System.Drawing.Imaging.ImageFormat.Png);
                        logo.ScaleToFit(70, 70);
                        logo.Alignment = iTextSharp.text.Image.UNDERLYING;
                        pdfDoc.Add(logo);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al cargar el logo: " + ex.Message);
                    }

                    // Encabezado
                    Paragraph header = new Paragraph();
                    header.Alignment = Element.ALIGN_CENTER;
                    header.Add(Chunk.NEWLINE);
                    header.Add(new Chunk("HISTORIAL DE CAJA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 21)));
                    header.Add(Chunk.NEWLINE);
                    header.Add(Chunk.NEWLINE);
                    header.Add(new Chunk($"{sucursal.nombre} - {sucursal.calle_id.nombre} {sucursal.numeracion_calle} - {sucursal.provincia_id.nombre} - {sucursal.pais_id.nombre}.",
                        FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                    header.Add(Chunk.NEWLINE);
                    header.Add(Chunk.NEWLINE);
                    header.Add(Chunk.NEWLINE);
                    pdfDoc.Add(header);

                    // Tabla con 4 columnas
                    PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 20f, 25f, 25f, 30f });

                    string[] columnas = { "Caja Nº", "Apertura", "Cierre", "Empleado" };
                    foreach (string col in columnas)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(col, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)))
                        {
                            BackgroundColor = new BaseColor(200, 200, 200),
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };
                        table.AddCell(cell);
                    }

                    foreach (DataGridViewRow row in dataGridViewHistorialCaja.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string caja = row.Cells[0].FormattedValue?.ToString() ?? "-";
                        string apertura = row.Cells[1].Value is DateTime fa ? fa.ToString("dd/MM/yyyy HH:mm\" hs\"") : "-";
                        string cierre = row.Cells[2].Value is DateTime fc ? fc.ToString("dd/MM/yyyy HH:mm\" hs\"") : "-";
                        string empleado = row.Cells[3].FormattedValue?.ToString() ?? "-";

                        table.AddCell(new PdfPCell(new Phrase(caja)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(apertura)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(cierre)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(empleado)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    }

                    pdfDoc.Add(table);

                    // Footer
                    Paragraph footer = new Paragraph();
                    footer.Add(Chunk.NEWLINE);
                    footer.Add(new Chunk("Reporte generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), FontFactory.GetFont(FontFactory.HELVETICA, 8)));
                    footer.Alignment = Element.ALIGN_CENTER;
                    pdfDoc.Add(footer);

                    pdfDoc.Close();
                    stream.Close();
                }

                MessageBox.Show("PDF generado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
