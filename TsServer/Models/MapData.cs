using System;
using System.Runtime.InteropServices;
using Ts.DataTools;
using TsServer.Buffer;

namespace TsServer.Models
{
    public class MapData
    {
        public ushort id;
        public BufferInformation bufferInformation;

        public readonly List<NpcOnMap> npcs = new();
        public readonly List<ItemOnMap> items = new();
        public readonly List<Step> steps = new();
        public readonly List<EveInfo> eveList = new();
        public readonly List<Quest> quests = new();
        public readonly Dictionary<ushort, BattleInfo> battleListOnMap = new();
        public readonly List<MapWarp> warps = new();
        public readonly List<MapGate> gates = new();
        public readonly List<Dialog> dialogs = new();
    }

    public struct EveInfo
    {
        public ushort id;
        public byte length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 254)]
        public byte[] dialog;
    }

    public class NpcOnMap
    {
        public ushort NpcIndex;
        public ushort NpcId;
        public ushort unk_1;
        public ushort unk_2;
        public ushort unk_3;
        public ushort totalTalking;
        public byte type;
        public int posX;
        public int posY;
        public List<byte> Events = new();
        public List<Tuple<ushort, ushort>> Patrol = new();
    }

    public struct PackageSend
    {
        public byte[] package;
        public string type;
    }

    public struct Step
    {
        public ushort questId;
        public ushort stepId;
        public PackageSend[] packageSend;
        public ushort npcIdInMap;
        public List<ushort> subStepIds;
        public ushort type; // 2 = Talk | 8 = Reward | A(10) = Select Option | 7 = Condition 
        public ushort optionId;
        public ushort idDialog;
        public ushort qIndex;
        public ushort resBattle;
        public ushort status;
        public ushort requiredLevel;
        public Dictionary<ushort, List<ushort>> requiredQuests;
        public Dictionary<ushort, List<ushort>> receivedQuests;
        public List<ushort> requiredNpc;
        public Dictionary<ushort, ushort> requiredItems;
        public bool requiredSlotPet;
        public bool normalTalk;
        public List<ushort> rootBit;
    }

    public struct Talks
    {
        public ushort npcIdInMap;
        public byte[] dialog;
    }

    public struct Quest
    {
        public ushort questId;
        public List<Step> steps;
        public int[] requiredQuests;
        public int status;
        public ushort npcIdOnMap;

    }

    public struct ItemOnMap
    {
        public ushort idItemOnMap;
        public ushort idItem;
        public ushort posX;
        public ushort posY;
        public ushort timeDelay;
    }

    public struct MapWarp
    {
        public uint WarpId { get; set; }
        public ushort DestId { get; set; }
        public uint DestX { get; set; }
        public uint DestY { get; set; }
        public byte[] Unk { get; set; }
    }

    public struct Dialog
    {
        byte First { get; set; }
        byte NbDd1 { get; set; }
        byte NbDd2 { get; set; }
        byte NbDd3 { get; set; }
    }

    public class MapGate
    {
        public ushort GateId { get; set; }
        public List<byte> Eve { get; set; }
    }
}
