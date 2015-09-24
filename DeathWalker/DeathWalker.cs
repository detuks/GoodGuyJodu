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

        public static bool azir = false;

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

        //cant cancel attacks
        private static readonly string[] AttacksCantCancel =
        {
            "azirbasicattacksoldier",
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

        private static int previousAttack = 0;

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


        public static List<Obj_AI_Base> enemiesAround = new List<Obj_AI_Base>();

        public static List<Obj_HQ> EnemyHQ = new List<Obj_HQ>();

        public static int attackTime
        {
            get { return (int) (player.AttackDelay*1000 + menu.Item("AttDelay").GetValue<Slider>().Value); }
        }

        public static float moveTime
        {
            get { return (int)(player.AttackCastDelay * 1000 + menu.Item("MovDelay").GetValue<Slider>().Value); }
        }

        private static DeathDraw dDraw;

        private static void init()
        {
            dDraw = new DeathDraw();
            //While testing menu
            AllEnemys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy).ToList();
            AllAllys = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly).ToList();

            EnemyTowers = ObjectManager.Get<Obj_AI_Turret>().Where(tow => tow.IsEnemy).ToList();

            EnemyBarracs = ObjectManager.Get<Obj_BarracksDampener>().Where(tow => tow.IsEnemy).ToList();

            EnemyHQ = ObjectManager.Get<Obj_HQ>().Where(tow => tow.IsEnemy).ToList();

        }

        private void onLoad(EventArgs args)
        {
        }

        private static void OnEndScene(EventArgs args)
        {
            var attPrec = ((attackTime - canAttackAfter()) * 100) / attackTime;

            dDraw.draw((int)attPrec);

            Drawing.DrawText(dDraw.mPos.X, dDraw.mPos.Y-20, Color.WhiteSmoke, attPrec+"%");
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
            if(!azir) return;
            if (sender.Name == "AzirSoldier" && sender.IsAlly)
            {
                Obj_AI_Minion myMin = sender as Obj_AI_Minion;
                if (myMin.SkinName == "AzirSoldier")
                    azirSoldiers.Add(myMin);
            }

        }

        private static void onDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                
                var spell = player.Spellbook.GetSpell(args.Slot);
                if (spell.IsAutoAttack() || args.SData.IsAutoAttack())
                {
                    lastAutoAttackMove = 0;
                }

            }
        }

        private static void onStopAutoAttack(Spellbook sender, SpellbookStopCastEventArgs args)
        {
            if (sender.Owner.IsMe && args.DestroyMissile)
            {
                Console.WriteLine("Cancel auto");
                var resetTo = (menu.Item("betaStut").GetValue<bool>()) ? previousAttack : 0;
                lastAutoAttack = resetTo;
                lastAutoAttackMove = resetTo;
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
            if (isAutoAttackReset(args.SData.Name))
            {
                Utility.DelayAction.Add((int)250, resetAutoAttackTimer);
            }

            if (IsAutoAttack(args.SData.Name))
            {
                previousAttack = lastAutoAttack;
                lastAutoAttack = now;
                lastAutoAttackMove = now;
            }
            if (IsCantCancel(args.SData.Name))
            {
                lastAutoAttackMove-=100;
            }
            //Fire after attack!a
            if (sender.IsMelee)
                Utility.DelayAction.Add(
                    (int)(sender.AttackCastDelay * 1000 + 40), () => FireAfterAttack(sender, (AttackableUnit)args.Target));

            
        }

        public static void delayAttackfor(int ms)
        {
           // lastAutoAttack = (lastAutoAttack < now + ms) ? now + ms : lastAutoAttack;
        }


        private static void onDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (args.SourceNetworkId != player.NetworkId)
                return;
            Console.WriteLine("dmg: "+sender.Health+"  : "+player.GetAutoAttackDamage((Obj_AI_Base)sender));


        }

        private static void onDraw(EventArgs args)
        {
            Utility.DrawCircle(player.Position, player.AttackRange + player.BoundingRadius, Color.Green);

            Drawing.DrawText(100, 100, Color.Red, " " + CurrentMode + " : " + HealthDeath.damagerSources.Count);

          //  foreach (var towTar in HealthDeath.activeTowerTargets)
         //   {
           //     Utility.DrawCircle(towTar.Value.target.Position, 50, Color.DarkViolet);
           // }
          
            return;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Base>().Where(ene => ene != null && ene.IsValidTarget(1000) && ene.IsEnemy && ene.Distance(player,true)<1000*1000))
            {
                //var timeToHit = timeTillDamageOn(enemy);

                var pOut = Drawing.WorldToScreen(enemy.Position);
                var count = HealthDeath.damagerSources.Count(ene => ene.Value.isValidDamager() && ene.Value.getTarget().NetworkId == enemy.NetworkId);
                var dmg = HealthDeath.getLaneClearPred(enemy, 1000, true);
                Drawing.DrawText(pOut.X, pOut.Y, Color.Red, "" + count);
                var hp2 = HealthPrediction.LaneClearHealthPrediction(enemy, (int) ((player.AttackDelay*1000)*2.3f),
                    menu.Item("farmDelay").GetValue<Slider>().Value);
                var hp = HealthDeath.getLaneClearPred(enemy, (int)((player.AttackDelay * 1000) * 2.2f));


                if (hp <= getRealAADmg(enemy) && hp > 0)
                {
                    Render.Circle.DrawCircle(enemy.Position,56,Color.Yellow);
                }
            }

        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                deathWalk(Game.CursorPos, CurrentMode != Mode.None ? getBestTarget() : ((azir)?getBestTarget(azir):null), CurrentMode == Mode.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void doAttack(AttackableUnit target)
        {
            if (player.IssueOrder(GameObjectOrder.AttackUnit, target))
            {
                FireBeforeAttack(target);
                playerStoped = false;
                previousAttack = lastAutoAttack;
                lastAutoAttack = now;
                lastAutoAttackMove = now;
                lastAutoAttackUnit = target;
            }
        }

        public static void deathWalk(Vector3 goalPosition)
        {
            if(CurrentMode == Mode.None)
                deathWalk(goalPosition, getBestTarget());
        }

        public static void deathWalk(Vector3 goalPosition, AttackableUnit target = null, bool noMove = false)
        {
            if (target != null && canAttack() && inAutoAttackRange(target))
            {
                doAttack(target);
            }
            if (canMove() && !noMove)
            {
                if (target != null && (CurrentMode == Mode.Lasthit || CurrentMode == Mode.Harass))
                    killUnit = target;
                if (killUnit != null && !(killUnit is Obj_AI_Hero) && killUnit.IsValid && !killUnit.IsDead && killUnit.Position.Distance(player.Position) > getRealAutoAttackRange(killUnit) - 30)//Get in range
                    moveTo(killUnit.Position);
                moveTo(goalPosition);
            }
        }

        public static AttackableUnit getBestTarget(bool onlySolider = false)
        {
            bool soliderHit = false;
            if (ForcedTarget != null && !onlySolider)
            {
                if (inAutoAttackRange(ForcedTarget))
                    return ForcedTarget;
                ForcedTarget = null;
            }
            if (azir)
            {
                enemiesAround = ObjectManager.Get<Obj_AI_Base>()
                .Where(targ => targ.IsValid && inAutoAttackRange(targ) && targ.IsEnemy).ToList();
            }
            else
            {
                enemiesAround = ObjectManager.Get<Obj_AI_Base>()
                    .Where(targ => targ.IsValidTarget(getTargetSearchDist()) && targ.IsEnemy).ToList();
            }

            Obj_AI_Base best = null;

            //Lat hit
            float bestPredHp = float.MaxValue;

            if (azir)
            {
                var hero1 = GetBestHeroTarget(out soliderHit);

                if (hero1 != null && (enemyInAzirRange(hero1) || hero1 is Obj_AI_Minion) && (!onlySolider || soliderHit))
                    return hero1;
            }
            if (!onlySolider)
            //check motherfuckers that are attacked by tower
            if (CurrentMode == Mode.Harass || CurrentMode == Mode.Lasthit || CurrentMode == Mode.LaneClear)
            {

                foreach (var targ in enemiesAround)
                {
                    var towerShot = HealthDeath.attackedByTurret(targ);
                    if (towerShot == null) continue;
                    var hpOnDmgPred = HealthDeath.getLaneClearPred(targ, towerShot.hitOn+10-now);

                    var aa = getRealAADmg(targ);
                   // Console.WriteLine("AAdmg: " + aa + " Hp after: " + hpOnDmgPred + " hit: " + (towerShot.hitOn - now));
                    if (hpOnDmgPred > aa && hpOnDmgPred <= aa*2f)
                    {
                        //Console.WriteLine("Tower under shoting");
                        //Notifications.AddNotification("Tower shoot");
                        //2x hit tower target
                        
                        return targ;
                    }
                }
            }
            if (!onlySolider)
            if (CurrentMode == Mode.Harass || CurrentMode == Mode.Lasthit || CurrentMode == Mode.LaneClear)
            {
                //Last hit
                foreach (var targ in enemiesAround.OrderByDescending(min => HealthDeath.getLastHitPred(min, timeTillDamageOn(min))))
                {
                    var hpOnDmgPred = HealthDeath.getLastHitPredPeriodic(targ, timeTillDamageOn(targ));
                    if (hpOnDmgPred <= 0 && (lastAutoAttackUnit == null || lastAutoAttackUnit.NetworkId != targ.NetworkId))
                        FireOnUnkillable(player, targ, HealthDeath.getTimeTillDeath(targ));
                    if (hpOnDmgPred <= 0 || hpOnDmgPred > (int)getRealAADmg(targ))
                        continue;
                    var cannonBonus = (targ.SkinName == "SRU_ChaosMinionSiege") ? 100 : 0;
                    if (best == null || hpOnDmgPred - cannonBonus < bestPredHp)
                    {
                        best = targ;
                        bestPredHp = hpOnDmgPred;
                    }
                }
                if (best != null)
                    return best;
            }
            var hero = GetBestHeroTarget(out soliderHit);

            if (hero != null && (!onlySolider || soliderHit))
                return hero;
            if (!onlySolider)

                if (ShouldWaitAllTogether())
                    return null;
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

            if (!onlySolider)
            //Laneclear
            if (CurrentMode == Mode.LaneClear)
            {
                best = enemiesAround.Where(min => !ShouldWaitMinion(min))
                    .OrderByDescending(targ => targ.Health+(enemyInAzirRange(targ)?1000:0)).FirstOrDefault();
            }


            return best;
        }

        private static Obj_AI_Base GetBestHeroTarget(out bool soliderHit)
        {
            Obj_AI_Hero killableEnemy = null;
            var hitsToKill = double.MaxValue;

            if (azir)
            {
                foreach (var ene in AllEnemys.OrderBy(enemy => enemy.Health))
                {
                    if (ene == null || ene.IsDead)
                        continue;
                    foreach (var sol in getActiveSoliders())
                    {
                        if (sol == null || sol.IsDead)
                            continue;
                        var solAarange = 325;
                        solAarange *= solAarange;
                        if (ene.ServerPosition.Distance(sol.ServerPosition, true) < solAarange)
                        {
                            soliderHit = true;
                            return ene;
                        }
                        foreach (var around in enemiesAround.Where(arou => arou != null && arou.IsValid && !arou.IsDead && arou.Position.Distance(sol.Position, true) <= ((325) * (325))))
                        {
                            if (around == null || around.IsDead || ene == null)
                                continue;
                            DeathMath.Polygon poly = DeathMath.getPolygonOn(sol, around, 50 + ene.BoundingRadius / 2, 375 + ene.BoundingRadius / 2);
                            var posi = LeagueSharp.Common.Prediction.GetPrediction(ene, player.AttackCastDelay);
                            try
                            {

                                if (posi != null &&
                                    poly.pointInside(posi.UnitPosition.To2D()))
                                {
                                    soliderHit = true;
                                    return around;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            foreach (var enemy in AllEnemys.Where(hero => hero.IsValid && inAutoAttackRange(hero)))
            {
                var killHits = CountKillhits(enemy);
                if (killableEnemy != null && !(killHits < hitsToKill))
                    continue;
                killableEnemy = enemy;
                hitsToKill = killHits;
            }
            soliderHit = false;
            return hitsToKill < 4 ? killableEnemy : TargetSelector.GetTarget(player.AttackRange+player.BoundingRadius + 100, TargetSelector.DamageType.Physical);
        }

        private static double CountKillhits(Obj_AI_Hero enemy)
        {
            return enemy.Health / getRealAADmg(enemy);
        }

        private static bool ShouldWaitAllTogether()
        {
           /* var cEnemy = getCloestEnemyChamp();

            bool enemySoonInRange = cEnemy != null &&
                                    inAutoAttackRange(cEnemy,
                                        LeagueSharp.Common.Prediction.GetPrediction(cEnemy,player.AttackDelay*1000).UnitPosition.To2D());
            if (enemySoonInRange)
                return true;*/

            foreach (var minion in MinionManager.GetMinions(getTargetSearchDist(),MinionTypes.All))
            {
                if (minion.IsValidTarget())
                {
                    //var hpKillable = HealthDeath.getLastHitPredPeriodic(minion, timeTillDamageOn(minion));
                   // if(hpKillable<0)
                    //    continue;
                    var dmgAt = timeTillDamageOn(minion);
                    var hp = HealthDeath.getLaneClearPred(minion, (int)((player.AttackDelay * 1000) * 1.26f));
                    if (hp <= getRealAADmg(minion))
                        return true;
                }
            }
            return false;
        }

        private static bool ShouldWaitMinion(Obj_AI_Base minion)
        {
            var hp = HealthDeath.getLaneClearPred(minion, (int)((player.AttackDelay * 1000) * 2.26f));
            if (hp <= getRealAADmg(minion))
                return true;
            return false;
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
            int addTime = -menu.Item("farmDelay").GetValue<Slider>().Value -((azir)?100:0);//some farm delay
            if (!inAutoAttackRange(unit))//+ check if want to move to killabel minion and range it wants to
            {
                var realDist = realDistanceTill(unit);
                var aaRange = getRealAutoAttackRange(unit);

                addTime+= (int)(((realDist - aaRange)*1000)/player.MoveSpeed);
            }

            if (player.IsMelee || azir)
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

            if (azir && unit is Obj_AI_Base && enemyInAzirRange((Obj_AI_Base)unit))
            {
                return true;
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
            return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower())) ||
            Attacks.Contains(name.ToLower());
        }

        public static bool IsCantCancel(string name)
        {
            return AttacksCantCancel.Contains(name.ToLower());
        }


        public static bool inAutoAttackRange(AttackableUnit unit, Vector2 pos)
        {
            if (!unit.IsValidTarget())
            {
                return false;
            }

            if (azir && unit is Obj_AI_Base && enemyInAzirRange((Obj_AI_Base)unit))
            {
                return true;
            }

            var myRange = getRealAutoAttackRange(unit);
            return
                Vector2.DistanceSquared(
                    pos,
                    player.ServerPosition.To2D()) <= myRange * myRange;
        }

        public static bool inAutoAttackRange(Obj_AI_Base source, AttackableUnit target)
        {
            if (!target.IsValidTarget())
            {
                return false;
            }

            if (source.IsMe && azir && target is Obj_AI_Base && enemyInAzirRange((Obj_AI_Base)target))
            {
                return true;
            }

            var myRange = getRealAutoAttackRange(source,target);
            return
                Vector2.DistanceSquared(
                    target.Position.To2D(),
                    source.ServerPosition.To2D()) <= myRange * myRange;
        }

        public static float getRealAutoAttackRange(AttackableUnit unit)
        {
            return getRealAutoAttackRange(player,unit);
        }

        public static float getRealAutoAttackRange(Obj_AI_Base source,AttackableUnit target)
        {
            var result = source.AttackRange + source.BoundingRadius;
            if (target.IsValidTarget())
            {
                return result + target.BoundingRadius;
            }
            return result;
        }

        public static void moveTo(Vector3 goalPosition)
        {
            if (now - lastmove < 120)//Humanizer
                return;
            if (player.ServerPosition.Distance(goalPosition) < 70)
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
            var after = player.AttackDelay * 1000 + lastAutoAttack - now + menu.Item("AttDelay").GetValue<Slider>().Value;
            return (int)(after > 0 ? after : 0);
        }

        public static bool canMove()
        {
            return canMoveAfter() == 0 && !player.IsAttackingPlayer;
        }

        public static int canMoveAfter()
        {
            var after = lastAutoAttackMove + player.AttackCastDelay * 1000 - now + menu.Item("MovDelay").GetValue<Slider>().Value + ((hyperCharged()) ? 150 : 0);
            return (int)(after > 0 ? after : 0);
        }

        private static bool hyperCharged()
        {
            return player.Buffs.Any(buffs => buffs.Name == "jaycehypercharge");
        }

        public static void resetAutoAttackTimer()
        {
            lastAutoAttack = 0;
            lastAutoAttackMove = 0;
        }

        public static float realDistanceTill(AttackableUnit unit)
        {
            return realDistanceTill(player,unit);
        }

        public static float realDistanceTill(Obj_AI_Base source, AttackableUnit target)
        {
            float dist = 0;
            var dists = source.GetPath(target.Position);
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
            if (AfterAttack != null)
            {
                AfterAttack(unit, target);
            }
        }

        private static void FireOnUnkillable(AttackableUnit unit, AttackableUnit target, int msTillDead)
        {
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
            menuIn.AddItem(new MenuItem("AttDelay", "Attack delay").SetValue(new Slider(0, -100, 250)));
            menuIn.AddItem(new MenuItem("MovDelay", "Move delay").SetValue(new Slider(0, -100, 250)));
            menuIn.AddItem(new MenuItem("farmDelay", "Farm delay").SetValue(new Slider(100, -100, 250)));
            menuIn.AddItem(new MenuItem("runCS", "Run CS distance").SetValue(new Slider(25, 0, 500)));
            menuIn.AddItem(new MenuItem("betaStut", "Beta anti stut").SetValue(true));

            menu = menuIn;

            init();

            Drawing.OnDraw += onDraw;

            Drawing.OnEndScene += OnEndScene;

            Obj_AI_Base.OnProcessSpellCast += onStartAutoAttack;
            Spellbook.OnStopCast += onStopAutoAttack;

            Obj_AI_Base.OnDoCast += onDoCast;

            GameObject.OnCreate += onCreate;
            GameObject.OnDelete += onDelete;
            Obj_AI_Minion.OnPlayAnimation += Obj_AI_Minion_OnPlayAnimation;

            Game.OnUpdate += OnUpdate;
        }


        public static float getRealAADmg(Obj_AI_Base targ)
        {
            if (!azir)
                return (float)player.GetAutoAttackDamage(targ,true);
            var solAround = solidersAroundEnemy(targ);

            if (solAround == 0)
                return (float)player.GetAutoAttackDamage(targ);
            int[] solBaseDmg = {50,55,60,65,70,75,80,85,90,95,100,110,120,130,140,150,160,170};

            var solDmg = solBaseDmg[player.Level - 1] + player.FlatMagicDamageMod*0.6f;

            return (float)player.CalcDamage(targ, Damage.DamageType.Magical, solDmg + (solAround - 1) * solDmg * 0.25f);

        }


        //Azir stuff
        //Tnx Kortatu ;)
        private static Dictionary<int, string> Animations = new Dictionary<int, string>();

        public static List<Obj_AI_Minion> azirSoldiers = new List<Obj_AI_Minion>();

        public static List<Obj_AI_Minion> getUsableSoliders()
        {
            return azirSoldiers.Where(sol => !sol.IsDead && sol != null).ToList();
        }

        public static List<Obj_AI_Minion> getActiveSoliders()
        {
            return azirSoldiers.Where(s => s.IsValid && !s.IsDead && !s.IsMoving && s.ServerPosition.Distance(player.Position,true)<=900*900 /*(!Animations.ContainsKey(s.NetworkId) || Animations[s.NetworkId] != "Inactive")*/).ToList();
        }

        public static bool solisAreStill()
        {
            List<Obj_AI_Minion> solis = getActiveSoliders();
            return solis.All(sol => !sol.IsWindingUp);
        }

        public static List<Obj_AI_Hero> getEnemiesInSolRange()
        {
            List<Obj_AI_Minion> solis = getActiveSoliders();
            List<Obj_AI_Hero> inRange = new List<Obj_AI_Hero>();

            if (solis.Count == 0)
                return null;
            foreach (var ene in AllEnemys.Where(ene => ene.IsEnemy && ene.IsVisible && !ene.IsDead))
            {
                foreach (var sol in solis)
                {
                    if (ene.Distance(sol) < 350)
                    {
                        inRange.Add(ene);
                        break;
                    }
                }
            }
            return inRange;
        }

        public static bool enemyInAzirRange(Obj_AI_Base ene)
        {
            var solis = getActiveSoliders();

            return !ene.IsDead && solis.Count != 0 && solis.Where(sol => !sol.IsMoving).Any(sol => ene.Distance(sol) < 325);
        }

        public static int solidersAroundEnemy(Obj_AI_Base ene)
        {
            var solis = getActiveSoliders();

            return solis.Count(sol => ene.Distance(sol) < 350);
        }

        static void Obj_AI_Minion_OnPlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!azir) return;
            if (sender.Name == "AzirSoldier" && sender.IsAlly)
            {
                Obj_AI_Minion myMin = sender as Obj_AI_Minion;
                if (myMin.SkinName == "AzirSoldier")
                {
                    Animations[sender.NetworkId] = args.Animation;
                }
            }
        }

        private static void onDelete(GameObject sender, EventArgs args)
        {
            if(!azir)
                return;
            azirSoldiers.RemoveAll(s => s.NetworkId == sender.NetworkId);
            Animations.Remove(sender.NetworkId);
        }
    }
}
