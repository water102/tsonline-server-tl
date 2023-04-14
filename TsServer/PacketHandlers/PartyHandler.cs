using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ts.Client;

namespace Ts.PacketHandlers
{
    class PartyHandler
    {
        public PartyHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1:
                    if (client.battle != null)
                    {
                        client.battle.SetBattlePet(client, data);
                    }
                    else
                    {
                        if (client.getChar().SetBattlePet(PacketReader.read16(data, 2)))
                            client.reply(new PacketCreator(data).Send());
                    }
                    break;
                case 2:
                    // modified by zFan
                    // In battle
                    if (client.battle != null)
                    {
                        client.battle.UnBattlePet(client, data);
                    }
                    else
                    {
                        if (client.getChar().UnsetBattlePet())
                            client.reply(new PacketCreator(data).Send());
                    }
                    break;
                default:
                    Console.WriteLine("Party Handler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
