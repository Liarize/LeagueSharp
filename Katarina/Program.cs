#region

using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Katarina
{
    internal class Program
    {
        public const string CharName = "Katarina";
        public static bool InUlt;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;
        public static Items.Item Dfg;
        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        private static long _lastECast; //Last E Cast
        public static bool PacketCast; //Packet Casting

        //Items
        public static Items.Item Biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != CharName)  return;

            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 375);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);

            IgniteSlot = Player.GetSpellSlot("summonerdot");

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
            Config.AddSubMenu(new Menu("Smart Combo Settings", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("smartR", "Use Smart R").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("wjCombo", "Use WardJump in Combo").SetValue(true));

            //Killsteal
            Config.AddSubMenu(new Menu("Killsteal Settings", "KillSteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("KillSteal", "Smart Killsteal enabled").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("wjKS", "Use WardJump in KillSteal").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("jumpsS", "Use E jumping in KillSteal").SetValue(true));

            //Harass Menu
            Config.AddSubMenu(new Menu("Harass Settings", "harass"));
            Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("harassKey", "Harass Key").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("hMode", "Harass Mode: ").SetValue(new StringList(new[] { "Q only", "Q+W", "Q+E+W" })));

            //Farm
            Config.AddSubMenu(new Menu("Farming Settings", "farm"));
            Config.SubMenu("farm").AddItem(new MenuItem("smartFarm", "Use Smart Farm").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("qFarm", "Farm with Q").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("wFarm", "Farm with W").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("eFarm", "Farm with E").SetValue(true));

            //Jungle Clear Menu
            Config.AddSubMenu(new Menu("Jungle Clear Settings", "jungle"));
            Config.SubMenu("jungle").AddItem(new MenuItem("qJungle", "Clear with Q").SetValue(true));
            Config.SubMenu("jungle").AddItem(new MenuItem("wJungle", "Clear with W").SetValue(true));
            Config.SubMenu("jungle").AddItem(new MenuItem("eJungle", "Clear with E").SetValue(true));

            //Drawing Menu
            Config.AddSubMenu(new Menu("Draw Settings", "drawing"));
            Config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "Disable all drawings").SetValue(false));
            Config.SubMenu("drawing").AddItem(new MenuItem("ultiDmgDraw", "Draw Ultimate damage").SetValue(false));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("Target", "Highlight Target").SetValue(
                        new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 0))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("QDraw", "Draw Q Range").SetValue(
                        new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("WDraw", "Draw W Range").SetValue(
                        new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("EDraw", "Draw E Range").SetValue(
                        new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("RDraw", "Draw R Range").SetValue(
                        new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            //Misc Menu
            Config.AddSubMenu(new Menu("Misc Settings", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("usePackets", "Use Packets to Cast Spells").SetValue(false));
            Config.SubMenu("misc")
                .AddItem(
                    new MenuItem("autolvlup", "Auto Level Spells").SetValue(
                        new StringList(new[] { "W->E->Q", "Q>W>E" })));
            Config.SubMenu("misc").AddItem(new MenuItem("playLegit", "Legit E").SetValue(false));
            Config.SubMenu("misc")
                .AddItem(new MenuItem("legitCastDelay", "Legit E Delay").SetValue(new Slider(1000, 0, 2000)));

            //Wardjump Menu
            Config.AddSubMenu(new Menu("WardJump Settings", "wardjump"));
            Config.SubMenu("wardjump")
                .AddItem(
                    new MenuItem("wardjumpkey", "WardJump key").SetValue(
                        new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("wardjump")
                .AddItem(
                    new MenuItem("wjpriority", "Wardjump priority").SetValue(new StringList(new[] { "Coming", "Soon" })));

            //AutoPots menu
            Config.AddSubMenu(new Menu("AutoPot", "AutoPot"));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AutoPot", "AutoPot enabled").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H", "Health Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H_Per", "Health Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_Ign", "Auto pot when ignited").SetValue(true));

            if (IgniteSlot != SpellSlot.Unknown)
            {
                Config.SubMenu("misc").AddItem(new MenuItem("autoIgnite", "Auto ignite when killable").SetValue(true));
            }

            //Make menu visible
            Config.AddToMainMenu();

            PacketCast = Config.Item("usePackets").GetValue<bool>();

            //Damage Drawer
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            //Necessary Stuff
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnPlayAnimation += PlayAnimation;
            Game.OnGameUpdate += Game_OnGameUpdate;

            //Announce that the assembly has been loaded
            Game.PrintChat("<font color=\"#00BFFF\">SmartKatarina -</font> <font color=\"#FFFFFF\">Loaded</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Select default target
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            //Main features with Orbwalker
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass(target);
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Farm();
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
            }

            AutoPot();
            KillSteal();
            Harass(target);

            //Ultimate fix
            if (ObjectManager.Player.IsDead)
            {
                return;
            }
            if (InUlt)
            {
                Orbwalker.SetAttack(false);
                Orbwalker.SetMovement(false);
            }
            else
            {
                Orbwalker.SetAttack(true);
                Orbwalker.SetMovement(true);
            }

            // WardJump
            //if (wardjumpKey != null)
            //{
            //wardjump();
            //}           
        }

        //E Humanizer
        public static void CastE(Obj_AI_Base unit)
        {
            var playLegit = Config.Item("playLegit").GetValue<bool>();
            var legitCastDelay = Config.Item("legitCastDelay").GetValue<Slider>().Value;

            if (playLegit)
            {
                if (Environment.TickCount > _lastECast + legitCastDelay)
                {
                    E.CastOnUnit(unit, PacketCast);
                    _lastECast = Environment.TickCount;
                }
            }
            else
            {
                E.CastOnUnit(unit, PacketCast);
                _lastECast = Environment.TickCount;
            }
        }

        //Ultimate fix
        private static void PlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Animation == "Spell4")
                {
                    InUlt = true;
                }
                else if (args.Animation == "Run" || args.Animation == "Idle1" || args.Animation == "Attack2" ||
                         args.Animation == "Attack1")
                {
                    InUlt = false;
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
                    ObjectManager.Player.Position, spell.Range,
                    spell.IsReady() ? System.Drawing.Color.Green : System.Drawing.Color.Red);
            }

            //Target Drawing
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Config.Item("Target").GetValue<Circle>().Active && target != null)
            {
                Render.Circle.DrawCircle(target.Position, 50, Config.Item("Target").GetValue<Circle>().Color);
            }
        }

        /*
        //JumpKS
        private static void JumpKs(Obj_AI_Hero hero)
        {
            foreach (Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward =>
                E.IsReady() && Q.IsReady() && ward.Name.ToLower().Contains("ward") &&
                ward.Distance(hero.ServerPosition) < Q.Range && ward.Distance(Player) < E.Range))
            {
                E.Cast(ward);
                return;
            }

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(jumph =>
                E.IsReady() && Q.IsReady() && jumph.Distance(hero.ServerPosition) < Q.Range &&
                jumph.Distance(Player) < E.Range && jumph.IsValidTarget(E.Range)))
            {
                E.Cast(jumph);
                return;
            }

            foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(jump =>
                E.IsReady() && Q.IsReady() && jumpm.Distance(hero.ServerPosition) < Q.Range &&
                jumpm.Distance(Player) < E.Range && jumpm.IsValidTarget(E.Range)))
            {
                E.Cast(jumpm);
                return;
            }
        }
        */

        //Killsteal
        private static void KillSteal()
        {
            if (Config.Item("KillSteal").GetValue<bool>())
            {
                foreach (
                    Obj_AI_Hero hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                hero =>
                                    ObjectManager.Player.Distance(hero.ServerPosition) <= E.Range && !hero.IsMe &&
                                    hero.IsValidTarget() && hero.IsEnemy && !hero.IsInvulnerable))
                {
                    //Variables
                    var qdmg = Q.GetDamage(hero);
                    var wdmg = W.GetDamage(hero);
                    var edmg = E.GetDamage(hero);
                    var markDmg = Player.CalcDamage(hero, Damage.DamageType.Magical, Player.FlatMagicDamageMod * 0.15 + Player.Level * 15);
                    float ignitedmg;

                    //Ignite Damage
                    if (IgniteSlot != SpellSlot.Unknown)
                    {
                        ignitedmg = (float) Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
                    }

                    else

                    {
                        ignitedmg = 0f;
                    }

                    //W + Mark
                    if (hero.HasBuff("katarinaqmark") && hero.Health - wdmg - markDmg < 0 && W.IsReady() &&
                        W.IsInRange(hero))
                    {
                        W.Cast(PacketCast);
                    }
                    //Ignite
                    if (hero.Health - ignitedmg < 0 && IgniteSlot.IsReady())
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, hero);
                    }
                    // E
                    if (hero.Health - edmg < 0 && E.IsReady())
                    {
                        E.Cast(hero, PacketCast);
                    }
                    // Q
                    if (hero.Health - qdmg < 0 && Q.IsReady() && Q.IsInRange(hero))
                    {
                        Q.Cast(hero, PacketCast);
                    }
                    /*else if (Q.IsReady() && E.IsReady() && Player.Distance(hero.ServerPosition) <= 1375 && Config.Item("jumpKs", true).GetValue<bool>())
                    {
                        JumpKs(hero);
                        Q.Cast(hero, PacketCast);
                        return;
                    } */
                    // E + W
                    if (hero.Health - edmg - wdmg < 0 && E.IsReady() && W.IsReady())
                    {
                        CastE(hero);
                        W.Cast(PacketCast);
                    }
                    // E + Q
                    if (hero.Health - edmg - qdmg < 0 && E.IsReady() && Q.IsReady())
                    {
                        CastE(hero);
                        Q.Cast(hero, PacketCast);
                    }
                    // E + Q + W (don't proc Mark)
                    if (hero.Health - edmg - wdmg - qdmg < 0 && E.IsReady() && Q.IsReady() && W.IsReady())
                    {
                        CastE(hero);
                        Q.Cast(hero, PacketCast);
                        W.Cast(PacketCast);
                    }
                    // E + Q + W + Mark
                    if (hero.Health - edmg - wdmg - qdmg - markDmg < 0 && E.IsReady() && Q.IsReady() && W.IsReady())
                    {
                        CastE(hero);
                        Q.Cast(hero, PacketCast);
                        W.Cast(PacketCast);
                    }
                    // E + Q + W + Ignite
                    if (hero.Health - edmg - wdmg - qdmg - ignitedmg < 0 && E.IsReady() && Q.IsReady() && W.IsReady() &&
                        IgniteSlot.IsReady())
                    {
                        CastE(hero);
                        Q.Cast(hero, PacketCast);
                        W.Cast(PacketCast);
                        Player.Spellbook.CastSpell(IgniteSlot, hero);
                    }
                }

                foreach (
                    Obj_AI_Base target in
                        ObjectManager.Get<Obj_AI_Base>()
                            .Where(
                                target =>
                                    ObjectManager.Player.Distance(target.ServerPosition) <= E.Range && !target.IsMe &&
                                    target.IsTargetable && !target.IsInvulnerable))
                {
                    foreach (
                        Obj_AI_Hero focus in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(
                                    focus =>
                                        focus.Distance(focus.ServerPosition) <= Q.Range && focus.IsEnemy && !focus.IsMe &&
                                        !focus.IsInvulnerable && focus.IsValidTarget()))
                    {
                        //Variables
                        var qdmg = Q.GetDamage(focus);
                        var wdmg = W.GetDamage(focus);
                        float ignitedmg;

                        //Ignite Damage
                        if (IgniteSlot != SpellSlot.Unknown)
                        {
                            ignitedmg =
                                (float) Player.GetSummonerSpellDamage(focus, Damage.SummonerSpell.Ignite);
                        }
                        else
                        {
                            ignitedmg = 0f;
                        }

                        //Mark Damage
                        var markDmg = Player.CalcDamage(focus, Damage.DamageType.Magical, Player.FlatMagicDamageMod * 0.15 + Player.Level * 15);

                        //Q
                        if (focus.Health - qdmg < 0 && E.IsReady() && Q.IsReady() &&
                            focus.Distance(target.ServerPosition) <= Q.Range)
                        {
                            CastE(target);
                            Q.Cast(focus, PacketCast);
                        }
                        // Q + W
                        if (focus.Distance(target.ServerPosition) <= W.Range && focus.Health - qdmg - wdmg < 0 &&
                            E.IsReady() && Q.IsReady())
                        {
                            CastE(target);
                            Q.Cast(focus, PacketCast);
                            W.Cast(PacketCast);
                        }
                        // Q + W + Mark
                        if (focus.Distance(target.ServerPosition) <= W.Range && focus.Health - qdmg - wdmg - markDmg < 0 &&
                            E.IsReady() && Q.IsReady() && W.IsReady())
                        {
                            CastE(target);
                            Q.Cast(focus, PacketCast);
                            W.Cast(PacketCast);
                        }
                        // Q + Ignite
                        if (focus.Distance(target.ServerPosition) <= 600 && focus.Health - qdmg - ignitedmg < 0 &&
                            E.IsReady() && Q.IsReady() && IgniteSlot.IsReady())
                        {
                            CastE(target);
                            Q.Cast(focus, PacketCast);
                            Player.Spellbook.CastSpell(IgniteSlot, focus);
                        }
                        // Q + W + Ignite
                        if (focus.Distance(target.ServerPosition) <= W.Range &&
                            focus.Health - qdmg - wdmg - ignitedmg < 0 && E.IsReady() && Q.IsReady() && W.IsReady() &&
                            IgniteSlot.IsReady())
                        {
                            CastE(target);
                            Q.Cast(focus, PacketCast);
                            W.Cast(PacketCast);
                            Player.Spellbook.CastSpell(IgniteSlot, focus);
                        }
                    }
                }
            }
        }

        // Auto pot
        private static void AutoPot()
        {
            if (Config.Item("AutoPot").GetValue<bool>())
            {
                if (Config.SubMenu("AutoPot").Item("AP_Ign").GetValue<bool>())
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
            }
        }

        //Combo
        private static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            var rdmg = R.GetDamage(target, 1);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && target.IsValidTarget() && !InUlt)
            {
                //Smart Q->E
                if (Q.IsInRange(target))
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(target, PacketCast);
                    }
                    if (E.IsReady())
                    {
                        CastE(target);
                    }
                }
                else
                {
                    if (E.IsReady())
                    {
                        CastE(target);
                    }
                    if (Q.IsReady())
                    {
                        Q.Cast(target, PacketCast);
                    }
                }

                //Cast W
                if (W.IsReady() && W.IsInRange(target))
                {
                    Orbwalker.SetAttack(false);
                    Orbwalker.SetMovement(false);
                    W.Cast(PacketCast);
                    return;
                }

                //Smart R
                if (Config.Item("smartR").GetValue<bool>())
                {
                    if (R.IsReady() && target.Health - rdmg < 0 && !InUlt && !E.IsReady())
                    {
                        Orbwalker.SetAttack(false);
                        Orbwalker.SetMovement(false);
                        InUlt = true;
                        R.Cast(PacketCast);
                    }
                }
                else if (R.IsReady() && !InUlt && !E.IsReady())
                {
                    Orbwalker.SetAttack(false);
                    Orbwalker.SetMovement(false);
                    InUlt = true;
                    R.Cast(PacketCast);
                }
            }
        }

        //Harass
        private static void Harass(Obj_AI_Base target)
        {
            var harassKey = Config.Item("harassKey").GetValue<KeyBind>().Active;
            var menuItem = Config.Item("hMode").GetValue<StringList>().SelectedIndex; //Select the Harass Mode

            if (harassKey && target != null)
            {
                switch (menuItem)
                {
                    case 0: //1st mode: Q only
                        if (Q.IsReady())
                        {
                            Q.CastOnUnit(target, PacketCast);
                        }
                        break;
                    case 1: //2nd mode: Q and W
                        if (Q.IsReady() && W.IsReady())
                        {
                            Q.Cast(target, PacketCast);
                            if (W.IsInRange(target))
                            {
                                W.Cast(PacketCast);
                            }
                        }
                        break;
                    case 2: //3rd mode: Q, E and W
                        if (Q.IsReady() && W.IsReady() && E.IsReady())
                        {
                            Q.Cast(target, PacketCast);
                            CastE(target);
                            W.Cast(PacketCast);
                        }
                        break;
                }
            }
        }

        //Farm
        private static void Farm()
        {
            if (Config.Item("smartFarm").GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                foreach (
                    var minion in
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                minion =>
                                    minion.IsValidTarget() && minion.IsEnemy &&
                                    minion.Distance(Player.ServerPosition) < E.Range))
                {
                    var qdmg = Q.GetDamage(minion);
                    var wdmg = W.GetDamage(minion);
                    var edmg = E.GetDamage(minion);
                    var markDmg = Player.CalcDamage(minion, Damage.DamageType.Magical, Player.FlatMagicDamageMod * 0.15 + Player.Level * 15);

                    //Killable with Q
                    if (minion.Health - qdmg <= 0 && minion.Distance(Player.ServerPosition) <= Q.Range && Q.IsReady() &&
                        (Config.Item("wFarm").GetValue<bool>()))
                    {
                        Q.Cast(minion, PacketCast);
                    }

                    //Killable with W
                    if (minion.Health - wdmg <= 0 && minion.Distance(Player.ServerPosition) <= W.Range && W.IsReady() &&
                        (Config.Item("wFarm").GetValue<bool>()))
                    {
                        W.Cast(PacketCast);
                    }

                    //Killable with E
                    if (minion.Health - edmg <= 0 && minion.Distance(Player.ServerPosition) <= E.Range && E.IsReady() &&
                        (Config.Item("eFarm").GetValue<bool>()))
                    {
                        CastE(minion);
                    }

                    //Killable with Q and W
                    if (minion.Health - wdmg - qdmg <= 0 && minion.Distance(Player.ServerPosition) <= W.Range &&
                        Q.IsReady() && W.IsReady() && (Config.Item("qFarm").GetValue<bool>()) &&
                        (Config.Item("wFarm").GetValue<bool>()))
                    {
                        Q.Cast(minion, PacketCast);
                        W.Cast(PacketCast);
                    }

                    //Killable with Q, W and Mark
                    if (minion.Health - wdmg - qdmg - markDmg <= 0 && minion.Distance(Player.ServerPosition) <= W.Range &&
                        Q.IsReady() && W.IsReady() && (Config.Item("qFarm").GetValue<bool>()) &&
                        (Config.Item("wFarm").GetValue<bool>()))
                    {
                        Q.Cast(minion, PacketCast);
                        W.Cast(PacketCast);
                    }

                    //Killable with Q, W, E and Mark
                    if (minion.Health - wdmg - qdmg - markDmg - edmg <= 0 &&
                        minion.Distance(Player.ServerPosition) <= W.Range && E.IsReady() && Q.IsReady() && W.IsReady() &&
                        (Config.Item("qFarm").GetValue<bool>()) && (Config.Item("wFarm").GetValue<bool>()) &&
                        (Config.Item("eFarm").GetValue<bool>()))
                    {
                        CastE(minion);
                        Q.Cast(minion, PacketCast);
                        W.Cast(PacketCast);
                    }
                }
            }
        }

        //Jungleclear
        private static void JungleClear()
        {
            //Get mobs in range, try to order them by max health to get the big ones
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

            if (Config.Item("qJungle").GetValue<bool>() && Q.IsReady())
            {
                Q.CastOnUnit(mob, PacketCast);
            }

            if (Config.Item("wJungle").GetValue<bool>() && W.IsReady())
            {
                W.CastOnUnit(mob, PacketCast);
            }

            if (Config.Item("eJungle").GetValue<bool>() && E.IsReady())
            {
                E.CastOnUnit(mob, PacketCast);
            }
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
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (Config.Item("ultiDmgDraw").GetValue<bool>())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.R, 1);
            }
            return (float) dmg;
        }
    }
}