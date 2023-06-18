using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmersiveCallouts.Callouts
{
    [CalloutInfo("PersonStealingCar", CalloutProbability.Medium)]
    class PersonStealingCar : Callout
    {

        private Ped suspect;
        private Vehicle vehicle;
        private Blip suspectBlip;
        private LHandle pursuit;
        public Vector3 spawnpoint;
        private bool pursuitCreated = false;
        private bool hasBegunAction = false;
        private bool foundVehicle = false;
        private bool accepted = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(500f));
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 30f);
            AddMinimumDistanceCheck(50f, spawnpoint);

            CalloutMessage = "Person trying to steal a Car";
            CalloutPosition = spawnpoint;
            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_CARJACKING IN_OR_ON_POSITION", spawnpoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            accepted = true;

            suspect = new Ped(spawnpoint);
            suspect.IsPersistent = true;
            suspect.BlockPermanentEvents = true;

            //suspectBlip = suspect.AttachBlip();
            suspectBlip = new Blip(spawnpoint, 100f);
            suspectBlip.Color = System.Drawing.Color.Red;
            suspectBlip.Alpha = 0.6f;
            suspectBlip.IsRouteEnabled = true;

            //vehicle = (Vehicle)World.GetClosestEntity(suspect.Position, 30f, GetEntitiesFlags.ConsiderCars);

            pursuitCreated = false;
            hasBegunAction = false;

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Person stealing a Car", "~b~Dispatch: ~w~Respond ~r~Code 3");
            Functions.PlayScannerAudio("GP_RESPOND_CODE_3");

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if(!pursuitCreated)
            {
                if (Game.LocalPlayer.Character.DistanceTo(suspect) <= 200f && !foundVehicle)
                {
                    float cDistance = 200f;
                    foreach (Vehicle v in World.GetAllVehicles())
                    {
                        if (v.DistanceTo(suspect) < 200f && v.IsEmpty && v.IsCar)
                        {
                            if (v.DistanceTo(suspect) < cDistance)
                            {
                                vehicle = v;
                                cDistance = v.DistanceTo(suspect);
                            }
                        }
                    }
                    suspect.Position = vehicle.GetOffsetPositionRight(1f);
                    foundVehicle = true;
                }
                if (Game.LocalPlayer.Character.DistanceTo(suspect)<= 100f && !hasBegunAction)
                {
                    if (vehicle != null)
                    {
                        suspect.Tasks.EnterVehicle(vehicle, -1, EnterVehicleFlags.AllowJacking);
                        suspectBlip.Delete();
                        suspectBlip = suspect.AttachBlip();
                        suspectBlip.Alpha = 1f;
                        suspectBlip.IsRouteEnabled = true;
                    } else
                    {
                        End();
                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Person stealing a Car", "~b~Dispatch: ~w~False alarm.");
                    }
                    hasBegunAction = true;
                }
                if (Game.LocalPlayer.Character.DistanceTo(suspect) <= 10f)
                {
                    pursuit = Functions.CreatePursuit();
                    Functions.AddPedToPursuit(pursuit, suspect);
                    Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                    pursuitCreated = true;
                }
                if (vehicle != null)
                {
                    if (suspect.IsInVehicle(vehicle, false))
                    {
                        pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(pursuit, suspect);
                        Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                        pursuitCreated = true;
                    }
                }
            }

            if (pursuitCreated && !Functions.IsPursuitStillRunning(pursuit) || suspect.IsDead || Functions.IsPedArrested(suspect))
            {
                End();
            }

            if (Game.LocalPlayer.Character.IsDead)
            {
                End();
            }
        }

        public override void End()
        {
            base.End();

            if (suspect.Exists()) { suspect.Dismiss(); }
            if (suspectBlip.Exists()) { suspectBlip.Delete(); }

            if (accepted)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Person stealing a Car", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
                Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
            }
        }

    }
}
