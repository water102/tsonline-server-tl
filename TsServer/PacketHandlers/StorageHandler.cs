using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ts.Client;
namespace Ts.PacketHandlers
{
    class StorageHandler
    {
        public StorageHandler(TSClient client, byte[] data)
        {
            TSCharacter chr = client.getChar();
            switch (data[1])
            {
                case 1: //Item Out
                    PacketCreator getitem;
                    int countinv = 25;
                    for (int i = 0; i < 25; i++)
                    {
                        if (chr.inventory.items[i] != null)
                            countinv--;
                    }
                    byte[] slotinsto1 = new byte[data.Length - 2];

                    for (int i = 2; i < data.Length; i++)
                    {
                        slotinsto1[i - 2] = data[i];
                    }
                    for (int i = 0; i < slotinsto1.Length; i++)
                    {
                        if (i + 1 <= countinv)
                        {
                            //clear item in sto

                            getitem = new PacketCreator(0x1e, 0x01);
                            getitem.AddByte(data[i + 2]);//slot 
                            getitem.Send();

                            getitem = new PacketCreator(0x1e, 0x05);
                            getitem.AddByte(data[i + 2]);
                            getitem.AddByte(50);
                            client.reply(getitem.Send());

                            //additem to inv
                            getitem = new PacketCreator(0x17, 0x06);
                            getitem.Add16(chr.storage.items[data[i + 2] - 1].Itemid);
                            getitem.Add16(chr.storage.items[data[i + 2] - 1].quantity);
                            getitem.AddByte(0);
                            getitem.AddByte(0);
                            getitem.AddByte(0);
                            getitem.AddByte(0);
                            getitem.AddByte(0);
                            getitem.AddByte(0);
                            getitem.AddByte(0);
                            client.reply(getitem.Send());


                            //process sto and inv
                            chr.inventory.addItem(chr.storage.items[data[i + 2] - 1].Itemid, chr.storage.items[data[i + 2] - 1].quantity, false);
                            chr.storage.destroyItem(data[i + 2]);
                        }
                        else
                        {
                            getitem = new PacketCreator(0x1e, 0x01);
                            getitem.AddByte(data[i + 2]);//slot 
                            getitem.Send();
                            break;
                        }

                    }
                    break;
                case 2: //Item In
                    PacketCreator sendthungdo;
                    int countsto = 50;
                    for (int i = 0; i < 50; i++)
                    {
                        if (chr.storage.items[i] != null)
                            countsto--;

                    }
                    byte[] slotinsto = new byte[data.Length - 2];

                    for (int i = 2; i < data.Length; i++)
                    {
                        slotinsto[i - 2] = data[i];
                    }
                    for (int i = 0; i < slotinsto.Length; i++)
                    {
                        if (i + 1 <= countsto)
                        {
                            //clear chosen

                            sendthungdo = new PacketCreator(0x1e, 2);
                            sendthungdo.AddByte(data[i + 2]);
                            sendthungdo.Send();
                            //clear item in inv

                            sendthungdo = new PacketCreator(0x17, 9);
                            sendthungdo.AddByte(data[i + 2]);//slot
                            sendthungdo.Add16(50);//so luong
                            client.reply(sendthungdo.Send());


                            //additem in sto
                            sendthungdo = new PacketCreator(0x1e, 2);
                            sendthungdo.Add16(chr.inventory.items[data[i + 2] - 1].Itemid);
                            sendthungdo.Add16(chr.inventory.items[data[i + 2] - 1].quantity);
                            sendthungdo.AddByte(0);
                            sendthungdo.AddByte(0);
                            sendthungdo.AddByte(0);
                            sendthungdo.AddByte(0);
                            sendthungdo.AddByte(0);
                            sendthungdo.AddByte(0);
                            sendthungdo.AddByte(0);
                            sendthungdo.AddByte(0);
                            client.reply(sendthungdo.Send());

                            //process sto and inv
                            chr.storage.addItem(chr.inventory.items[data[i + 2] - 1].Itemid, chr.inventory.items[data[i + 2] - 1].quantity, false);
                            chr.inventory.destroyItem(data[i + 2]);
                        }
                        else
                        {
                            sendthungdo = new PacketCreator(0x1e, 2);
                            sendthungdo.AddByte(data[i + 2]);
                            sendthungdo.Send();
                            break;
                        }

                    }


                    break;
                case 8:
                    client.idNpcTalking = 0;
                    client.selectMenu = 0;
                    client.continueMoving();
                    break;
                default:
                    break;
            }
        }
    }

}
