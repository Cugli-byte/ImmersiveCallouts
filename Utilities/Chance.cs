using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmersiveCallouts.Utilities
{
    class Chance
    {

        public bool Next(double chance)
        {
            return new Random().NextDouble()*100d < chance;
        }

    }
}
