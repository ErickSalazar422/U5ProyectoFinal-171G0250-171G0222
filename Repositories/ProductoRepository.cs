using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RopaMexicana_171G0250_171G0222.Models;
using RopaMexicana_171G0250_171G0222.Models.ViewModels;

namespace RopaMexicana_171G0250_171G0222.Repositories
{
    public class ProductoRepository : Repository<Producto>
    {
        public ProductoRepository(sistem14_ropa_mexicanaContext cxc) : base(cxc)
        {

        }

        public IEnumerable<Producto> GetProductosByAfiliados(string marca)
        { return Context.Producto.Where(x => x.IdMarcaAfiNavigation.Marca == marca); }

        public Producto GetProductoByAfiliadoMarca(string marca, string nombre)
        {
            return Context.Producto.Include(x => x.IdMarcaAfiNavigation).FirstOrDefault(x => x.IdMarcaAfiNavigation.Nombre == marca && x.Nombre == nombre);
        }

        public Producto GetProductosByNombre(string nombre)
        {
            nombre = nombre.Replace("-", " ");
            return Context.Producto
                .Include(x => x.IdMarcaAfiNavigation)
                .FirstOrDefault(x => x.Nombre == nombre);
        }



        public override bool Validar(Producto entidad)
        {
            if (string.IsNullOrWhiteSpace(entidad.Nombre))
                throw new Exception("Introduzca el nombre del producto");
            if (entidad.Costo <= 0)
                throw new Exception("Asigne un costo al producto");
            if (string.IsNullOrWhiteSpace(entidad.Descripción))
                throw new Exception("Añada una descripción al producto");

            if (string.IsNullOrWhiteSpace(entidad.Color))
                throw new Exception("Especifique el color del producto");
            if (string.IsNullOrWhiteSpace(entidad.Material))
                throw new Exception("Especifique el material del que está hecho el producto");
            if (string.IsNullOrWhiteSpace(entidad.Hipervinculo))
                throw new Exception("Añada un hipervinculo para dirigir a la compra del producto");
            return true;
        }

        public IEnumerable<ProductoViewModel> GetProductosgg()
        {
            return Context.Producto.OrderBy(x => x.Nombre)
                .Select(x => new ProductoViewModel
                {
                    Id = x.Id,
                    Nombre = x.Nombre
                });
        }
        public IEnumerable<ProductoViewModel> GetProductoByLetraInicial(string letra)
        {
            return GetProductosgg().Where(x => x.Nombre.StartsWith(letra));
        }
        public IEnumerable<char> GetLetrasIniciales()
        {
            return Context.Producto.OrderBy(x => x.Nombre)
                .Select(x => x.Nombre.First());
        }

    }

}
