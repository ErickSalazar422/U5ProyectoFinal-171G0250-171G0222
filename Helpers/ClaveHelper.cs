using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RopaMexicana_171G0250_171G0222.Helpers
{
    public class ClaveHelper
    {

        public static int ClaveActivacion()
        {
            Random r = new Random();
            int ClaveGenerada = r.Next(1000, 10000);
            return (ClaveGenerada);
        }
    }
}
