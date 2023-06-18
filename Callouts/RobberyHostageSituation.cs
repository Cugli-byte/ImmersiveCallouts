using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using ImmersiveCallouts.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmersiveCallouts.Callouts
{
    [CalloutInfo("RobberyHostageSituation", CalloutProbability.VeryLow)]
    class RobberyHostageSituation : Callout
    {

        private Ped suspect;
        private Ped victim1;
        private Ped victim2;

        private Vehicle unit;

        private string[] victimList = new string[] { "s_m_m_ammucountry", "mp_m_shopkeep_01", "s_f_m_sweatshop_01", "a_m_y_bevhills_01", "g_m_m_chigoon_01" };
        private string[] suspectList = new string[] { "mp_g_m_pros_01", "g_m_m_chicold_01" };
        private string[] wepList = new string[] { "WEAPON_PISTOL", "WEAPON_SMG", "WEAPON_MACHINEPISTOL", "WEAPON_PUMPSHOTGUN", "weapon_heavypistol", "weapon_minismg" };

        private SimpleDialog mainDialog = new SimpleDialog(new string[] { "~r~Suspect: ~w~Don't come any closer or I will shoot those bastards!", "~y~You: ~w~Ok, what do you want?", "~r~Suspect: ~w~Let me flee and I will let them live." });
        private SimpleDialog madDialog = new SimpleDialog(new string[] { "~y~You: ~w~If you surrender now, you will get a reduced setence", "~r~Suspect: ~w~No I won't!"});
        private SimpleDialog surrenderDialog = new SimpleDialog(new string[] { "~y~You: ~w~If you surrender now, you will get a reduced setence", "~r~Suspect: ~w~Ok ok, I will surrender!" });
        private SimpleDialog surrenderOneHostageDialog = new SimpleDialog(new string[] { "~y~You: ~w~If you surrender now, you will get a reduced setence", "~r~Suspect: ~w~I will let one hostage go!" });

        private int dialogOption = 1;

        private bool hasBegunAction = false;
        private bool accepted = false;

        private Blip blip;
        private Blip speakArea;
        private Blip killArea;
        public Vector3 spawnpoint;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnPeds();
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 15f);
            CalloutMessage = "Robbery In Progress";
            CalloutPosition = spawnpoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_ROBBERY IN_OR_ON_POSITION", spawnpoint);

            return base.OnBeforeCalloutDisplayed();
        }

        private void spawnPeds()
        {
            Random random = new Random();
            List<Vector3> list = new List<Vector3>();
            Tuple<Vector3, float, Vector3, float, Vector3, float>[] SpawningLocationList = {
                    Tuple.Create(new Vector3(73.87572f, -1392.849f, 29.37613f),263.9599f, new Vector3(75.91751f, -1391.883f, 29.37615f), 114.3616f,new Vector3(76.16069f, -1389.982f, 29.37615f), 19.93308f),
                    Tuple.Create(new Vector3(427.145f, -806.8593f, 29.49114f),97.54604f,new Vector3(425.0366f, -807.4084f, 29.49113f), 291.1364f,new Vector3(424.7376f, -809.5753f, 29.49224f), 217.1934f),
                    Tuple.Create(new Vector3(-822.653f, -1071.912f, 11.32811f),208.5814f, new Vector3(-820.9958f, -1073.383f, 11.32811f), 43.43138f,new Vector3(-819.3976f, -1072.761f, 11.32906f), 335.4849f),
                    Tuple.Create(new Vector3(1695.573f, 4822.746f, 42.06311f),95.36308f,new Vector3(1693.365f, 4821.716f, 42.06312f), 289.7472f,new Vector3(1693.838f, 4819.305f, 42.0641f), 215.6228f),
                    Tuple.Create(new Vector3(-1102.171f, 2711.92f, 19.10787f),244.3523f,new Vector3(-1100.294f, 2710.429f, 19.10785f), 48.32264f,new Vector3(-1098.755f, 2712.339f, 19.10868f), 338.6742f),
                    Tuple.Create(new Vector3(1197.127f, 2711.719f, 38.22263f),155.001f,new Vector3(1197.668f, 2709.616f, 38.2226f), 13.32392f,new Vector3(1200.074f, 2709.42f, 38.22372f), 318.5346f),
                    Tuple.Create(new Vector3(5.792656f, 6511.06f, 31.87785f),53.38403f, new Vector3(3.368677f, 6512.238f, 31.87785f), 242.7346f,new Vector3(1.847083f, 6510.697f, 31.87863f), 164.7863f),
                    Tuple.Create(new Vector3(372.285f, 326.7229f, 103.5664f),245.4788f, new Vector3(374.295f, 325.8951f, 103.5664f), 71.65144f,new Vector3(374.8215f, 327.8206f, 103.5664f), 67.57134f),
                    Tuple.Create(new Vector3(1164.891f, -321.9834f, 69.20512f),109.593f,new Vector3(1163.107f, -322.1323f, 69.20507f), 279.6259f,new Vector3(1163.093f, -324.2025f, 69.20507f), 279.2135f),
                    Tuple.Create(new Vector3(1133.908f, -981.8345f, 46.41584f),277.1535f,new Vector3(1135.899f, -981.3351f, 46.41584f), 106.3927f,new Vector3(1136.195f, -982.9218f, 46.41584f), 84.65549f),
            };
            for (int i = 0; i < SpawningLocationList.Length; i++)
            {
                list.Add(SpawningLocationList[i].Item1);
            }
            int num = LocationChooser.nearestLocationIndex(list);
            spawnpoint = SpawningLocationList[num].Item1;

            suspect = new Ped(suspectList[new Random().Next(suspectList.Length)], SpawningLocationList[num].Item3, SpawningLocationList[num].Item4);

            victim1 = new Ped(victimList[new Random().Next(victimList.Length)], spawnpoint, SpawningLocationList[num].Item2);
            victim2 = new Ped(victimList[new Random().Next(victimList.Length)], SpawningLocationList[num].Item5, SpawningLocationList[num].Item6);
        }

        public override bool OnCalloutAccepted()
        {
            accepted = true;

            Vector3 searchArea = spawnpoint.Around2D(1f, 2f);
            blip = new Blip(searchArea);
            blip.EnableRoute(System.Drawing.Color.Yellow);
            blip.Color = System.Drawing.Color.Yellow;

            speakArea = new Blip(searchArea, 30f);
            speakArea.Alpha = 0.6f;
            speakArea.Color = System.Drawing.Color.Yellow;

            killArea = new Blip(searchArea, 10f);
            killArea.Color = System.Drawing.Color.Red;

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Robbery In Progress", "~b~Dispatch: ~w~Respond ~r~Code 3");
            Functions.PlayScannerAudio("GP_RESPOND_CODE_3");

            victim1.IsPersistent = true;
            victim1.BlockPermanentEvents = true;
            victim1.Face(suspect);
            victim1.KeepTasks = true;

            victim2.IsPersistent = true;
            victim2.BlockPermanentEvents = true;
            victim2.Face(suspect);
            victim2.KeepTasks = true;

            suspect.IsPersistent = true;
            suspect.BlockPermanentEvents = true;
            suspect.Armor = 200;
            suspect.Inventory.GiveNewWeapon(new WeaponAsset(wepList[new Random().Next((int)wepList.Length)]), 500, true);
            suspect.Face(victim1);
            suspect.KeepTasks = true;

            new RelationshipGroup("A");
            new RelationshipGroup("V");
            victim1.RelationshipGroup = "V";
            victim2.RelationshipGroup = "V";
            suspect.RelationshipGroup = "A";
            Game.LocalPlayer.Character.RelationshipGroup = "V";
            Game.SetRelationshipBetweenRelationshipGroups("A", "V", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups("V", "A", Relationship.Hate);

            suspect.Tasks.AimWeaponAt(victim1, -1);
            victim2.Tasks.PutHandsUp(-1, suspect);
            victim1.Tasks.PutHandsUp(-1, suspect);

            dialogOption = new Random().Next(3);

            unit = Functions.RequestBackup(spawnpoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.AirUnit);

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();

            if (suspect) { suspect.Delete(); }
            if (victim1) { victim1.Delete(); }
            if (victim2) { victim2.Delete(); }
            if (blip) { blip.Delete(); }
            if (speakArea) { speakArea.Delete(); }
            if (killArea) { killArea.Delete(); }
        }

        public override void Process()
        {
            base.Process();

            if (suspect.DistanceTo(Game.LocalPlayer.Character) < 10f)
            {
                if (!hasBegunAction)
                {
                    suspect.Tasks.FightAgainstClosestHatedTarget(1000f);
                    if (hasBegunAction && victim1.IsDead && victim2.IsDead)
                    {
                        suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
                    }
                    hasBegunAction = true;
                }

                if (hasBegunAction && victim1.IsDead && victim2.IsDead)
                {
                    suspect.Tasks.TakeCoverAt(spawnpoint, Game.LocalPlayer.Character.Position, -1, true);
                }

            }
            else if (suspect.DistanceTo(Game.LocalPlayer.Character) < 30f)
            {
                Game.DisplayHelp("Press Y to speak with the suspect");
                
                if(Game.IsKeyDown(Keys.Y))
                {
                    if(!mainDialog.IsDone())
                    {
                        mainDialog.NextLine();
                    } else
                    {
                        if(dialogOption==0)
                        {
                            madDialog.NextLine();
                        } else if(dialogOption==1) {
                            surrenderDialog.NextLine();
                        } else
                        {
                            surrenderOneHostageDialog.NextLine();
                        }
                    }
                }

                if (dialogOption == 0 && madDialog.IsDone())
                {
                    if (!hasBegunAction)
                    {
                        suspect.Tasks.FightAgainstClosestHatedTarget(1000f);
                        if (hasBegunAction && victim1.IsDead && victim2.IsDead)
                        {
                            suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
                        }
                        hasBegunAction = true;
                    }
                }
                else if (dialogOption == 1 && surrenderDialog.IsDone())
                {
                    if (!hasBegunAction)
                    {
                        suspect.Tasks.PutHandsUp(-1, Game.LocalPlayer.Character);
                        //suspect.Tasks.GoStraightToPosition(Game.LocalPlayer.Character.Position, 5f, 90f, 0f, 0);
                        hasBegunAction = true;
                    }
                }
                else if (dialogOption > 1 && surrenderOneHostageDialog.IsDone())
                {
                    if (!hasBegunAction)
                    {
                        //victim2.Tasks.PutHandsUp(-1, Game.LocalPlayer.Character);
                        victim2.Tasks.GoStraightToPosition(Game.LocalPlayer.Character.Position, 5f, 90f, 0f, -1);
                        hasBegunAction = true;
                    }
                }
            }

            if (suspect.IsDead || Functions.IsPedArrested(suspect))
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

            if (suspect) { suspect.Dismiss(); }
            if (victim1) { victim1.Dismiss(); }
            if (victim2) { victim2.Dismiss(); }
            if (blip) { blip.Delete(); }
            if (speakArea) { speakArea.Delete(); }
            if (killArea) { killArea.Delete(); }
            if(unit) { unit.Dismiss(); }

            if (accepted)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Robbery In Progress", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
                Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
            }
        }

    }
}
