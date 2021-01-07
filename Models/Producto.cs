using System;
using System.Collections.Generic;

namespace RopaMexicana_171G0250_171G0222.Models
{
    public partial class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Costo { get; set; }
        public string Descripción { get; set; }
        public string Color { get; set; }
        public string Material { get; set; }
        public string Hipervinculo { get; set; }
        public int IdMarcaAfi { get; set; }

        public virtual MarcaAfiliada IdMarcaAfiNavigation { get; set; }
    }
}
