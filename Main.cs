using System;
using System.IO;
using Rage;
using LSPD_First_Response.Mod.API;

namespace ImmersiveCallouts
{

    public class Main:Plugin
    {

        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnDutyStateChanged;

            Game.LogTrivial("ImmersiveCallouts by Cugli has been initialized.");
        }

        public override void Finally()
        {

        }

        private void OnDutyStateChanged(bool onDuty)
        {
            if(onDuty)
            {
                RegisterCallouts();
                Game.DisplayNotification("Immersive Callouts by Cugli has been loaded.");
            }
        }

        private void RegisterCallouts()
        {
            Functions.RegisterCallout(typeof(Callouts.StolenVehiclePursuit));
            Functions.RegisterCallout(typeof(Callouts.RobberyHostageSituation));
            Functions.RegisterCallout(typeof(Callouts.PersonStealingCar));
            Functions.RegisterCallout(typeof(Callouts.DrunkDriver));
            Functions.RegisterCallout(typeof(Callouts.ActiveShooter));
            Functions.RegisterCallout(typeof(Callouts.OfficerShootout));
            Functions.RegisterCallout(typeof(Callouts.Shoplifting));
            Functions.RegisterCallout(typeof(Callouts.WantedSightingCallout));
        }
    }
}
