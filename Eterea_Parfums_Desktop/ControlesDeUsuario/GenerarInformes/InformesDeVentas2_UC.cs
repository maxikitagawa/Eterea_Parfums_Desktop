using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Net.Http;

namespace Eterea_Parfums_Desktop.ControlesDeUsuario.GenerarInformes
{
    public partial class InformesDeVentas2_UC : UserControl
    {
        private List<Perfume> perfumes;
        private List<Stock> stocks;
        private int numeroSucursal;

        public InformesDeVentas2_UC(int numeroSucursal)
        {
            InitializeComponent();
            this.numeroSucursal = numeroSucursal;

            dataGridViewPerfumes.RowTemplate.Height = 80;

            // Cargar asíncrono cuando el UC ya está cargado (no congela la UI)
            this.Load += async (_, __) => await cargarDatosAsync();

            this.numeroSucursal = numeroSucursal;
            var sucursal = SucursalControlador.getAll().FirstOrDefault(s => s.id == numeroSucursal);
            lbl_info.Text = sucursal.nombre;

            this.Cursor = Cursors.Default;
            this.UseWaitCursor = false;
        }

        private System.Drawing.Image redimensionarImagen(System.Drawing.Image img, int cellWidth, int cellHeight)
        {
            // Calcula el ratio de ancho y alto original
            float ratioX = (float)cellWidth / img.Width;
            float ratioY = (float)cellHeight / img.Height;
            // Elige el ratio más pequeño para que encaje manteniendo la proporción
            float ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);

            // Crear un bitmap del tamaño exacto de la celda
            Bitmap resizedImage = new Bitmap(cellWidth, cellHeight);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.Clear(Color.Transparent); // Fondo transparente o el color que prefieras
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                // Centrar la imagen en la celda
                int posX = (cellWidth - newWidth) / 2;
                int posY = (cellHeight - newHeight) / 2;

                graphics.DrawImage(img, posX, posY, newWidth, newHeight);
            }

