﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ts.Server;
using Ts.Client;
using Ts.DataTools;

namespace Ts.PacketHandlers
{
    class BattleHandler
    {
        public BattleHandler(TSClient client, byte[] data)
        {
            if (client.idBattle > 0)
            {
                ushort map_id = client.map.mapid;
                ushort battleid = client.idBattle;
                new TSBattleNPC(client, 3, EveData.listMapData[map_id].battleListOnMap[battleid].getGround(), EveData.listMapData[map_id].battleListOnMap[battleid].getNpcId());
                client.idBattle = 0;
                return;
            }
            switch (data[1])
            {
                case 1:
                    client.continueMoving();
                    break;
                case 0x2: //pk on map
                    if (data[2] == 3) //pk NPC
                        new TSBattleNPC(client, 3, PacketReader.read16(data, 7), new ushort[] {0,0, (ushort)PacketReader.read32(data, 3), 0,0,0,0,0,0,0 });
                    if (data[2] == 2) // pvp
                    {
                        uint eneId = PacketReader.read16(data,3);
                        new TSBattlePvp(client, 3, PacketReader.read16(data, 7), client.map.listPlayers[eneId]);
                    }
                    break;
                    
            }
        }
    }
}
