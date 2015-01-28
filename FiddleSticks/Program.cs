#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Fiddlesticks
{
    internal class Program
    {
        public const string CharName = "Fiddlesticks";
        public static bool InDrain;
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

        //Packet casting
        public static bool PacketCast;

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
            /*if (ObjectManager.Player.ChampionName != CharName)
            {
                return;
            }
            */
            Q = new Spell(SpellSlot.Q, 575);
            W = new Spell(SpellSlot.W, 575);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 800);

            IgniteSlot = Player.GetSpellSlot("summonerdot");
            SetSmiteSlot();

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);
            Spells.Add(R);

            Config = new Menu(CharName, CharName, true);

            //Orbwalker Menu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Target Selector Menu
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            //Combo Menu
            Config.AddSubMenu(new Menu("Combo Settings", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("useQ", "Use Q in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useW", "Use W in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items with Combo").SetValue(true));

            //Killsteal
            Config.AddSubMenu(new Menu("Killsteal Settings", "KillSteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("KillSteal", "Auto KS enabled").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("ksW", "KS with W").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("ksE", "KS with E").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("ksI", "KS with Ignite").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("ksS", "KS with Smite").SetValue(true));

            //Harass Menu
            Config.AddSubMenu(new Menu("Harass Settings", "harass"));
            Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("harassKey", "Harass Key").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("harass")
                .AddItem(new MenuItem("hMode", "Harass Mode:").SetValue(new StringList(new[] { "E", "Q+W", "Q+E+W" })));
            Config.SubMenu("harass").AddItem(new MenuItem("harassMana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Farm Menu
            Config.AddSubMenu(new Menu("Farming Settings", "farm"));
            Config.SubMenu("farm").AddItem(new MenuItem("eFarm", "Farm with E").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("farmMana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Jungle Clear Menu
            Config.AddSubMenu(new Menu("Jungle Clear Settings", "jungle"));
            Config.SubMenu("jungle").AddItem(new MenuItem("wJungle", "Clear with W").SetValue(true));
            Config.SubMenu("jungle").AddItem(new MenuItem("eJungle", "Clear with E").SetValue(true));

            //Drawing Menu
            Config.AddSubMenu(new Menu("Draw Settings", "drawing"));
            Config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "Disable all drawings").SetValue(false));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("Target", "Highlight Target").SetValue(
                        new Circle(true, Color.FromArgb(255, 255, 255, 0))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("QDraw", "Draw Q Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("WDraw", "Draw W Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("EDraw", "Draw E Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("RDraw", "Draw R Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            //Misc Menu
            Config.AddSubMenu(new Menu("Misc Settings", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("stopChannel", "Interrupt Spells").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("gapcloser", "Interrupt Gapclosers").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("usePackets", "Use Packets to Cast Spells").SetValue(false));
            Config.SubMenu("misc")
                .AddItem(
                    new MenuItem("autolvlup", "Auto Level Spells").SetValue(
                        new StringList(new[] { "W>E>Q", "W>Q>E" })));

            //AutoPots menu
            Config.AddSubMenu(new Menu("AutoPot", "AutoPot"));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AutoPot", "AutoPot enabled").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H", "Health Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_M", "Mana Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H_Per", "Health Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_M_Per", "Mana Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_Ign", "Auto pot when ignite").SetValue(true));

            if (SmiteSlot != SpellSlot.Unknown)
            {
                Config.SubMenu("combo").AddItem(new MenuItem("autoSmite", "Smite enemy in Combo").SetValue(true));
            }

            //Make menu visible
            Config.AddToMainMenu();

            PacketCast = Config.Item("usePackets").GetValue<bool>();

            //Damage Drawer
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            //Necessary Stuff
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnPlayAnimation += PlayAnimation;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            //Announce that the assembly has been loaded
            Game.PrintChat("<font color=\"#00BFFF\">Fiddlesticks# -</font> <font color=\"#FFFFFF\">Loaded</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Select default target
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

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

            //Drain Fix
            if (ObjectManager.Player.IsDead)
            {
                return;
            }
            if (InDrain)
            {
                Orbwalker.SetAttack(false);
                Orbwalker.SetMovement(false);
            }
            else
            {
                Orbwalker.SetAttack(true);
                Orbwalker.SetMovement(true);
            }
        }

        //Drain Fix
        private static void PlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Animation == "Spell2")
                {
                    InDrain = true;
                }
                else if (args.Animation == "Run" || args.Animation == "Idle1" || args.Animation == "Attack2" ||
                         args.Animation == "Attack1")
                {
                    InDrain = false;
                }
            }
        }

        //Drawing
        private static void Drawing_OnDraw(EventArgs args)
        {
            //Main drawing switch
            if (Config.Item("mDraw").GetValue<bool>())
            {
                return;
            }

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
            if (!Config.Item("gapcloser").GetValue<bool>())
            {
                return;
            }

            if (gapcloser.Sender.IsValidTarget(Q.Range))
            {
                Q.CastOnUnit(gapcloser.Sender, PacketCast);
            }
        }

        // Interrupter
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.SubMenu("Combo").Item("stopChannel").GetValue<bool>())
            {
                return;
            }

            if ((unit.Distance(unit.Position) <= Q.Range) && Q.IsReady())
            {
                Q.CastOnUnit(unit, PacketCast);
            }
        }

        //Killsteal
        private static void KillSteal()
        {
            if (Config.Item("KillSteal").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (target == null)
                {
                    return;
                }

                if (E.IsReady() && W.IsReady() && target.Health < W.GetDamage(target, 1) + E.GetDamage(target) &&
                    ObjectManager.Player.Distance(target) <= E.Range + target.BoundingRadius)
                {
                    E.Cast(target, PacketCast);
                    W.Cast(target, PacketCast);
                }
                if (IgniteSlot != SpellSlot.Unknown &&
                    ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                    ObjectManager.Player.Distance(target) < 600)
                {
                    if (ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
                    }
                }
            }
        }

        //Auto pot
        private static void AutoPot()
        {
            if (Config.Item("AutoPot").GetValue<bool>())
            {
                if (Player.HasBuff("summonerdot") || Player.HasBuff("MordekaiserChildrenOfTheGrave"))
                {
                    if (!Player.InFountain())
                    {
                        if (Items.HasItem(Biscuit.Id) && Items.CanUseItem(Biscuit.Id) &&
                            !Player.HasBuff("ItemMiniRegenPotion"))
                        {
                            Biscuit.Cast(Player);
                        }
                        else if (Items.HasItem(HPpot.Id) && Items.CanUseItem(HPpot.Id) &&
                                 !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("Health Potion"))
                        {
                            HPpot.Cast(Player);
                        }
                        else if (Items.HasItem(Flask.Id) && Items.CanUseItem(Flask.Id) &&
                                 !Player.HasBuff("ItemCrystalFlask"))
                        {
                            Flask.Cast(Player);
                        }
                    }
                }

                if (ObjectManager.Player.HasBuff("Recall") || Player.InFountain() && Player.InShop())
                {
                    return;
                }

                //Health Pots
                if (Player.Health / 100 <= Config.Item("AP_H_Per").GetValue<Slider>().Value &&
                    !Player.HasBuff("RegenerationPotion", true))
                {
                    Items.UseItem(2003);
                }
                //Mana Pots
                if (Player.Health / 100 <= Config.Item("AP_M_Per").GetValue<Slider>().Value &&
                    !Player.HasBuff("FlaskOfCrystalWater", true))
                {
                    Items.UseItem(2004);
                }
            }
        }

        //Combo
        private static void Combo(Obj_AI_Base target)
        {
            if (target == null) // Check if there is a target
            {
                return;
            }

            if (!R.IsReady() && !Config.Item("useR").GetValue<bool>())
            {
                return;
            }
            R.Cast(target.ServerPosition, PacketCast);

            if (Config.Item("comboItems").GetValue<bool>())
            {
                UseItems(target);
                return;
            }

            if (Config.Item("autoSmite").GetValue<bool>())
            {
                if (SmiteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                {
                    Player.Spellbook.CastSpell(SmiteSlot, target);
                }
            }

            if (Q.IsReady() && (Config.Item("useQ").GetValue<bool>()))
            {
                Q.CastOnUnit(target, PacketCast);
                return;
            }

            if (E.IsReady() && (Config.Item("useE").GetValue<bool>()))
            {
                E.CastOnUnit(target, PacketCast);
                return;
            }

            if (W.IsReady() && (Config.Item("useW").GetValue<bool>()))
            {
                Game.PrintChat("Hello");
                Orbwalker.SetAttack(false);
                Orbwalker.SetMovement(false);
                W.CastOnUnit(target, PacketCast);
            }
        }
        
        //Harass
        private static void Harass(Obj_AI_Base target)
        {
            var harassKey = Config.Item("harassKey").GetValue<KeyBind>().Active;
            var menuItem = Config.Item("hMode").GetValue<StringList>().SelectedIndex; //Select the Harass Mode
            var mana = Player.MaxMana * (Config.Item("harassMana").GetValue<Slider>().Value / 100.0);

            if (harassKey && target != null)
            {
                return;
            }

            if ((Player.Mana > mana))
            {
                switch (menuItem)
                {
                    case 0: //1st mode: E only
                        if (E.IsReady())
                        {
                            E.Cast(target, PacketCast);
                        }
                        break;
                    case 1: //2nd mode: Q and W
                        if (Q.IsReady() && W.IsReady())
                        {
                            Q.Cast(target, PacketCast);
                            W.Cast(target, PacketCast);
                        }
                        break;
                    case 2: //3rd mode: Q, E and W
                        if (Q.IsReady() && W.IsReady() && E.IsReady())
                        {
                            Q.Cast(target, PacketCast);
                            E.Cast(target, PacketCast);
                            W.Cast(target, PacketCast);
                        }
                        break;
                }
            }
        }
        
        //Farm
        private static void Farm()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, W.Range);
            var mana = Player.MaxMana * (Config.Item("farmMana").GetValue<Slider>().Value / 100.0);
            if (!(Player.Mana > mana)) //Check if player has enough mana
            {
                return;
            }

            if (Config.Item("eFarm").GetValue<bool>() && E.IsReady())
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
                E.CastOnUnit(mob, PacketCast);
            }
            if (Config.Item("wJungle").GetValue<bool>() && W.IsReady())
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
            int[] redSmite = { 3715, 3718, 3717, 3716, 3714 };
            int[] blueSmite = { 3706, 3710, 3709, 3708, 3707 };

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