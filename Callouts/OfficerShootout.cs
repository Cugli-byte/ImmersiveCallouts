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
    [CalloutInfo("OfficerShootout", CalloutProbability.Low)]
    class OfficerShootout : Callout
    {

        private Vector3 spawnpoint;

        private Ped suspect;
        private Vehicle suspectVehicle;
        private Blip suspectBlip;

        private Blip blip;

        private Ped cop1;
        private Vehicle copVehicle;
        private Blip cop1Blip;
        
        private bool hasBegunShooting = false;
        private bool accepted = false;

        private string[] wepList = new string[] { "WEAPON_PISTOL", "WEAPON_SMG", "WEAPON_MACHINEPISTOL", "WEAPON_PUMPSHOTGUN", "weapon_heavypistol", "weapon_minismg" };

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(750f));
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 50f);
            AddMinimumDistanceCheck(50f, spawnpoint);

            CalloutMessage = "Officer pressed Panic Button";
            CalloutPosition = spawnpoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", spawnpoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            accepted = true;

            suspectVehicle = new Vehicle("buccaneer", spawnpoint);
            suspectVehicle.IsPersistent = true;
            suspectVehicle.GetDoors()[0].IsFullyOpen = true;
            copVehicle = new Vehicle("police2", World.GetNextPositionOnStreet(suspectVehicle.GetOffsetPositionFront(40f)));
            copVehicle.IsPersistent = true;
            copVehicle.GetDoors()[0].IsFullyOpen = true;
            copVehicle.GetDoors()[1].IsFullyOpen = true;

            copVehicle.Face(suspectVehicle);
            suspectVehicle.Face(copVehicle);

            suspect = new Ped(suspectVehicle.GetOffsetPositionRight(-2f));
            suspect.Inventory.GiveNewWeapon(new WeaponAsset(wepList[new Random().Next((int)wepList.Length)]), 500, true);
            suspect.Armor = 350;
            suspect.IsPersistent = true;
            suspect.BlockPermanentEvents = false;

            suspectBlip = suspect.AttachBlip();
            suspectBlip.Color = System.Drawing.Color.Red;

            cop1 = new Ped("s_m_y_cop_01", copVehicle.GetOffsetPositionRight(-2f), 90f);
            cop1.IsPersistent = true;
            cop1.Inventory.GiveNewWeapon(WeaponHash.CombatPistol, 500, true);
            cop1.Armor = 175;
            cop1Blip = cop1.AttachBlip();
            cop1Blip.Color = System.Drawing.Color.Blue;

            blip = new Blip(suspectVehicle.GetOffsetPositionFront(20f), 50f);
            blip.Color = System.Drawing.Color.Red;
            blip.Alpha = 0.6f;
            blip.IsRouteEnabled = true;

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Officer Shootout", "~b~Dispatch: ~w~Respond ~r~Code 3");
            Functions.PlayScannerAudio("UNITS_RESPOND_CODE_99");

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            GameFiber.StartNew(delegate
            {
                if (!hasBegunShooting)
                {
                    if (Game.LocalPlayer.Character.DistanceTo(suspectVehicle) < 100f)
                    {
                        cop1.Tasks.FightAgainst(suspect);

                        suspect.RelationshipGroup.SetRelationshipWith(cop1.RelationshipGroup, Relationship.Hate);
                        suspect.RelationshipGroup.SetRelationshipWith(Game.LocalPlayer.Character.RelationshipGroup, Relationship.Hate);

                        suspect.Tasks.FightAgainstClosestHatedTarget(1000f);

                        hasBegunShooting = true;
                    }
                }

                if (suspect.IsDead || Functions.IsPedArrested(suspect) || Game.LocalPlayer.Character.IsDead)
                {
                    End();
                }
            }, "Officer Shootout");
        }

        public override void End()
        {
            base.End();

            if (suspect) { suspect.Dismiss(); }
            if (cop1) { cop1.Dismiss(); }
            if (blip) { blip.Delete(); }
            if (suspectBlip) { suspectBlip.Delete(); }
            if (cop1Blip) { cop1Blip.Delete(); }
            if (suspectVehicle) { suspectVehicle.Dismiss(); }
            if (copVehicle) { copVehicle.Dismiss(); }

            if (accepted)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Officer Shootout", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
                Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
            }
        }

    }
}
