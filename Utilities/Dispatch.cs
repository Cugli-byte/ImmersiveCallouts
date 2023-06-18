using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace ImmersiveCallouts.Utilities
{
    class Dispatch
    {

        private bool codeChanged = false;

        void changeCodeOnSiren(Code code)
        {
            if(!codeChanged)
            {
                string codeString = "";
                switch (code)
                {
                    case Code.TWO:
                        codeString = "~y~Code 2";
                        break;
                    case Code.THREE:
                        codeString = "~r~Code 3";
                        break;
                }
                Game.DisplayNotification("Changing respond code into " + codeString);
                codeChanged = true;
            }
        }

    }

    enum Code
    {
        TWO = 2,
        THREE = 3,
        NINETY_NINE = 99

    }
}
