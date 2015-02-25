using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace MasterYi
{
    class Program
    {
        private static String ChampionName = "Master Yi";
        public static Obj_AI_Hero Player;
        private static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell Q, W, E, R;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.ChampionName != ChampionName)
            {
                return;
            }

            _menu = new Menu("Smart Master Yi", "masteryi", true);

            var orbwalkerMenu = new Menu("Orbwalker", "masteryi.orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);
        }
    }
}
