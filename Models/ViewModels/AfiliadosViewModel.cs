using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RopaMexicana_171G0250_171G0222.Models.ViewModels
{
    public class AfiliadosViewModel
    {
        public string MarcaAfiliado { get; set; }
        public IEnumerable<Models.Producto> Productos { get; set; }
    }
}
