#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Olafisback
{
    internal class Program
    {
        public const string ChampionName = "Olaf";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;

        //Menu
        public static Menu Config;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325);

            Q.SetSkillshot(0.25f, 90f, 1600f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);

            //Create the menu
            Config = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            //Load the orbwalker and add it to the submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("WRange", "W range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("ERange", "E range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.AddToMainMenu();

            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat(ChampionName + " Loaded!");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass();
            }
        }

        private static void Combo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);

            if (target.IsValidTarget() && Config.Item("UseQCombo").GetValue<bool>() && Q.IsReady() && Player.Distance(target) <= Q.Range)
            {
                Q.Cast(target, true);
            }
            if (target.IsValidTarget() && Config.Item("UseECombo").GetValue<bool>() && E.IsReady() && Player.Distance(target) <= E.Range)
            {
                E.CastOnUnit(target);
            }
            if (target.IsValidTarget() && Config.Item("UseWCombo").GetValue<bool>() && W.IsReady() && Player.Distance(target) <= Player.AttackRange)
            {
                W.Cast();
            }
        }
        private static void Harass()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (target.IsValidTarget() && Q.IsReady() && Config.Item("UseQHarass").GetValue<bool>() && Player.Distance(target) <= Q.Range)
            {
                PredictionOutput Qpredict = Q.GetPrediction(target);
                if (Qpredict.Hitchance >= HitChance.High)
                    Q.Cast(Qpredict.CastPosition);
            }
            if (E.IsReady() && Config.Item("UseEHarass").GetValue<bool>() && Player.Distance(target) <= E.Range)
                E.CastOnUnit(target);
        }

    }
}
