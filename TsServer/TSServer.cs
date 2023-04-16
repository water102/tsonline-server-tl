using System;
using System.Collections.Generic;
using Ts.Server;
using Ts.DataTools;
using Ts.Client;
using TsServer.DataTools2;

namespace Ts
{
    public class TSServer
    {
        private static TSServer? instance = null;
        private ServerHandler? handler = null;
        public TSWorld? World { get; private set; }
        private readonly Dictionary<uint, TSClient> players = new();
        private readonly Dictionary<ushort, TSMap> listMap = new();

        public static TSServer GetInstance()
        {
            if (instance == null)
            {
                instance = new TSServer();
            }
            return instance;
        }

        public void Run()
        {
            Console.WriteLine("Loading item data ...");
            ItemData.LoadItems();
            Console.WriteLine("Loaded " + ItemData.itemList.Count + " items");
            ItemData.writeToFile("item.txt");

            Console.WriteLine("Loading NPC data ...");
            NpcData.LoadNpcs();
            Console.WriteLine("Loaded " + NpcData.npcList.Count + " NPCs");
            NpcData.writeToFile("npc.txt");

            Console.WriteLine("Loading Eve data ...");
            OldEveData.loadAllWarp();
            EveData.LoadData("data/eve.Emg");

            Console.WriteLine("Loading Warp data ...");
            WarpData.LoadTxt("data/warps.txt");
            //WarpData.loadWarpEx();
            //WarpData.loadWarpDoDo();
            Console.WriteLine("Loaded " + WarpData.warpCount + " Warp Gates");
            WarpData.writeToFile("newWarp.txt");

            Console.WriteLine("Loading Skill data ...");
            SkillData.LoadSkills();
            Console.WriteLine("Loaded " + SkillData.skillList.Count + " Skills");
            SkillData.writeToFile("skill.txt");

            Console.WriteLine("Loading Scene data ...");
            SceneData.LoadScenes();
            Console.WriteLine("Loaded " + SceneData.sceneList.Count + " Scenes");
            SceneData.writeToFile("scene.txt");

            Console.WriteLine("Loading Talk data ...");
            TalkData.LoadTalks();
            Console.WriteLine("Loaded " + TalkData.talkList.Count + " Dialogs");
            TalkData.writeToFile("talk.txt");

            Console.WriteLine("Loading Battle data ...");
            BattleData.loadBattle("data/battle.txt");

            handler = new ServerHandler(6414);

            World = new TSWorld(this);
            players.Clear();
            listMap.Clear();

            Console.WriteLine("Server is started...");
        }

        static void Main(string[] args)
        {
            GetInstance().Run();
            Console.ReadLine();
        }

        public TSMap InitMap(ushort mapid)
        {
            TSMap m = new (mapid);
            listMap.Add(mapid, m);
            return m;
        }

        public void Warp(TSClient client, ushort warpid)
        {
            ///- Send Enter Door action response
            client.reply(new PacketCreator(new byte[] { 20, 0x07 }).Send());
            client.reply(new PacketCreator(new byte[] { 0x29, 0x0E }).Send());

            ushort start = client.map.mapid;
            if (WarpData.warpList.ContainsKey(start))
            {
                if (WarpData.warpList[start].ContainsKey(warpid))
                {
                    ushort[] dest = WarpData.warpList[start][warpid];

                    if (!listMap.ContainsKey(dest[0]))
                    {
                        listMap.Add(dest[0], new TSMap(dest[0]));
                    }

                    client.getChar().mapID = dest[0];
                    client.getChar().mapX = dest[1];
                    client.getChar().mapY = dest[2];
                    //client.map.removePlayer(client.accID);

                    listMap[dest[0]].addPlayerWarp(client, dest[1], dest[2]);
                    return;
                }
                else if (warpid == 2 & start == 10851)
                {
                    if (!listMap.ContainsKey(12003))
                    {
                        listMap.Add(12003, new TSMap(12003));
                    }
                    client.getChar().mapID = 12003;
                    client.getChar().mapX = 555;
                    client.getChar().mapY = 555;
                    listMap[12003].addPlayerWarp(client, 555, 555);
                }
                else
                {
                    Console.WriteLine("Warp data helper : warpid " + warpid + " not found");
                    EveData.loadCoor(start, 12000, warpid);
                }
            }

            else
            {
                Console.WriteLine("Warp data helper : mapid " + start + " warpid " + warpid + " not found");
                EveData.loadCoor(start, 12000, warpid);
            }
            client.AllowMove();
        }

        public void AddPlayer(TSClient c)
        {
            players.Add(c.accID, c);
            TSMap m;
            if (!listMap.ContainsKey(c.getChar().mapID))
                m = InitMap(c.getChar().mapID);
            else
                m = listMap[c.getChar().mapID];
            m.addPlayer(c);
        }

        public void RemovePlayer(uint id)
        {
            players.Remove(id);
        }

        public TSClient? GetPlayerById(int id)
        {
            if (players.ContainsKey((uint)id))
                return players[(uint)id];
            return null;
        }

        public void BroadCast(TSClient client, byte[] data, bool self)
        {
            foreach (TSMap m in listMap.Values)
            {
                m.BroadCast(client, data, self);
            }
        }
    }
}
