using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using System.Drawing;


namespace PerfectGraves
{
    class Program
    {

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static Menu menu, ComboMenu, DrawingsMenu, FarmMenu, HarassMenu, UpdateMenu;
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Spell.Chargeable Passive;
        private static Vector3 mousePos { get { return Game.CursorPos; } }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 250, 2000, 40);
            W = new Spell.Skillshot(SpellSlot.W, 850, SkillShotType.Circular, 250, 1650, 200);
            E = new Spell.Skillshot(SpellSlot.E, 425, SkillShotType.Circular);
            R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Linear, 250, 2100, 100);

            menu = MainMenu.AddMenu("Perfect Graves", "PerfectGraves");

            ComboMenu = menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.Add("useQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("useWCombo", new CheckBox("Use W"));
            ComboMenu.Add("useECombo", new CheckBox("Use E"));
            ComboMenu.Add("useRCombo", new CheckBox("Use R"));
            ComboMenu.Add("useItems", new CheckBox("Use Items"));
            ComboMenu.AddLabel("BOTRK,Bilgewater Cutlass Settings");
            ComboMenu.Add("botrkHP", new Slider("My HP < %", 60, 0, 100));
            ComboMenu.Add("botrkenemyHP", new Slider("Enemy HP < %", 60, 0, 100));

            HarassMenu = menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("useItems", new CheckBox("Use Items"));

            FarmMenu = menu.AddSubMenu("LaneClear Settings", "Farm");
            FarmMenu.Add("useQ", new CheckBox("Use Q"));

            DrawingsMenu = menu.AddSubMenu("Draw Settings", "Drawings");
            DrawingsMenu.Add("DrawQ", new CheckBox("Draw Q"));
            DrawingsMenu.Add("DrawW", new CheckBox("Draw W"));
            DrawingsMenu.Add("DrawE", new CheckBox("Draw E"));
            DrawingsMenu.Add("DrawR", new CheckBox("Draw R"));

            UpdateMenu = menu.AddSubMenu("Updates", "Update");
            UpdateMenu.AddLabel("0.0.1 Shared");



            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) LaneClear();
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 250, 400, 550 }[Program.R.Level] + 1.5 * _Player.FlatPhysicalDamageMod));
        }
        internal static void HandleItems()
        {
            var botrktarget = TargetSelector.GetTarget(550, DamageType.Physical);
            var target = TargetSelector.GetTarget(400, DamageType.Physical);
            var useItem = ComboMenu["useItems"].Cast<CheckBox>().CurrentValue;
            var useBotrkHP = ComboMenu["botrkHP"].Cast<Slider>().CurrentValue;
            var useBotrkEnemyHP = ComboMenu["botrkenemyHP"].Cast<Slider>().CurrentValue;
            //HYDRA
            if (useItem && Item.HasItem(3077) && Item.CanUseItem(3077))
                Item.UseItem(3077);

            //TİAMAT
            if (useItem && Item.HasItem(3074) && Item.CanUseItem(3074))
                Item.UseItem(3074);

            //NEW ITEM
            if (useItem && Item.HasItem(3748) && Item.CanUseItem(3748))
                Item.UseItem(3748);

            //BİLGEWATER CUTLASS
            if (useItem && Item.HasItem(3144) && Item.CanUseItem(3144) && botrktarget.HealthPercent <= useBotrkEnemyHP && _Player.HealthPercent <= useBotrkHP)
                Item.UseItem(3144, botrktarget);

            //BOTRK
            if (useItem && Item.HasItem(3153) && Item.CanUseItem(3153) && botrktarget.HealthPercent <= useBotrkEnemyHP && _Player.HealthPercent <= useBotrkHP)
                Item.UseItem(3153, botrktarget);

            //YOUMU
            if (useItem && Item.HasItem(3142) && Item.CanUseItem(3142))
                Item.UseItem(3142);

            //QSS
            if (useItem && Item.HasItem(3140) && Item.CanUseItem(3140) && (_Player.HasBuffOfType(BuffType.Charm) || _Player.HasBuffOfType(BuffType.Blind) || _Player.HasBuffOfType(BuffType.Fear) || _Player.HasBuffOfType(BuffType.Polymorph) || _Player.HasBuffOfType(BuffType.Silence) || _Player.HasBuffOfType(BuffType.Sleep) || _Player.HasBuffOfType(BuffType.Snare) || _Player.HasBuffOfType(BuffType.Stun) || _Player.HasBuffOfType(BuffType.Suppression) || _Player.HasBuffOfType(BuffType.Taunt)))
            {
                Item.UseItem(3140);
            }
        }
        public static void Harass()
        {
            var target = TargetSelector.GetTarget(_Player.GetAutoAttackRange(), DamageType.Physical);
            var UseItems = HarassMenu["useItems"].Cast<CheckBox>().CurrentValue;
            var useQ = HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue;

            if (UseItems)
            {
                HandleItems();
            }
            if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High)
            {
                Q.Cast(Q.GetPrediction(target).CastPosition);
            }
        }
        public static void Combo()
        {
            var UseItems = ComboMenu["useItems"].Cast<CheckBox>().CurrentValue;
            var useQ = ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["useRCombo"].Cast<CheckBox>().CurrentValue;
            

            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(1100) && !o.IsDead && !o.IsZombie))
            {
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(Q.GetPrediction(target).CastPosition);
                }
                if (useW && W.IsReady() && W.GetPrediction(target).HitChance >= HitChance.Medium && target.IsValidTarget(W.Range))
                {
                    W.Cast(target);
                }
                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast(mousePos);
                }
                if (useR && R.IsReady() && R.GetPrediction(target).HitChance >= HitChance.Medium && target.Health <= RDamage(target) && target.IsValidTarget(R.Range))
                {
                    R.Cast(R.GetPrediction(target).CastPosition);
                }
                if (UseItems)
                {
                    HandleItems();
                }
            }
        }
        public static void LaneClear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(Q.Range));
            var useQ = FarmMenu["useQ"].Cast<CheckBox>().CurrentValue;
            if (useQ && Q.IsReady() && Q.IsInRange(minions.FirstOrDefault().Position) && Q.GetPrediction(minions.FirstOrDefault()).HitChance >= HitChance.High)
            {
                Q.Cast(minions.First().Position);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingsMenu["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, Q.Range, System.Drawing.Color.Red);
            }

            if (DrawingsMenu["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, W.Range, System.Drawing.Color.Red);
            }

            if (DrawingsMenu["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, E.Range, System.Drawing.Color.Red);
            }

            if (DrawingsMenu["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, R.Range, System.Drawing.Color.Red);
            }

        }
    }
}

