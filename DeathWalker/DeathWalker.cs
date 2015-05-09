using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DetuksSharp.Prediction;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace DetuksSharp
{

    public class DeathWalker
    {

        //Spells that reset the attack timer.
        private static readonly string[] AttackResets =
        {
            "dariusnoxiantacticsonh", "fioraflurry", "garenq",
            "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "luciane", "lucianw",
            "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade",
            "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack",
            "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq",
            "xenzhaocombotarget", "yorickspectral", "reksaiq"
        };

        //Spells that are not attacks even if they have the "attack" word in their name.
        private static readonly string[] NoAttacks =
        {
            "jarvanivcataclysmattack", "monkeykingdoubleattack",
            "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack", "zyragraspingplantattack2",
            "zyragraspingplantattackfire", "zyragraspingplantattack2fire", "viktorpowertransfer"
        };

        //Spells that are attacks even if they dont have the "attack" word in their name.
        private static readonly string[] Attacks =
        {
            "caitlynheadshotmissile", "frostarrow", "garenslash2",
            "kennenmegaproc", "lucianpassiveattack", "masteryidoublestrike", "quinnwenhanced", "renektonexecute",
            "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "xenzhaothrust2",
            "xenzhaothrust3", "viktorqbuff"
        };

        public static int now
        {
            get { return (int)DateTime.Now.TimeOfDay.TotalMilliseconds; }
        }

        public static Menu menu;
        public enum Mode
        {
            Combo,
            Harass,
            LaneClear,
            LaneFreeze,
            Lasthit,
            Flee,
            None,
        }

        public static Mode CurrentMode
        {
            get
            {
                if (menu.Item("Combo_Key").GetValue<KeyBind>().Active)
                    return Mode.Combo;
                if (menu.Item("Harass_Key").GetValue<KeyBind>().Active)
                    return Mode.Harass;
                if (menu.Item("LaneClear_Key").GetValue<KeyBind>().Active)
                    return Mode.LaneClear;
                if (menu.Item("LastHit_Key").GetValue<KeyBind>().Active)
                    return Mode.Lasthit;
                return Mode.None;
            }
        }


        public delegate void BeforeAttackEvenH(BeforeAttackEventArgs args);
        public delegate void AfterAttackEvenH(AttackableUnit unit, AttackableUnit target);
        public delegate void OnUnkillableEvenH(AttackableUnit unit, AttackableUnit target, int msTillDead);

        public static event BeforeAttackEvenH BeforeAttack;
        public static event AfterAttackEvenH AfterAttack;
        public static event OnUnkillableEvenH OnUnkillable;
        public static Obj_AI_Hero player = ObjectManager.Player;

        public static int lastDmg = HealthDeath.now;

        private static int lastAutoAttack = 0;
        private static int lastAutoAttackMove = 0;
        private static int lastmove = 0;

        private static bool attack = true;

        private static bool disableNextAttack = false;

        private static bool playerStoped = false;

        private static AttackableUnit killUnit = null;

        public static Obj_AI_Base ForcedTarget = null;


        public static AttackableUnit lastAutoAttackUnit = null;


        public static List<Obj_AI_Hero> AllEnemys = new List<Obj_AI_Hero>();
        public static List<Obj_AI_Hero> AllAllys = new List<Obj_AI_Hero>();

        public static List<Obj_AI_Turret> EnemyTowers = new List<Obj_AI_Turret>();

        public static List<Obj_BarracksDampener> EnemyBarracs = new List<Obj_BarracksDampener>();

        public static List<Obj_HQ> EnemyHQ = new List<Obj_HQ>();


        private static void init()
        {
            //While testing menu
            AllEnemys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy).ToList();
            AllAllys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly).ToList();

            EnemyTowers = ObjectManager.Get<Obj_AI_Turret>().Where(tow => tow.IsEnemy).ToList();

            EnemyBarracs = ObjectManager.Get<Obj_BarracksDampener>().Where(tow => tow.IsEnemy).ToList();

            EnemyHQ = ObjectManager.Get<Obj_HQ>().Where(tow => tow.IsEnemy).ToList();

        }

        private void onLoad(EventArgs args)
        {
            /*Config = new Menu("DeathWalker", "dWalk", true);

            AddToMenu(Config);
            Config.AddToMainMenu();*/
            Drawing.OnDraw += onDraw;


            Obj_AI_Base.OnProcessSpellCast += onStartAutoAttack;
            Spellbook.OnStopCast += onStopAutoAttack;


            Obj_AI_Base.OnDamage += onDamage;

            Game.OnUpdate += OnUpdate;

        }

        private static void onCreate(GameObject sender, EventArgs args)
        {
            //Console.WriteLine("sender: "+sender.Name+" type: "+sender.Type);

            if (sender is MissileClient)
            {
                var mis = (MissileClient) sender;

                if (mis.SpellCaster.IsMe && IsAutoAttack(mis.SData.Name))
                {
                    FireAfterAttack(player,(AttackableUnit) mis.Target);
                }
            }
        }

        private static void onStopAutoAttack(Spellbook sender, SpellbookStopCastEventArgs args)
        {
            if (sender.Owner.IsMe && args.DestroyMissile)
            {
                lastAutoAttack = 0;
                lastAutoAttackMove = 0;
            }
        }

        private static void onStartAutoAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if(!sender.IsMe)
                return;
            /*foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(args.SData))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(args.SData);
                Console.WriteLine("{0}={1}", name, value);
            }*/
            if (isAutoAttackReset(args.SData.Name) && sender.IsMe)
            {
                
                Utility.DelayAction.Add((int)250, resetAutoAttackTimer);
            }
            if (IsAutoAttack(args.SData.Name))
            {
                lastAutoAttack = now;
                lastAutoAttackMove = now;
            }
            //Fire after attack!
            if (sender.IsMeele)
                Utility.DelayAction.Add(
                    (int)(sender.AttackCastDelay * 1000 + 40), () => FireAfterAttack(sender, (AttackableUnit)args.Target));
        }


        private static void onDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (args.SourceNetworkId != player.NetworkId)
                return;


        }

        private static void onDraw(EventArgs args)
        {
            Utility.DrawCircle(player.Position, player.AttackRange+player.BoundingRadius, Color.Green);

           // Drawing.DrawText(100, 100, Color.Red, "targ Spells: " + HealthDeath.activeDamageMakers.Count + " : " + canAttackAfter());
            foreach (var enemy in ObjectManager.Get<Obj_AI_Base>().Where(ene => ene != null && ene.IsValidTarget(1000) && ene.IsEnemy && ene.Distance(player,true)<1000*1000))
            {
                var timeToHit = timeTillDamageOn(enemy);

                var hp = HealthDeath.getLastHitPredPeriodic(enemy, (int) timeToHit);
                if (hp <= player.GetAutoAttackDamage(enemy) && hp>0)
                {
                    Render.Circle.DrawCircle(enemy.Position,56,Color.Green);
                }
            }

        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                if(CurrentMode != Mode.None)
                    deathWalk(Game.CursorPos, getBestTarget());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void deathWalk(Vector3 goalPosition, AttackableUnit target)
        {

            if (target != null && canAttack() && inAutoAttackRange(target))
            {
                if (player.IssueOrder(GameObjectOrder.AttackUnit, target))
                {
                    FireBeforeAttack(target);
                    playerStoped = false;
                    lastAutoAttack = now;
                    lastAutoAttackMove = now;
                    lastAutoAttackUnit = target;
                }
            }
            if (canMove())
            {
                if (target != null && CurrentMode == Mode.Lasthit)
                    killUnit = target;
                if (killUnit != null && !(killUnit is Obj_AI_Hero) && killUnit.IsValid && !killUnit.IsDead && killUnit.Position.Distance(player.Position) > getRealAutoAttackRange(killUnit) - 30)//Get in range
                    moveTo(killUnit.Position);
                moveTo(goalPosition);
            }
        }

        public static AttackableUnit getBestTarget()
        {
            if (ForcedTarget != null)
            {
                if (inAutoAttackRange(ForcedTarget))
                    return ForcedTarget;
                ForcedTarget = null;
            }

            var enemiesAround = ObjectManager.Get<Obj_AI_Base>()
                .Where(targ => targ.IsValidTarget(getTargetSearchDist()) && targ.IsEnemy).ToList();

            Obj_AI_Base best = null;

            //Lat hit
            float bestPredHp = float.MaxValue;
            if (CurrentMode == Mode.Harass || CurrentMode == Mode.Lasthit || CurrentMode == Mode.LaneClear)
            {
                //Last hit
                foreach (var targ in enemiesAround)
                {
                    var hpOnDmgPred = HealthDeath.getLastHitPredPeriodic(targ, timeTillDamageOn(targ));
                    if (hpOnDmgPred <= 0 && (lastAutoAttackUnit == null || lastAutoAttackUnit.NetworkId != targ.NetworkId))
                        FireOnUnkillable(player, targ,HealthDeath.getTimeTillDeath(targ));
                    if (hpOnDmgPred <= 0 || hpOnDmgPred > (int) player.GetAutoAttackDamage(targ, true))
                        continue;
                    if (best == null || hpOnDmgPred < bestPredHp)
                    {
                        best = targ;
                        bestPredHp = hpOnDmgPred;
                    }
                }
                if (best != null)
                    return best;
            }

            //check motherfuckers that are attacked by tower
            if (CurrentMode == Mode.Harass || CurrentMode == Mode.Lasthit || CurrentMode == Mode.LaneClear)
            {

                foreach (var targ in enemiesAround)
                {
                    var towerShot = HealthDeath.attackedByTurret(targ);
                    if (towerShot == null) continue;
                    var hpOnDmgPred = HealthDeath.getLastHitPredPeriodic(targ, towerShot.hitOn+10-now);

                    var aa = player.GetAutoAttackDamage(targ);
                   // Console.WriteLine("AAdmg: " + aa + " Hp after: " + hpOnDmgPred + " hit: " + (towerShot.hitOn - now));
                    if (hpOnDmgPred > aa && hpOnDmgPred <= aa*2.2f)
                    {
                        //Console.WriteLine("Tower under shoting");
                        //Notifications.AddNotification("Tower shoot");
                        //2x hit tower target
                        return targ;
                    }
                }
            }

            var hero = GetBestHeroTarget();

            if (hero != null)
                return hero;

            /* turrets / inhibitors / nexus */
            if (CurrentMode == Mode.LaneClear)
            {
                /* turrets */
                foreach (var turret in
                   EnemyTowers.Where(t => t.IsValidTarget() && inAutoAttackRange(t)))
                {
                    return turret;
                }

                /* inhibitor */
                foreach (var turret in
                    EnemyBarracs.Where(t => t.IsValidTarget() && inAutoAttackRange(t)))
                {
                    return turret;
                }

                /* nexus */
                foreach (var nexus in
                    EnemyHQ.Where(t => t.IsValidTarget() && inAutoAttackRange(t)))
                {
                    return nexus;
                }
            }


            //Laneclear
            if (CurrentMode == Mode.LaneClear && !ShouldWait())
            {
                best = enemiesAround
                    .OrderByDescending(targ => targ.Health).FirstOrDefault();
            }


            return best;
        }

        private static Obj_AI_Base GetBestHeroTarget()
        {
            Obj_AI_Hero killableEnemy = null;
            var hitsToKill = double.MaxValue;
            foreach (var enemy in AllEnemys.Where(hero => hero.IsValidTarget() && inAutoAttackRange(hero)))
            {
                var killHits = CountKillhits(enemy);
                if (killableEnemy != null && !(killHits < hitsToKill))
                    continue;
                killableEnemy = enemy;
                hitsToKill = killHits;
            }
            return hitsToKill < 4 ? killableEnemy : TargetSelector.GetTarget(player.AttackRange+player.BoundingRadius + 100, TargetSelector.DamageType.Physical);
        }

        private static double CountKillhits(Obj_AI_Hero enemy)
        {
            return enemy.Health / player.GetAutoAttackDamage(enemy);
        }

        private static bool ShouldWait()
        {
           /* var cEnemy = getCloestEnemyChamp();

            bool enemySoonInRange = cEnemy != null &&
                                    inAutoAttackRange(cEnemy,
                                        LeagueSharp.Common.Prediction.GetPrediction(cEnemy,player.AttackDelay*1000).UnitPosition.To2D());
            if (enemySoonInRange)
                return true;*/

            bool minDeadSoon =
                ObjectManager.Get<Obj_AI_Minion>()
            .Any(
            minion =>
            minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral &&
            inAutoAttackRange(minion) &&
            HealthPrediction.LaneClearHealthPrediction(
            minion, (int)((player.AttackDelay * 1000) * 2.3f), menu.Item("farmDelay").GetValue<Slider>().Value) <= player.GetAutoAttackDamage(minion));
            return minDeadSoon;
        }

        public static Obj_AI_Hero getCloestEnemyChamp()
        {
            return AllEnemys
                .Where(ob => ob.IsValid && !ob.IsDead)
                .OrderBy(ob => ob.Distance(player, true))
                .FirstOrDefault();
        }

        public static float getTargetSearchDist()
        {
            return player.AttackRange + player.BoundingRadius + menu.Item("runCS").GetValue<Slider>().Value;
        }

        public static int timeTillDamageOn(Obj_AI_Base unit)
        {
            var dist = unit.ServerPosition.Distance(player.ServerPosition);
            int addTime = -menu.Item("farmDelay").GetValue<Slider>().Value;//some farm delay
            if (!inAutoAttackRange(unit))//+ check if want to move to killabel minion and range it wants to
            {
                var realDist = realDistanceTill(unit);
                var aaRange = getRealAutoAttackRange(unit);

                addTime+= (int)(((realDist - aaRange)*1000)/player.MoveSpeed);
            }

            if (player.IsMeele)
            {
                return (int)(canAttackAfter() + player.AttackCastDelay * 1000) + addTime;
            }
            else
            {
                var misDist = dist;
                return (int)(canAttackAfter() + player.AttackCastDelay * 1000 + (misDist * 1000) / player.BasicAttack.MissileSpeed) + addTime;
            }
        }

        public static bool inAutoAttackRange(AttackableUnit unit)
        {
            if (!unit.IsValidTarget())
            {
                return false;
            }
            var myRange = getRealAutoAttackRange(unit);
            return
                Vector2.DistanceSquared(
                    (unit is Obj_AI_Base) ? ((Obj_AI_Base)unit).ServerPosition.To2D() : unit.Position.To2D(),
                    player.ServerPosition.To2D()) <= myRange * myRange;
        }

        public static bool isAutoAttackReset(string name)
        {
            return AttackResets.Contains(name.ToLower());
        }

        public static bool IsAutoAttack(string name)
        {
            Console.WriteLine(name);
            return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower())) ||
            Attacks.Contains(name.ToLower());
        }

        public static bool inAutoAttackRange(AttackableUnit unit, Vector2 pos)
        {
            if (!unit.IsValidTarget())
            {
                return false;
            }
            var myRange = getRealAutoAttackRange(unit);
            return
                Vector2.DistanceSquared(
                    pos,
                    player.ServerPosition.To2D()) <= myRange * myRange;
        }

        public static float getRealAutoAttackRange(AttackableUnit unit)
        {
            var result = player.AttackRange + player.BoundingRadius;
            if (unit.IsValidTarget())
            {
                return result + unit.BoundingRadius;
            }
            return result;
        }

        public static void moveTo(Vector3 goalPosition)
        {
            if (now - lastmove < 80)//Humanizer
                return;
            if (player.ServerPosition.Distance(goalPosition) < 60)
            {
                if (!playerStoped)
                {
                    player.IssueOrder(GameObjectOrder.Stop, player.ServerPosition);
                    playerStoped = true;
                }
                return;
            }
            playerStoped = false;
            if (player.IssueOrder(GameObjectOrder.MoveTo, goalPosition))
                lastmove = now;
        }

        public static void setAttack(bool val)
        {
            attack = val;
        }

        public static bool canAttack()
        {
            return canAttackAfter() == 0 && attack;
        }

        public static int canAttackAfter()
        {
            var after = lastAutoAttack + player.AttackDelay*1000 - now + Game.Ping;
            return (int)(after > 0 ? after : 0);
        }

        public static bool canMove()
        {
            return canMoveAfter() == 0;
        }

        public static int canMoveAfter()
        {
            var after = lastAutoAttack + player.AttackCastDelay * 1000 - now + Game.Ping + menu.Item("WindUp").GetValue<Slider>().Value;
            return (int)(after > 0 ? after : 0);
        }

        public static void resetAutoAttackTimer()
        {
            Console.WriteLine("Reset AA");
            lastAutoAttack = 0;
            lastAutoAttackMove = 0;
        }

        public static float realDistanceTill(AttackableUnit unit)
        {
            float dist = 0;
            var dists = player.GetPath(unit.Position);
            if (dists.Count() == 0)
                return 0;
            Vector3 from = dists[0];
            foreach (var to in dists)
            {
                dist += Vector3.Distance(from, to);
                from = to;
            }
            return dist;
        }

        public class BeforeAttackEventArgs
        {
            public AttackableUnit Target;
            public Obj_AI_Base Unit = ObjectManager.Player;
            private bool _process = true;
            public bool Process
            {
                get
                {
                    return _process;
                }
                set
                {
                    disableNextAttack = !value;
                    _process = value;
                }
            }
        }

        private static void FireBeforeAttack(AttackableUnit target)
        {
            if (BeforeAttack != null)
            {
                BeforeAttack(new BeforeAttackEventArgs
                {
                    Target = target
                });
            }
            else
            {
                disableNextAttack = false;
            }
        }

        private static void FireAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            lastAutoAttackMove = 0;
            //set can move
            Console.WriteLine("after attack");
            if (AfterAttack != null)
            {
                AfterAttack(unit, target);
            }
        }

        private static void FireOnUnkillable(AttackableUnit unit, AttackableUnit target, int msTillDead)
        {
            lastAutoAttackMove = 0;
            //set can move
            if (OnUnkillable != null)
            {
                OnUnkillable(unit, target, msTillDead);
            }
        }

        public static void AddToMenu(Menu menuIn)
        {
            menuIn.AddItem(new MenuItem("Combo_Key", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            menuIn.AddItem(new MenuItem("Harass_Key", "harass Key").SetValue(new KeyBind('C', KeyBindType.Press)));
            menuIn.AddItem(new MenuItem("LaneClear_Key", "LaneClear Key").SetValue(new KeyBind('V', KeyBindType.Press)));
            menuIn.AddItem(new MenuItem("LastHit_Key", "LastHir Key").SetValue(new KeyBind('X', KeyBindType.Press)));
            menuIn.AddItem(new MenuItem("WindUp", "WindUp").SetValue(new Slider(60, 0, 250)));
            menuIn.AddItem(new MenuItem("farmDelay", "Farm delay").SetValue(new Slider(60, 0, 250)));
            menuIn.AddItem(new MenuItem("runCS", "Run CS distance").SetValue(new Slider(60, 0, 500)));

            menu = menuIn;

            init();

            Drawing.OnDraw += onDraw;


            Obj_AI_Base.OnProcessSpellCast += onStartAutoAttack;
            Spellbook.OnStopCast += onStopAutoAttack;

            Obj_AI_Base.OnDamage += onDamage;

            GameObject.OnCreate += onCreate;

            Game.OnUpdate += OnUpdate;
        }
    }
}
