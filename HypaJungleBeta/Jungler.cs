using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace HypaJungle
{
    abstract class Jungler
    {
        //Ty tc-crew

        public enum StartCamp
        {
            Any =0,
            Blue = 1,
            Red = 2,
            Frog = 3,
            Golems = 4,
        }

        private enum PotionType
        {
            Health = 2003,
            Mana = 2004,
            Biscuit = 2009,
            CrystalFlask = 2041,
        }

        internal class ItemToShop
        {
            public int goldReach;
            public List<int> itemIds;
            public List<int> itemsMustHave;
        }

        public static bool canBuyItems = true;

        public SpellSlot smite = SpellSlot.Unknown;

        

        public static Obj_AI_Hero player = ObjectManager.Player;

        public static Spellbook sBook = player.Spellbook;

        public SpellDataInst Qdata = sBook.GetSpell(SpellSlot.Q);
        public SpellDataInst Wdata = sBook.GetSpell(SpellSlot.W);
        public SpellDataInst Edata = sBook.GetSpell(SpellSlot.E);
        public SpellDataInst Rdata = sBook.GetSpell(SpellSlot.R);
        public Spell Q;//Emp 1470
        public Spell W;
        public Spell E;
        public Spell R;
        public Spell recall;
        public Spell smiteSpell;
        public Spell[] levelUpSeq;
        public List<ItemToShop> buyThings;

        public ItemToShop nextItem;
        public FightSimulator FightSimulator;



        public int dragOnLvl = 20;
        public float dpsFix = 0;
        public float bonusItemDps = 0;
        public int buffPriority = 7;
        public bool gotMana = true;
        public bool gotOverTime = false;
        public string overTimeName = "";
        public float damageTaken = 1.0f;
        public int extraWindUp = 150;
        public StartCamp startCamp = StartCamp.Any;

        public abstract void setUpSpells();
        public abstract void setUpItems();

        public abstract void UseQ(Obj_AI_Minion minion);
        public abstract void UseW(Obj_AI_Minion minion);
        public abstract void UseE(Obj_AI_Minion minion);
        public abstract void UseR(Obj_AI_Minion minion);

        public abstract void attackMinion(Obj_AI_Minion minion, bool onlyAA);
        public abstract void castWhenNear(Camp camp);
        public abstract void doAfterAttack(Obj_AI_Base minion);

        public abstract void doWhileRunningIdlin();


        public abstract float getDPS(Obj_AI_Minion minion);

        public abstract bool canMove();
        public abstract bool canRecall();

        public abstract float canHeal(float inTime, float toKillCamp);
        public abstract float getSkillAoePerSec();
      //  public abstract float getDmgDoneInTime(Camp.JungleMinion camp, float time, float cdResetTime);
        public abstract float getAoeDmgDoneInTime(Camp.JungleMinion camp, float time, float cdResetTime);
        public abstract float getTimeToDoDmgAoe(Camp.JungleMinion camp, float damageToDo, float cdResetTime);
        public abstract float getTimeToDoDmg(Camp.JungleMinion camp, float damageToDo, float cdResetTime);

        public abstract float getAAperSecond();


        public Jungler()
        {
            setupSmite();
            FightSimulator = new FightSimulator(this);
        }

        public void setupSmite()
        {
            if (player.Spellbook.GetSpell(SpellSlot.Summoner1).SData.Name.ToLower().Contains("smite"))
            {
                smite = SpellSlot.Summoner1;
                smiteSpell = new Spell(smite);
            }
            else if (player.Spellbook.GetSpell(SpellSlot.Summoner2).SData.Name.ToLower().Contains("smite"))
            {
                smite = SpellSlot.Summoner2;
                smiteSpell = new Spell(smite);
            }
        }

        private void doSmite(Obj_AI_Base target)
        {
            if (player.Spellbook.CanUseSpell(smite) == SpellState.Ready)
            {
                player.Spellbook.CastSpell(smite, target);
            }
        }

        public void startAttack(Camp.JungleMinion minion, bool smite)
        {
            usePots();
            getDPS(minion.unit);

            if (minion == null || !minion.unit.IsValid || !minion.unit.IsVisible)
                return;
            if ((smite ||player.Health/player.MaxHealth<0.14f) && minion.smiteBuff != Camp.JungleMinion.SmiteBuff.None)
            {
                if (HypaJungle.Config.Item("smiteToKill").GetValue<bool>())
                {
                    if (player.GetSummonerSpellDamage(minion.unit, Damage.SummonerSpell.Smite) >= minion.unit.Health)
                        doSmite(minion.unit);
                }
                else
                {
                    //if (minion.Health / getDPS(minion) > ((!HypaJungle.jTimer.jungleCamps.Where(cp => cp.isBuff).Any()) ? 8 : 5) * (player.Health / player.MaxHealth) || (JungleClearer.focusedCamp.isBuff && minion.MaxHealth >= 1400))
                    if(minion.unit.Health>getSmiteDmg()*0.70f)
                        doSmite(minion.unit);
                }
            }
            attackMinion(minion.unit,smite);
        }

        public void usePots()
        {
            if (player.Health / player.MaxHealth <= 0.4f && !player.HasBuff("Health Potion"))
                CastPotion(PotionType.Health);

            // Mana Potion
            if(!gotMana) return;

            if (player.Mana / player.MaxMana <= 0.3f && !player.HasBuff("Mana Potion"))
                CastPotion(PotionType.Mana);
        }

        private static void CastPotion(PotionType type)
        {
            try
            {
                player.Spellbook.CastSpell(
                player.InventoryItems.First(
                    item =>
                        item.Id == (type == PotionType.Health ? (ItemId) 2003 : (ItemId) 2004) ||
                        (item.Id == (ItemId) 2010) || (item.Id == (ItemId) 2041 && item.Charges > 0)).SpellSlot);
            }
            catch (Exception)
            {
                
            }
        }

        public void useSmiteOnHero(Obj_AI_Base target)
        {
            if (smite != SpellSlot.Unknown && player.Spellbook.CanUseSpell(smite) == SpellState.Ready)
            {
                if (target.Distance(player, true) <= 700 * 700 && (junglerGotItemRange(3714, 3718) || junglerGotItemRange(3706, 3710)))
                {

                    player.Spellbook.CastSpell(smite, target);
                }
            }
        }

        public void setFirstLvl()
        {
            sBook.LevelUpSpell(levelUpSeq[0].Slot);
            buyItems();
            checkItems();
        }

        public void levelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            if (sender.NetworkId == player.NetworkId)
            {
                    sBook.LevelUpSpell(levelUpSeq[args.NewLevel - 1].Slot);
            }
        }

        public bool canKill(JungleCamp camp,float timeTo)
        {
            if (dpsFix == 0 || camp.dps == 0)
                return true;

            float realDps = dpsFix + bonusItemDps;
            float bonusDmg = 0;
            float aproxSecTK = (camp.health - bonusDmg) / realDps;

            float secTillIDie = (player.Health + canHeal(timeTo, aproxSecTK)) / (camp.dps * 0.8f * damageTaken*(100+player.Level*3)/100);
            if (smiteSpell.IsReady((int) (secTillIDie + timeTo)*1000))
            {
                bonusDmg += getSmiteDmg();
            }

            float secToKill = (camp.health - bonusDmg) / realDps;
            if (secToKill*1.1f > secTillIDie)
                return false;
            return true;
        }

        public float getSmiteDmg()
        {
            return new float[] { 390, 410, 430, 450, 480, 510, 540, 570, 600, 640, 680, 720, 760, 800, 850, 900, 950, 1000 }[
                    player.Level - 1];
        }

        public void checkItems()
        {
            getItemPassiveBoostDps();
            if (!canBuyItems)
                return;
            for (int i = buyThings.Count - 1; i >= 0; i--)
            {
                if (hasAllItems(buyThings[i]))
                {
                    nextItem = buyThings[i];
                    if (i == buyThings.Count - 1)
                    {
                        canBuyItems = false;
                    }

                    return;
                }
            }
        }

        public bool hasAllItems(ItemToShop its)
        {
            bool[] usedItems = new bool[player.InventoryItems.Count()+1];
            int itemsMatch = 0;
            for (int j = 0; j < its.itemsMustHave.Count; j++)
            {
                for (int i = 0; i < player.InventoryItems.Count(); i++)
                {
                    if (usedItems[i])
                        continue;
                    if (its.itemsMustHave[j] == (int)player.InventoryItems[i].Id)
                    {
                        usedItems[i] = true;
                        itemsMatch++;
                        break;
                    }
                }              
            }
            return itemsMatch == its.itemsMustHave.Count;
        }

        public void buyItems()
        {
            if (inSpwan())
            {
                foreach (var item in nextItem.itemIds)
                {
                    if (!Items.HasItem(item))
                    {
                        Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(item, ObjectManager.Player.NetworkId))
                            .Send();
                    }
                }
            }
            checkItems();
        }

        public bool inSpwan()
        {
            Vector3 spawnPos1 = new Vector3(14286f, 14382f, 172f);
            Vector3 spawnPos0 = new Vector3(416f, 468f, 182f);
            if (player.Distance(spawnPos1) < 600 || player.Distance(spawnPos0) < 600)
                return true;
            return false;
        }

        public float getItemPassiveBoostDps()
        {
            float dps = 0;
          //  int[] listNewItemsJng = new[] {3726, 3725, 3723, 3722, 3721, 3720, 3719, 3718, 3717, 3716, 3714,3713,3711, 3710, 3709, 3708, 3707, 3706};
            //3706-3726
            if (junglerGotItemRange(3706, 3726))
            {
                dps += 45/2;
                gotOverTime = true;
                overTimeName = "itemmonsterburn";
            }
            bonusItemDps = dps;



            if (junglerGotItemRange(3714, 3718))
            {
                damageTaken = 0.8f;
            }
            if (junglerGotItemRange(1039, 1039))
            {
                bonusItemDps += 15;
                gotOverTime = true;
                overTimeName = "itemmonsterburn";
            }
            return bonusItemDps;
        }

        public bool gotAoeSmite()
        {
            return (junglerGotItemRange(3713, 3713) || junglerGotItemRange(3723, 3726));
        }

        private bool junglerGotItemRange(int from, int to)
        {
            return player.InventoryItems.Any(item => (int) item.Id >= @from && (int) item.Id <= to);
        }

        public float getSpellDmgRaw(SpellSlot slot,int stage =0)
        {
            var spell = Damage.Spells[player.ChampionName].FirstOrDefault(s => s.Slot == slot && s.Stage == stage);
            if (spell == null)
                return 0;

            var rawDamage = (float)spell.Damage(player, player, Math.Max(1, Math.Min(player.Spellbook.GetSpell(slot).Level - 1, 6)));

            return rawDamage;
        }

        public float realPhysDmgDoneToMe(float amount)
        {
            var armorPenFlat = 0;
            var armorPenPercent = 1;
            var armor = (player.Armor * armorPenPercent) - armorPenFlat;

            float k;
            if (armor < 0)
            {
                k = 2 - 100 / (100 - armor);
            }
            else
            {
                k = 100 / (100 + armor);
            }

            return k * amount * damageTaken;
        }

        public float bonusRegenInFight()
        {
            float regenPerSek = 0;
            //  int[] listNewItemsJng = new[] {3726, 3725, 3723, 3722, 3721, 3720, 3719, 3718, 3717, 3716, 3714,3713,3711, 3710, 3709, 3708, 3707, 3706};
            //3706-3726
            if (junglerGotItemRange(3706, 3726))
            {
                regenPerSek += 10;
            }

            if (junglerGotItemRange(1039, 1039))
            {
                regenPerSek += 7;
            }
            //Console.WriteLine("Bonus heal: " + regenPerSek);
            return regenPerSek;
        }

        public float getFulHeal(float inTime, float killtime)
        {
            //Console.WriteLine("Will heal till it " + canHeal(inTime, killtime));
            return canHeal(inTime, killtime) + bonusRegenInFight()*killtime;

        }

        public float getFrogBuffAoe(float timeTillCamp,float fightTime)
        {
            float timeTillEnd = timeTillFrogBuffEnd();
            if (timeTillEnd - timeTillCamp <= 0)
                return 0;
            float effectFor = (fightTime >= timeTillEnd+2) ? timeTillEnd+2 : fightTime;
            float dps = 13 + player.Level;
            return effectFor*dps;
        }

        public float getRedBuffDmg(float timeTillCamp, float fightTime)
        {
            float timeTillEnd = timeTillRedBuffEnd();
            if (timeTillEnd - timeTillCamp <= 0)
                return 0;
            float effectFor = (fightTime >= timeTillEnd + 1) ? timeTillEnd + 1 : fightTime;
            float dps = 15 + player.Level;
            return effectFor * dps;
        }

        public float getKrugBuffDmgRemove(float timeTillCamp, float fightTime)
        {
            float timeTillEnd = timeTillKrugBuffEnd();
            if (timeTillEnd - timeTillCamp <= 0)
                return 1;
            float effectFor = (fightTime >= timeTillEnd + 1) ? timeTillEnd : fightTime;
            float everyStunIn = player.AttackDelay*5;
            float prop = 1.5f/everyStunIn;
            if (prop > 1) prop = 1;


            return effectFor / fightTime * prop;
        }



        /* Jungle Buff Manager */

        public float timeTillFrogBuffEnd()
        {
            var buff = player.Buffs.FirstOrDefault(b => b.Name == "s5junglemushroomarmor");
            return buff == null ? 0 : (buff.EndTime - Game.Time);
        }

        public float timeTillKrugBuffEnd()
        {
            var buff = player.Buffs.FirstOrDefault(b => b.Name == "KrugStoneFist");
            return buff == null ? 0 : (buff.EndTime - Game.Time);
        }

        public float timeTillRedBuffEnd()//15+leve sek?
        {
            var buff = player.Buffs.FirstOrDefault(b => b.Name == "blessingofthelizardelder");
            return buff == null ? 0 : (buff.EndTime - Game.Time);
        }
    }
}
