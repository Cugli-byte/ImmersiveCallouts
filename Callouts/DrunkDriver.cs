using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using System.Windows.Forms;
using ImmersiveCallouts.Utilities;

namespace ImmersiveCallouts.Callouts
{
    [CalloutInfo("DrunkDriver", CalloutProbability.High)]
    class DrunkDriver : Callout
    {

        private Ped suspect;
        private Vehicle suspectVehicle;
        private Blip suspectBlip;
        private LHandle pursuit;
        public Vector3 spawnpoint;
        private bool pursuitCreated = false;
        private bool willFlee = false;
        private bool accepted = false;

        private SimpleDialog dialog = new SimpleDialog(new String[] {"~y~You: ~w~Do you know why I stopped you?", "~b~Driver: ~w~No.. sir I don't", "~y~You: ~w~You drove in wavy lines, are you alright?"});
        //private Dispatch dp = new Dispatch();

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(800f));
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 30f);
            AddMinimumDistanceCheck(50f, spawnpoint);
            //AddMaximumDistanceCheck()
            CalloutMessage = "Stolen Vehicle";
            CalloutPosition = spawnpoint;
            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_TRAFFIC_VIOLATION IN_OR_ON_POSITION", spawnpoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            accepted = true;
            
            suspectVehicle = new Vehicle("BALLER", spawnpoint);
            suspectVehicle.IsPersistent = true;
            suspectVehicle.IsStolen = true;

            suspect = new Ped(suspectVehicle.GetOffsetPositionFront(5f));
            suspect.IsPersistent = true;
            suspect.BlockPermanentEvents = true;
            suspect.WarpIntoVehicle(suspectVehicle, -1);
            suspect.Tasks.CruiseWithVehicle(20f);

            AnimationSet drunkAnimset = new AnimationSet("move_m@drunk@verydrunk");
            drunkAnimset.LoadAndWait();

            suspect.MovementAnimationSet = drunkAnimset;
            Rage.Native.NativeFunction.Natives.SET_PED_IS_DRUNK(suspect, true);
            suspect.Tasks.CruiseWithVehicle(suspectVehicle, 12f, VehicleDrivingFlags.FollowTraffic);

            suspectBlip = suspect.AttachBlip();
            suspectBlip.Color = System.Drawing.Color.Yellow;
            suspectBlip.IsRouteEnabled = true;

            pursuitCreated = false;

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~w~Drunk Driver", "~b~Dispatch: ~w~Respond ~y~Code 2");
            Functions.PlayScannerAudio("GP_RESPOND_CODE_2");

            if(new Random().Next(99)+1 <= 30)
            {
                willFlee = true;
            }

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if(!pursuitCreated)
            {
                if(Game.LocalPlayer.Character.DistanceTo(suspect) < 30f && !Functions.IsPlayerPerformingPullover())
                {
                    Game.DisplayHelp("Pull over the suspect");
                }
                if(Functions.IsPlayerPerformingPullover() && Functions.GetPulloverSuspect(Functions.GetCurrentPullover()))
                {
                    if(!willFlee)
                    {
                        if (Game.LocalPlayer.Character.DistanceTo(suspect) < 10f)
                        {
                            if (!dialog.IsDone())
                            {
                                Game.DisplayHelp("Press Y to speak with the suspect");
                            }
                            if (Game.IsKeyDown(Keys.Y))
                            {
                                if(!dialog.IsDone())
                                {
                                    dialog.NextLine();
                                } else
                                {
                                    if(new Random().Next(100) < 30)
                                    {
                                        Functions.ForceEndCurrentPullover();
                                        pursuit = Functions.CreatePursuit();
                                        Functions.AddPedToPursuit(pursuit, suspect);
                                        Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                                        pursuitCreated = true;
                                    } else
                                    {
                                        Game.DisplayHelp("You may arrest the driver now");
                                    }
                                }
                            }
                        }
                    } else
                    {
                        Functions.ForceEndCurrentPullover();
                        pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(pursuit, suspect);
                        Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                        pursuitCreated = true;
                    }
                } else
                {
                    suspect.Tasks.CruiseWithVehicle(20f);
                    if (new Random().Next(100) < 5)
                    {
                        if (new Random().Next(100) < 50)
                        {
                            suspect.Tasks.PerformDrivingManeuver(VehicleManeuver.SwerveRight);
                        } else
                        {
                            suspect.Tasks.PerformDrivingManeuver(VehicleManeuver.SwerveRight);
                        }
                    }
                }
            }

            if (suspect.IsDead || Functions.IsPedArrested(suspect) || (pursuitCreated && !Functions.IsPursuitStillRunning(pursuit)))
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
            if (suspectVehicle.Exists()) { suspectVehicle.Dismiss(); }
            if (suspectBlip.Exists()) { suspectBlip.Delete(); }

            if (accepted)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~w~Drunk Driver", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
                Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
            }
        }

    }
}
