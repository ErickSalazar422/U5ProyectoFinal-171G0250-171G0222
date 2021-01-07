using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RopaMexicana_171G0250_171G0222.Models;
using Microsoft.EntityFrameworkCore;


namespace RopaMexicana_171G0250_171G0222.Repositories
{
    public class MarcaRepository: Repository<MarcaAfiliada>
    {
        public MarcaRepository(sistem14_ropa_mexicanaContext cxc) : base(cxc) { }
        public MarcaAfiliada GetAfiliadosByMarca(string marca)
        {
            return Context.MarcaAfiliada.FirstOrDefault(x => x.Marca == marca);
        }

        public MarcaAfiliada GetProductosById(int id)
        {
            return Context.MarcaAfiliada.Include(x => x.Producto).FirstOrDefault(x => x.Id == id);
        }
    }
}


