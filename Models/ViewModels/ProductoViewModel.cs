using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RopaMexicana_171G0250_171G0222.Models.ViewModels
{
    public class ProductoViewModel
    {

        public int Id { get; set; }
        public string Nombre { get; set; }
        public Producto Producto { get; set; }
        public IFormFile Archivo { get; set; }
        public string Imagen { get; set; }
        public MarcaAfiliada MarcaAfiliada { get; set; }
        public IEnumerable<MarcaAfiliada> MarcaAfiliadas { get; set; }
    }
}
