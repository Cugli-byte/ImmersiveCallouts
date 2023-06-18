using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImmersiveCallouts.Utilities;
using System.Windows.Forms;

namespace ImmersiveCallouts.Callouts
{
    [CalloutInfo("WantedSightingCallout", CalloutProbability.Medium)]
    class WantedSightingCallout : Callout
    {

        private Ped suspect;
        private Vehicle suspectVehicle;
        private Blip suspectBlip;

        private LHandle pursuit;

        public Vector3 spawnpoint;

        private int wantedIndex = 0;

        private string[] reasonList = new string[] { "Murder", "Battery", "DUI", "Tax Fraud", "Theft", "Grand Theft Auto", "Insurance Fraud" };
        private RiskLevel[] riskList = new RiskLevel[] {RiskLevel.VERY_HIGH, RiskLevel.HIGH, RiskLevel.MID, RiskLevel.LOW, RiskLevel.MID, RiskLevel.HIGH, RiskLevel.LOW };
        private double[] fleeChance = new double[] { 90d, 80d, 60d, 20d, 50d, 70d, 20d };

        private SimpleDialog mainDialog = new SimpleDialog(new string[] { "~y~You: ~w~ Hi, do you know why I've stopped you today?", "~r~Suspect: ~w~No, I have no idea", "~y~You: ~w~I've got a warrant for your arrest" });
        private SimpleDialog secondDialog;

