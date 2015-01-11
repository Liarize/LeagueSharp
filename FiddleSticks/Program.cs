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

        // Items
        public static Items.Item biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

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

            //Orbwalker Menu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //TargetSelector Menu
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);
            
            //Combo Menu
            Config.AddSubMenu(new Menu("Combo Settings", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("comboKey", "Full Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("combo").AddItem(new MenuItem("useQ", "Use Q in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useW", "Use W in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items with Burst").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("KillSteal", "Auto KS with Spells").SetValue(true));

            //Harass Menu
            Config.AddSubMenu(new Menu("Harass Settings", "harass"));
            Config.SubMenu("harass").AddItem(new MenuItem("harassKey", "Harass Key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("harass").AddItem(new MenuItem("hMode", "Harass Mode: ").SetValue(new StringList(new[] {"E", "Q+W", "Q+E+W"})));
            
            //Farm Menu
            Config.AddSubMenu(new Menu("Farming Settings", "farm"));
            Config.SubMenu("farm").AddItem(new MenuItem("farmKey", "Farming Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("farm").AddItem(new MenuItem("eFarm", "Farm with E").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("farmMana", "Min. Mana Percent: ").SetValue(new Slider(50)));

            //Jungle Clear Menu
            Config.AddSubMenu(new Menu("Jungle Clear Settings", "jungle"));
            Config.SubMenu("jungle").AddItem(new MenuItem("jungleKey", "Jungle Clear Key").SetValue(new KeyBind("V".ToCharArray()[0],KeyBindType.Press)));           
            Config.SubMenu("jungle").AddItem(new MenuItem("wJungle", "Farm with W").SetValue(true));
            Config.SubMenu("jungle").AddItem(new MenuItem("eJungle", "Farm with E").SetValue(true));

            //Drawing Menu
            Config.AddSubMenu(new Menu("Draw Settings", "drawing"));
            Config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "Disable all drawings").SetValue(false));
            Config.SubMenu("drawing").AddItem(new MenuItem("Target", "Draw Circle on Target").SetValue(new Circle(true,Color.FromArgb(255, 255, 0, 0))));
            Config.SubMenu("drawing").AddItem(new MenuItem("QDraw", "Draw Q Range").SetValue(new Circle(true,Color.FromArgb(255, 178, 0, 0))));
            Config.SubMenu("drawing").AddItem(new MenuItem("WDraw", "Draw W Range").SetValue(new Circle(false,Color.FromArgb(255, 32, 178, 170))));
            Config.SubMenu("drawing").AddItem(new MenuItem("EDraw", "Draw E Range").SetValue(new Circle(true,Color.FromArgb(255, 128, 0, 128))));

            //Misc Menu
            Config.AddSubMenu(new Menu("Misc Settings", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("stopChannel", "Interrupt Channeling Spells").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("gapcloser", "Interrupt Gapclosers").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("usePackets", "Use Packets to Cast Spells").SetValue(false));
            Config.SubMenu("misc").AddItem(new MenuItem("autolvlup", "").SetValue(new StringList(new[] { "W->E->Q", "Q>W>E(2E)" }, 0)));

            //AutoPots menu
            Config.AddSubMenu(new Menu("AutoPot", "AutoPot"));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H", "Health Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_M", "Mana Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H_Per", "Health Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H_Per", "Mana Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_Ign", "Auto pot when ignite").SetValue(true));

            if (SmiteSlot != SpellSlot.Unknown)
            {
                Config.SubMenu("combo").AddItem(new MenuItem("autoSmite", "Use Smite on Target if QWE Available").SetValue(true));
            }

            if (IgniteSlot != SpellSlot.Unknown)
            {
                Config.SubMenu("misc").AddItem(new MenuItem("autoIgnite", "Auto ignite when killable").SetValue(true));
            }    

            //Make menu visible
            Config.AddToMainMenu();

            //Damage Drawer
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            //Necessary Stuff
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            
            //Announce that the assembly has been loaded
            Game.PrintChat("<font color=\"#00BFFF\">Fiddlesticks# -</font> <font color=\"#FFFFFF\">Loaded</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Select default target
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            // Keybinds
            var comboKey = Config.Item("comboKey").GetValue<KeyBind>().Active;
            var harassKey = Config.Item("harassKey").GetValue<KeyBind>().Active;
            var farmKey = Config.Item("farmKey").GetValue<KeyBind>().Active;
            var jungleClearKey = Config.Item("jungleKey").GetValue<KeyBind>().Active;

            // AutoPot
            if (Config.SubMenu("Misc").Item("AutoPot").GetValue<bool>())
            {
                AutoPot();
            }

            if (Config.SubMenu("misc").Item("autoIgnite").GetValue<bool>() && target  != null)
            {
                Autoignite(target);
            }

            // Main Features
            if (comboKey && target != null) 
            {
                Combo(target);
                KillSteal();
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
                //Evade.Enabled(false);
                Orbwalker.SetMovement(false);
                Orbwalker.SetAttack(false);
            }
            if (!Player.HasBuff("Drain"))
            {
                //Evade.Enabled(true);
                Orbwalker.SetMovement(true);
                Orbwalker.SetAttack(true);
            }
            
        }

        //Drawing
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("mDraw").GetValue<bool>())
            {
                return;
            }

            //Killability
            /*
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead))
            {
                if (enemy.IsVisible)
                {
                    var dmg = ComboDamage(enemy);
                    if (enemy.Health < dmg)
                    {
                        Render.Text text = new Render.Text("Killable", 28, SharpDX.Color.Blue);
                    }
                    else if (enemy.Health <= dmg)
                    {
                        Render.Text text = new Render.Text(new Vector2(0, 0), "Almost Killable", 28, SharpDX.Color.Orange);
                    }
                    else if (enemy.Health > dmg)
                    {
                        Render.Text text = new Render.Text(new Vector2(0, 0), "Not Killable", 28, SharpDX.Color.Red);
                    }
                }
            }
             */
            foreach (var spell in Spells.Where(spell => Config.Item(spell.Slot + "Draw").GetValue<Circle>().Active))
            {
                Render.Circle.DrawCircle(Player.Position, spell.Range, Config.Item(spell.Slot + "Draw").GetValue<Circle>().Color);
            }

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Config.Item("Target").GetValue<Circle>().Active && target != null)
            {
                Render.Circle.DrawCircle(target.Position, 50, Config.Item("Target").GetValue<Circle>().Color);
            }
        }

        // Auto Ignite
        private static void Autoignite(Obj_AI_Base target)
        {
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

        // Anti Gapcloser
       private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Config.Item("gapcloser").GetValue<bool>())
            {
                return;
            }
           
            if (gapcloser.Sender.IsValidTarget(Q.Range))
            {
                Q.CastOnUnit(gapcloser.Sender, Config.Item("usePackets").GetValue<bool>());
            }
        }

        // Interrupter
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.SubMenu("Combo").Item("stopChannel").GetValue<bool>())
            {
                return;
            }

            if ((Player.Distance(unit.Position) <= Q.Range) && Q.IsReady())
            {
                Q.CastOnUnit(unit, Config.Item("usePackets").GetValue<bool>());
            }           
        }

        // Killsteal
        private static void KillSteal()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead))
            {
                if (enemy.IsValidTarget(Q.Range) && enemy.IsVisible)
                {
                    var eDmg = E.GetDamage(enemy);
                    var wDmg = W.GetDamage(enemy);

                    if (enemy.Health <= eDmg)
                    {
                        E.Cast(enemy);
                    }
                    else if (enemy.Health <= wDmg)
                    {
                        W.Cast(enemy);
                    }
                    else if (enemy.Health <= eDmg + wDmg)
                    {
                        E.Cast(enemy);
                        W.Cast(enemy);
                    }
                }
            }
        }

        // Auto HP pot
        private static void AutoPot()
        {
            if (Config.SubMenu("AutoPot").Item("AP_Ign").GetValue<bool>())
            if (Player.HasBuff("summonerdot") || Player.HasBuff("MordekaiserChildrenOfTheGrave"))
            {
                if (!Player.InFountain())

                    if (Items.HasItem(biscuit.Id) && Items.CanUseItem(biscuit.Id) && !Player.HasBuff("ItemMiniRegenPotion"))
                    {
                        biscuit.Cast(Player);
                    }
                    else if (Items.HasItem(HPpot.Id) && Items.CanUseItem(HPpot.Id) && !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("Health Potion"))
                    {
                        HPpot.Cast(Player);
                    }
                    else if (Items.HasItem(Flask.Id) && Items.CanUseItem(Flask.Id) && !Player.HasBuff("ItemCrystalFlask"))
                    {
                        Flask.Cast(Player);
                    }
            }

            if (ObjectManager.Player.HasBuff("Recall") || Player.InFountain() && Player.InShop())
            {
                return;
            }

            //Health Pots
            if (Player.Health/100 <= Config.Item("AP_H_Per").GetValue<Slider>().Value && !Player.HasBuff("RegenerationPotion", true))
            {
               Items.UseItem(2003);
            }
            //Mana Pots
            if (Player.Health/100 <= Config.Item("A_M_Per").GetValue<Slider>().Value && !Player.HasBuff("FlaskOfCrystalWater", true))
            {
                Items.UseItem(2004);
            }
        }


        //Combo
        private static void Combo(Obj_AI_Base target)
        {
            if (target == null) // Check if there is a target
            {
                return;
            }

            if (R.IsReady() && Config.Item("useR").GetValue<bool>())
            {
                R.Cast(target.ServerPosition, Config.SubMenu("Misc").Item("usePackets").GetValue<bool>());
            }
 
            if (Q.IsReady() && (Config.SubMenu("combo").Item("useQ").GetValue<bool>()))
            {
                Q.CastOnUnit(target, Config.SubMenu("misc").Item("usePackets").GetValue<bool>());
            }
            
            if (E.IsReady() && (Config.SubMenu("combo").Item("useE").GetValue<bool>()))
            {
                E.CastOnUnit(target, Config.SubMenu("misc").Item("usePackets").GetValue<bool>());
            }

            if (W.IsReady() && (Config.SubMenu("combo").Item("useW").GetValue<bool>()))
            {
                W.CastOnUnit(target, Config.SubMenu("misc").Item("usePackets").GetValue<bool>());
            }

            /*if (Config.SubMenu("Combo").Item("comboItems").GetValue<bool>())
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
            }*/
        }
     
        //Harass
        private static void Harass(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            var mana = Player.MaxMana*(Config.Item("harassMana").GetValue<Slider>().Value/100.0); //Check if player has enough mana
            if ((Player.Mana > mana))
            {

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
                        Q.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
                        W.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
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
                            HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position))) <=
                            Player.GetSpellDamage(minion, SpellSlot.E)))
                {
                    E.CastOnUnit(minion, Config.Item("usePackets").GetValue<bool>());
                    return;
                }
            }

            
        }

        //Jungleclear
        private static void JungleClear() 
        {
            // Get mobs in range, try to order them by max health to get the big ones
            var mobs = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count <= 0)
            {
                return;
            }

            var mob = mobs[0];
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