#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

#endregion

namespace Fiddlesticks
{
    internal class Program
    {
        public const string CharName = "Fiddlesticks";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q, W, E, R;
        public static SpellSlot IgniteSlot;
        public static SpellSlot SmiteSlot;
        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        // Custom vars
        public static bool PacketCast;
        public static bool DebugEnabled;
        // Items
        public static Items.Item Biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);
        public static Items.Item Zhonya = new Items.Item(3157);
        

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            /*if (ObjectManager.Player.ChampionName != CharName)
            {
                return;
            }
            */

            Q = new Spell(SpellSlot.Q, 575);
            W = new Spell(SpellSlot.W, 575);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 800);

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);
            Spells.Add(R);

            IgniteSlot = Player.GetSpellSlot("summonerdot");
            SetSmiteSlot();

            Config = new Menu("Fiddlesticks Sharp", "fiddlesticks", true);

            //Orbwalker Menu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Target Selector Menu
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            //Combo Menu
            Config.AddSubMenu(new Menu("[FS] Combo Settings", "fiddlesticks.combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("combo.useQ", "Use Q in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("combo.useW", "Use W in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("combo.useE", "Use E in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("combo.useR", "Use R in Combo").SetValue(true));

            //Killsteal
            Config.AddSubMenu(new Menu("[FS] Killsteal Settings", "fiddlesticks.killsteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("killsteal.enabled", "Auto KS enabled").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("killsteal.useE", "KS with E").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("killsteal.useIgnite", "KS with Ignite").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("killsteal.useSmite", "KS with Smite").SetValue(true));

            //Harass Menu
            Config.AddSubMenu(new Menu("[FS] Harass Settings", "fiddlesticks.harass"));
            Config.SubMenu("harass").AddItem(new MenuItem("harass.enabledPress", "Press Harass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("harass").AddItem(new MenuItem("harass.enabledToggle", "Toggle Harass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("harass").AddItem(new MenuItem("harass.mode", "Harass Mode:").SetValue(new StringList(new[] {"E", "Q+W", "Q+E+W"})));
            Config.SubMenu("harass").AddItem(new MenuItem("harass.mana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Farm Menu
            Config.AddSubMenu(new Menu("[FS] Farming Settings", "fiddlesticks.farm"));
            Config.SubMenu("farm").AddItem(new MenuItem("farm.useE", "Farm with E").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("farm.mana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Jungle Clear Menu
            Config.AddSubMenu(new Menu("[FS] Jungle Clear Settings", "fiddlesticks.jungle"));
            Config.SubMenu("fiddlesticks.jungle").AddItem(new MenuItem("jungle.useE", "Clear with E").SetValue(true));
            Config.SubMenu("fiddlesticks.jungle").AddItem(new MenuItem("jungle.useW", "Clear with W").SetValue(true));

            //Drawing Menu
            Config.AddSubMenu(new Menu("[FS] Draw Settings", "fiddlesticks.drawing"));
            Config.SubMenu("fiddlesticks.drawing").AddItem(new MenuItem("drawing.disableAll", "Disable drawing").SetValue(false));
            Config.SubMenu("fiddlesticks.drawing").AddItem(new MenuItem("drawing.target", "Highlight Target").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 0))));
            Config.SubMenu("fiddlesticks.drawing").AddItem(new MenuItem("drawing.drawQ", "Draw Q Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("fiddlesticks.drawing").AddItem(new MenuItem("drawing.drawW", "Draw W Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("fiddlesticks.drawing").AddItem(new MenuItem("drawing.drawE", "Draw E Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("fiddlesticks.drawing").AddItem(new MenuItem("drawing.drawR", "Draw R Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            //Misc Menu
            Config.AddSubMenu(new Menu("[FS] Misc Settings", "fiddlesticks.misc"));
            Config.SubMenu("fiddlesticks.misc").AddItem(new MenuItem("misc.interruptSpells", "Interrupt Spells").SetValue(true));
            Config.SubMenu("fiddlesticks.misc").AddItem(new MenuItem("misc.interruptGapclosers", "Interrupt Gapclosers").SetValue(true));
            Config.SubMenu("fiddlesticks.misc").AddItem(new MenuItem("misc.usePackets", "Use Packets to Cast Spells").SetValue(true));
            Config.SubMenu("fiddlesticks.misc").AddItem(new MenuItem("misc.debug", "Enable debug").SetValue(false));
            Config.SubMenu("fiddlesticks.misc").AddItem(new MenuItem("misc.autoZhonya.enabled", "Auto Zhonya").SetValue(true));
            Config.SubMenu("fiddlesticks.misc").AddItem(new MenuItem("misc.autoZhonya.percent" "Auto Zhonya").SetValue(new Slider(10));

            /*Config.SubMenu("fiddlesticks.misc")
                .AddItem(
                    new MenuItem("autolvlup", "Auto Level Spells").SetValue(
                        new StringList(new[] { "W>E>Q", "W>Q>E" })));*/

            //AutoPots menu
            Config.AddSubMenu(new Menu("[FS] AutoPot", "fiddlesticks.autopot"));
            Config.SubMenu("fiddlesticks.autopot").AddItem(new MenuItem("autopot.enabled", "AutoPot enabled").SetValue(true));
            Config.SubMenu("fiddlesticks.autopot").AddItem(new MenuItem("autopot.hp", "Health Pot").SetValue(true));
            Config.SubMenu("fiddlesticks.autopot").AddItem(new MenuItem("autopot.mp", "Mana Pot").SetValue(true));
            Config.SubMenu("fiddlesticks.autopot").AddItem(new MenuItem("autopot.hp.percent", "Health Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("fiddlesticks.autopot").AddItem(new MenuItem("autopot.mp.percent", "Mana Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("fiddlesticks.autopot").AddItem(new MenuItem("autopot.ignite", "Auto pot when ignite").SetValue(true));

            if (SmiteSlot != SpellSlot.Unknown)
            {
                Config.SubMenu("fiddlesticks.combo").AddItem(new MenuItem("combo.useSmite", "Use Smite").SetValue(true));
            }

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
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            //Announce that the assembly has been loaded
            Game.PrintChat("<font color=\"#00BFFF\">Fiddlesticks# -</font> <font color=\"#FFFFFF\">Loaded</font>");
            Game.PrintChat("<font color=\"#00BFFF\">Fiddlesticks# -</font> <font color=\"#FFFFFF\">Thank you for using my scripts, feel free to suggest features and report bugs on the forums.</font>");
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            // Select default target
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (InDrain())
            {
                Orbwalker.SetAttack(false);
                Orbwalker.SetMovement(false);
                Notifications.AddNotification("Draining detected, Orbwalker stopped", 3000);
            }
            else
            {
                Orbwalker.SetAttack(true);
                Orbwalker.SetMovement(true);
            }

            //Main features with Orbwalker
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(target);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
            }

            AutoPot();
            KillSteal();
            Harass(target);
            InDrain();
            AutoZhonya();
        }

        //Auto Zhonya - not tested 
        private static void AutoZhonya()
        {
            var zhonyaPercent = Player.MaxHealth*(Config.Item("misc.autoZhonya.percent").GetValue<Slider>().Value/100.0);

            if (!Config.Item("misc.autoZhonya.enabled").GetValue<bool>();) return;
            if (!Items.HasItem(Zhonya) return;
            if (Player.HasBuff("Crowstorm"))
            {
                if (Player.HealthPercent < zhonyaPercent)
                {
                    Items.UseItem(Zhonya);
                }
            }
        }

        private static bool InDrain()
        {
            return Player.HasBuff("Drain") || Player.IsChannelingImportantSpell();
        }

        //Interrupter
        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Config.Item("misc.interruptSpells").GetValue<bool>()) return;

            if ((!(sender.Distance(sender.Position) <= Q.Range)) || !Q.IsReady()) return;

            Q.CastOnUnit(sender, PacketCast);

            if (DebugEnabled) Game.PrintChat("Debug - Q Casted to interrupt SPELL");
        }

        //Drawing
        private static void Drawing_OnDraw(EventArgs args)
        {
            //Main drawing switch
            if (Config.Item("drawing.disableAll").GetValue<bool>()) return;

            //Spells drawing
            foreach (var spell in Spells.Where(spell => Config.Item(spell.Slot + "Draw").GetValue<Circle>().Active))
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
            if (!Config.Item("gapcloser").GetValue<bool>()) return;
            if (!gapcloser.Sender.IsValidTarget(Q.Range)) return;

            Q.CastOnUnit(gapcloser.Sender, PacketCast);

            if (DebugEnabled) Game.PrintChat("Debug - Q Casted to interrupt GAPCLOSER");
        }

        //Killsteal
        private static void KillSteal()
        {
            if (!Config.Item("killsteal.enabled").GetValue<bool>()) return;

            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            #region E KS
            if (E.IsReady() && 
                target.Health < E.GetDamage(target) && 
                ObjectManager.Player.Distance(target) <= E.Range + target.BoundingRadius)
            {
                E.Cast(target, PacketCast);
                if (DebugEnabled) Game.PrintChat("Debug - E casted to KILLSTEAL.");
            }

            #endregion

            #region Ignite KS
            if (IgniteSlot == SpellSlot.Unknown || 
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready || 
                !(ObjectManager.Player.Distance(target) < 600)) return;

            if (!(ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health))
                return;
            ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
            if (DebugEnabled) Game.PrintChat("Debug - Ignite casted to KILLSTEAL.");

            #endregion

        #region Smite KS
            if (SmiteSlot != SpellSlot.Unknown
                || ObjectManager.Player.Spellbook.CanUseSpell(SmiteSlot) != SpellState.Ready
                || !(ObjectManager.Player.Distance(target) < 600)) return;

            if (!(ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Smite) > target.Health))
                return;
            ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, target);
            if (DebugEnabled) Game.PrintChat("Debug - Smite casted to KILLSTEAL.");
        #endregion
        }

        //Auto pot
        private static void AutoPot()
        {
            if (!Config.Item("autopot.enabled").GetValue<bool>()) return;

            //Auto Ignite Counter
            if (Config.Item("autopot.ignite").GetValue<bool>())
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
            if (!Config.Item("autopot.hp").GetValue<bool>()) return;
            if (Player.Health/100 <= Config.Item("autopot.hp.percent").GetValue<Slider>().Value &&
                !Player.HasBuff("RegenerationPotion", true))
            {
                Items.UseItem(2003);
                if (DebugEnabled) Game.PrintChat("Debug - HP Pot used because of LOW HP");
            }

            //Mana Pots
            if (!Config.Item("autopot.mp").GetValue<bool>()) return;
            if (Player.Mana/100 <= Config.Item("autopot.mp.percent").GetValue<Slider>().Value &&
                !Player.HasBuff("FlaskOfCrystalWater", true))
            {
                Items.UseItem(2004);
                if (DebugEnabled) Game.PrintChat("Debug - MP Pot used because of LOW MP");
            }
        }

        //Combo
        private static void Combo(Obj_AI_Base target)
        {
            #region R Cast
            if (!R.IsReady() || !Config.Item("combo.useR").GetValue<bool>()) return;
            R.Cast(target.ServerPosition, PacketCast);
            if (DebugEnabled) Game.PrintChat("Debug - R used to initiate COMBO");
            #endregion

            #region Smite Cast
            if (Config.Item("combo.useSmite").GetValue<bool>())
            {
                if (SmiteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                {
                    Player.Spellbook.CastSpell(SmiteSlot, target);
                    if (DebugEnabled) Game.PrintChat("Debug - Smite used in COMBO");
                }
            }
            #endregion

            #region Q Cast
            if (!Q.IsReady() || !(Config.Item("combo.useQ").GetValue<bool>())) return;
            Q.CastOnUnit(target, PacketCast);
            if (DebugEnabled) Game.PrintChat("Debug - Q used in COMBO");
            #endregion

            #region E Cast
            if (!E.IsReady() || !(Config.Item("combo.useE").GetValue<bool>())) return;
            E.CastOnUnit(target, PacketCast);
            if (DebugEnabled) Game.PrintChat("Debug - E used in COMBO");

            #endregion

            #region W Cast
            if (!W.IsReady() || !Config.Item("combo.useW").GetValue<bool>() || InDrain()) return;

            /*
            //This should be automatic now ^^
            InDrain = true;
            Orbwalker.SetAttack(false);
            Orbwalker.SetMovement(false);
            */
            W.CastOnUnit(target, PacketCast);
            if (DebugEnabled) Game.PrintChat("Debug - W used in COMBO");

            #endregion
        }

        //Harass
        private static void Harass(Obj_AI_Base target)
        {
            var harassMode = Config.Item("harass.mode").GetValue<StringList>().SelectedIndex;
            var harassMana = Player.MaxMana*(Config.Item("harass.Mana").GetValue<Slider>().Value/100.0);

            if (Config.Item("harass.enabledPress").GetValue<KeyBind>().Active || Config.Item("harass.enabledToggle").GetValue<KeyBind>().Active) return;

            if (!(Player.Mana > harassMana)) return;

            switch (harassMode)
            {
                case 0: //1st mode: E only
                    if (E.IsReady() && E.IsInRange(target))
                    {
                        E.CastOnUnit(target, PacketCast);
                        if (DebugEnabled) Game.PrintChat("Debug - E used in HARASS - MODE 0");

                    }
                    break;
                case 1: //2nd mode: Q and W
                    if (Q.IsReady() && W.IsReady() && Q.IsInRange(target) && W.IsInRange(target))
                    {
                        Q.CastOnUnit(target, PacketCast);
                        if (DebugEnabled) Game.PrintChat("Debug - Q used in HARASS - MODE 1");

                        W.CastOnUnit(target, PacketCast);
                        if (DebugEnabled) Game.PrintChat("Debug - W used in HARASS - MODE 1");
                    }
                    break;
                case 2: //3rd mode: Q, E and W
                    if (Q.IsReady() && W.IsReady() && E.IsReady() && Q.IsInRange(target) && W.IsInRange(target) && E.IsInRange(target))
                    {
                        Q.CastOnUnit(target, PacketCast);
                        if (DebugEnabled) Game.PrintChat("Debug - Q used in HARASS - MODE 2");

                        E.CastOnUnit(target, PacketCast);
                        if (DebugEnabled) Game.PrintChat("Debug - E used in HARASS - MODE 2");

                        W.CastOnUnit(target, PacketCast);
                        if (DebugEnabled) Game.PrintChat("Debug - W used in HARASS - MODE 2");
                    }
                    break;
            }
        }

        //Farm
        private static void Farm()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, W.Range);
            var mana = Player.MaxMana*(Config.Item("farm.mana").GetValue<Slider>().Value/100.0);
            if (!(Player.Mana > mana)) //Check if player has enough mana
            {
                return;
            }

            if (Config.Item("farm.useE").GetValue<bool>() && E.IsReady())
            {
                // Logic for getting killable minions
                foreach (var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(E.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int) (Player.Distance(minion.Position))) <=
                            Player.GetSpellDamage(minion, SpellSlot.E)))
                {
                    E.CastOnUnit(minion, PacketCast);
                    return;
                }
            }
        }

        //Jungleclear
        private static void JungleClear()
        {
            // Get mobs in range, try to order them by max health to get the big ones
            var mobs = MinionManager.GetMinions(
                Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0) return;

            var mob = mobs[0];
            if (mob == null) return;

            if (Config.Item("jungle.useE").GetValue<bool>() && E.IsReady())
            {
                E.CastOnUnit(mob, PacketCast);
            }
            if (Config.Item("jungle.useW").GetValue<bool>() && W.IsReady())
            {
                W.CastOnUnit(mob, PacketCast);
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

            if (SmiteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
            {
                dmg += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Smite);
            }

            return (float) dmg;
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

        //Setting Smite slot
        public static void SetSmiteSlot()
        {
            foreach (var spell in
                ObjectManager.Player.Spellbook.Spells.Where(
                    spell => String.Equals(spell.Name, SmiteType(), StringComparison.CurrentCultureIgnoreCase)))
            {
                SmiteSlot = spell.Slot;
                break;
            }
        }
    }
}