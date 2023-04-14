using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ts.Client;

namespace Ts.PacketHandlers
{
    class CreateChar
    {
        public CreateChar(TSClient client, byte[] data)
        {
            PacketCreator p =new PacketCreator(9);
            if (data[1] == 2)
            {
                p.AddByte(3);
                byte[] name = PacketReader.toByteArray(data, 2, data.Length - 2);
                p.AddByte(checkUnique(name,client));
                client.reply(p.Send());
                return;
            }

            if (data[1] == 1)
            {
                if (data.Length > 2)
                {
                    client.createChar(data);
                    p.AddByte(1);
                    client.reply(p.Send());
                }
                else
                    client.getChar().loginChar();
            }
        }

        private byte checkUnique(byte[] name, TSClient c)
        {
            //check database for name, return 1 if name exist
            c.name_temp = name;
            return 0;
        }
    }
}
