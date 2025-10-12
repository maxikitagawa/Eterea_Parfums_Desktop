using System;

namespace Eterea_Parfums_Desktop.Modelos
{
    public class Promocion
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin { get; set; }
        public int descuento { get; set; }
        public bool activo { get; set; }
        public string descripcion { get; set; }
        public string banner { get; set; }
        public string imagen_URL { get; set; }

        public Promocion(int id, string nombre, DateTime fecha_inicio, DateTime fecha_fin, int descuento, bool activo, string descripcion, string banner, string imagen_URL)
        {
            this.id = id;
            this.nombre = nombre;
            this.fecha_inicio = fecha_inicio;
            this.fecha_fin = fecha_fin;
            this.descuento = descuento;
            this.activo = activo;
            this.descripcion = descripcion;
            this.banner = banner;
            this.imagen_URL = imagen_URL;
        }

        public Promocion()
        {

        }
    }
}
