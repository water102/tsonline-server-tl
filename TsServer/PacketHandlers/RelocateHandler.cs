using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ts.Client;

namespace Ts.PacketHandlers
{
    class RelocateHandler
    {
        public RelocateHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1: //relocation ok
                    client.continueMoving();
                    break;
                default:
                    Console.WriteLine("RelocationHandler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
