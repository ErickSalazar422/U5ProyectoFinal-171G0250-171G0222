using System;
using System.Collections.Generic;

namespace RopaMexicana_171G0250_171G0222.Models
{
    public partial class MarcaAfiliada
    {
        public MarcaAfiliada()
        {
            Producto = new HashSet<Producto>();
        }

        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Marca { get; set; }
        public string Contrasena { get; set; }
        public int? ClaveAct { get; set; }
        public ulong? Activo { get; set; }

        public virtual ICollection<Producto> Producto { get; set; }
    }
}
