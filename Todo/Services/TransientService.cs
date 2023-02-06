using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Todo.Services
{
    public class TransientService
    {
        public int 次數 = 0;

        public void 次數加一()
        {
            次數 = 次數 + 1;
        }
    }
}
