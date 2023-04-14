using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ts.Client;

namespace Ts.PacketHandlers
{
    public class RebornPetHandler
    {
        public RebornPetHandler(TSClient client, byte[] data)
        {
            switch (data[1])
            {
                case 1:
                    client.getChar().RebornPet(1,data[2]);
                    break;
                case 3:
                    client.getChar().RebornPet(2, data[2]);
                    break;
                default:
                    Console.WriteLine("Pet Reborn Handler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
