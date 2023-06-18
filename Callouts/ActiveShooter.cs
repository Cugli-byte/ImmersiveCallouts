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
    [CalloutInfo("ActiveShooter", CalloutProbability.Never)]
    class ActiveShooter : Callout
    {

        private Ped suspect;
        private Blip searchArea;
        private Blip suspectBlip;
        private Vector3 spawnpoint;
        private bool blipAttached = false;
        private bool hasBegunShooting = false;
        private bool accepted = false;
        private bool unitResponding = false;

        private Vehicle unit;

        private string[] wepList = new string[] { "WEAPON_PISTOL", "WEAPON_SMG", "WEAPON_MACHINEPISTOL", "WEAPON_PUMPSHOTGUN", "weapon_heavypistol", "weapon_minismg" };

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(800f));
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 30f);
            AddMinimumDistanceCheck(50f, spawnpoint);
            CalloutMessage = "Active Shooter";
            if (new Random().Next(100) < 80)
            {
                CalloutAdvisory = "One unit is already responding";
                unitResponding = true;
            }
            CalloutPosition = spawnpoint;
            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS WE_HAVE CRIME_GUNFIRE IN_OR_ON_POSITION", spawnpoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            accepted = true;

            suspect = new Ped(spawnpoint);
            suspect.IsPersistent = true;
            suspect.Armor = 200;
            suspect.Inventory.GiveNewWeapon(new WeaponAsset(wepList[new Random().Next((int)wepList.Length)]), 500, true);
            suspect.RelationshipGroup = "A";
            //suspect.Tasks.Wander();

            searchArea = new Blip(spawnpoint.Around(60f), 75f);
            searchArea.Color = System.Drawing.Color.Red;
            searchArea.Alpha = 0.6f;
            searchArea.IsRouteEnabled = true;

            suspect.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);
            Game.LocalPlayer.Character.RelationshipGroup.SetRelationshipWith(suspect.RelationshipGroup, Relationship.Hate);

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Active Shooter", "~b~Dispatch: ~w~Respond ~r~Code 3");
            Functions.PlayScannerAudio("UNITS_RESPOND_CODE_99");

            foreach (Ped p in World.GetAllPeds())
            {
                if(p.Exists())
                {
                    if (p != Game.LocalPlayer.Character && p != suspect)
                    {
                        p.RelationshipGroup.SetRelationshipWith(suspect.RelationshipGroup, Relationship.Hate);
                    }
                }
            }

            if (unitResponding)
            {
                unit = Functions.RequestBackup(suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
            }

            //suspect.Tasks.FightAgainstClosestHatedTarget(1000f);

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            GameFiber.StartNew(delegate
            {
                if (!blipAttached)
                {
                    if (Game.LocalPlayer.Character.Position.DistanceTo(suspect) < 75f)
                    {
                        if (searchArea.Exists()) { searchArea.Delete(); }
                        suspectBlip = suspect.AttachBlip();
                        suspectBlip.Color = System.Drawing.Color.Red;
                        blipAttached = true;
                    }
                }

                if (!hasBegunShooting)
                {
                    if (Game.LocalPlayer.Character.Position.DistanceTo(suspect) < 300f)
                    {
                        suspect.Tasks.FightAgainstClosestHatedTarget(9999f);
                    }
                    hasBegunShooting = true;
                }

                if (suspect.IsDead || Functions.IsPedArrested(suspect) || Game.LocalPlayer.Character.IsDead)
                {
                    End();
                }
            }, "Active Shooter");
        }

        public override void End()
        {
            base.End();

            if (suspect.Exists()) { suspect.Dismiss(); }
            if (suspectBlip.Exists()) { suspectBlip.Delete(); }
            if (searchArea.Exists()) { searchArea.Delete(); }
            if (unit.Exists()) { unit.Dismiss(); }

            if (accepted)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Active Shooter", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
                Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
            }
        }

    }
}
