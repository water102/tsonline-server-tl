using System.Text;
using System.Runtime.InteropServices;
using TsServer.Buffer;
using TsServer.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ts.DataTools
{
    public static class EveData
    {
        private static string fileLocation = string.Empty;
        public static readonly Dictionary<ushort, MapData> listMapData = new();

        //public static object[] arr = new object[] { };

        // Reads directly from stream to structure
        public static T ReadFromItems<T>(Stream fs, int off)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fs.Read(buffer, off, Marshal.SizeOf(typeof(T)));
            GCHandle Handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T RetVal = (T)Marshal.PtrToStructure(Handle.AddrOfPinnedObject(), typeof(T));
            Handle.Free();

            return RetVal;
        }
        public static void addIntToArray(ref ushort[] a, ushort val)
        {
            List<ushort> temp = a.ToList();
            temp.Add(val);
            a = temp.ToArray();
        }

        private static bool LoadHeaders()
        {
            try
            {
                using FileStream fs = new(fileLocation, FileMode.Open, FileAccess.Read);

                int nb_headers = 0;
                fs.Seek(2, 0);
                int pos = 2;
                uint endHeader = 1000000;

                listMapData.Clear();
                while (fs.Position < endHeader)
                {
                    byte[] buffer = new byte[0x20]; // 0x20 = 32
                    fs.Read(buffer, 0, buffer.Length);
                    pos += buffer.Length;

                    //Console.WriteLine("buffer >>" + String.Join(",", buffer));
                    ushort mapid = ushort.Parse(buffer.GetString(1, 5));
                    //Console.WriteLine("map id >> "+ mapid.ToString());
                    //string content = Encoding.Default.GetString(buffer);
                    //Console.WriteLine("content id >> " + content);
                    uint off = buffer.ReadU32(0x18);
                    uint len = buffer.ReadU32(0x1c);
                    //offsets[mapid] = new Tuple<int, int>(off, len);
                    nb_headers++;
                    if (nb_headers == 1) endHeader = off;

                    // refactor
                    var mapData = new MapData
                    {
                        id = mapid,
                        bufferInformation = new BufferInformation
                        {
                            offset = off,
                            length = len
                        }
                    };
                    listMapData.Add(mapData.id, mapData);
                }
                //Console.WriteLine(">>> offsets >>" + String.Join(",", offsets));
                fs.Close();
                fs.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static bool LoadData(string fileLocation)
        {
            EveData.fileLocation = fileLocation;
            var isOk = LoadHeaders();
            if (isOk)
                LoadAllWarp();
            return isOk;
        }

        private static void LoadAllWarp()
        {
            foreach (KeyValuePair<ushort, MapData> entry in listMapData)
            {
                LoadWarp(entry.Value);
            }
        }

        public static void LoadWarp(MapData mapData)
        {
            var tsBuffer = TsBuffer.LoadFromFile(fileLocation, mapData.bufferInformation);
            var pos = LoadNpcs(mapData, tsBuffer);
            pos = LoadItems(mapData, tsBuffer);
            pos = LoadGates(mapData, tsBuffer);
            pos = LoadUnknowData1(tsBuffer);
            pos = LoadDialogs(mapData, tsBuffer);
            pos = LoadWarpPoints(mapData, tsBuffer);
            pos = LoadUnknowData2(tsBuffer);
            pos = LoadQuestsAndSteps(mapData, tsBuffer);

            if (pos == 0) return;
            LoadBattles(mapData, tsBuffer);
        }

        private static uint LoadNpcs(MapData mapData, TsBuffer tsBuffer)
        {
            int nb_npc = tsBuffer.GetNumberAt<int>(0x67);
            tsBuffer.IncreasePosition(4);
            //NPC, later
            mapData.npcs.Clear();
            for (int i = 0; i < nb_npc; i++)
            {
                LoadNpc(mapData, tsBuffer);
            }

            return tsBuffer.Pos;
        }

        private static void LoadNpc(MapData mapData, TsBuffer tsBuffer)
        {
            ushort npcIndex = tsBuffer.Read16();
            ushort npcID = tsBuffer.Read16();

            //
            ushort nb1 = tsBuffer.Read16();
            var events = new byte[nb1];
            for (int j = 0; j < nb1; j++)
            {
                events[j] = tsBuffer[tsBuffer.Pos + (uint)j];
                tsBuffer.IncreasePosition(1);
            }

            //
            byte nb2 = tsBuffer.ReadByte();
            tsBuffer.IncreasePosition(nb2);

            //
            byte nb_f = tsBuffer.ReadByte();
            tsBuffer.IncreasePosition(8);
            List<Tuple<ushort, ushort>> patrol = new();
            for (int j = 0; j < nb_f; j++)
            {
                int moveX = tsBuffer.Read32();
                int moveY = tsBuffer.Read32();
                patrol.Add(
                    new Tuple<ushort, ushort>(
                        (ushort)(moveX * 20 - 10), 
                        (ushort)(moveY * 20 - 10)
                    )
                );
            }

            //
            tsBuffer.IncreasePosition(31);
            int posX = tsBuffer.Read32();
            int posY = tsBuffer.Read32();
            tsBuffer.IncreasePosition(4);
            byte type = (byte)tsBuffer.Read16();
            tsBuffer.IncreasePosition(35);

            var npcOnMap = new NpcOnMap
            {
                NpcIndex = npcIndex,
                NpcId = npcID,
                Events = events.ToList(),
                unk_1 = nb1,
                unk_2 = nb2,
                unk_3 = nb_f,
                Patrol = patrol,
                type = type,
                posX = posX,
                posY = posY,
            };
            mapData.npcs.Add(npcOnMap);

            if (mapData.id == 12015 & npcIndex == 1) Console.WriteLine(" type " + type);
        }

        private static uint LoadItems(MapData mapData, TsBuffer tsBuffer)
        {
            ushort nb_entry_exit = tsBuffer.Buffer.Read16(tsBuffer.Pos);
            tsBuffer.IncreasePosition(1);
            mapData.items.Clear();
            for (int i = 0; i < nb_entry_exit; i++)
            {
                LoadItem(mapData, tsBuffer);
            }
            return tsBuffer.Pos;
        }

        private static void LoadItem(MapData mapData, TsBuffer tsBuffer)
        {
            tsBuffer.IncreasePosition(1);
            ushort id = tsBuffer.GetNumber<ushort>();
            tsBuffer.IncreasePosition(1);
            ushort idItem = tsBuffer.Read16();
            ushort posX = tsBuffer.Read16();
            tsBuffer.IncreasePosition(2);
            ushort posY = tsBuffer.Read16();
            tsBuffer.IncreasePosition(2);
            ushort timeDelay = tsBuffer.Buffer.Read16(tsBuffer.Pos);
            tsBuffer.IncreasePosition(1);

            ItemOnMap itemOnMap = new()
            {
                idItemOnMap = id,
                idItem = idItem,
                posX = posX,
                posY = posY,
                timeDelay = timeDelay
            };

            mapData.items.Add(itemOnMap);
        }

        private static uint LoadGates(MapData mapData, TsBuffer tsBuffer)
        {
            mapData.gates.Clear();
            tsBuffer.IncreasePosition(1);
            ushort totalGates = tsBuffer.Read16();
            for (int i = 0; i < totalGates; i++)
            {
                LoadGate(mapData, tsBuffer);
            }

            return tsBuffer.Pos;
        }

        private static void LoadGate(MapData mapData, TsBuffer tsBuffer)
        {
            var GateId = tsBuffer.Read16();
            ushort nb = tsBuffer.Read16();
            var eve = tsBuffer.Buffer.Read((int)tsBuffer.Pos, nb);
            tsBuffer.IncreasePosition(nb);
            tsBuffer.IncreasePosition(21);

            mapData.gates.Add(new MapGate
            {
                GateId = GateId,
                Eve = eve.ToList(),
            });
        }

        private static uint LoadUnknowData1(TsBuffer tsBuffer)
        {
            ushort nb_unk2 = tsBuffer.Read16();
            for (int i = 0; i < nb_unk2; i++)
            {
                tsBuffer.IncreasePosition(2);
                ushort nb = tsBuffer.Read16();
                tsBuffer.IncreasePosition(nb);
                tsBuffer.IncreasePosition(17);
                //Console.WriteLine(posX + " " + posY); 
            }

            return tsBuffer.Pos;
        }

        private static uint LoadDialogs(MapData mapData, TsBuffer tsBuffer)
        {
            mapData.dialogs.Clear();
            ushort nb_dialog = tsBuffer.Read16();
            for (int i = 0; i < nb_dialog; i++)
            {
                byte first = tsBuffer[tsBuffer.Pos];
                tsBuffer.IncreasePosition(4);
                byte nb_d = tsBuffer[tsBuffer.Pos];
                tsBuffer.IncreasePosition(4);
                byte nb_d_2 = tsBuffer[tsBuffer.Pos];
                tsBuffer.IncreasePosition((uint)5 * nb_d);
                byte nb_d_3 = tsBuffer[tsBuffer.Pos];
            }

            return tsBuffer.Pos;
        }

        private static uint LoadWarpPoints(MapData mapData, TsBuffer tsBuffer)
        {
            mapData.warps.Clear();
            ushort nb_warp = tsBuffer.Read16();
            for (int i = 0; i < nb_warp; i++)
            {
                LoadWarpPoint(mapData, tsBuffer);
            }

            return tsBuffer.Pos;
        }

        private static void LoadWarpPoint(MapData mapData, TsBuffer tsBuffer)
        {
            try
            {
                ushort WarpId = tsBuffer.Read16();
                ushort DestId = tsBuffer.Read16();
                tsBuffer.IncreasePosition(2);
                uint DestX = tsBuffer.ReadU32() * 20 - 10;
                uint DestY = tsBuffer.ReadU32() * 20 - 10;
                var Unk = tsBuffer.Buffer.Read((int)tsBuffer.Pos, 0x19);
                tsBuffer.IncreasePosition(0x19);

                mapData.warps.Add(
                    new MapWarp
                    {
                        WarpId = WarpId,
                        DestId = DestId,
                        DestX = DestX,
                        DestY = DestY,
                        Unk = Unk
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static uint LoadUnknowData2(TsBuffer tsBuffer)
        {
            ushort nb_random = tsBuffer.Read16();

            for (int i = 0; i < nb_random; i++)
            {
                ushort idx = tsBuffer.Read16();
                ushort unk_1 = tsBuffer.Read16();
                ushort unk_2 = tsBuffer.Read16();
                ushort unk_3 = tsBuffer.Buffer.Read16(tsBuffer.Pos);

                ushort total = tsBuffer.GetNumber<ushort>();
                tsBuffer.IncreasePosition(1);
                tsBuffer.IncreasePosition((uint)(total * 2 + 1));
            }

            return tsBuffer.Pos;
        }

        private static uint LoadQuestsAndSteps(MapData mapData, TsBuffer tsBuffer)
        {
            var mapid = mapData.id;
            var steps = mapData.steps;

            ushort nb_talk_quest = tsBuffer.Read16();
            if (nb_talk_quest == 0)
            {
                return 0;
            }

            if (tsBuffer.Pos >= tsBuffer.Buffer.Length)
            {
                return 0;
            }
            //List<ushort> mapExclude = new List<ushort>() { 10965, 10966, 10987, 10995, 10996, 11552, 14552, 15025 };
            //if (mapExclude.FindIndex(item => item == mapid) >= 0)
            //{
            //    continue;
            //}
            //List<Quest> listQuests = new List<Quest>();

            steps.Clear();
            for (int i = 0; i < nb_talk_quest; i++)
            {
                LoadStep(tsBuffer, mapid, steps, i);
            }

            return tsBuffer.Pos;
        }

        private static void LoadStep(TsBuffer tsBuffer, ushort mapid, List<Step> steps, int i)
        {
            try
            {
                #region
                ushort idx = tsBuffer.GetNumber<ushort>();
                tsBuffer.IncreasePosition(15);
                ushort totalTalk = tsBuffer.GetNumber<ushort>();
                tsBuffer.IncreasePosition(1);
                ushort conditions = 0;
                ushort condition = 0;
                ushort idxStepAddCondition = 0;
                for (int j = 0; j < totalTalk; j++)
                {
                    if (conditions > 1 & condition < conditions)
                    {
                        condition = LoadRefStep(tsBuffer, mapid, steps, i, condition, idxStepAddCondition);
                        if (i == 0 && j == 0)
                        {
                            condition = 0;
                        }
                        continue;
                    }
                    else
                    {
                        conditions = 0;
                        condition = 0;
                    }

                    ushort stepId = tsBuffer.GetNumber<ushort>();
                    tsBuffer.IncreasePosition(1);
                    ushort stepType = tsBuffer.GetNumber<ushort>();
                    tsBuffer.IncreasePosition(1);
                    Step step = new()
                    {
                        stepId = stepId,
                        requiredItems = new Dictionary<ushort, ushort>(),
                        requiredNpc = new List<ushort>(),
                        requiredQuests = new Dictionary<ushort, List<ushort>>(),
                        receivedQuests = new Dictionary<ushort, List<ushort>>(),
                        rootBit = new List<ushort>(),
                        qIndex = idx,
                        subStepIds = new List<ushort>(),
                        normalTalk = false,
                        type = stepType
                    };
                    tsBuffer.IncreasePosition(1);
                    ushort questId = tsBuffer.Buffer.Read16(tsBuffer.Pos - 1);
                    //if (mapid == 12001 & i == 15) Console.WriteLine("questId >>" + questId);
                    ushort optionId = 0;
                    ushort resBattle = 0;
                    ushort idBox = 0;
                    // Select options

                    switch ((StepType)stepType)
                    {
                        case StepType.Type0:
                            step.normalTalk = true;
                            break;
                        case StepType.Type1:
                            ushort itemRequired = tsBuffer.Buffer.Read16(tsBuffer.Pos - 1);
                            ushort quantity = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos + 1);
                            ushort case_1_unknown_1 = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos + 2);
                            ushort case_1_unknown_2 = tsBuffer.Buffer.Read16(tsBuffer.Pos + 3);
                            step.requiredItems.Add(itemRequired, quantity);
                            break;
                        case StepType.Type2:
                            step.questId = questId;
                            ushort case_2_unknown_1 = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos + 1); // 1 + 2 + 3 
                            ushort case_2_unknown_2 = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos + 2); // 0 + 5
                            ushort required = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos + 3); // 0 + 1 + 2 
                            if (required == 1 | required == 2)
                            {
                                step.requiredQuests.Add(
                                    questId,
                                    new List<ushort> {
                                    case_2_unknown_1,
                                    case_2_unknown_2,
                                    required
                                });
                            }
                            if (required == 0 || case_2_unknown_1 == 2)
                            {
                                step.receivedQuests.Add(
                                    questId,
                                    new List<ushort> {
                                    case_2_unknown_1,
                                    case_2_unknown_2,
                                    required
                                });
                            }
                            step.rootBit.Add(case_2_unknown_1);
                            step.rootBit.Add(case_2_unknown_2);
                            step.rootBit.Add(required);
                            break;
                        case StepType.Type3:
                            idBox = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos - 1);
                            break;
                        case StepType.Type7:
                            // 02 07 00 00 01 04 0F 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00
                            // 11 07 05 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 0F 00 00 00 00
                            // 11 07 05 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 0F 00 00 00 00
                            ushort type_required = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos - 1);
                            ushort case_7_unknown_1 = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos);
                            ushort case_7_unknown_2 = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos + 1);
                            ushort case_7_unknown_3 = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos + 2);
                            ushort requiredLevel = tsBuffer.Buffer.Read16(tsBuffer.Pos + 3);
                            if (type_required == 0 & requiredLevel > 0)
                            {
                                step.requiredLevel = requiredLevel;
                            }
                            if (type_required == 5 & case_7_unknown_2 == 1)
                            {
                                // Full pet 
                                step.requiredSlotPet = true;
                            }
                            if (type_required == 5 && case_7_unknown_2 == 2)
                            {
                                // available slot pet 
                                step.requiredSlotPet = false;
                            }
                            break;
                        case StepType.Type8:
                            // Res Battle 1 ==> Win 
                            // Res Battle 2 ==> Lose
                            // Res battle 3 ==> Runout
                            resBattle = tsBuffer.Buffer.Read16(tsBuffer.Pos + 1);
                            if (mapid == 12001)
                            {
                                Console.WriteLine("resBattle >>" + resBattle);
                            }
                            step.resBattle = resBattle;
                            break;
                        case StepType.Type9:
                            //sample 09 00 00 01 05 86 2F
                            ushort case_9_unknown_1 = tsBuffer.Buffer.Read16(tsBuffer.Pos - 1);
                            ushort case_9_unknown_2 = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos + 1);
                            ushort case_9_unknown_3 = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos + 2);
                            ushort requiredNpc = tsBuffer.Buffer.Read16(tsBuffer.Pos + 3);
                            step.requiredNpc.Add(requiredNpc);
                            break;
                        case StepType.Type10:
                            ushort idDialog = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos - 1);
                            step.idDialog = idDialog;
                            optionId = tsBuffer.Buffer.Read16(tsBuffer.Pos + 1);
                            step.optionId = optionId;
                            break;
                    }

                    tsBuffer.IncreasePosition(2);
                    ushort unk2 = tsBuffer.Buffer.Read16(tsBuffer.Pos - 1);
                    tsBuffer.IncreasePosition(2);

                    ushort unk3 = tsBuffer.Buffer.Read16(tsBuffer.Pos - 1);
                    tsBuffer.IncreasePosition(2);
                    if (stepType == 2)
                    {
                        step.status = unk3;
                    }

                    ushort unk4 = tsBuffer.Read16();
                    tsBuffer.IncreasePosition(7);

                    ushort prevStep = tsBuffer.GetNumber<ushort>();
                    tsBuffer.IncreasePosition(1);

                    conditions = tsBuffer.GetNumber<ushort>();
                    tsBuffer.IncreasePosition(1);
                    if (conditions > 1)
                    {
                        condition++;
                        idxStepAddCondition = (ushort)(steps.Count + 1);
                    }
                    // Here pos is total sub step
                    tsBuffer.IncreasePosition(2);
                    uint toPos = tsBuffer.Pos;
                    ushort totalPackages = tsBuffer.GetNumber<ushort>();
                    if (i == 0)
                    {
                        i = 0;
                    }
                    tsBuffer.IncreasePosition(1);
                    List<PackageSend> listPackages = new();
                    ushort npcId = 0;

                    for (int k = 0; k < totalPackages; k++)
                    {
                        byte[] package = tsBuffer.Buffer.Read((int)tsBuffer.Pos, 13);
                        PackageSend pg = new()
                        {
                            package = package
                        };
                        listPackages.Add(pg);
                        if (idBox > 0)
                        {
                            step.npcIdInMap = idBox;
                        }
                        else if ((tsBuffer[tsBuffer.Pos + 3] == 1 | tsBuffer[tsBuffer.Pos + 3] == 6) & tsBuffer[tsBuffer.Pos + 4] == 3)
                        {
                            npcId = tsBuffer[tsBuffer.Pos + 5];
                            if (step.npcIdInMap == 0)
                                step.npcIdInMap = npcId;
                        }
                        if (tsBuffer[tsBuffer.Pos + 3] == 6 & tsBuffer[tsBuffer.Pos + 4] == 3)
                        {
                            ushort idDialogOption = tsBuffer.Buffer.Read16(tsBuffer.Pos + 12);
                        }
                        if (tsBuffer[tsBuffer.Pos + 3] == 3)
                        {
                            ushort idBattle = tsBuffer.Buffer.Read16(tsBuffer.Pos + 12);
                        }
                        if (tsBuffer[tsBuffer.Pos + 3] == 0 & tsBuffer[tsBuffer.Pos + 4] == 3)
                        {
                            ushort idNpcInMapJoin = tsBuffer[tsBuffer.Pos + 5];
                        }
                        if (tsBuffer[tsBuffer.Pos + 3] == 0 & tsBuffer[tsBuffer.Pos + 4] == 1)
                        {
                            ushort idItemReceived = tsBuffer.Buffer.Read16(tsBuffer.Pos + 5);
                            ushort unknown = tsBuffer[tsBuffer.Pos + 7];
                            ushort quantity = tsBuffer[tsBuffer.Pos + 8];
                        }
                        tsBuffer.IncreasePosition(14);
                    }
                    //if (option > 1)
                    //{
                    //    step.options = new Dictionary<ushort, List<PackageSend>>();
                    //    step.options.Add(option, listPackages);
                    //}
                    //if (mapid == 12002 & npcId <= nb_npc & npcId > 0) Console.WriteLine("NPC >>> "+ npcId);
                    step.packageSend = listPackages.ToArray();
                    steps.Add(step);
                }

                //quest.steps = steps;

                //listQuests.Add(quest);
                //TalkQuestItem talkQuestItem = new TalkQuestItem();
                //ushort id_talk_quest = data[pos];
                //talkQuestItem.idTalking = id_talk_quest;
                //pos += 16;
                //if (pos >= data.Length)
                //{
                //    return;
                //}
                //ushort total_talk_quest = data[pos];
                //talkQuestItem.totalTalking = total_talk_quest;
                ////pos += 1;
                //for (int j = 0; j < total_talk_quest; j++)
                //{
                //    pos += 23;
                //    ushort total_step = data[pos];
                //    //pos += 1;
                //    for (int k = 0; k < total_step; k++)
                //    {
                //        pos += 2;
                //        byte[] dialog = new byte[12];
                //        for (int l = 0; l < 12; l++)
                //        {
                //            dialog[l] = data[pos + l];
                //        }
                //        pos += 14;
                //    }

                //}
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static ushort LoadRefStep(TsBuffer tsBuffer, ushort mapid, List<Step> steps, int i, ushort condition, ushort idxStepAddCondition)
        {
            //01 3C A8 01 02 00 00 00 00 00 00 00 00 00 00 00 00 02 00 00 00 00 
            //07 00 00 01 04 0F 00 00 00 00 00 00 00 00 00 00 00 04 00 00 00 00
            //02 11 27 01 05 01 00 00 00 00 00 00 00 00 00 00 00 0C 01 00 00 01
            //07 05 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 0F 00 00 00 00
            //09 00 00 01 06 86 2F 00 00 00 00 00 00 00 00 00 00 09 00 00 00 00
            condition++;

            tsBuffer.IncreasePosition(1);
            ushort typeCondition = tsBuffer.GetNumber<ushort>();
            tsBuffer.IncreasePosition(1);

            ushort unknown_1 = tsBuffer[tsBuffer.Pos]; // 0 + 5 
            ushort unknown_2 = tsBuffer[tsBuffer.Pos + 1]; // 0
            ushort questRequired = tsBuffer.Read16(); // quest Id or Item Id

            ushort quantity = tsBuffer.GetNumber<ushort>(); ; // 1 - 2 or quantity
            tsBuffer.IncreasePosition(1);
            ushort unknown_4 = tsBuffer.GetNumber<ushort>(); ; // 0 - 4 - 5
            tsBuffer.IncreasePosition(1);

            ushort requiredLevel = tsBuffer.Buffer.Read16(tsBuffer.Pos); // Level required - NPC ID 
            ushort required = tsBuffer.Buffer.GetNumberAt<ushort>(tsBuffer.Pos); // 0 1 2
            if (mapid == 12001 & i == 15)
            {
                Console.WriteLine(" unknown_1 >>> " + unknown_1);
                Console.WriteLine(" unknown_2 >>> " + quantity);
            }
            switch (typeCondition)
            {
                case 7:
                    {
                        Step tempStep = steps.ElementAt(idxStepAddCondition - 1);
                        steps[idxStepAddCondition - 1] = tempStep;
                        if (unknown_1 == 0 & requiredLevel > 0)
                        {
                            tempStep.requiredLevel = requiredLevel;
                        }
                        if (unknown_1 == 5 & quantity == 1)
                        {
                            if (mapid == 12001 & i == 15)
                            {
                                Console.WriteLine("  tempStep.stepId >>> " + tempStep.stepId);
                            }
                            // Full pet 
                            tempStep.requiredSlotPet = true;
                        }
                        if (unknown_1 == 5 && quantity == 2)
                        {
                            // available slot pet 
                            tempStep.requiredSlotPet = false;
                        }
                        steps[idxStepAddCondition - 1] = tempStep;
                        break;
                    }
                case 2:
                    {
                        Step tempStep = steps.ElementAt(idxStepAddCondition - 1);

                        if (required == 1 | required == 2)
                        {
                            if (!tempStep.requiredQuests.ContainsKey(questRequired))
                            {
                                tempStep.requiredQuests.Add(questRequired, new List<ushort> { quantity, unknown_4, required });
                            }
                            //ushort temp = (ushort)(questRequired + quantity + unknown_4 + required);

                        }
                        if (required == 0 || unknown_1 == 2)
                        {
                            //ushort temp = (ushort)(questRequired + quantity + unknown_4 + required);
                            if (!tempStep.receivedQuests.ContainsKey(questRequired))
                            {
                                tempStep.receivedQuests.Add(questRequired, new List<ushort> { quantity, unknown_4, required });
                            }

                        }
                        steps[idxStepAddCondition - 1] = tempStep;
                        break;
                    }
                case 9:
                    {
                        Step tempStep = steps.ElementAt(idxStepAddCondition - 1);
                        tempStep.requiredNpc.Add(requiredLevel);
                        steps[idxStepAddCondition - 1] = tempStep;
                        break;
                    }
                case 1:
                    {
                        Step tempStep = steps.ElementAt(idxStepAddCondition - 1);
                        if (!tempStep.requiredItems.ContainsKey(questRequired))
                        {
                            tempStep.requiredItems.Add(questRequired, quantity);
                            steps[idxStepAddCondition - 1] = tempStep;
                        }
                        break;
                    }
                    //case 8:
                    //    {
                    //        // Res Battle 1 ==> Win 
                    //        // Res Battle 2 ==> Lose
                    //        // Res battle 3 ==> Runout
                    //        resBattle = read16(data, pos + 1);
                    //        if (mapid == 12001)
                    //        {
                    //            Console.WriteLine("resBattle >>" + resBattle);
                    //        }
                    //        step.resBattle = resBattle;
                    //        break;
                    //    }

                    //case 3:
                    //    {
                    //        idBox = data[pos - 1];
                    //        break;
                    //    }
            }

            tsBuffer.IncreasePosition(17);
            return condition;
        }

        private static void LoadBattles(MapData mapData, TsBuffer tsBuffer)
        {
            mapData.battleListOnMap.Clear();
            ushort nb_battle = tsBuffer.Read16();
            ushort[] listNpcId;
            for (int i = 0; i < nb_battle; i++)
            {
                ushort defaultGround = 876;
                ushort index = tsBuffer.GetNumber<ushort>();
                tsBuffer.IncreasePosition(5);
                ushort quantityNpc = tsBuffer.GetNumber<ushort>();
                tsBuffer.IncreasePosition(2);
                listNpcId = new ushort[11];
                for (int j = 0; j < quantityNpc; j++)
                {
                    ushort indexNpc = tsBuffer.Read16();
                    ushort npcId = tsBuffer.Read16();
                    ushort posNpc = (ushort)(tsBuffer.GetNumber<ushort>() - 1);
                    tsBuffer.IncreasePosition(1);
                    ushort turnBatle = tsBuffer.GetNumber<ushort>();
                    tsBuffer.IncreasePosition(1);
                    //if (mapid == 12001) { Console.WriteLine(" index >>> " + indexNpc);
                    //    Console.WriteLine(" index >>> " + indexNpc);
                    //    Console.WriteLine(" npcId >>> " + npcId);
                    //    Console.WriteLine(" posNpc >>> " + posNpc);
                    //    Console.WriteLine(" turnBatle >>> " + turnBatle);
                    //}
                    if (posNpc > 10) continue;
                    listNpcId[posNpc] = npcId;
                }
                mapData.battleListOnMap.Add(index, new BattleInfo(defaultGround, listNpcId));
                ushort unknow_1 = tsBuffer.Read16();
                ushort unknow_2 = tsBuffer.Read16();
                for (int j = 0; j < unknow_2; j++)
                {
                    ushort unknow_3 = tsBuffer[tsBuffer.Pos + 11];
                    tsBuffer.IncreasePosition(12);
                    tsBuffer.IncreasePosition(unknow_3 * (uint)13);
                }
            }
        }

        public static int findSubArrInArr(byte[] data, int curr, int[] subArr)
        {
            //bool firstCondition = data[i] == firstQuest[0] & data[i + 1] == firstQuest[1] & data[i + 2] == firstQuest[2] & data[i + 3] == firstQuest[3] & data[i + 4] == firstQuest[4];
            //bool seccondCondition = data[i + 5] == firstQuest[5] & data[i + 6] == firstQuest[6] & data[i + 7] == firstQuest[7] & data[i + 8] == firstQuest[8];
            for (int i = curr; i < (data.Length - subArr.Length - 1); i++)
            {
                if (data[i] == subArr[0])
                {
                    bool is_existed = true;
                    for (int j = 1; j < subArr.Length; j++)
                    {
                        if (data[i + j] != subArr[j])
                        {
                            is_existed = false;
                            break;
                        }
                    }
                    if (is_existed)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        //public static Step getStepByQuest(TSClient client, ushort npcIdOnMap) {
        //    List<Step> steps = listStepOnMap[client.map.mapid].FindAll(item => item.npcIdInMap == npcIdOnMap);

        //    List<Step> stepsQ = steps.FindAll(item => !item.questId.Equals(null) | item.questId > 0);
        //    Step step;
        //    if (stepsQ.Count > 0)
        //    {
        //        ushort questId = stepsQ[0].questId;
        //        int currentStep = client.checkQuest(client, questId);
        //        if (currentStep > -1)
        //        {
        //            step = stepsQ.Find(item => item.stepId == currentStep);
        //        }
        //        else
        //        {
        //            step = stepsQ.Find(item => item.status != 1);
        //            currentStep = step.stepId;
        //        }
        //        client.insertOrUpdateQuest(client, questId, (ushort)currentStep);
        //    }
        //    else
        //    {
        //        step = steps[0];
        //    }
        //    return step;
        //}

        public static Tuple<ushort, ushort>? loadCoor(ushort mapid, ushort destid, ushort warpId)
        {
            if (!listMapData.ContainsKey(mapid)) return null;

            var tsBuffer = TsBuffer.LoadFromFile(fileLocation, listMapData[mapid].bufferInformation);

            int nb_npc = tsBuffer.GetNumberAt<int>(0x67);

            //Console.WriteLine((data[0] + (data[1] << 8)));
            //Console.WriteLine("NPC POst << " + nb_npc);

            tsBuffer.IncreasePosition(4);
            //NPC, later
            for (int i = 0; i < nb_npc; i++)
            {
                ushort clickID = tsBuffer.Read16();
                //Console.WriteLine("click Id ID >>>" + clickID.ToString());
                ushort npcID = tsBuffer.Read16();
                //Console.WriteLine("npc ID >>>" + npcID.ToString()); 
                ushort nb1 = tsBuffer.Read16();
                tsBuffer.IncreasePosition(nb1);
                byte nb2 = tsBuffer.ReadByte();
                tsBuffer.IncreasePosition(nb2);
                byte nb_f = tsBuffer.ReadByte();
                tsBuffer.IncreasePosition(8);
                tsBuffer.IncreasePosition((uint)8 * nb_f);
                tsBuffer.IncreasePosition(31);

                int posX = tsBuffer.Read32();
                int posY = tsBuffer.Read32();
                tsBuffer.IncreasePosition(41);
            }

            ushort nb_entry_exit = tsBuffer.Read16();
            for (int i = 0; i < nb_entry_exit; i++)
            {
                uint id = tsBuffer.ReadU32();
                tsBuffer.DecreasePosition(1); // ???
                uint posX = tsBuffer.ReadU32();
                uint posY = tsBuffer.ReadU32();
                tsBuffer.IncreasePosition(3);
            }

            ushort nb_unk1 = tsBuffer.Read16();
            for (int i = 0; i < nb_unk1; i++)
            {
                tsBuffer.IncreasePosition(2);
                ushort nb = tsBuffer.Read16();
                tsBuffer.IncreasePosition(nb);
                tsBuffer.IncreasePosition(21);
                //Console.WriteLine(posX + " " + posY); 
            }

            ushort nb_unk2 = tsBuffer.Read16();
            //Console.WriteLine(nb_unk2); 
            for (int i = 0; i < nb_unk2; i++)
            {
                tsBuffer.IncreasePosition(2);
                ushort nb = tsBuffer.Read16();
                tsBuffer.IncreasePosition(2);
                tsBuffer.IncreasePosition(nb);
                tsBuffer.IncreasePosition(17);
                //Console.WriteLine(posX + " " + posY); 
            }

            ushort nb_dialog = tsBuffer.Read16();
            for (int i = 0; i < nb_dialog; i++)
            {
                tsBuffer.IncreasePosition(4);
                byte nb_d = tsBuffer.ReadByte();
                tsBuffer.IncreasePosition(4);
                tsBuffer.IncreasePosition((uint)(5 * nb_d));
                //Console.WriteLine(nb_d); 
            }

            ushort nb_warp = tsBuffer.Read16();
            uint X = 0, Y = 0;
            int count = 0;
            for (int i = 0; i < nb_warp; i++)
            {
                ushort warp_id = tsBuffer.Read16();
                ushort dest_map = tsBuffer.Read16();
                tsBuffer.IncreasePosition(2);
                uint posX = tsBuffer.ReadU32() * 20 - 10;
                uint posY = tsBuffer.ReadU32() * 20 - 10;
                tsBuffer.IncreasePosition(0x19);

                if (dest_map == destid & warpId == warp_id)
                {
                    X = posX; Y = posY;
                    count++;
                }

                //Console.WriteLine(mapid + " " + warp_id + " " + dest_map + " " + posX + " " + posY);
            }

            if (count == 1)
                return new Tuple<ushort, ushort>((ushort)X, (ushort)Y);
            return null;
        }
    }
}
