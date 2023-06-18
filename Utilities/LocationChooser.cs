using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace ImmersiveCallouts.Utilities
{
    public static class LocationChooser
    {
        public static Vector3 chooseNearestLocation(List<Vector3> list)
        {
            Vector3 closestLocation = list[0];
            float closestDistance = Vector3.Distance(Game.LocalPlayer.Character.Position, list[0]);
            for (int i = 1; i < list.Count; i++)
            {
                if (Vector3.Distance(Game.LocalPlayer.Character.Position, list[i]) <= closestDistance)
                {
                    closestDistance = Vector3.Distance(Game.LocalPlayer.Character.Position, list[i]);
                    closestLocation = list[i];
                }
            }
            return closestLocation;
        }
        public static int nearestLocationIndex(List<Vector3> list)
        {
            int closestLocationIndex = 0;
            float closestDistance = Vector3.Distance(Game.LocalPlayer.Character.Position, list[0]);
            for (int i = 1; i < list.Count; i++)
            {
                if (Vector3.Distance(Game.LocalPlayer.Character.Position, list[i]) <= closestDistance)
                {
                    closestDistance = Vector3.Distance(Game.LocalPlayer.Character.Position, list[i]);
                    closestLocationIndex = i;
                }
            }
            return closestLocationIndex;
        }
    }
}