            return resizedImage;
        }



        private void cargarDatos()
        {
            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            dataGridViewPerfumes.RowHeadersVisible = false;
            perfumes = PerfumeControlador.getAll()
            .Where(p => p.activo == true)
            .ToList();

            stocks = StockControlador.getAll()
            .Where(s => s.sucursal != null && s.sucursal.id == numeroSucursal)
            .ToList();
      

            dataGridViewPerfumes.Rows.Clear();
            foreach (Stock stock in stocks)
            {
                Perfume perfume = perfumes.Find(p => p.id == stock.perfume.id);
                if (perfume != null)
                {
                    int rowIndex = dataGridViewPerfumes.Rows.Add();
                    string rutaCompletaImagen = Program.Ruta_Base + perfume.imagen1 + ".jpg";

                    int cellWidth = dataGridViewPerfumes.Columns[0].Width;
                    int cellHeight = dataGridViewPerfumes.RowTemplate.Height;
                    // Verificar si la imagen existe antes de cargarla

                    if (File.Exists(rutaCompletaImagen))
                    {
                        using (System.Drawing.Image originalImage = System.Drawing.Image.FromFile(rutaCompletaImagen))
                        {
                            dataGridViewPerfumes.Rows[rowIndex].Cells[0].Value = redimensionarImagen(originalImage, cellWidth, cellHeight);
                        }
                    }
                    else
                    {
                        dataGridViewPerfumes.Rows[rowIndex].Cells[0].Value = null; // O una imagen por defecto
                    }
                    ((DataGridViewImageCell)dataGridViewPerfumes.Rows[rowIndex].Cells[0]).ImageLayout = DataGridViewImageCellLayout.Zoom;
                    dataGridViewPerfumes.Rows[rowIndex].Cells[1].Value = perfume.codigo;
                    dataGridViewPerfumes.Rows[rowIndex].Cells[2].Value = (MarcaControlador.getById(perfume.marca.id)).nombre;
                    dataGridViewPerfumes.Rows[rowIndex].Cells[3].Value = perfume.nombre.ToString();
                    dataGridViewPerfumes.Rows[rowIndex].Cells[4].Value = (TipoDePerfumeControlador.getById(perfume.tipo_de_perfume.id)).tipo_de_perfume;
                    dataGridViewPerfumes.Rows[rowIndex].Cells[5].Value = (GeneroControlador.getById(perfume.genero.id)).genero;
                    dataGridViewPerfumes.Rows[rowIndex].Cells[6].Value = perfume.presentacion_ml.ToString() + " ml";


                  

                    dataGridViewPerfumes.Rows[rowIndex].Cells[7].Value = "$ " + perfume.precio_en_pesos.ToString();
                    dataGridViewPerfumes.Rows[rowIndex].Cells[8].Value = SucursalControlador.getById(stock.sucursal.id).nombre;
                    dataGridViewPerfumes.Rows[rowIndex].Cells[9].Value = stock.cantidad;
                }

                //dataGridViewPerfumes.CellPainting += dataGridView1_CellPainting;

            }
        }

        private void btn_exportar_pdf_Click(object sender, EventArgs e)
        {
            // Validar si hay datos en el DataGridView
            if (dataGridViewPerfumes.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Obtener la sucursal actual
            Sucursal sucursal = SucursalControlador.getById(numeroSucursal);
            SaveFileDialog guardarInventario = new SaveFileDialog
            {
                FileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf",
                Filter = "Archivos PDF (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                AddExtension = true
            };

            if (guardarInventario.ShowDialog() == DialogResult.OK)
            {
                using (FileStream stream = new FileStream(guardarInventario.FileName, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4.Rotate(), 25, 25, 25, 25);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

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

                    // Crear el encabezado del PDF
                    Paragraph headerParagraph = new Paragraph();
                    headerParagraph.Alignment = Element.ALIGN_CENTER;
                    headerParagraph.Add(Chunk.NEWLINE);
                    headerParagraph.Add(new Chunk("INVENTARIO", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 21)));
                    headerParagraph.Add(Chunk.NEWLINE);
                    headerParagraph.Add(Chunk.NEWLINE); // ⬅ espacio extra debajo del título

                    headerParagraph.Add(new Chunk($"{sucursal.nombre} - {sucursal.calle_id.nombre} {sucursal.numeracion_calle} - {sucursal.provincia_id.nombre} - {sucursal.pais_id.nombre}.",
                        FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                    headerParagraph.Add(Chunk.NEWLINE);
                    headerParagraph.Add(Chunk.NEWLINE);
                    headerParagraph.Add(Chunk.NEWLINE);
                    headerParagraph.Add(Chunk.NEWLINE); // ⬅ espacio extra antes del contenido principal

                    pdfDoc.Add(headerParagraph);

                    // Crear tabla con 10 columnas
                    PdfPTable table = new PdfPTable(10) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f });

                    string[] columnas = { "Imagen", "Código", "Marca", "Nombre", "Tipo", "Género", "Presentación", "Precio", "Sucursal", "Stock" };
                    foreach (string encabezado in columnas)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(encabezado, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)))
                        {
                            BackgroundColor = new BaseColor(200, 200, 200),
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };
                        table.AddCell(cell);
                    }

                    int totalStock = 0;
                   

                    foreach (DataGridViewRow row in dataGridViewPerfumes.Rows)
                    {
                        if (row.IsNewRow) continue;
                        if (row.Cells.Count < 10) continue;

                        // Imagen
                        if (row.Cells[0].Value is System.Drawing.Image img)
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                iTextSharp.text.Image imgPdf = iTextSharp.text.Image.GetInstance(ms.ToArray());
                                imgPdf.ScaleAbsolute(40f, 40f);
                                PdfPCell imgCell = new PdfPCell(imgPdf, true)
                                {
                                    HorizontalAlignment = Element.ALIGN_CENTER,
                                    VerticalAlignment = Element.ALIGN_MIDDLE,
                                    Padding = 5
                                };
                                table.AddCell(imgCell);
                            }
                        }
                        else
                        {
                            table.AddCell(new PdfPCell(new Phrase("Sin imagen"))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE
                            });
                        }

                        // Otras columnas: 1 a 9
                        for (int i = 1; i <= 9; i++)
                        {
                            string valor = row.Cells[i].FormattedValue?.ToString() ?? "N/A";
                            table.AddCell(new PdfPCell(new Phrase(valor))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE
                            });
                        }

                        // 👉 Suma total de stock
                        if (int.TryParse(row.Cells[9].Value?.ToString(), out int stock))
                        {
                            totalStock += stock;
                        }
                    
                }
                    pdfDoc.Add(table);

                    // 👇 Mostrar total de stock
                    Paragraph totalStockParagraph = new Paragraph($"Total de unidades en stock: {totalStock}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12))
                    {
                        Alignment = Element.ALIGN_RIGHT
                    };
                    pdfDoc.Add(totalStockParagraph);


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


        public async void ActualizarSucursal(Sucursal nuevaSucursal)
        {
            this.numeroSucursal = nuevaSucursal.id;
            lbl_info.Text = nuevaSucursal.nombre;
            await cargarDatosAsync();
        }

        // Devuelve la URL pública de la imagen 1 del perfume.
        // Prioriza imagen1_URL. Si no hay, compone con /imagenes/{imagen1}.jpg
        private string GetImagenUrl(Perfume p)
        {
            if (p == null) return null;

            if (!string.IsNullOrWhiteSpace(p.imagen1_URL))
                return p.imagen1_URL.Trim();

            var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(p.imagen1))
                return null;

            return $"{baseUrl}/imagenes/{p.imagen1}.jpg";
        }

        private async Task cargarDatosAsync()
        {
            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            dataGridViewPerfumes.RowHeadersVisible = false;

            perfumes = PerfumeControlador.getAll()
                .Where(p => p.activo == true)
                .ToList();

            stocks = StockControlador.getAll()
                .Where(s => s.sucursal != null && s.sucursal.id == numeroSucursal)
                .ToList();

            dataGridViewPerfumes.Rows.Clear();

            int cellWidth = dataGridViewPerfumes.Columns[0].Width;
            int cellHeight = dataGridViewPerfumes.RowTemplate.Height;

            foreach (Stock stock in stocks)
            {
                Perfume perfume = perfumes.Find(p => p.id == stock.perfume.id);
                if (perfume == null) continue;

                int rowIndex = dataGridViewPerfumes.Rows.Add();

                // Placeholder inicial (si tenés un recurso “sinImagen”)
                dataGridViewPerfumes.Rows[rowIndex].Cells[0].Value = Eterea_Parfums_Desktop.Properties.Resources.sinImagen;
                ((DataGridViewImageCell)dataGridViewPerfumes.Rows[rowIndex].Cells[0]).ImageLayout = DataGridViewImageCellLayout.Zoom;

                dataGridViewPerfumes.Rows[rowIndex].Cells[1].Value = perfume.codigo;
                dataGridViewPerfumes.Rows[rowIndex].Cells[2].Value = (MarcaControlador.getById(perfume.marca.id)).nombre;
                dataGridViewPerfumes.Rows[rowIndex].Cells[3].Value = perfume.nombre;
                dataGridViewPerfumes.Rows[rowIndex].Cells[4].Value = (TipoDePerfumeControlador.getById(perfume.tipo_de_perfume.id)).tipo_de_perfume;
                dataGridViewPerfumes.Rows[rowIndex].Cells[5].Value = (GeneroControlador.getById(perfume.genero.id)).genero;
                dataGridViewPerfumes.Rows[rowIndex].Cells[6].Value = perfume.presentacion_ml + " ml";
                dataGridViewPerfumes.Rows[rowIndex].Cells[7].Value = "$ " + perfume.precio_en_pesos.ToString();
                dataGridViewPerfumes.Rows[rowIndex].Cells[8].Value = SucursalControlador.getById(stock.sucursal.id).nombre;
                dataGridViewPerfumes.Rows[rowIndex].Cells[9].Value = stock.cantidad;

                // ↓ Descarga desde tu API y redimensiona a la celda
                try
                {
                    var url = GetImagenUrl(perfume);
                    var img = await ApiImageUploader.DownloadImageAsync(url); // usa tu helper
                    if (img != null)
                    {
                        var resized = redimensionarImagen(img, cellWidth, cellHeight);
                        if (rowIndex < dataGridViewPerfumes.Rows.Count)
                            dataGridViewPerfumes.Rows[rowIndex].Cells[0].Value = resized;
                    }
                }
                catch
                {
                    // Dejá la imagen por defecto si falla
                }
            }
        }

    }
}
