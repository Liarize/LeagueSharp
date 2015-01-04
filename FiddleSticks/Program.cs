#region

using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

#endregion

namespace Fiddlesticks
{
    internal class Program
    {
        public const string CharName = "Fiddlesticks";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;
        public static SpellSlot SmiteSlot;
        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q, 575);
            W = new Spell(SpellSlot.W, 575);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 800);

            SetIgniteSlot();
            SetSmiteSlot();

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);
            Spells.Add(R);

            Config = new Menu(CharName, CharName, true);
            
            //Combo Menu
            Config.AddSubMenu(new Menu("Combo Settings", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("comboKey", "Full Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("combo").AddItem(new MenuItem("useQ", "Use Q in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useW", "Use W in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items with Burst").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("beginwithR", "Begin combo with Ultimate").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("dive", "Dive under enemy turrets with Ultimate").SetValue(false));

            if (SmiteSlot != SpellSlot.Unknown)
            {
                Config.SubMenu("combo").AddItem(new MenuItem("autoSmite", "Use Smite on Target if QWE Available").SetValue(true));
            }

            if (IgniteSlot != SpellSlot.Unknown)
            {
                Config.SubMenu("misc").AddItem(new MenuItem("autoIgnite", "Auto ignite when killable").SetValue(true));
            }

            //Harass Menu
            Config.AddSubMenu(new Menu("Harass Settings", "harass"));
            Config.SubMenu("harass").AddItem(new MenuItem("harassKey", "Harass Key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("harass").AddItem(new MenuItem("hMode", "Harass Mode: ").SetValue(new StringList(new[] {"E", "Q+W", "Q+E+W"})));
            
            //Farm Menu
            Config.AddSubMenu(new Menu("Farming Settings", "farm"));
            Config.SubMenu("farm").AddItem(new MenuItem("farmKey", "Farming Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("farm").AddItem(new MenuItem("wFarm", "Farm with W").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("eFarm", "Farm with E").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("farmMana", "Min. Mana Percent: ").SetValue(new Slider(50)));

            //Jungle Clear Menu
            Config.AddSubMenu(new Menu("Jungle Clear Settings", "jungle"));
            Config.SubMenu("jungle").AddItem(new MenuItem("jungleKey", "Jungle Clear Key").SetValue(new KeyBind("V".ToCharArray()[0],KeyBindType.Press)));           
            Config.SubMenu("jungle").AddItem(new MenuItem("wJungle", "Farm with W").SetValue(true));
            Config.SubMenu("jungle").AddItem(new MenuItem("eJungle", "Farm with E").SetValue(true));

            //Drawing Menu
            Config.AddSubMenu(new Menu("Draw Settings", "drawing"));
            Config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "Disable All Range Draws").SetValue(false));
            Config.SubMenu("drawing").AddItem(new MenuItem("Target", "Draw Circle on Target").SetValue(new Circle(true,Color.FromArgb(255, 255, 0, 0))));
            Config.SubMenu("drawing").AddItem(new MenuItem("QDraw", "Draw Q Range").SetValue(new Circle(true,Color.FromArgb(255, 178, 0, 0))));
            Config.SubMenu("drawing").AddItem(new MenuItem("WDraw", "Draw W Range").SetValue(new Circle(false,Color.FromArgb(255, 32, 178, 170))));
            Config.SubMenu("drawing").AddItem(new MenuItem("EDraw", "Draw E Range").SetValue(new Circle(true,Color.FromArgb(255, 128, 0, 128))));

            //Misc Menu
            Config.AddSubMenu(new Menu("Misc Settings", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("stopChannel", "Interrupt Channeling Spells").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("usePackets", "Use Packets to Cast Spells").SetValue(false));

            //Orbwalker Menu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //TargetSelector Menu
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            //Make menu visible
            Config.AddToMainMenu();

            //Damage Drawer
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            //Necessary Stuff
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            
            //Announce that the assembly has been loaded
            Game.PrintChat("<font color=\"#00BFFF\">Fiddlesticks# Beta -</font> <font color=\"#FFFFFF\">Loaded</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Select default target
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            // Keybinds
            var comboKey = Config.Item("comboKey").GetValue<KeyBind>().Active;
            var harassKey = Config.Item("harassKey").GetValue<KeyBind>().Active;
            var farmKey = Config.Item("farmKey").GetValue<KeyBind>().Active;
            var jungleClearKey = Config.Item("jungleKey").GetValue<KeyBind>().Active;

            if (comboKey && target != null) 
            {
                Combo(target);
            }
            else
            {
                if (harassKey && target != null)
                {
                    Harass(target);
                }

                if (farmKey)
                {
                    Farm();
                }

                if (jungleClearKey)
                {
                    JungleClear();
                }
            }

            //W cancel fix for Orbwalker
            if (Player.HasBuff("Drain"))
            {
                Orbwalker.SetMovement(false);
                Orbwalker.SetAttack(false);
            }
            if (!Player.HasBuff("Drain"))
            {
                Orbwalker.SetMovement(true);
                Orbwalker.SetAttack(true);
            }

            //Autoignite
            if (!Config.SubMenu("misc").Item("autoIgnite").GetValue<bool>()) //Check if Auto-Ignite is enabled
            {
                return;
            }

            if (IgniteSlot == SpellSlot.Unknown ||
                Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready) //Check if Ignite is present and ready
            {
                return;
            }

            if (!(Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) >= target.Health)) //Check if target is killable with ignite
            {
                return;
            }

            Player.Spellbook.CastSpell(IgniteSlot, target);
        }

        //Drawing
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("mDraw").GetValue<bool>())
            {
                return;
            }

            foreach (var spell in Spells.Where(spell => Config.Item(spell.Slot + "Draw").GetValue<Circle>().Active))
            {
                Utility.DrawCircle(Player.Position, spell.Range,
                    Config.Item(spell.Slot + "Draw").GetValue<Circle>().Color);
            }

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Config.Item("Target").GetValue<Circle>().Active && target != null)
            {
                Utility.DrawCircle(target.Position, 50, Config.Item("Target").GetValue<Circle>().Color, 1, 50);
            }
        }

        // Interrupter
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.SubMenu("Combo").Item("stopChannel").GetValue<bool>())
            {
                return;
            }

            if (!(Player.Distance(unit) <= Q.Range) || !Q.IsReady())
            {
                return;
            }

            Q.CastOnUnit(unit, Config.Item("usePackets").GetValue<bool>());
        }

        //Combo
        private static void Combo(Obj_AI_Base target)
        {
            if (target == null) // Check if there is a target
            {
                return;
            }
            // Ultimate logic
            if (Config.SubMenu("Combo").Item("beginwithR").GetValue<bool>() && (Config.SubMenu("Combo").Item("useR").GetValue<bool>() && (R.IsReady()) && (Utility.UnderTurret(target, true) == false))) // If target is not under turret -> Cast R
            {
                R.Cast(target.ServerPosition, Config.SubMenu("Misc").Item("usePackets").GetValue<bool>());
            }

            if (Config.SubMenu("Combo").Item("beginwithR").GetValue<bool>() && (Config.SubMenu("Combo").Item("useR").GetValue<bool>() && (R.IsReady()) && (Utility.UnderTurret(target, true)) == false && (Config.Item("dive").GetValue<bool>()))) // If target is under turret and Turret dive is ON -> Dive with R
            {
                R.Cast(target.ServerPosition, Config.SubMenu("Misc").Item("usePackets").GetValue<bool>());
            }

            if (Config.SubMenu("Combo").Item("comboItems").GetValue<bool>())
            {
                UseItems(target);
            }

            if (Config.Item("autoSmite").GetValue<bool>())
            {
                if (SmiteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                {
                    if (Q.IsReady() && W.IsReady() && E.IsReady())
                    {
                        Player.Spellbook.CastSpell(SmiteSlot, target);
                    }
                }
            }
                
            if (Q.IsReady() && (Config.SubMenu("Combo").Item("useQ").GetValue<bool>()))
            {
                Q.CastOnUnit(target, Config.SubMenu("Misc").Item("usePackets").GetValue<bool>());
            }

            if (E.IsReady() && (Config.SubMenu("Combo").Item("useE").GetValue<bool>()))
            {
                E.Cast(target, Config.SubMenu("Misc").Item("usePackets").GetValue<bool>());
            }

            if (W.IsReady() && (Config.SubMenu("Combo").Item("useW").GetValue<bool>()))
            {
                W.CastOnUnit(target, Config.SubMenu("Misc").Item("usePackets").GetValue<bool>());
            }     
        }

        //Harass
        private static void Harass(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            var mana = Player.MaxMana*(Config.Item("harassMana").GetValue<Slider>().Value/100.0); //Check if player has enough mana
            if (!(Player.Mana > mana))
            {
                return;
            }

            var menuItem = Config.Item("hMode").GetValue<StringList>().SelectedIndex; //Select the Harass Mode
            switch (menuItem)
            {
                case 0: //1st mode: E only
                    if (E.IsReady())
                    {
                        E.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
                    }
                    break;
                case 1: //2nd mode: Q and W
                    if (Q.IsReady() && W.IsReady())
                    {
                        Q.Cast(target, Config.Item("usePackets").GetValue<bool>());
                        W.Cast(target, Config.Item("usePackets").GetValue<bool>());
                    }
                    break;
                case 2: //3rd mode: Q, E and W
                    if (Q.IsReady() && W.IsReady() && E.IsReady())
                    {
                        Q.Cast(target, Config.Item("usePackets").GetValue<bool>());
                        E.Cast(target, Config.Item("usePackets").GetValue<bool>());
                        W.Cast(target, Config.Item("usePackets").GetValue<bool>());
                    }
                    break;
            }
        }

        //Farm
        private static void Farm()
        {

            var minions = MinionManager.GetMinions(Player.ServerPosition, W.Range);
            var mana = Player.MaxMana*(Config.Item("farmMana").GetValue<Slider>().Value/100.0);
            if (!(Player.Mana > mana)) //Check if player has enough mana
            {
                return;
            }

            if (Config.Item("eFarm").GetValue<bool>() && E.IsReady())
            {
                // Logic for getting killable minions
                foreach ( 
                var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(E.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion))) <=
                            Player.GetSpellDamage(minion, SpellSlot.E)))
                {
                    E.CastOnUnit(minion, Config.Item("usePackets").GetValue<bool>());
                    return;
                }
            }

            if (Config.Item("wFarm").GetValue<bool>() || !W.IsReady())
            {
                // Logic for getting killable minions, isn't working, idk why
                foreach (
                var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(W.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int) (Player.Distance(minion))) <=
                            Player.GetSpellDamage(minion, SpellSlot.W)))
            {
                W.CastOnUnit(minion, Config.Item("usePackets").GetValue<bool>());
                return;
            }
            }
        }

        //Jungleclear
        private static void JungleClear() 
        {

            var mobs = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count <= 0)
            {
                return;
            }

            var mob = mobs[0]; // Idk what this is for :DD
            if (mob == null)
            {
                return;
            }
            if (Config.Item("eJungle").GetValue<bool>() && E.IsReady())
            {
                E.Cast(mob, Config.Item("usePackets").GetValue<bool>());
            }
            if (Config.Item("wJungle").GetValue<bool>() && W.IsReady())
            {
                W.CastOnUnit(mob, Config.Item("usePackets").GetValue<bool>());
            }

            
        }

        //Combo Damage calculating
        private static float ComboDamage(Obj_AI_Base target)
        {
            var dmg = 0d;

            if (W.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.W, 1);
            }

            if (E.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (R.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.R, 1);
            }

            if (IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                dmg += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }

            if (SmiteSlot  != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
            {
                dmg += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Smite);
            }

            return (float) dmg;
        }

        //Items using 
        public static void UseItems(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            Int16[] targetedItems = { 3128 }; // DFG 

            foreach (var itemId in targetedItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
            {
                Items.UseItem(itemId, target);
            }
        }

        //Get smite type
        public static string SmiteType()
        {
            int[] redSmite = {3715, 3718, 3717, 3716, 3714};
            int[] blueSmite = {3706, 3710, 3709, 3708, 3707};

            return blueSmite.Any(itemId => Items.HasItem(itemId))
                ? "s5_summonersmiteplayerganker"
                : (redSmite.Any(itemId => Items.HasItem(itemId)) ? "s5_summonersmiteduel" : "summonersmite");
        }

        //Setting Smite and Ignite Slots
        public static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, SmiteType(), StringComparison.CurrentCultureIgnoreCase)))
            {
                SmiteSlot = spell.Slot;
                break;
            }
        }

        public static void SetIgniteSlot()
        {
            IgniteSlot = Player.GetSpellSlot("summonerdot");
        }
    }
}