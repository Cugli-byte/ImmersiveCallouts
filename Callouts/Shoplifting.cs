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
    [CalloutInfo("Shoplifting", CalloutProbability.Medium)]
    class Shoplifting : Callout
    {

        private Vector3 spawnpoint;

        private Ped suspect;
        private Ped security;

        private string[] suspectList = new string[] { "a_f_m_downtown_01", "g_f_y_families_01", "a_m_m_mexlabor_01", "a_m_m_salton_02", "g_m_m_chigoon_01" };
        private string[] securityList = new string[] { "ig_fbisuit_01", "cs_fbisuit_01", "s_m_m_highsec_01", "s_m_m_security_01" };

        private int stolenIndex = 0;

        private string[] itemList = new string[] { "Bag of Chips", "Bottle of Water", "T-Shirt", "Bottle of Gin", "Revolver" };
        private float[] priceList = new float[] { 1.99f, 2.50f, 21.99f, 40f, 299.99f };

        private string s_pn; //personal pronoun
        private string s_pn2; //personal pronoun2
        private string s_ppn; //possesive pronoun

        private SimpleDialog mainDialog;
        private SimpleDialog lieDialog;
        private SimpleDialog truthDialog;

        private Blip blip;
        private Blip suspectBlip;
        private Blip securityBlip;

        private bool suspectWillLie = false;
        private bool beenFrisked = false;
        private bool accepted = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnPeds();

            if(suspect.IsMale)
            {
                s_pn = "he";
                s_pn2 = "him";
                s_ppn = "his";
            } else if(suspect.IsFemale)
            {
                s_pn = "she";
                s_pn2 = "her";
                s_ppn = "her";
            }

            stolenIndex = new Random().Next(itemList.Length);

            suspectWillLie = priceList[stolenIndex] > 10f;

            mainDialog = new SimpleDialog(new string[] {"~g~Security: ~w~Hello Officer", "~y~You: ~w~Hi, why did you call?", "~g~Security: ~w~I called because I caught " + s_pn2 + " stealing something"});
            if(new Chance().Next(70d))
            {
                lieDialog = new SimpleDialog(new string[] { "~r~Suspect: ~w~ No I didn't", "~g~Securtiy: ~w~I am pretty sure that " + s_pn + " stole something", "~y~You: ~w~Ok I will frisk " + s_pn2 });
            } else
            {
                lieDialog = new SimpleDialog(new string[] { "~r~Suspect: ~w~Yeah but I only stole a ~b~" + itemList[new Random().Next(2)], "~y~You: ~w~Ok, but I have to frisk you to make sure" });
            }
            truthDialog = new SimpleDialog(new string[] { "~r~Suspect: ~w~Yeah but I only stole a ~b~" + itemList[stolenIndex], "~y~You: ~w~Ok, but I have to frisk you to make sure" });

            CalloutMessage = "Shoplifting";
            CalloutAdvisory = "Security has arrested the suspect";
            CalloutPosition = spawnpoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_DISTURBING_THE_PEACE IN_OR_ON_POSITION", spawnpoint);

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

            security = new Ped(securityList[new Random().Next(securityList.Length)], SpawningLocationList[num].Item3, SpawningLocationList[num].Item4);
            security.IsPersistent = true;

            suspect = new Ped(suspectList[new Random().Next(suspectList.Length)], SpawningLocationList[num].Item5, SpawningLocationList[num].Item6);
            suspect.IsPersistent = true;
            security.Face(suspect);
            suspect.Face(security);
            Functions.SetPedCantBeArrestedByPlayer(security, true);
            //Functions.SetPedCuffedTask(suspect, true);
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();

            if (suspect) { suspect.Delete(); }
            if (security) { security.Delete(); }
        }

        public override bool OnCalloutAccepted()
        {
            accepted = true;

            blip = new Blip(spawnpoint, 15f);
            blip.Color = System.Drawing.Color.Yellow;
            blip.Alpha = 0.6f;
            blip.IsRouteEnabled = true;

            suspectBlip = suspect.AttachBlip();
            suspectBlip.Color = System.Drawing.Color.Red;

            securityBlip = security.AttachBlip();
            securityBlip.Color = System.Drawing.Color.LimeGreen;

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Shoplifting", "~b~Dispatch: ~w~Respond ~y~Code 2");
            Functions.PlayScannerAudio("GP_RESPOND_CODE_2");

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (Game.LocalPlayer.Character.DistanceTo(spawnpoint) < 15f)
            {
                if (!lieDialog.IsDone() && suspectWillLie || !suspectWillLie && !truthDialog.IsDone())
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
                            if (suspectWillLie)
                            {
                                if (!lieDialog.IsDone())
                                {
                                    lieDialog.NextLine();
                                }
                            }
                            else
                            {
                                if (!truthDialog.IsDone())
                                {
                                    truthDialog.NextLine();
                                }
                            }
                        }
                    }
                } else
                {
                    if (!beenFrisked)
                    {
                        Game.DisplayHelp("Press Y to frisk the suspect");
                        if (Game.IsKeyDown(Keys.Y))
                        {
                            Game.DisplayNotification("~w~You found a ~b~" + itemList[stolenIndex] + " ~g~(" + priceList[stolenIndex] + "$) ~w~in " + s_ppn + " pockets");
                            beenFrisked = true;
                        }
                    }
                    else
                    {
                        Game.DisplayHelp("You may arrest the suspect or let " + s_pn2 + " go with a warning by pressing Y");
                        if (Game.IsKeyDown(Keys.Y))
                        {
                            Game.DisplaySubtitle("~y~You: ~w~I am letting you go with a warning this time but next time it could be different and you could go to jail!");
                            End();
                        }
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
            if (security) { security.Dismiss(); }
            if (suspectBlip) { suspectBlip.Delete(); }
            if (securityBlip) { securityBlip.Delete(); }
            if (blip) { blip.Delete(); }

            if (accepted)
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ImmersiveCallouts", "~y~Shoplifting", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
                Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
            }
        }

    }
}
