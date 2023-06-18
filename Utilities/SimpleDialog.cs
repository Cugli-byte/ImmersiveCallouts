using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace ImmersiveCallouts.Utilities
{
    class SimpleDialog
    {

        int n = -1;
        private String[] dialog;

        public SimpleDialog(String[] dialog)
        {
            this.dialog = dialog;
        }

        public void NextLine()
        {
            n++;
            if (n < dialog.Length)
            {
                Game.DisplaySubtitle(dialog[n]);
            }
        }

        public void NextLine(int duration)
        {
            n++;
            if (n < dialog.Length)
            {
                Game.DisplaySubtitle(dialog[n], duration);
            }
        }

        public bool IsDone()
        {
            return n > dialog.Length - 2;
        }

    }
}
