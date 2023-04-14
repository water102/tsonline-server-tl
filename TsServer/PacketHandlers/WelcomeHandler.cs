using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ts.Client;

namespace Ts.PacketHandlers
{
    class WelcomeHandler
    {
        public WelcomeHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1:
                    {
                        PacketCreator p = new PacketCreator(0x21, 1);
                        p.Add8(data[2]);
                        client.map.BroadCast(client, data, true);
                        client.reply(p.Send());
                        break;
                    }
                case 2:
                    {
                        PacketCreator p = new PacketCreator(0x21, 2);
                        p.Add8(data[2]);
                        client.reply(p.Send());
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
