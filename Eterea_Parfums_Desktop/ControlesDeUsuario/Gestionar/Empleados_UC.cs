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
    public partial class Empleados_UC : UserControl
    {
        private List<Empleado> empleados = new List<Empleado>();
        private List<Empleado> empleadosFiltrados = new List<Empleado>();

        // Paginación
        private int pageSize = 12;   // o el número que quieras
        private int currentPage = 0;

        // (Opcional) cache de sucursales para no llamar getById en cada fila
        private Dictionary<int, string> sucursalesPorId = new Dictionary<int, string>();

        public Empleados_UC()
        {
            InitializeComponent();

            this.Scale(new SizeF(Program.ScaleFactor, Program.ScaleFactor));

            // Asocia el evento KeyPress para aceptar solo números
            txt_buscar_dni.KeyPress += txt_buscar_dni_KeyPress;
            txt_buscar_dni.TextChanged += txt_buscar_dni_TextChanged;

            dataGridViewEmpleados.RowHeadersVisible = false;

            // Enganchar el CellPainting UNA sola vez
            dataGridViewEmpleados.CellPainting += dataGridView1_CellPainting;

            // 1) Cargar desde BD
            CargarEmpleadosDesdeBD();

            // 2) Aplicar filtro vacío y mostrar primera página
            AplicarFiltroYRefrescar();
        }

        /*private void cargarEmpleados(string filtroDni = "")
        {
            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            dataGridViewEmpleados.RowHeadersVisible = false;

            empleados = EmpleadoControlador.obtenerTodos();

            dataGridViewEmpleados.Rows.Clear();
            foreach (Empleado empleado in empleados)
            {
                if (string.IsNullOrEmpty(filtroDni) || empleado.dni.ToString().Contains(filtroDni))
                {
                    int rowIndex = dataGridViewEmpleados.Rows.Add();

                    dataGridViewEmpleados.Rows[rowIndex].Cells[0].Value = empleado.id.ToString();
                    dataGridViewEmpleados.Rows[rowIndex].Cells[1].Value = empleado.usuario.ToString();
                    dataGridViewEmpleados.Rows[rowIndex].Cells[2].Value = empleado.nombre.ToString();
                    dataGridViewEmpleados.Rows[rowIndex].Cells[3].Value = empleado.apellido.ToString();
                    dataGridViewEmpleados.Rows[rowIndex].Cells[4].Value = empleado.dni.ToString();


                    dataGridViewEmpleados.Rows[rowIndex].Cells[5].Value = empleado.celular.ToString();
                    dataGridViewEmpleados.Rows[rowIndex].Cells[6].Value = empleado.e_mail.ToString();


                    dataGridViewEmpleados.Rows[rowIndex].Cells[7].Value = (SucursalControlador.getById(empleado.sucursal_id.id)).nombre;


                    dataGridViewEmpleados.Rows[rowIndex].Cells[8].Value = empleado.rol;
                    dataGridViewEmpleados.Rows[rowIndex].Cells[9].Value = empleado.activo ? "Activo" : "Inactivo";

                    if (!empleado.activo)
                    {
                        dataGridViewEmpleados.Rows[rowIndex].Cells[9].Style.ForeColor = Color.Red;
                    }
                    else
                    {
                        dataGridViewEmpleados.Rows[rowIndex].Cells[9].Style.ForeColor = Color.Green; // Opcional
                    }

                    dataGridViewEmpleados.Rows[rowIndex].Cells[10].Value = "Editar";
                    dataGridViewEmpleados.Rows[rowIndex].Cells[11].Value = "Eliminar";
                }
                dataGridViewEmpleados.ClearSelection();

                dataGridViewEmpleados.CellPainting += dataGridView1_CellPainting;
            }
        }*/

        private void btn_crear_empleado_Click(object sender, EventArgs e)
        {
            FormCrearEmpleado frmVend = new FormCrearEmpleado();
            DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(frmVend);

            if (dr == DialogResult.OK)
            {
                Trace.WriteLine("OK");
                CargarEmpleadosDesdeBD();
                AplicarFiltroYRefrescar(txt_buscar_dni.Text.Trim());
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex].Name == "Editar")
            {
                //EDITAMOS

                int id = int.Parse(dataGridViewEmpleados.Rows[e.RowIndex].Cells[0].Value.ToString());

                Trace.WriteLine("El id es: " + id);

                Empleado empleado_editar = EmpleadoControlador.obtenerPorId(id);

                FormEditarEmpleado frmVend = new FormEditarEmpleado(empleado_editar);

                // ✅ Mostrar con fondo oscuro
                DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(frmVend);

                if (dr == DialogResult.OK)
                {
                    Trace.WriteLine("OK");
                    CargarEmpleadosDesdeBD();
                    AplicarFiltroYRefrescar(txt_buscar_dni.Text.Trim());
                }
            }
            else if (senderGrid.Columns[e.ColumnIndex].Name == "Eliminar")
            {
                //ELIMINAMOS
                int id = int.Parse(dataGridViewEmpleados.Rows[e.RowIndex].Cells[0].Value.ToString());

                Trace.WriteLine("El id es: " + id);

                Empleado empleado_eliminar = EmpleadoControlador.obtenerPorId(id);

                FormEliminarEmpleado frmVend = new FormEliminarEmpleado(empleado_eliminar, id);

                // ✅ Mostrar con fondo oscuro
                DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(frmVend);

                if (dr == DialogResult.OK)
                {
                    Trace.WriteLine("OK");
                    CargarEmpleadosDesdeBD();
                    AplicarFiltroYRefrescar(txt_buscar_dni.Text.Trim());
                }
            }

        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridViewEmpleados.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
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

        private void CargarEmpleadosDesdeBD()
        {
            empleados = EmpleadoControlador.obtenerTodos();

            // Cachear sucursales para evitar getById dentro del foreach
            var sucursalesActivas = SucursalControlador.getSucursalesActivas();
            sucursalesPorId = new Dictionary<int, string>();
            foreach (var s in sucursalesActivas)
            {
                if (!sucursalesPorId.ContainsKey(s.id))
                    sucursalesPorId.Add(s.id, s.nombre);
            }
        }

        private void AplicarFiltroYRefrescar(string filtroDni = "")
        {
            if (string.IsNullOrWhiteSpace(filtroDni))
            {
                empleadosFiltrados = new List<Empleado>(empleados);
            }
            else
            {
                empleadosFiltrados = empleados.FindAll(e =>
                    e.dni.ToString().Contains(filtroDni));
            }

            currentPage = 0;
            PintarPaginaActual();
        }

        private void PintarPaginaActual()
        {
            dataGridViewEmpleados.SuspendLayout();
            dataGridViewEmpleados.Rows.Clear();

            int totalPages = (int)Math.Ceiling((double)empleadosFiltrados.Count / pageSize);
            if (totalPages == 0) totalPages = 1;

            var pagina = empleadosFiltrados
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (Empleado empleado in pagina)
            {
                int rowIndex = dataGridViewEmpleados.Rows.Add();

                dataGridViewEmpleados.Rows[rowIndex].Cells[0].Value = empleado.id.ToString();
                dataGridViewEmpleados.Rows[rowIndex].Cells[1].Value = empleado.usuario;
                dataGridViewEmpleados.Rows[rowIndex].Cells[2].Value = empleado.nombre;
                dataGridViewEmpleados.Rows[rowIndex].Cells[3].Value = empleado.apellido;
                dataGridViewEmpleados.Rows[rowIndex].Cells[4].Value = empleado.dni.ToString();
                dataGridViewEmpleados.Rows[rowIndex].Cells[5].Value = empleado.celular;
                dataGridViewEmpleados.Rows[rowIndex].Cells[6].Value = empleado.e_mail;

                // Nombre sucursal: usar cache si la tenés
                string nombreSucursal = "";
                int idSucursal = empleado.sucursal_id.id;
                if (sucursalesPorId.TryGetValue(idSucursal, out var nomSuc))
                {
                    nombreSucursal = nomSuc;
                }
                else
                {
                    // fallback por si algo falta en el diccionario
                    nombreSucursal = SucursalControlador.getById(idSucursal)?.nombre;
                }
                dataGridViewEmpleados.Rows[rowIndex].Cells[7].Value = nombreSucursal;

                dataGridViewEmpleados.Rows[rowIndex].Cells[8].Value = empleado.rol;
                dataGridViewEmpleados.Rows[rowIndex].Cells[9].Value = empleado.activo ? "Activo" : "Inactivo";
                dataGridViewEmpleados.Rows[rowIndex].Cells[9].Style.ForeColor =
                    empleado.activo ? Color.Green : Color.Red;

                dataGridViewEmpleados.Rows[rowIndex].Cells[10].Value = "Editar";
                dataGridViewEmpleados.Rows[rowIndex].Cells[11].Value = "Eliminar";
            }

            dataGridViewEmpleados.ClearSelection();
            dataGridViewEmpleados.ResumeLayout();

           
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
            int totalPages = (int)Math.Ceiling((double)empleadosFiltrados.Count / pageSize);
            if (totalPages == 0) totalPages = 1;

            if (currentPage < totalPages - 1)
            {
                currentPage++;
                PintarPaginaActual();
            }
        }
    }
}
