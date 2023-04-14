using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ts.Client;
using Ts.DataTools;

namespace Ts.Server.BattleClasses
{
    public class BattleParticipant
    {
        public BattleAbstract battle;
        public byte row, col;
        public bool exist;
        public byte type;
        public TSCharacter chr;
        public TSPet pet;
        public BattleNpcAI npc;
        public byte effect;
        public int buff, debuff, disable, aura;
        public ushort buff_type, debuff_type, disable_type, aura_type;
        public ushort reflect, reflect_dmg, reflect_hp;
        public byte purge_type;
        public bool def = false;
        public bool alreadyCommand = false;
        public bool death = false;
        public bool outBattle = false;

        public BattleParticipant(BattleAbstract b, byte r, byte c)
        {
            row = r;
            col = c;
            battle = b;
            exist = false;
        }

        public void charIn(TSCharacter c)
        {
            exist = true;
            type = 1;
            chr = c;
        }

        public void petIn(TSPet p)
        {
            exist = true;
            type = 2;
            pet = p;
        }

        public void npcIn(BattleNpcAI n)
        {
            exist = true;
            type = 3;
            npc = n;
        }

        public PacketCreator announce(byte header, ushort count)
        {
            PacketCreator ret = new PacketCreator();
            ret.Add8(header);
            if (type == 1)
            {
                ret.Add8(2);
                ret.Add32(chr.client.accID);
                ret.AddZero(6);
            }
            else if (type == 2)
            {
                ret.Add8(4);
                ret.Add32(pet.NPCid);
                ret.Add16(count);
                ret.Add32(pet.owner.client.accID); //owner
            }
            else
            {
                ret.Add8(7);
                ret.Add32(npc.npcid);
                ret.Add16(count);
                ret.Add32(0);
            }
            ret.Add8(row); ret.Add8(col);
            ret.Add16((ushort)getMaxHp());
            ret.Add16((ushort)getMaxSp());
            ret.Add16((ushort)getHp());
            ret.Add16((ushort)getSp());
            ret.Add8((byte)getLvl());
            ret.Add8((byte)getElem());

            return ret;
        }

        public int getHp()
        {
            return type == 1 ? chr.hp : type == 2 ? pet.hp : type == 3 ? npc.hp : -1;
        }

        public int getSp()
        {
            return type == 1 ? chr.sp : type == 2 ? pet.sp : type == 3 ? npc.sp : -1;
        }

        public void setHp(int x)
        {
            if (type == 1)
                chr.setHp(x);
            else if (type == 2)
                pet.setHp(x);
            else if (type == 3)
                if (npc.hp <= -x) npc.hp = 0;
                else npc.hp = (ushort)(npc.hp + x);
        }

        public void setSp(int x)
        {
            if (type == 1)
                chr.setSp(x);
            else if (type == 2)
                pet.setSp(x);
            else if (type == 3)
                if (npc.sp <= -x) npc.sp = 0;
                else npc.sp = (ushort)(npc.sp + x);
        }

        public void refreshHp()
        {
            if (type == 1)
                chr.refresh(chr.hp, 0x19);
            else if (type == 2)
                pet.refresh(pet.hp, 0x19);
        }

        public void refreshSp()
        {
            if (type == 1)
                chr.refresh(chr.sp, 0x1a);
            else if (type == 2)
                pet.refresh(pet.sp, 0x1a);
        }

        public int getMaxHp()
        {
            return type == 1 ? chr.hp_max : type == 2 ? pet.hp_max : type == 3 ? npc.hpmax : -1;
        }
        public int getMaxSp()
        {
            return type == 1 ? chr.sp_max : type == 2 ? pet.sp_max : type == 3 ? npc.spmax : -1;
        }
        public int getMag()
        {
            return type == 1 ? chr.mag + chr.mag2 : type == 2 ? pet.mag + pet.mag2 : type == 3 ? npc.mag : -1;
        }
        public int getAtk()
        {
            return type == 1 ? chr.atk + chr.atk2 : type == 2 ? pet.atk + pet.atk2 : type == 3 ? npc.atk : -1;
        }
        public int getDef()
        {
            return type == 1 ? chr.def + chr.def2 : type == 2 ? pet.def + pet.def2 : type == 3 ? npc.def : -1;
        }
        public int getAgi()
        {
            return type == 1 ? chr.agi + chr.agi2 : type == 2 ? pet.agi + pet.agi2 : type == 3 ? npc.agi : -1;
        }
        public int getElem()
        {
            return type == 1 ? chr.element : type == 2 ? NpcData.npcList[pet.NPCid].element : type == 3 ? npc.elem : -1;
        }
        public int getLvl()
        {
            return type == 1 ? chr.level : type == 2 ? pet.level : type == 3 ? npc.level : -1;
        }

