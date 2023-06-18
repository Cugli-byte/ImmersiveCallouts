using System;
using System.Collections.Generic;
using System.Text;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;

namespace ImmersiveCallouts.Callouts
{

    [CalloutInfo("StolenVehiclePursuit", CalloutProbability.Medium)]

    class StolenVehiclePursuit : Callout
    {

        private Ped suspect;
        private Vehicle suspectVehicle;
        private Blip suspectBlip;
        private LHandle pursuit;
        public Vector3 spawnpoint;
        private bool pursuitCreated = false;
        private bool accepted = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(800f));
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 30f);
            AddMinimumDistanceCheck(50f, spawnpoint);
            //AddMaximumDistanceCheck()
            CalloutMessage = "Stolen Vehicle";
            CalloutPosition = spawnpoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE GP_STOLEN_VEHICLE IN_OR_ON_POSITION", spawnpoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            accepted = true;

            suspectVehicle = new Vehicle("BALLER",spawnpoint);
            suspectVehicle.IsPersistent = true;
            suspectVehicle.IsStolen = true;

            suspect = new Ped(suspectVehicle.GetOffsetPositionFront(5f));
            suspect.IsPersistent = true;
            suspect.BlockPermanentEvents = true;
            suspect.WarpIntoVehicle(suspectVehicle, -1);
            suspect.Tasks.CruiseWithVehicle(20f);

            suspectBlip = suspect.AttachBlip();
            suspectBlip.Color = System.Drawing.Color.Yellow;
            suspectBlip.IsRouteEnabled = true;

            pursuitCreated = false;

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Stolen Vehicle", "~b~Dispatch: ~w~Respond ~y~Code 2");
            Functions.PlayScannerAudio("GP_RESPOND_CODE_2");

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!pursuitCreated) {
                if (Game.LocalPlayer.Character.DistanceTo(suspectVehicle) <= 20f || (Game.LocalPlayer.Character.CurrentVehicle.IsSirenOn && Game.LocalPlayer.Character.DistanceTo(suspectVehicle) <= 100f))
                {
                    pursuit = Functions.CreatePursuit();
                    Functions.AddPedToPursuit(pursuit, suspect);
                    Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                    pursuitCreated = true;
                }
            }

            if(pursuitCreated && !Functions.IsPursuitStillRunning(pursuit))
            {
                End();
            }
        }

        public override void End()
        {
            base.End();

            if (suspect.Exists()) { suspect.Dismiss(); }
            if (suspectVehicle.Exists()) { suspectVehicle.Dismiss(); }
            if (suspectBlip.Exists()) { suspectBlip.Delete(); }

            if (accepted)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Stolen Vehicle", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
                Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
            }
        }

    }
}