        private bool pursuitCreated = false;
        private bool fleeingTried = false;
        private bool hasVehicle = false;
        private bool accepted = false;


        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(600f));
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 15f);
            AddMinimumDistanceCheck(50f, spawnpoint);

            wantedIndex = new Random().Next(reasonList.Length);

            CalloutMessage = "Wanted Suspect sighted";
            CalloutAdvisory = "Suspect is wanted for " + reasonList[wantedIndex];
            CalloutPosition = spawnpoint;

            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS SUSPECT_LAST_SEEN IN_OR_ON_POSITION", spawnpoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            accepted = true;

            suspect = new Ped(spawnpoint);
            suspect.IsPersistent = true;

            if(new Chance().Next(30d))
            {
                hasVehicle = true;
                suspectVehicle = new Vehicle("Baller", spawnpoint);
                suspect.IsPersistent = true;

                suspect.WarpIntoVehicle(suspectVehicle, -1);
                suspect.Tasks.CruiseWithVehicle(15f, VehicleDrivingFlags.FollowTraffic);

                secondDialog = new SimpleDialog(new string[] { "~r~Suspect: ~w~Really? For what?", "~y~You: ~w~You are wanted for ~r~" + reasonList[wantedIndex], "~y~You: ~w~Exit the vehicle and put your hands up" });
            } else
            {
                suspect.Tasks.Wander();

                secondDialog = new SimpleDialog(new string[] { "~r~Suspect: ~w~Really? For what?", "~y~You: ~w~You are wanted for ~r~" + reasonList[wantedIndex], "~y~You: ~w~Put your hands up and do not resist!" });
            }

            if(hasVehicle || riskList[wantedIndex] >= RiskLevel.HIGH)
            {
                suspectBlip = suspect.AttachBlip();
                suspectBlip.IsRouteEnabled = true;
                suspectBlip.Color = System.Drawing.Color.Red;
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~w~Wanted Suspect", "~b~Dispatch: ~w~Respond ~r~Code 3");
                Functions.PlayScannerAudio("GP_RESPOND_CODE_3");
            } else
            {
                suspectBlip = suspect.AttachBlip();
                suspectBlip.IsRouteEnabled = true;
                suspectBlip.Color = System.Drawing.Color.Yellow;
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~w~Wanted Suspect", "~b~Dispatch: ~w~Respond ~y~Code 2");
                Functions.PlayScannerAudio("GP_RESPOND_CODE_2");
            }

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~w~Wanted Suspect", "~b~Dispatch: ~w~Try to stop and arrest the suspect.");

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (hasVehicle)
            {
                if (!pursuitCreated)
                {
                    if (!Functions.IsPlayerPerformingPullover() && Game.LocalPlayer.Character.DistanceTo(suspect) < 60f)
                    {
                        Game.DisplayHelp("Pull over and speak with the suspect");
                    }
                    if (Functions.IsPlayerPerformingPullover() && riskList[wantedIndex] == RiskLevel.VERY_HIGH && new Chance().Next(50d))
                    {
                        Functions.ForceEndCurrentPullover();
                        pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(pursuit, suspect);
                        Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                        pursuitCreated = true;
                    }
                    if (Functions.IsPlayerPerformingPullover() && Game.LocalPlayer.Character.DistanceTo(suspect) < 5f)
                    {
                        if (!secondDialog.IsDone())
                        {
                            Game.DisplayHelp("Press Y to speak with the suspect");
                        }
                        if(Game.IsKeyDown(Keys.Y))
                        {
                            if(!mainDialog.IsDone())
                            {
                                mainDialog.NextLine();
                            } else
                            {
                                if(!fleeingTried)
                                {
                                    if(new Chance().Next(fleeChance[wantedIndex]))
                                    {
                                        Functions.ForceEndCurrentPullover();
                                        pursuit = Functions.CreatePursuit();
                                        Functions.AddPedToPursuit(pursuit, suspect);
                                        Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                                        pursuitCreated = true;
                                    }
                                    fleeingTried = true;
                                }
                                if(!secondDialog.IsDone())
                                {
                                    secondDialog.NextLine();
                                } else
                                {
                                    Game.DisplayHelp("Arrest the suspect");
                                    if (suspect.IsInVehicle(suspectVehicle, false))
                                    {
                                        suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                                    }
                                    suspect.Tasks.PutHandsUp(-1, Game.LocalPlayer.Character);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (!pursuitCreated)
                {
                    if(!Functions.IsPedStoppedByPlayer(suspect) && Game.LocalPlayer.Character.DistanceTo(suspect) < 60f)
                    {
                        Game.DisplayHelp("Stop and speak with the suspect");
                    }
                    if (Functions.IsPedStoppedByPlayer(suspect) && riskList[wantedIndex] == RiskLevel.VERY_HIGH && new Chance().Next(50d))
                    {
                        Functions.SetPedAsStopped(suspect, false);
                        pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(pursuit, suspect);
                        Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                        pursuitCreated = true;
                    }
                    if (Functions.IsPedStoppedByPlayer(suspect) && Game.LocalPlayer.Character.DistanceTo(suspect) < 5f)
                    {
                        Game.DisplayHelp("Press Y to speak with the suspect");
                        if (Game.IsKeyDown(Keys.Y))
                        {
                            if (!mainDialog.IsDone())
                            {
                                mainDialog.NextLine();
                            }
                            else
                            {
                                if (!fleeingTried)
                                {
                                    if (new Chance().Next(fleeChance[wantedIndex]))
                                    {
                                        Functions.SetPedAsStopped(suspect, false);
                                        pursuit = Functions.CreatePursuit();
                                        Functions.AddPedToPursuit(pursuit, suspect);
                                        Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                                        pursuitCreated = true;
                                    }
                                    fleeingTried = true;
                                }
                                if (!secondDialog.IsDone())
                                {
                                    secondDialog.NextLine();
                                }
                                else
                                {
                                    Game.DisplayHelp("Arrest the suspect");
                                    suspect.Tasks.PutHandsUp(-1, Game.LocalPlayer.Character);
                                }
                            }
                        }
                    }
                }
            }

            if(pursuitCreated && !Functions.IsPursuitStillRunning(pursuit) || Functions.IsPedArrested(suspect) || suspect.IsDead || Game.LocalPlayer.Character.IsDead)
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
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~w~Wanted Suspect", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
                Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
            }
        }

    }
}
