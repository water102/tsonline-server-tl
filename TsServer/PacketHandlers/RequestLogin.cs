using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ts.Client;

namespace Ts.PacketHandlers
{
    class RequestLogin
    {
        public RequestLogin(TSClient client)
        {
            if (client.isOnline())
                return;

            PacketCreator p1 = new PacketCreator();
            p1.AddByte(1);
            p1.AddByte(9);
            p1.AddByte(1);
            client.reply(p1.Send());

        }
    }
}
