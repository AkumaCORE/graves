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

        public static Menu menu, ComboMenu, DrawingsMenu, FarmMenu, HarassMenu, UpdateMenu, KSMenu;
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Spell.Skillshot R1;
        private static Vector3 mousePos { get { return Game.CursorPos; } }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 950, SkillShotType.Linear, 250, 2000, 60);
            W = new Spell.Skillshot(SpellSlot.W, 850, SkillShotType.Circular, 250, 1650, 150);
            E = new Spell.Skillshot(SpellSlot.E, 425, SkillShotType.Linear);
            R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Linear, 250, 2100, 100);
            R1 = new Spell.Skillshot(SpellSlot.R, 1400, SkillShotType.Linear, 250, 2100, 120);

            menu = MainMenu.AddMenu("Perfect Graves", "PerfectGraves");

            ComboMenu = menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.Add("useQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("useWCombo", new CheckBox("Use W"));
            ComboMenu.Add("useECombo", new CheckBox("Use E"));
            ComboMenu.Add("useRCombo", new CheckBox("Fast R Combo"));
            ComboMenu.Add("useItems", new CheckBox("Use Items"));
            ComboMenu.AddLabel("BOTRK,Bilgewater Cutlass Settings");
            ComboMenu.Add("botrkHP", new Slider("My HP < %", 60, 0, 100));
            ComboMenu.Add("botrkenemyHP", new Slider("Enemy HP < %", 60, 0, 100));

            KSMenu = menu.AddSubMenu("KS Settings", "KSSettings");
            KSMenu.Add("useQKS", new CheckBox("Use Q KS"));
            KSMenu.Add("useRKS", new CheckBox("Use R KS"));

            HarassMenu = menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("useItems", new CheckBox("Use Items"));

            FarmMenu = menu.AddSubMenu("Lane/Jungle Clear Settings", "Farm");
            FarmMenu.AddLabel("Lane Clear");
            FarmMenu.Add("useQ", new CheckBox("Use Q//Does not Work for Now."));

            FarmMenu.AddLabel("Jungle Clear");
            FarmMenu.Add("Qjungle", new CheckBox("Use Q"));
            FarmMenu.Add("QjungleMana", new Slider("Mana < %", 45, 0, 100));
            FarmMenu.Add("Ejungle", new CheckBox("Use E"));
            FarmMenu.Add("EjungleMana", new Slider("Mana < %", 45, 0, 100));

            DrawingsMenu = menu.AddSubMenu("Draw Settings", "Drawings");
            DrawingsMenu.Add("DrawQ", new CheckBox("Draw Q"));
            DrawingsMenu.Add("DrawW", new CheckBox("Draw W"));
            DrawingsMenu.Add("DrawE", new CheckBox("Draw E"));
            DrawingsMenu.Add("DrawR", new CheckBox("Draw R"));
            DrawingsMenu.Add("DrawR1", new CheckBox("Draw Extended R"));

            UpdateMenu = menu.AddSubMenu("Updates", "Update");
            UpdateMenu.AddLabel("0.0.3 Updated");
            UpdateMenu.AddLabel("+Jungle Clear Added!");
            UpdateMenu.AddLabel("+Kill Steal Added!");
            UpdateMenu.AddLabel("+Cast Q Improwed!");
            UpdateMenu.AddLabel("-Lane Clear Closed for Now.");



            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnTick(EventArgs args)
        {

            Orbwalker.ForcedTarget = null;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                LaneClear();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
            KillSteal();

        }
        public static void KillSteal()
        {
            var useR = KSMenu["useRKS"].Cast<CheckBox>().CurrentValue;
            var useQ = KSMenu["useQKS"].Cast<CheckBox>().CurrentValue;
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var targetR = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            var targetR1 = TargetSelector.GetTarget(R1.Range, DamageType.Physical);
            if (useR && R.IsReady() && targetR.IsValidTarget(R.Range) && targetR.Health < RDamage(targetR) && R.GetPrediction(targetR).HitChance >= HitChance.Medium)
            {
                R.Cast(R.GetPrediction(targetR).CastPosition);
            }
            if (useR && R.IsReady() && targetR1.IsValidTarget(R1.Range) && targetR1.Health < R1Damage(targetR1) && R.GetPrediction(targetR1).HitChance >= HitChance.Medium)
            {
                R.Cast(R.GetPrediction(targetR1).CastPosition);
            }
            if (useQ && Q.IsReady() && targetQ.IsValidTarget(Q.Range) && targetQ.Health < QDamage(targetQ) && Q.GetPrediction(targetQ).HitChance >= HitChance.Medium)
            {
                Q.Cast(Q.GetPrediction(targetQ).CastPosition);
            }
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 60, 80, 100, 120, 140 }[Program.Q.Level] + 0.75 * _Player.FlatPhysicalDamageMod));
        }
        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 250, 400, 550 }[Program.R.Level] + 1.5 * _Player.FlatPhysicalDamageMod));
        }
        public static float R1Damage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 200, 320, 440 }[Program.R.Level] + 1.2 * _Player.FlatPhysicalDamageMod));
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
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var UseItems = HarassMenu["useItems"].Cast<CheckBox>().CurrentValue;
            var useQ = HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue;
            if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
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
            var targetE = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var targetR = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            var targetR1 = TargetSelector.GetTarget(R1.Range, DamageType.Physical);
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (!Player.HasBuff("gravesbasicattackammo2"))
            {
                if (useE && E.IsReady() && targetE.IsValidTarget(E.Range))
                {
                    E.Cast(mousePos);
                }
            }
            if (useR && R.IsReady() && targetR.IsValidTarget(R.Range) && targetR.Health < RDamage(targetR) && R.GetPrediction(targetR).HitChance >= HitChance.Medium)
            {
                R.Cast(R.GetPrediction(targetR).CastPosition);
            }
            if (useQ && Q.IsReady() && Q.GetPrediction(targetQ).HitChance >= HitChance.Medium)
            {
                Q.Cast(Q.GetPrediction(targetQ).CastPosition);
            }
            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(1300) && !o.IsDead && !o.IsZombie))
            {
                if (useW && W.IsReady() && W.GetPrediction(target).HitChance >= HitChance.Medium && target.IsValidTarget(W.Range))
                {
                    W.Cast(target);
                }
                if (UseItems)
                {
                    HandleItems();
                }
            }
        }
        public static void LaneClear()
        {
            /*
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(Q.Range));
            var useQ = FarmMenu["useQ"].Cast<CheckBox>().CurrentValue;
            if (useQ && Q.IsReady() && Q.IsInRange(minions.FirstOrDefault().Position) && Q.GetPrediction(minions.FirstOrDefault()).HitChance >= HitChance.High)
            {
                //Q.Cast(minions.First().Position);
            }
            */
        }

        private static void JungleClear()
        {
            var useQ = FarmMenu["Qjungle"].Cast<CheckBox>().CurrentValue;
            var useQMana = FarmMenu["QjungleMana"].Cast<Slider>().CurrentValue;
            var useE = FarmMenu["Ejungle"].Cast<CheckBox>().CurrentValue;
            var useEMana = FarmMenu["EjungleMana"].Cast<Slider>().CurrentValue;
                foreach (var monster in EntityManager.MinionsAndMonsters.Monsters)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > useQMana)
                {
                    Q.Cast(monster);
                }
                if (useE && E.IsReady() && Player.Instance.HealthPercent > useEMana)
                {
                    E.Cast(mousePos);
                }

                HandleItems();
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

            if (DrawingsMenu["DrawR1"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, R1.Range, System.Drawing.Color.Red);
            }

        }
    }
}

