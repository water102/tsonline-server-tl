using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ts.Client;

namespace Ts.PacketHandlers
{
    class GroupHandler
    {
        public GroupHandler(TSClient client, byte[] data)
        {
            int subleaderId = 0;
            int teamleaderId = 0;
            int groupCommandType = data[1];
            switch (groupCommandType)
            {
                // Member request
                case 1:
                    {
                        TSCharacter requester = client.getChar();
                        uint target_id = PacketReader.read32(data, 2);
                        TSCharacter target = TSServer.GetInstance().GetPlayerById((int)target_id).getChar();

                        // current player is in battle
                        // Requester not join team
                        if (!requester.isJoinedTeam())
                        {
                            if (target != null)
                            {
                                var p = new PacketCreator(0x0D);
                                p.Add8(0x01);
                                p.Add32((uint)client.accID);
                                target.reply(p.Send());
                            }
                        }
                    }
                    break;
                case 2:
                    break;
                // Leader response
                case 8:
                case 3:
                    {
                        byte response = data[2];
                        uint memberId = PacketReader.read32(data, 3);
                        TSCharacter member = TSServer.GetInstance().GetPlayerById((int)memberId).getChar();

                        // Check joined team or in battle
                        ///- Notify response to requester
                        //if (!member.isJoinedTeam() || !member.isTeamLeader())

                        var p = new PacketCreator(0x0D);
                        if (groupCommandType == 0x03)
                            p.Add8((byte)groupCommandType);
                        else
                            p.Add8((byte)0x0A);
                        p.Add8(response);

                        if (response == 0x02)
                        {
                            p.Add32((uint)client.accID);
                            member.reply(p.Send());
                        }
                        else
                        {
                            p.Add32((uint)memberId);
                            client.reply(p.Send());
                        }

                        switch (response)
                        {
                            case 0x01: // accepted
                                {
                                    uint leader_id = 0;
                                    uint member_id = 0;
                                    TSParty party = null;
                                    TSCharacter player = client.getChar();

                                    // Requester is leader
                                    if (member.party != null && member.isTeamLeader())
                                    {
                                        if (!member.party.canJoin())
                                        {
                                            // team is full
                                        }
                                        else
                                        {
                                            member.party.member.Add(player);
                                            player.party = member.party;
                                        }
                                        party = member.party;
                                        leader_id = member.client.accID;
                                        member_id = player.client.accID;

                                        //if (player.isJoinedTeam())
                                        //member.sendUpdateTeam();
                                    }
                                    else
                                    {
                                        if (player.party == null)
                                        {
                                            party = new TSParty(player, member);
                                            member.party = party;
                                            player.party = party;
                                        }
                                        else
                                        {
                                            party = player.party;
                                            player.party.member.Add(member);
                                            member.party = player.party;
                                        }
                                        leader_id = player.client.accID;
                                        member_id = member.client.accID;

                                        //player.sendUpdateTeam();
                                    }

                                    // Notify to everyone
                                    p = new PacketCreator(0x0D);
                                    p.Add8(0x05);
                                    p.Add32(leader_id);
                                    p.Add32(member_id);
                                    member.replyToMap(p.Send(), true);

                                    // Refresh
                                    foreach (TSCharacter c in member.party.member)
                                    {
                                        c.refreshTeam();
                                    }

                                    break;
                                }
                            case 0x02: // rejected
                            case 0x03: // no response
                            default:
                                break;
                        }
                    }
                    break;
                // Member leave
                case 4:
                    {
                        teamleaderId = (int)PacketReader.read32(data, 2);
                        if (client.getChar().isTeamLeader())
                        {
                            // Dimiss Team
                            client.getChar().party.Disband(client.getChar());
                        }
                        else
                        {
                            // Remove 
                            client.getChar().party.LeaveTeam(client.getChar());
                        }
                    }
                    break;
                // Set subleader
                case 5:
                    subleaderId = (int)PacketReader.read32(data, 2);
                    client.getChar().party.SetSubleader(subleaderId, client);
                    break;
                // Unset subleader
                case 6:
                    subleaderId = (int)PacketReader.read32(data, 2);
                    client.getChar().party.unSetSubLeader(subleaderId, client);
                    break;
                // Ask player to join and be team leader
                case 7:
                    {
                        uint target_id = PacketReader.read32(data, 2);
                        TSCharacter requester = client.getChar();
                        TSCharacter target = TSServer.GetInstance().GetPlayerById((ushort)target_id).getChar();

                        if (requester.isTeamLeader())
                        {
                            if (target != null)
                            {
                                var p = new PacketCreator(0x0D);
                                p.Add8(0x09);
                                p.Add32((uint)client.accID);
                                target.reply(p.Send());
                            }
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Group Handler : unknown subcode" + data[1]);
                    break;
            }
        }
    }
}
