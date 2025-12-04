using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Helpers;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop.ControlesDeUsuario
{
    public partial class Clientes_UC : UserControl
    {
        private List<Cliente> clientes = new List<Cliente>();
        private List<Cliente> clientesFiltrados = new List<Cliente>();

        // Paginación
        private int pageSize = 15;   // cantidad de clientes por página
        private int currentPage = 0;

        public Clientes_UC()
        {
            InitializeComponent();
            txt_buscar_dni.MaxLength = 8;

            // Asocia el evento KeyPress para aceptar solo números
            txt_buscar_dni.KeyPress += txt_buscar_dni_KeyPress;
            txt_buscar_dni.TextChanged += txt_buscar_dni_TextChanged;

            dataGridViewClientes.RowHeadersVisible = false;

            // Enganchar una sola vez
            dataGridViewClientes.CellPainting += dataGridView1_CellPainting;

            // Cargar datos desde BD una sola vez
            CargarClientesDesdeBD();

            // Aplicar filtro vacío y mostrar primera página
            AplicarFiltroYRefrescar();
        }

        /*private void cargarClientes(string filtroDni = "")
        {
            dataGridViewClientes.RowHeadersVisible = false;
            clientes = ClienteControlador.obtenerTodos();
            dataGridViewClientes.Rows.Clear();

            foreach (Cliente cliente in clientes)
            {
                if (string.IsNullOrEmpty(filtroDni) || cliente.dni.ToString().Contains(filtroDni))
                {
                    int rowIndex = dataGridViewClientes.Rows.Add();

                    dataGridViewClientes.Rows[rowIndex].Cells[0].Value = cliente.id.ToString();
                    dataGridViewClientes.Rows[rowIndex].Cells[1].Value = cliente.usuario.ToString();
                    dataGridViewClientes.Rows[rowIndex].Cells[2].Value = cliente.nombre.ToString();
                    dataGridViewClientes.Rows[rowIndex].Cells[3].Value = cliente.apellido.ToString();
                    dataGridViewClientes.Rows[rowIndex].Cells[4].Value = cliente.dni.ToString();
                    dataGridViewClientes.Rows[rowIndex].Cells[5].Value = cliente.celular.ToString();
                    dataGridViewClientes.Rows[rowIndex].Cells[6].Value = cliente.e_mail.ToString();

                    dataGridViewClientes.Rows[rowIndex].Cells[7].Value = cliente.activo ? "Activo" : "Inactivo";

                    if (!cliente.activo)
                    {
                        dataGridViewClientes.Rows[rowIndex].Cells[7].Style.ForeColor = Color.Red;
                    }
                    else
                    {
                        dataGridViewClientes.Rows[rowIndex].Cells[7].Style.ForeColor = Color.Green; // Opcional
                    }

                    dataGridViewClientes.Rows[rowIndex].Cells[8].Value = "Editar";
                    dataGridViewClientes.Rows[rowIndex].Cells[9].Value = "Eliminar";
                }

                dataGridViewClientes.ClearSelection();
                dataGridViewClientes.CellPainting += dataGridView1_CellPainting;
            }
        }*/

        private void btn_crear_cliente_Click(object sender, EventArgs e)
        {
            FormCrearCliente formCrearClienteABM = new FormCrearCliente();


            // ✅ Mostrar con fondo oscuro
            DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(formCrearClienteABM);

            if (dr == DialogResult.OK)
            {
                Trace.WriteLine("OK");
                CargarClientesDesdeBD();
                AplicarFiltroYRefrescar(txt_buscar_dni.Text.Trim());
            }
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex].Name == "Editar")
            {
                //EDITAMOS

                int id = int.Parse(dataGridViewClientes.Rows[e.RowIndex].Cells[0].Value.ToString());

                Trace.WriteLine("El id es: " + id);

                Cliente cliente_editar = ClienteControlador.obtenerPorId(id);

                FormEditarCliente formEditarClienteABM = new FormEditarCliente(cliente_editar);

                // ✅ Mostrar con fondo oscuro
                DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(formEditarClienteABM);

                if (dr == DialogResult.OK)
                {
                    Trace.WriteLine("OK");
                    CargarClientesDesdeBD();
                    AplicarFiltroYRefrescar(txt_buscar_dni.Text.Trim());
                }
            }
            else if (senderGrid.Columns[e.ColumnIndex].Name == "Eliminar")
            {
                //ELIMINAMOS
                int id = int.Parse(dataGridViewClientes.Rows[e.RowIndex].Cells[0].Value.ToString());

                Trace.WriteLine("El id es: " + id);

                Cliente cliente_eliminar = ClienteControlador.obtenerPorId(id);

                FormEliminarCliente formEliminarClienteABM = new FormEliminarCliente(cliente_eliminar, id);

                // ✅ Mostrar con fondo oscuro
                DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(formEliminarClienteABM);

                if (dr == DialogResult.OK)
                {
                    Trace.WriteLine("OK");
                    CargarClientesDesdeBD();
                    AplicarFiltroYRefrescar(txt_buscar_dni.Text.Trim());
                }
            }
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridViewClientes.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                // Crear un rectángulo para el botón
                Rectangle buttonRect = e.CellBounds;
                buttonRect.Inflate(-2, -2); // Reducir tamaño para dar efecto de borde

                // Definir colores personalizados
                Color buttonColor = Color.FromArgb(228, 137, 164); // Color de fondo del botón
                Color textColor = Color.FromArgb(250, 236, 239); // Color del texto

                using (SolidBrush brush = new SolidBrush(buttonColor))
                {
                    e.Graphics.FillRectangle(brush, buttonRect);
                }

                // Dibujar el texto del botón
                TextRenderer.DrawText(e.Graphics, (string)e.Value, e.CellStyle.Font, buttonRect, textColor,
                                      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void txt_buscar_dni_TextChanged(object sender, EventArgs e)
        {
            string filtroDni = txt_buscar_dni.Text.Trim();

            //Ahora solo filtramos en memoria y refrescamos la página
            AplicarFiltroYRefrescar(filtroDni);
        }

        private void txt_buscar_dni_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir solo dígitos, retroceso y control (como copiar/pegar)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Ignorar entrada no válida
            }
        }

        private void CargarClientesDesdeBD()
        {
            clientes = ClienteControlador.obtenerTodos();
        }

        private void AplicarFiltroYRefrescar(string filtroDni = "")
        {
            if (string.IsNullOrWhiteSpace(filtroDni))
            {
                clientesFiltrados = new List<Cliente>(clientes);
            }
            else
            {
                clientesFiltrados = clientes.FindAll(c =>
                    c.dni.ToString().Contains(filtroDni));
            }

            currentPage = 0;
            PintarPaginaActual();
        }

        private void PintarPaginaActual()
        {
            dataGridViewClientes.SuspendLayout();
            dataGridViewClientes.Rows.Clear();

            int totalPages = (int)Math.Ceiling((double)clientesFiltrados.Count / pageSize);
            if (totalPages == 0) totalPages = 1; // evitar división por cero

            var pagina = clientesFiltrados
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (Cliente cliente in pagina)
            {
                int rowIndex = dataGridViewClientes.Rows.Add();

                dataGridViewClientes.Rows[rowIndex].Cells[0].Value = cliente.id.ToString();
                dataGridViewClientes.Rows[rowIndex].Cells[1].Value = cliente.usuario;
                dataGridViewClientes.Rows[rowIndex].Cells[2].Value = cliente.nombre;
                dataGridViewClientes.Rows[rowIndex].Cells[3].Value = cliente.apellido;
                dataGridViewClientes.Rows[rowIndex].Cells[4].Value = cliente.dni.ToString();
                dataGridViewClientes.Rows[rowIndex].Cells[5].Value = cliente.celular;
                dataGridViewClientes.Rows[rowIndex].Cells[6].Value = cliente.e_mail;

                dataGridViewClientes.Rows[rowIndex].Cells[7].Value = cliente.activo ? "Activo" : "Inactivo";
                dataGridViewClientes.Rows[rowIndex].Cells[7].Style.ForeColor =
                    cliente.activo ? Color.Green : Color.Red;

                dataGridViewClientes.Rows[rowIndex].Cells[8].Value = "Editar";
                dataGridViewClientes.Rows[rowIndex].Cells[9].Value = "Eliminar";
            }

            dataGridViewClientes.ClearSelection();
            dataGridViewClientes.ResumeLayout();

            // Actualizar label de página
            lbl_pagina.Text = $"Página {currentPage + 1} de {totalPages}";
        }

        private void btn_izq_Click(object sender, EventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                PintarPaginaActual();
            }
        }

        private void btn_der_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)clientesFiltrados.Count / pageSize);
            if (totalPages == 0) totalPages = 1;

            if (currentPage < totalPages - 1)
            {
                currentPage++;
                PintarPaginaActual();
            }
        }
    }
}