        public int getRb()
        {
            return type == 1 ? chr.rb : type == 2 ? pet.reborn : type == 3 ? npc.reborn : -1;
        }

        public byte getSkillLvl(ushort skill)
        {
            if (type == 1)
                if (chr.skill.ContainsKey(skill)) return chr.skill[skill]; else return 1;
            else if (type == 2)
            {
                if (NpcData.npcList[pet.NPCid].skill1 == skill) return pet.skill1_lvl;
                else if (NpcData.npcList[pet.NPCid].skill2 == skill) return pet.skill2_lvl;
                else if (NpcData.npcList[pet.NPCid].skill3 == skill) return pet.skill3_lvl;
                else if (NpcData.npcList[pet.NPCid].skill4 == skill) return pet.skill4_lvl;
                else return 1;
            }
            else return SkillData.skillList[skill].max_lvl;
        }

        public void useItem(ushort itemid)
        {
            if (type == 1)
                chr.inventory.dropItem((byte)(chr.inventory.getItemById(itemid) + 1), 1);
            else if (type == 2)
                pet.owner.inventory.dropItem((byte)(pet.owner.inventory.getItemById(itemid) + 1), 1);
        }

        public void updateStatus()
        {
            // update disable duration
            if (disable > 0)
            {
                disable--;
                if (disable == 0)
                {
                    battle.countDisabled--;
                    disable_type = 0;
                    battle.battleBroadcast(new PacketCreator(new byte[] { 0x35, 1, row, col, 1, 0, 0 }).Send());
                }
                if (disable_type == 10004) //cay tinh
                {
                    Console.WriteLine("tangle effect on " + row + " " + col);
                    battle.execCommand(new BattleCommand(row, col, row, col, 20001, 30));
                }
            }
            //update buff & aura duration
            if (buff > 0)
            {
                buff--;
                if (buff == 0)
                {
                    buff_type = 0;
                    battle.battleBroadcast(new PacketCreator(new byte[] { 0x35, 1, row, col, 2, 0, 0 }).Send());
                }
            }

            if (aura > 0)
            {
                aura--;
                if (aura == 0)
                {
                    aura_type = 0;
                    battle.battleBroadcast(new PacketCreator(new byte[] { 0x35, 1, row, col, 5, 0, 0 }).Send());
                }
            }
            //update debuff duration
            if (debuff > 0)
            {
                debuff--;
                if (debuff == 0)
                {
                    debuff_type = 0;
                    battle.battleBroadcast(new PacketCreator(new byte[] { 0x35, 1, row, col, 3, 0, 0 }).Send()); //need packet sniff update
                }
                if (debuff_type == 14015) //poison
                {
                    Console.WriteLine("poison effect on " + row + " " + col);
                    battle.execCommand(new BattleCommand(row, col, row, col, 20003, 90));
                }
            }

            def = false;
        }

        public void checkCommandEffect()
        {
            if (reflect != 0)
            {
                Console.WriteLine("Glass dmg " + row + " " + col + " " + reflect_dmg);
                battle.execCommand(new BattleCommand(row, col, row, col, 20003, reflect_dmg));
                reflect = 0;
                System.Threading.Thread.Sleep(300);
            }


            if (purge_type != 0)
            {
                Console.WriteLine("Purge " + row + " " + col + " " + purge_type);
                purge_status();
            }
        }

        public void purge_status()  // 1 : purge good status; 2 : purge bad status; 3 : purge all, 4 : purge disable, 5 : purge buff, 6 : purge debuff
        {

            if (disable > 0 && ((purge_type == 2) || (purge_type == 3) || (purge_type == 4)))
            {
                battle.countDisabled--;
                disable = 0;
                disable_type = 0;
                battle.battleBroadcast(new PacketCreator(new byte[] { 0x35, 1, row, col, 1, 0, 0 }).Send());
            }

            if (buff > 0 && ((purge_type == 1) || (purge_type == 3) || (purge_type == 5)))
            {
                buff = 0;
                buff_type = 0;
                battle.battleBroadcast(new PacketCreator(new byte[] { 0x35, 1, row, col, 2, 0, 0 }).Send());
            }

            if (aura > 0 && ((purge_type == 1) || (purge_type == 3) || (purge_type == 5)))
            {
                aura = 0;
                aura_type = 0;
                battle.battleBroadcast(new PacketCreator(new byte[] { 0x35, 1, row, col, 5, 0, 0 }).Send());
            }

            if (debuff > 0 && ((purge_type == 2) || (purge_type == 3) || (purge_type == 6)))
            {
                debuff = 0;
                debuff_type = 0;
                battle.battleBroadcast(new PacketCreator(new byte[] { 0x35, 1, row, col, 3, 0, 0 }).Send());
            }

            purge_type = 0;
        }
    }
}
