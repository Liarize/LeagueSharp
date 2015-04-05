#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

#endregion

namespace Nasus
{
    internal class Program
    {
        public const string CharName = "Nasus";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q, W, E, R;
        public static SpellSlot IgniteSlot;
        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        // Custom vars
        public static bool PacketCast;
        public static bool DebugEnabled;
        // Items
        public static Items.Item Biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != CharName) return;

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R);

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);
            Spells.Add(R);

            IgniteSlot = Player.GetSpellSlot("summonerdot");

            Config = new Menu("Smart Nasus", "nasus", true);

            //Orbwalker Menu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Target Selector Menu
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            //Combo Menu
            Config.AddSubMenu(new Menu("[SN] Combo Settings", "nasus.combo"));
            Config.SubMenu("nasus.combo").AddItem(new MenuItem("combo.useR", "Use R in Combo").SetValue(true));

            //Killsteal
            Config.AddSubMenu(new Menu("[SN] Killsteal Settings", "nasus.killsteal"));
            Config.SubMenu("nasus.killsteal").AddItem(new MenuItem("killsteal.enabled", "Smart KS Enabled").SetValue(true));
            Config.SubMenu("nasus.killsteal").AddItem(new MenuItem("killsteal.useIgnite", "KS with Ignite").SetValue(true));

            //Harass Menu
            Config.AddSubMenu(new Menu("[SN] Harass Settings", "nasus.harass"));
            Config.SubMenu("nasus.harass").AddItem(new MenuItem("harass.useW", "Use E in Harass")).SetValue(true);
            Config.SubMenu("nasus.harass").AddItem(new MenuItem("harass.mana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Farm Menu
            Config.AddSubMenu(new Menu("[SN] Farming Settings", "nasus.farm"));
            Config.SubMenu("nasus.farm").AddItem(new MenuItem("farm.useQ", "Farm with Q").SetValue(true));
            Config.SubMenu("nasus.farm").AddItem(new MenuItem("farm.useE", "Farm with E").SetValue(true));
            Config.SubMenu("nasus.farm").AddItem(new MenuItem("farm.mana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Jungle Clear Menu
            Config.AddSubMenu(new Menu("[SN] Jungle Clear Settings", "nasus.jungle"));
            Config.SubMenu("nasus.jungle").AddItem(new MenuItem("jungle.useQ", "Clear with Q").SetValue(true));
            Config.SubMenu("nasus.jungle").AddItem(new MenuItem("jungle.useE", "Clear with E").SetValue(true));

            //Drawing Menu
            Config.AddSubMenu(new Menu("[SN] Draw Settings", "nasus.drawing"));
            Config.SubMenu("nasus.drawing").AddItem(new MenuItem("drawing.disableAll", "Disable drawing").SetValue(false));
            Config.SubMenu("nasus.drawing").AddItem(new MenuItem("drawing.target", "Highlight Target").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 0))));
            Config.SubMenu("nasus.drawing").AddItem(new MenuItem("drawing.drawW", "Draw W Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("nasus.drawing").AddItem(new MenuItem("drawing.drawE", "Draw E Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            //Misc Menu
            Config.AddSubMenu(new Menu("[SN] Misc Settings", "nasus.misc"));
            Config.SubMenu("nasus.misc").AddItem(new MenuItem("misc.interruptGapclosers", "Interrupt Gapclosers").SetValue(true));
            Config.SubMenu("nasus.misc").AddItem(new MenuItem("misc.usePackets", "Use Packets to Cast Spells").SetValue(true));
            Config.SubMenu("nasus.misc").AddItem(new MenuItem("misc.debug", "Enable debug").SetValue(false));
            Config.SubMenu("nasus.misc").AddItem(new MenuItem("misc.autoR.enabled", "Auto R when HP low").SetValue(true));
            Config.SubMenu("nasus.misc").AddItem(new MenuItem("misc.autoR.percent", "Auto R HP %").SetValue(new Slider(10)));

            //AutoPots menu
            Config.AddSubMenu(new Menu("[SN] AutoPot", "nasus.autopot"));
            Config.SubMenu("nasus.autopot").AddItem(new MenuItem("autopot.enabled", "AutoPot enabled").SetValue(true));
            Config.SubMenu("nasus.autopot").AddItem(new MenuItem("autopot.hp", "Health Pot").SetValue(true));
            Config.SubMenu("nasus.autopot").AddItem(new MenuItem("autopot.mp", "Mana Pot").SetValue(true));
            Config.SubMenu("nasus.autopot").AddItem(new MenuItem("autopot.hp.percent", "Health Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("nasus.autopot").AddItem(new MenuItem("autopot.mp.percent", "Mana Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("nasus.autopot").AddItem(new MenuItem("autopot.ignite", "Auto pot if Ignited / Morde R").SetValue(true));

            //Make menu visible
            Config.AddToMainMenu();
            PacketCast = Config.Item("misc.usePackets").GetValue<bool>();
            DebugEnabled = Config.Item("misc.debug").GetValue<bool>();

            //Damage Drawer
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            //Events set up
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            //Announce that the assembly has been loaded
            Game.PrintChat("<font color=\"#00BFFF\">Smart Nasus -</font> <font color=\"#FFFFFF\">Loaded</font>");
            Game.PrintChat("<font color=\"#00BFFF\">Smart Nasus -</font> <font color=\"#FFFFFF\">test version 1</font>");
            Game.PrintChat("<font color=\"#00BFFF\">Smart Nasus -</font> <font color=\"#FFFFFF\">Thank you for using my scripts, feel free to suggest features and report bugs on the forums.</font>");
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            // Target variable
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            //Main features with Orbwalker
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ComboHandler(target);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    JungleClear();
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
            }

            AutoPot();
            KillSteal();
            Autos();
        }

        private static bool GetConfigBool(string c)
        {
            return Config.Item(c).GetValue<bool>();

        }

        //Auto Functions
        private static void Autos()
        {
            var rPercent = Config.Item("misc.autoR.percent").GetValue<Slider>().Value/100;

            if (!GetConfigBool("misc.autoR.enabled")) return;
            if (R.IsReady() && Player.Health <= rPercent)
                R.Cast(PacketCast);
        }

        //Drawing
        private static void Drawing_OnDraw(EventArgs args)
        {
            //Main drawing switch
            if (Config.Item("drawing.disableAll").GetValue<bool>()) return;

            //Spells drawing
            foreach (var spell in Spells.Where(spell => Config.Item("Draw " + spell.Slot + " Range").GetValue<Circle>().Active))
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, spell.Range, spell.IsReady() ? Color.Green : Color.Red);
            }

            //Target Drawing
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Config.Item("Target").GetValue<Circle>().Active && target != null)
            {
                Render.Circle.DrawCircle(target.Position, 50, Config.Item("Target").GetValue<Circle>().Color);
            }
        }

        //Anti Gapcloser
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!GetConfigBool("misc.interruptGapclosers")) return;
            if (!gapcloser.Sender.IsValidTarget(W.Range)) return;

            W.CastOnUnit(gapcloser.Sender, PacketCast);

            if (DebugEnabled) Game.PrintChat("Debug - W Casted to interrupt GAPCLOSER");
        }

        //Killsteal
        private static void KillSteal()
        {
            if (!GetConfigBool("killsteal.enabled")) return;

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && 
                target.Health < GetQDamage(target))
            {
                Q.Cast(target, PacketCast);
                if (DebugEnabled) Game.PrintChat("Debug - Q casted to KILLSTEAL.");
            }

            if (E.IsReady() &&
                target.Health < E.GetDamage(target) &&
                target.IsValidTarget(E.Range))
            {
                E.Cast(target, PacketCast);
                if (DebugEnabled) Game.PrintChat("Debug - Q casted to KILLSTEAL.");
            }           

            if (IgniteSlot == SpellSlot.Unknown || 
                Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready || 
                target.IsValidTarget(600)) return;
            if ((Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) < target.Health)) return;

            Player.Spellbook.CastSpell(IgniteSlot, target);
            if (DebugEnabled) Game.PrintChat("Debug - Ignite casted to KILLSTEAL.");

        }

        //Auto pot
        private static void AutoPot()
        {
            if (!GetConfigBool("autopot.enabled")) return;

            //Auto Ignite Counter
            if (GetConfigBool("autopot.ignite"))
            {
                if (Player.HasBuff("summonerdot") || Player.HasBuff("MordekaiserChildrenOfTheGrave"))
                {
                    if (!Player.InFountain())
                    {
                        if (Items.HasItem(Biscuit.Id) && Items.CanUseItem(Biscuit.Id) &&
                            !Player.HasBuff("ItemMiniRegenPotion"))
                        {
                            Biscuit.Cast(Player);
                            if (DebugEnabled) Game.PrintChat("Debug - Biscuit used to counter IGNITE.");

                        }
                        else if (Items.HasItem(HPpot.Id) && Items.CanUseItem(HPpot.Id) &&
                                 !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("Health Potion"))
                        {
                            HPpot.Cast(Player);
                            if (DebugEnabled) Game.PrintChat("Debug - HP Pot used to counter IGNITE.");

                        }
                        else if (Items.HasItem(Flask.Id) && Items.CanUseItem(Flask.Id) &&
                                 !Player.HasBuff("ItemCrystalFlask"))
                        {
                            Flask.Cast(Player);
                            if (DebugEnabled) Game.PrintChat("Debug - Flask used to counter IGNITE.");
                        }
                    }
                }
            }

            if (ObjectManager.Player.HasBuff("Recall") || Player.InFountain() && Player.InShop()) return;

            //Health Pots
            if (!GetConfigBool("autopot.hp")) return;
            if (Player.Health/100 <= Config.Item("autopot.hp.percent").GetValue<Slider>().Value &&
                !Player.HasBuff("RegenerationPotion", true))
            {
                Items.UseItem(2003);
                if (DebugEnabled) Game.PrintChat("Debug - HP Pot used because of LOW HP");
            }

            //Mana Pots
            if (!GetConfigBool("autopot.mp")) return;
            if (Player.Mana/100 <= Config.Item("autopot.mp.percent").GetValue<Slider>().Value &&
                !Player.HasBuff("FlaskOfCrystalWater", true))
            {
                Items.UseItem(2004);
                if (DebugEnabled) Game.PrintChat("Debug - MP Pot used because of LOW MP");
            }
        }

        //Combo Handler
        private static void ComboHandler(Obj_AI_Base target)
        {
            if (target == null) return;

            if (GetConfigBool("combo.useR") && R.IsReady() && R.IsInRange(target))
            {
                R.Cast(PacketCast);
            }

            if (!Q.IsInRange(target) && target.MoveSpeed >= Player.MoveSpeed)
            {
                W.CastOnUnit(target, PacketCast);
            }

            if (target.IsValidTarget(Q.Range) && Q.IsReady()) Q.Cast(PacketCast);
           
        }
            

        //Harass
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);            
            var harassMana = Config.Item("harass.Mana").GetValue<Slider>().Value;

            if (Player.Mana < harassMana) return;
            if (target == null) return;

            if (Q.IsReady() && target.IsValidTarget(Q.Range)) Q.CastOnUnit(target);
            if (GetConfigBool("harass.useW") && W.IsReady() && target.IsValidTarget(W.Range))
            {
                W.CastOnUnit(target, PacketCast);
                Q.CastOnUnit(target, PacketCast);
            }

        }

        //Farm
        private static void Farm()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            var mana = Player.MaxMana*(Config.Item("farm.mana").GetValue<Slider>().Value/100.0);
            if (Player.Mana < mana) return;

            if (GetConfigBool("farm.useQ") && Q.IsReady())
            {
                foreach (var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(Q.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int) (Player.Distance(minion.Position))) <=
                            Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    Q.CastOnUnit(minion, PacketCast);
                    return;
                }
            }
            
            if (GetConfigBool("farm.useE") && E.IsReady())
            {
                foreach (var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(E.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position))) <=
                            Player.GetSpellDamage(minion, SpellSlot.E)))
                {
                    E.Cast(minion, PacketCast);
                    return;
                }
            }

            if (GetConfigBool("farm.useW") && W.IsReady())
            {
                foreach (var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(W.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position))) <=
                            Player.GetSpellDamage(minion, SpellSlot.W)))
                {
                    W.CastOnUnit(minion, PacketCast);
                    return;
                }
            }
        }

        // Laneclear
        private static void LaneClear()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            var laneclearMana = Player.MaxMana * (Config.Item("laneclear.mana").GetValue<Slider>().Value / 100.0);
            if (Player.Mana < laneclearMana) return;

            if (GetConfigBool("laneclear.useQ") && Q.IsReady())
            {
                foreach (var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(Q.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position))) >
                            1.33 * Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    Q.CastOnUnit(minion, PacketCast);
                    return;
                }
            }

            if (GetConfigBool("laneclear.useE") && E.IsReady())
            {
                foreach (var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(E.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position))) >
                            1.33 * Player.GetSpellDamage(minion, SpellSlot.E)))
                {
                    E.Cast(minion, PacketCast);
                    return;
                }
            }

            if (GetConfigBool("laneclear.useW") && W.IsReady())
            {
                foreach (var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(W.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion.Position))) >
                            1.33 * Player.GetSpellDamage(minion, SpellSlot.W)))
                {
                    W.CastOnUnit(minion, PacketCast);
                    return;
                }
            }
            
        }

        //Jungleclear
        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(
                Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0) return;

            var mob = mobs[0];
            if (mob == null) return;

            
        }

        //Combo Damage calculating
        private static float ComboDamage(Obj_AI_Base target)
        {
            var dmg = 0d;

            if (Q.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (E.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (R.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.R);
            }

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                dmg += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Smite);
            }

            return (float) dmg;
        }
    }
}