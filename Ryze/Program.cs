#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

#endregion

namespace Ryze
{
    internal class Program
    {
        public const string CharName = "Ryze";
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
        public static Items.Item SEmbrace = new Items.Item(3048);
        

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != CharName) return;

            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R);

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);
            Spells.Add(R);

            IgniteSlot = Player.GetSpellSlot("summonerdot");

            Config = new Menu("Smart Ryze", "ryze", true);

            //Orbwalker Menu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Target Selector Menu
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            //Combo Menu
            Config.AddSubMenu(new Menu("[SR] Combo Settings", "ryze.combo"));
            Config.SubMenu("ryze.combo").AddItem(new MenuItem("combo.mode", "Combo Mode").SetValue(new StringList(new[] { "Burst", "Smart" })));
            Config.SubMenu("ryze.combo").AddItem(new MenuItem("combo.useR", "Use R in Combo").SetValue(true));

            //Killsteal
            Config.AddSubMenu(new Menu("[SR] Killsteal Settings", "ryze.killsteal"));
            Config.SubMenu("ryze.killsteal").AddItem(new MenuItem("killsteal.enabled", "Smart KS Enabled").SetValue(true));
            Config.SubMenu("ryze.killsteal").AddItem(new MenuItem("killsteal.useIgnite", "KS with Ignite").SetValue(true));

            //Harass Menu
            Config.AddSubMenu(new Menu("[SR] Harass Settings", "ryze.harass"));
            Config.SubMenu("ryze.harass").AddItem(new MenuItem("harass.enabledPress", "Press Harass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("ryze.harass").AddItem(new MenuItem("harass.enabledToggle", "Toggle Harass").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("ryze.harass").AddItem(new MenuItem("harass.mana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Farm Menu
            Config.AddSubMenu(new Menu("[SR] Farming Settings", "ryze.farm"));
            Config.SubMenu("ryze.farm").AddItem(new MenuItem("farm.useQ", "Farm with Q").SetValue(true));
            Config.SubMenu("ryze.farm").AddItem(new MenuItem("farm.useW", "Farm with W").SetValue(true));
            Config.SubMenu("ryze.farm").AddItem(new MenuItem("farm.useE", "Farm with E").SetValue(true));
            Config.SubMenu("ryze.farm").AddItem(new MenuItem("farm.mana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Jungle Clear Menu
            Config.AddSubMenu(new Menu("[SR] Jungle Clear Settings", "ryze.jungle"));
            Config.SubMenu("ryze.jungle").AddItem(new MenuItem("jungle.useQ", "Clear with Q").SetValue(true));
            Config.SubMenu("ryze.jungle").AddItem(new MenuItem("jungle.useW", "Clear with W").SetValue(true));
            Config.SubMenu("ryze.jungle").AddItem(new MenuItem("jungle.useE", "Clear with E").SetValue(true));

            //Drawing Menu
            Config.AddSubMenu(new Menu("[SR] Draw Settings", "ryze.drawing"));
            Config.SubMenu("ryze.drawing").AddItem(new MenuItem("drawing.disableAll", "Disable drawing").SetValue(false));
            Config.SubMenu("ryze.drawing").AddItem(new MenuItem("drawing.target", "Highlight Target").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 0))));
            Config.SubMenu("ryze.drawing").AddItem(new MenuItem("drawing.drawQ", "Draw Q Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("ryze.drawing").AddItem(new MenuItem("drawing.drawW", "Draw W Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            //Misc Menu
            Config.AddSubMenu(new Menu("[SR] Misc Settings", "ryze.misc"));
            Config.SubMenu("ryze.misc").AddItem(new MenuItem("misc.interruptGapclosers", "Interrupt Gapclosers").SetValue(true));
            Config.SubMenu("ryze.misc").AddItem(new MenuItem("misc.usePackets", "Use Packets to Cast Spells").SetValue(true));
            Config.SubMenu("ryze.misc").AddItem(new MenuItem("misc.debug", "Enable debug").SetValue(false));
            Config.SubMenu("ryze.misc").AddItem(new MenuItem("misc.autoSEmbrace.enabled", "Auto Serapths Embrace").SetValue(true));
            Config.SubMenu("ryze.misc").AddItem(new MenuItem("misc.autoSEmbrace.percent", "SEmbrace HP %").SetValue(new Slider(10)));

            //AutoPots menu
            Config.AddSubMenu(new Menu("[SR] AutoPot", "ryze.autopot"));
            Config.SubMenu("ryze.autopot").AddItem(new MenuItem("autopot.enabled", "AutoPot enabled").SetValue(true));
            Config.SubMenu("ryze.autopot").AddItem(new MenuItem("autopot.hp", "Health Pot").SetValue(true));
            Config.SubMenu("ryze.autopot").AddItem(new MenuItem("autopot.mp", "Mana Pot").SetValue(true));
            Config.SubMenu("ryze.autopot").AddItem(new MenuItem("autopot.hp.percent", "Health Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("ryze.autopot").AddItem(new MenuItem("autopot.mp.percent", "Mana Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("ryze.autopot").AddItem(new MenuItem("autopot.ignite", "Auto pot when ignited").SetValue(true));

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
            Game.PrintChat("<font color=\"#00BFFF\">Smart Ryze -</font> <font color=\"#FFFFFF\">Loaded</font>");
            Game.PrintChat("<font color=\"#00BFFF\">Smart Ryze -</font> <font color=\"#FFFFFF\">test version 2</font>");
            Game.PrintChat("<font color=\"#00BFFF\">Smart Ryze -</font> <font color=\"#FFFFFF\">Thank you for using my scripts, feel free to suggest features and report bugs on the forums.</font>");
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            // Select default target
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var harassKey = Config.Item("harass.enabledToggle").GetValue<KeyBind>().Active || Config.Item("harass.enabledPress").GetValue<KeyBind>().Active;

            //Main features with Orbwalker
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ComboHandler(target);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    JungleClear();
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
            }

            if (harassKey) Harass(target);
            AutoPot();
            KillSteal();
            Autos();
        }

        private static bool GetConfigBool(string c)
        {
            return Config.Item(c).GetValue<bool>();

        }

        //Auto SEmbrace
        private static void Autos()
        {
            var SEmbracePercent = (Config.Item("misc.autoSEmbrace.percent").GetValue<Slider>().Value/100;

            if (!GetConfigBool("misc.autoSEmbrace.enabled")) return;
            if (!Items.HasItem(SEmbrace.Id)) return;
            if (Player.HasBuff("Crowstorm"))
            {
                if (Player.HealthPercent < SEmbracePercent)
                {
                    Items.UseItem(SEmbrace.Id);
                }
            }

            var RPercent = Config.Item("misc.autoR.percent").GetValue<Slider>().Value/100;
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
                target.Health < Q.GetDamage(target) && 
                Player.Distance(target) <= Q.Range + target.BoundingRadius)
            {
                Q.Cast(target, PacketCast);
                if (DebugEnabled) Game.PrintChat("Debug - Q casted to KILLSTEAL.");
            }

            if (E.IsReady() &&
                target.Health < E.GetDamage(target) &&
                Player.Distance(target) <= E.Range + target.BoundingRadius)
            {
                E.Cast(target, PacketCast);
                if (DebugEnabled) Game.PrintChat("Debug - Q casted to KILLSTEAL.");
            }

            if (W.IsReady() &&
                target.Health < W.GetDamage(target) &&
                Player.Distance(target) <= W.Range + target.BoundingRadius)
            {
                W.Cast(target, PacketCast);
                if (DebugEnabled) Game.PrintChat("Debug - Q casted to KILLSTEAL.");
            }

            

            if (IgniteSlot == SpellSlot.Unknown || 
                Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready || 
                (Player.Distance(target) > 600)) return;

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

            if (GetConfigBool("combo.useR") && R.IsReady())
            {
                if (target.MoveSpeed > Player.MoveSpeed && target.Distance(Player) < 1200) R.Cast(PacketCast);                   
            }
                

            var comboMode = Config.Item("combo.mode").GetValue<StringList>().SelectedIndex;

            switch (comboMode)
            {
                case 0:
                    {
                        Combo_Burst(target);
                        break;
                    }
                case 1:
                    {
                        Combo_Smart(target);
                        break;
                    }
            }
           
        }

        private static void Combo_Burst(Obj_AI_Base target)
        {
            if (Q.IsReady() && Q.CanCast(target) && Q.IsInRange(target))
            {
                Q.CastOnUnit(target);
            }
            if (W.IsReady() && W.CanCast(target) && W.IsInRange(target))
            {
                W.CastOnUnit(target);
            }
            if (Q.IsReady() && Q.CanCast(target) && Q.IsInRange(target))
            {
                Q.CastOnUnit(target);
            }
            if (E.IsReady() && E.CanCast(target) && E.IsInRange(target))
            {
                E.CastOnUnit(target);
            }
            if (Q.IsReady() && Q.CanCast(target) && Q.IsInRange(target))
            {
                Q.CastOnUnit(target);
            }
        }

        private static void Combo_Smart(Obj_AI_Base target)
        {
            if (Player.Distance(target) >= 575 && !target.IsFacing(Player) && W.IsReady())
            {
                W.CastOnUnit(target, PacketCast);
                if (Q.IsReady() && Q.CanCast(target) && Q.IsInRange(target))
                {
                    Q.CastOnUnit(target);
                }
                if (E.IsReady() && E.CanCast(target) && E.IsInRange(target))
                {
                    E.CastOnUnit(target);
                }
                if (Q.IsReady() && Q.CanCast(target) && Q.IsInRange(target))
                {
                    Q.CastOnUnit(target);
                }
            }
            else
                Combo_Burst(target);            
        }

        //Harass
        private static void Harass(Obj_AI_Base target)
        {
            if (target == null) return;
            var harassMana = Config.Item("harass.Mana").GetValue<Slider>().Value;

            if (Q.IsReady() && Q.IsInRange(target)) Q.CastOnUnit(target);

        }

        //Farm
        private static void Farm()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            var mana = Player.MaxMana*(Config.Item("farm.mana").GetValue<Slider>().Value/100.0);
            if (!(Player.Mana > mana)) return;

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

        //Jungleclear
        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(
                Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0) return;

            var mob = mobs[0];
            if (mob == null) return;

            Combo_Burst(mob);
        }

        //Combo Damage calculating
        private static float ComboDamage(Obj_AI_Base target)
        {
            var dmg = 0d;

            if (Q.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (W.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (E.IsReady())
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