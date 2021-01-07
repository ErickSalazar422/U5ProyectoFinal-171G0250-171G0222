using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RopaMexicana_171G0250_171G0222.Models;

namespace RopaMexicana_171G0250_171G0222.Repositories
{
    public class Repository<T> where T : class
    {
        public sistem14_ropa_mexicanaContext Context { get; set; }

        public Repository(sistem14_ropa_mexicanaContext cxc)
        {
            Context = cxc;
        }

        public MarcaAfiliada GetUsByCorreo(string correo)
        {
            return Context.MarcaAfiliada.FirstOrDefault(x => x.Correo.ToUpper() == correo.ToUpper());
        }
        public virtual IEnumerable<T> GetAll()
        {
            return Context.Set<T>();
        }
        public T Get(object id)
        {
            return Context.Find<T>(id);
        }
        public virtual bool Validar(T entidad)
        {
            return true;
        }
        public virtual void Insert(T entidad)
        {
            if (Validar(entidad))
            {
                Context.Add(entidad);
                Context.SaveChanges();
            }
        }
        public virtual void Update(T entidad)
        {
            if (Validar(entidad))
            {
                Context.Update<T>(entidad);
                Context.SaveChanges();
            }
        }
        public virtual void Delete(T entidad)
        {
            Context.Remove<T>(entidad);
            Context.SaveChanges();
        }
        public bool validar(MarcaAfiliada us)
        {
            if (string.IsNullOrWhiteSpace(us.Nombre))
                throw new Exception("Introduzca su nombre");
            if (string.IsNullOrWhiteSpace(us.Correo))
                throw new Exception("Introduzca su correo electrónico");
            if (string.IsNullOrWhiteSpace(us.Marca))
                throw new Exception("Introduzca el nombre de su marca");
            if (string.IsNullOrWhiteSpace(us.Contrasena))
                throw new Exception("Escriba su contraseña");
            if (Context.MarcaAfiliada.Any(x => x.Correo.ToUpper() == us.Correo.ToUpper() && x.Id != us.Id))
                throw new Exception("Este correo ya está vinculado a una marca");
            return true;
        }
    }
}
