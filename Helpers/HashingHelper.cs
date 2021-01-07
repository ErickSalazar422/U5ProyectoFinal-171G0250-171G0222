using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RopaMexicana_171G0250_171G0222.Helpers
{
    public class HashingHelper
    {
        public static string GetHash(string cadena)
        {
            var alg = SHA256.Create();
            byte[] codificar = Encoding.UTF8.GetBytes(cadena);
            byte[] hash = alg.ComputeHash(codificar);
            string c = "";
            foreach (var item in hash)
            {
                c += item.ToString("x2");
            }
            return c;
        }
    }
}
