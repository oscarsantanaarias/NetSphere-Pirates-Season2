using System;
using System.IO;
using System.Runtime.CompilerServices;
using BlubLib.Serialization;
using Netsphere.Network.Data.Game;
using Netsphere.Network.Serializers;
using ProudNet;
using ProudNet.Serialization.Serializers;

namespace Netsphere.Network.Message.Game
{
    [BlubContract]
    public class SLoginAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public GameLoginResult Result { get; set; }

        [BlubMember(2)]
        public ulong Unk { get; set; }

        public SLoginAckMessage()
        { }

        public SLoginAckMessage(GameLoginResult result, ulong accountId)
        {
            AccountId = accountId;
            Result = result;
        }

        public SLoginAckMessage(GameLoginResult result)
        {
            Result = result;
        }
    }

    [BlubContract]
    public class SBeginAccountInfoAckMessage : IGameMessage
    {
        // Season 2 reads seven uints before anything else. Season 1 does not.
        [BlubMember(0)]
        public uint Pre1 { get; set; }

        [BlubMember(1)]
        public uint Pre2 { get; set; }

        [BlubMember(2)]
        public uint Pre3 { get; set; }

        [BlubMember(3)]
        public uint Pre4 { get; set; }

        [BlubMember(4)]
        public uint Pre5 { get; set; }

        [BlubMember(5)]
        public uint Pre6 { get; set; }

        [BlubMember(6)]
        public uint Pre7 { get; set; }

        [BlubMember(7)]
        public byte Unk1 { get; set; } // IsGM?

        [BlubMember(8)]
        public uint Unk2 { get; set; }

        [BlubMember(9)]
        public byte Level { get; set; }

        [BlubMember(10)]
        public byte Unk3 { get; set; }

        [BlubMember(11)]
        public uint TotalExp { get; set; }

        [BlubMember(12)]
        public uint AP { get; set; }

        [BlubMember(13)]
        public uint PEN { get; set; }

        [BlubMember(14)]
        public uint TutorialState { get; set; }

        [BlubMember(15, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(16)]
        public uint Unk4 { get; set; } // something with licenses needed to enter s4league

        [BlubMember(17)]
        public DMStatsDto DMStats { get; set; }

        [BlubMember(18)]
        public TDStatsDto TDStats { get; set; }

        [BlubMember(19)]
        public ChaserStatsDto ChaserStats { get; set; }

        [BlubMember(20)]
        public BRStatsDto BRStats { get; set; }

        [BlubMember(21)]
        public CPTStatsDto CPTStats { get; set; }

        // And a sixth block plus five more uints at the end, also Season 2 only.
        [BlubMember(22)]
        public SeizeStatsDto SeizeStats { get; set; }

        [BlubMember(23)]
        public uint Post1 { get; set; }

        [BlubMember(24)]
        public uint Post2 { get; set; }

        [BlubMember(25)]
        public uint Post3 { get; set; }

        [BlubMember(26)]
        public uint Post4 { get; set; }

        [BlubMember(27)]
        public uint Post5 { get; set; }

        public SBeginAccountInfoAckMessage()
        {
            DMStats = new DMStatsDto();
            TDStats = new TDStatsDto();
            ChaserStats = new ChaserStatsDto();
            BRStats = new BRStatsDto();
            CPTStats = new CPTStatsDto();
            SeizeStats = new SeizeStatsDto();
        }
    }

    [BlubContract]
    public class SOpenCharacterInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Slot { get; set; }

        [BlubMember(1)]
        public byte Unk1 { get; set; }

        [BlubMember(2)]
        public byte Unk2 { get; set; }

        [BlubMember(3)]
        public CharacterStyle Style { get; set; }

        public SOpenCharacterInfoAckMessage()
        {
            Unk1 = 1; // max skill?
            Unk2 = 3; // max weapons?
        }
    }

    [BlubContract]
    public class SCharacterEquipInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Slot { get; set; }

        [BlubMember(3, typeof(ArrayWithIntPrefixAndIndexSerializer))]
        public ulong[] Weapons { get; set; }

        [BlubMember(4, typeof(ArrayWithIntPrefixAndIndexSerializer))]
        public ulong[] Skills { get; set; }

        [BlubMember(5, typeof(ArrayWithIntPrefixAndIndexSerializer))]
        public ulong[] Clothes { get; set; }

        public SCharacterEquipInfoAckMessage()
        {
            Weapons = new ulong[9];
            Skills = new ulong[1];
            Clothes = new ulong[7];
        }
    }

    [BlubContract(typeof(Serializer))]
    public class SInventoryInfoAckMessage : IGameMessage
    {
        public ItemDto[] Items { get; set; }

        public SInventoryInfoAckMessage()
        {
            Items = Array.Empty<ItemDto>();
        }

        internal class Serializer : ISerializer<SInventoryInfoAckMessage>
        {
            public bool CanHandle(Type type) => type == typeof(SInventoryInfoAckMessage);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Serialize(BinaryWriter writer, SInventoryInfoAckMessage value)
            {
                using (var w2 = new BinaryWriter(new MemoryStream()))
                {
                    w2.Write((ushort)value.Items.Length);

                    var serializer = BlubLib.Serialization.Serializer.GetSerializer<ItemDto>();
                    foreach (var item in value.Items)
                        serializer.Serialize(w2, item);

                    var data = w2.ToArray().CompressLZO();
                    writer.WriteStruct(data);
                    writer.Write(data.Length);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SInventoryInfoAckMessage Deserialize(BinaryReader reader)
            {
                var message = new SInventoryInfoAckMessage();
                var compressed = reader.ReadStruct();
                reader.ReadUInt32(); // length

                var decompressed = compressed.DecompressLZO(compressed.Length * 4);

                using (var r2 = decompressed.ToBinaryReader())
                {
                    message.Items = new ItemDto[r2.ReadInt16()];
                    var serializer = BlubLib.Serialization.Serializer.GetSerializer<ItemDto>();
                    for (var i = 0; i < message.Items.Length; i++)
                        message.Items[i] = serializer.Deserialize(r2);
                }
                return message;
            }
        }
    }

    [BlubContract]
    public class SSuccessDeleteCharacterAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Slot { get; set; }

        public SSuccessDeleteCharacterAckMessage()
        { }

        public SSuccessDeleteCharacterAckMessage(byte slot)
        {
            Slot = slot;
        }
    }

    [BlubContract]
    public class SSuccessSelectCharacterAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Slot { get; set; }

        public SSuccessSelectCharacterAckMessage()
        { }

        public SSuccessSelectCharacterAckMessage(byte slot)
        {
            Slot = slot;
        }
    }

    [BlubContract]
    public class SSuccessCreateCharacterAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Slot { get; set; }

        [BlubMember(1)]
        public CharacterStyle Style { get; set; }

        [BlubMember(2)]
        public byte MaxSkills { get; set; }

        [BlubMember(3)]
        public byte MaxWeapons { get; set; }

        public SSuccessCreateCharacterAckMessage()
        {
            MaxSkills = 1;
            MaxWeapons = 3;
        }

        public SSuccessCreateCharacterAckMessage(byte slot, CharacterStyle style)
            : this()
        {
            Slot = slot;
            Style = style;
        }
    }

    [BlubContract]
    public class SServerResultInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ServerResult Result { get; set; }

        public SServerResultInfoAckMessage()
        { }

        public SServerResultInfoAckMessage(ServerResult result)
        {
            Result = result;
        }
    }

    [BlubContract]
    public class SCreateNickAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Nickname { get; set; }
    }

    [BlubContract]
    public class SCheckNickAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(IntBooleanSerializer))]
        public bool IsTaken { get; set; }

        public SCheckNickAckMessage()
        { }

        public SCheckNickAckMessage(bool isTaken)
        {
            IsTaken = isTaken;
        }
    }

    [BlubContract]
    public class SUseItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte CharacterSlot { get; set; }

        [BlubMember(1)]
        public byte EquipSlot { get; set; }

        [BlubMember(2)]
        public ulong ItemId { get; set; }

        [BlubMember(3)]
        public UseItemAction Action { get; set; }
    }

    [BlubContract]
    public class SInventoryActionAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public InventoryAction Action { get; set; }

        [BlubMember(1)]
        public ItemDto Item { get; set; }

        public SInventoryActionAckMessage()
        {
            Item = new ItemDto();
        }

        public SInventoryActionAckMessage(InventoryAction action, ItemDto item)
        {
            Action = action;
            Item = item;
        }
    }

    [BlubContract]
    public class SIdsInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }

        [BlubMember(1)]
        public byte Slot { get; set; }

        public SIdsInfoAckMessage()
        { }

        public SIdsInfoAckMessage(uint unk, byte slot)
        {
            Unk = unk;
            Slot = slot;
        }
    }

    [BlubContract]
    public class SEnteredPlayerAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public RoomPlayerDto Player { get; set; }

        public SEnteredPlayerAckMessage()
        {
            Player = new RoomPlayerDto();
        }

        public SEnteredPlayerAckMessage(RoomPlayerDto plr)
        {
            Player = plr;
        }
    }

    [BlubContract]
    public class SEnteredPlayerClubInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public PlayerClubInfoDto Player { get; set; }

        public SEnteredPlayerClubInfoAckMessage()
        {
            Player = new PlayerClubInfoDto();
        }
    }

    [BlubContract]
    public class SEnteredPlayerListAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public RoomPlayerDto[] Players { get; set; }

        public SEnteredPlayerListAckMessage()
        {
            Players = Array.Empty<RoomPlayerDto>();
        }

        public SEnteredPlayerListAckMessage(RoomPlayerDto[] players)
        {
            Players = players;
        }
    }

    [BlubContract]
    public class SEnteredPlayerClubInfoListAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public PlayerClubInfoDto[] Players { get; set; }

        public SEnteredPlayerClubInfoListAckMessage()
        {
            Players = Array.Empty<PlayerClubInfoDto>();
        }
    }

    [BlubContract]
    public class SSuccessEnterRoomAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public EnterRoomInfoDto RoomInfo { get; set; }

        public SSuccessEnterRoomAckMessage()
        {
            RoomInfo = new EnterRoomInfoDto();
        }

        public SSuccessEnterRoomAckMessage(EnterRoomInfoDto roomInfo)
        {
            RoomInfo = roomInfo;
        }
    }

    [BlubContract]
    public class SLeavePlayerAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        public SLeavePlayerAckMessage()
        { }

        public SLeavePlayerAckMessage(ulong accountId)
        {
            AccountId = accountId;
        }
    }

    [BlubContract]
    [Obsolete("This handler is empty inside the client")]
    public class SJoinTunnelPlayerAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }
    }

    [BlubContract]
    public class STimeSyncAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint ClientTime { get; set; }

        [BlubMember(1)]
        public uint ServerTime { get; set; }
    }

    [BlubContract]
    public class SPlayTogetherSignAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class SPlayTogetherInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; }

        [BlubMember(1)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class SPlayTogetherSignInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class SPlayTogetherCancelAckMessage : IGameMessage
    { }

    [BlubContract]
    public class SChangeGameRoomAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public RoomDto Room { get; set; }

        public SChangeGameRoomAckMessage()
        {
            Room = new RoomDto();
        }

        public SChangeGameRoomAckMessage(RoomDto room)
        {
            Room = room;
        }
    }

    [BlubContract]
    public class SNewShopUpdateRequestAckMessage : IGameMessage
    { }

    [BlubContract]
    public class SLogoutAckMessage : IGameMessage
    { }

    [BlubContract]
    public class SPlayTogetherKickAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class SChannelListInfoAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ChannelInfoDto[] Channels { get; set; }

        public SChannelListInfoAckMessage()
        {
            Channels = Array.Empty<ChannelInfoDto>();
        }

        public SChannelListInfoAckMessage(ChannelInfoDto[] channels)
        {
            Channels = channels;
        }
    }

    [BlubContract]
    public class SChannelDeployPlayerAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public uint Unk1 { get; set; } // room id?

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk2 { get; set; } // maybe nickname

        public SChannelDeployPlayerAckMessage()
        {
            Unk2 = "";
        }
    }

    [BlubContract]
    public class SChannelDisposePlayerAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract(typeof(Serializer))]
    public class SGameRoomListAckMessage : IGameMessage
    {
        public ChannelInfoRequest ListType { get; set; }
        public RoomDto[] Rooms { get; set; }

        public SGameRoomListAckMessage()
        {
            Rooms = Array.Empty<RoomDto>();
        }

        public SGameRoomListAckMessage(RoomDto[] rooms)
        {
            ListType = ChannelInfoRequest.RoomList;
            Rooms = rooms;
        }

        internal class Serializer : ISerializer<SGameRoomListAckMessage>
        {
            public bool CanHandle(Type type) => type == typeof(SGameRoomListAckMessage);

            public void Serialize(BinaryWriter writer, SGameRoomListAckMessage value)
            {
                using (var w2 = new BinaryWriter(new MemoryStream()))
                {
                    w2.WriteEnum(value.ListType);
                    w2.Write((ushort)value.Rooms.Length);

                    var serializer = BlubLib.Serialization.Serializer.GetSerializer<RoomDto>();
                    foreach (var room in value.Rooms)
                        serializer.Serialize(w2, room);

                    var data = w2.ToArray().CompressLZO();
                    writer.WriteStruct(data);
                    writer.Write(data.Length);
                }
            }

            public SGameRoomListAckMessage Deserialize(BinaryReader reader)
            {
                var message = new SGameRoomListAckMessage();
                var compressed = reader.ReadStruct();
                reader.ReadUInt32(); // length

                var decompressed = compressed.DecompressLZO(compressed.Length * 4);

                using (var r2 = decompressed.ToBinaryReader())
                {
                    message.ListType = r2.ReadEnum<ChannelInfoRequest>();
                    message.Rooms = new RoomDto[r2.ReadInt16()];
                    var serializer = BlubLib.Serialization.Serializer.GetSerializer<RoomDto>();
                    for (var i = 0; i < message.Rooms.Length; i++)
                        message.Rooms[i] = serializer.Deserialize(r2);
                }
                return message;
            }
        }
    }

    [BlubContract]
    public class SDeployGameRoomAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public RoomDto Room { get; set; }

        public SDeployGameRoomAckMessage()
        {
            Room = new RoomDto();
        }

        public SDeployGameRoomAckMessage(RoomDto room)
        {
            Room = room;
        }
    }

    [BlubContract]
    public class SDisposeGameRoomAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint RoomId { get; set; }

        public SDisposeGameRoomAckMessage()
        { }

        public SDisposeGameRoomAckMessage(uint roomId)
        {
            RoomId = roomId;
        }
    }

    [BlubContract]
    public class SGamePingAverageAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; } // ping?
    }

    [BlubContract]
    public class SBuyItemAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Ids { get; set; }

        [BlubMember(1)]
        public ItemBuyResult Result { get; set; }

        [BlubMember(2)]
        public ShopItemDto Item { get; set; }

        public SBuyItemAckMessage()
        {
            Ids = Array.Empty<ulong>();
            Item = new ShopItemDto();
        }

        public SBuyItemAckMessage(ItemBuyResult result)
            : this()
        {
            Result = result;
        }

        public SBuyItemAckMessage(ulong[] ids, ShopItemDto item)
        {
            Ids = ids;
            Result = ItemBuyResult.OK;
            Item = item;
        }
    }

    [BlubContract]
    public class SRepairItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ItemRepairResult Result { get; set; }

        [BlubMember(1)]
        public ulong ItemId { get; set; }
    }

    [BlubContract]
    public class SItemDurabilityInfoAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ItemDurabilityInfoDto[] Items { get; set; }

        public SItemDurabilityInfoAckMessage()
        {
            Items = Array.Empty<ItemDurabilityInfoDto>();
        }

        public SItemDurabilityInfoAckMessage(ItemDurabilityInfoDto[] items)
        {
            Items = items;
        }

    }

    [BlubContract]
    public class SRefundItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ItemRefundResult Result { get; set; }

        [BlubMember(1)]
        public ulong ItemId { get; set; }
    }

    [BlubContract]
    public class SRefreshCashInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint PEN { get; set; }

        [BlubMember(1)]
        public uint AP { get; set; }

        public SRefreshCashInfoAckMessage()
        { }

        public SRefreshCashInfoAckMessage(uint pen, uint ap)
        {
            PEN = pen;
            AP = ap;
        }
    }

    [BlubContract]
    public class SAdminActionAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Result { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Message { get; set; }

        public SAdminActionAckMessage()
        {
            Message = "";
        }
    }

    [BlubContract]
    public class SAdminShowWindowAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public bool DisableConsole { get; set; }

        public SAdminShowWindowAckMessage()
        { }

        public SAdminShowWindowAckMessage(bool disableConsole)
        {
            DisableConsole = disableConsole;
        }
    }

    [BlubContract]
    public class SNoticeMessageAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Message { get; set; }

        public SNoticeMessageAckMessage()
        {
            Message = "";
        }
        public SNoticeMessageAckMessage(string message)
        {
            Message = message;
        }
    }

    [BlubContract]
    public class SCharacterSlotInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte CharacterCount { get; set; }

        [BlubMember(1)]
        public byte MaxSlots { get; set; }

        [BlubMember(2)]
        public byte ActiveCharacter { get; set; }
    }

    [BlubContract]
    public class SRefreshInvalidEquipItemAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Items { get; set; }

        public SRefreshInvalidEquipItemAckMessage()
        {
            Items = Array.Empty<ulong>();
        }
    }

    [BlubContract]
    public class SClearInvalidateItemAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public InvalidateItemInfoDto[] Items { get; set; }

        public SClearInvalidateItemAckMessage()
        {
            Items = Array.Empty<InvalidateItemInfoDto>();
        }
    }

    [BlubContract]
    public class SRefreshItemTimeInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong Unk1 { get; set; }

        [BlubMember(1)]
        public ulong Unk2 { get; set; }

        [BlubMember(2)]
        public ulong Unk3 { get; set; }
    }

    [BlubContract]
    [Obsolete("This handler is empty inside the client")]
    public class SEnableAccountStatusAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }
    }

    [BlubContract]
    public class SActiveEquipPresetAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class SMyLicenseInfoAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public uint[] Licenses { get; set; }

        public SMyLicenseInfoAckMessage()
        {
            Licenses = Array.Empty<uint>();
        }

        public SMyLicenseInfoAckMessage(uint[] licenses)
        {
            Licenses = licenses;
        }
    }

    [BlubContract]
    public class SLicensedAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ItemLicense ItemLicense { get; set; }

        [BlubMember(1)]
        public ItemNumber ItemNumber { get; set; }

        public SLicensedAckMessage()
        { }

        public SLicensedAckMessage(ItemLicense itemLicense, ItemNumber itemNumber)
        {
            ItemLicense = itemLicense;
            ItemNumber = itemNumber;
        }
    }

    [BlubContract]
    public class SCoinEventAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class SCombiCompensationAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong Unk1 { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }

        [BlubMember(2)]
        public uint Unk3 { get; set; }

        [BlubMember(3)]
        public uint Unk4 { get; set; }

        [BlubMember(4)]
        public uint Unk5 { get; set; }
    }

    [BlubContract]
    public class SClubInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public PlayerClubInfoDto ClubInfo { get; set; }

        public SClubInfoAckMessage()
        {
            ClubInfo = new PlayerClubInfoDto();
        }

        public SClubInfoAckMessage(PlayerClubInfoDto clubInfo)
        {
            ClubInfo = clubInfo;
        }
    }

    [BlubContract]
    public class SClubHistoryAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ClubHistoryDto History { get; set; }

        public SClubHistoryAckMessage()
        {
            History = new ClubHistoryDto();
        }
    }

    [BlubContract]
    public class SEquipedBoostItemAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Items { get; set; }

        public SEquipedBoostItemAckMessage()
        {
            Items = Array.Empty<ulong>();
        }
    }

    [BlubContract]
    public class SGetClubInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ClubInfoDto ClubInfo { get; set; }

        public SGetClubInfoAckMessage()
        {
            ClubInfo = new ClubInfoDto();
        }
    }

    [BlubContract]
    public class STaskInfoAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public TaskDto[] Tasks { get; set; }

        public STaskInfoAckMessage()
        {
            Tasks = Array.Empty<TaskDto>();
        }
    }

    [BlubContract]
    public class STaskUpdateAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint TaskId { get; set; }

        [BlubMember(1)]
        public ushort Progress { get; set; }
    }

    [BlubContract]
    public class STaskRequestAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint TaskId { get; set; }

        [BlubMember(1)]
        public MissionRewardType RewardType { get; set; }

        [BlubMember(2)]
        public uint Reward { get; set; }

        [BlubMember(3)]
        public byte Slot { get; set; }
    }

    [BlubContract]
    public class SExchangeItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong Unk1 { get; set; }

        [BlubMember(1)]
        public ulong Unk2 { get; set; }
    }

    [BlubContract]
    public class STaskIngameUpdateAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint TaskId { get; set; }

        [BlubMember(1)]
        public ushort Progress { get; set; }
    }

    [BlubContract]
    public class STaskRemoveAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint TaskId { get; set; }
    }

    [BlubContract]
    public class SRandomShopChanceInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Progress { get; set; }
    }

    [BlubContract]
    public class SRandomShopItemInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public RandomShopItemDto Item { get; set; }

        public SRandomShopItemInfoAckMessage()
        {
            Item = new RandomShopItemDto();
        }
    }

    [BlubContract]
    public class SRandomShopInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public RandomShopDto Info { get; set; }

        public SRandomShopInfoAckMessage()
        {
            Info = new RandomShopDto();
        }
    }

    [BlubContract]
    public class SSetCoinAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint ArcadeCoins { get; set; }

        [BlubMember(1)]
        public uint BuffCoins { get; set; }
    }

    [BlubContract]
    public class SApplyEsperChipItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public EsperChipItemInfoDto Chip { get; set; }
    }

    [BlubContract]
    public class SArcadeRewardInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ArcadeRewardDto Reward { get; set; }
    }

    [BlubContract]
    public class SArcadeMapScoreAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ArcadeMapScoreDto[] Scores { get; set; }

        public SArcadeMapScoreAckMessage()
        {
            Scores = Array.Empty<ArcadeMapScoreDto>();
        }
    }

    [BlubContract]
    public class SArcadeStageScoreAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ArcadeStageScoreDto[] Scores { get; set; }

        public SArcadeStageScoreAckMessage()
        {
            Scores = Array.Empty<ArcadeStageScoreDto>();
        }
    }

    [BlubContract]
    public class SMixedTeamBriefingInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public MixedTeamBriefingDto[] Briefing { get; set; }

        public SMixedTeamBriefingInfoAckMessage()
        {
            Briefing = Array.Empty<MixedTeamBriefingDto>();
        }
    }

    [BlubContract]
    public class SSetGameMoneyAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }
    }

    [BlubContract]
    public class SUseCapsuleAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public CapsuleRewardDto[] Rewards { get; set; }

        [BlubMember(1)]
        public byte Result { get; set; }

        public SUseCapsuleAckMessage()
        {
            Rewards = Array.Empty<CapsuleRewardDto>();
        }

        public SUseCapsuleAckMessage(byte result)
            : this()
        {
            Result = result;
        }

        public SUseCapsuleAckMessage(CapsuleRewardDto[] rewards, byte result)
        {
            Rewards = rewards;
            Result = result;
        }
    }

    [BlubContract]
    public class SHGWKickAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Message { get; set; }

        public SHGWKickAckMessage()
        {
            Message = "";
        }
    }

    [BlubContract]
    public class SClubJoinAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Message { get; set; }

        public SClubJoinAckMessage()
        {
            Message = "";
        }
    }

    [BlubContract]
    public class SClubUnJoinAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }
    }

    [BlubContract]
    public class SNewShopUpdateCheckAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Date01 { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Date02 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Date03 { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Date04 { get; set; }

        public SNewShopUpdateCheckAckMessage()
        {
            Date01 = "";
            Date02 = "";
            Date03 = "";
            Date04 = "";
        }
    }

    [BlubContract]
    public class SNewShopUpdateInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ShopResourceType Type { get; set; }

        [BlubMember(1, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }

        [BlubMember(2)]
        public uint Unk1 { get; set; } // size of Data?

        [BlubMember(3)]
        public uint Unk2 { get; set; } // checksum?

        [BlubMember(4, typeof(StringSerializer))]
        public string Date { get; set; }

        public SNewShopUpdateInfoAckMessage()
        {
            Data = Array.Empty<byte>();
            Date = "";
        }
    }

    [BlubContract]
    public class SGiveItemResultAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public GiveItemResultDto[] Items { get; set; }

        public SGiveItemResultAckMessage()
        {
            Items = Array.Empty<GiveItemResultDto>();
        }

        public SGiveItemResultAckMessage(GiveItemResultDto[] items)
        {
            Items = items;
        }
    }

    [BlubContract]
    public class SDefaultNickAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Nickname { get; set; }

        public SDefaultNickAckMessage()
        {
            Nickname = "";
        }

        public SDefaultNickAckMessage(string nickname)
        {
            Nickname = nickname;
        }
    }

    [BlubContract]
    public class SRandomShopUpdateCheckAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Date { get; set; }

        public SRandomShopUpdateCheckAckMessage()
        {
            Date = "";
        }
    }

    [BlubContract]
    public class SRandomShopUpdateInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ShopResourceType Type { get; set; }

        [BlubMember(1, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }

        [BlubMember(2)]
        public uint Unk1 { get; set; }

        [BlubMember(3)]
        public uint Unk2 { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Date { get; set; }

        public SRandomShopUpdateInfoAckMessage()
        {
            Data = Array.Empty<byte>();
            Date = "";
        }
    }

    [BlubContract]
    public class SUseChangeNickItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Result { get; set; }

        [BlubMember(1)]
        public ulong Unk2 { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk3 { get; set; }

        public SUseChangeNickItemAckMessage()
        {
            Unk3 = "";
        }
    }

    [BlubContract]
    public class SUseResetRecordItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Result { get; set; }

        [BlubMember(1)]
        public ulong Unk2 { get; set; }
    }

    [BlubContract]
    public class SUseCoinFillingItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Result { get; set; }
    }

    [BlubContract]
    public class SDiscardItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Result { get; set; }

        [BlubMember(1)]
        public ulong ItemId { get; set; }
    }

    [BlubContract]
    public class SDeleteItemInventoryAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong ItemId { get; set; }

        public SDeleteItemInventoryAckMessage()
        { }

        public SDeleteItemInventoryAckMessage(ulong itemId)
        {
            ItemId = itemId;
        }
    }

    [BlubContract]
    public class SClubAddressAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Fingerprint { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }

        public SClubAddressAckMessage()
        {
            Fingerprint = "";
        }

        public SClubAddressAckMessage(string fingerprint, uint unk2)
        {
            Fingerprint = fingerprint;
            Unk2 = unk2;
        }
    }

    [BlubContract]
    public class SSmallLoudSpeakerAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk3 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Unk4 { get; set; }

        public SSmallLoudSpeakerAckMessage()
        {
            Unk3 = "";
            Unk4 = "";
        }
    }

    [BlubContract]
    public class SIngameEquipCheckAckMessage : IGameMessage
    { }

    [BlubContract]
    public class SUseCoinRandomShopChanceAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }
    }

    [BlubContract]
    public class SChangeNickCancelAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }
    }

    [BlubContract]
    public class SEventRewardAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }

        [BlubMember(2)]
        public uint Unk3 { get; set; }

        [BlubMember(3)]
        public uint Unk4 { get; set; }

        [BlubMember(4)]
        public uint Unk5 { get; set; }

        [BlubMember(5)]
        public uint Unk6 { get; set; }
    }

    // 60048: b4,b4,b4,b4
    [BlubContract]
    public class SGameGuardAuthAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }

        [BlubMember(2)]
        public uint Unk3 { get; set; }

        [BlubMember(3)]
        public uint Unk4 { get; set; }
    }

    // 60049: b2,str
    [BlubContract]
    public class SHShieldMakeResponseAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ushort Unk1 { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Unk2 { get; set; }

        public SHShieldMakeResponseAckMessage()
        {
            Unk2 = "";
        }
    }

    // 60085: u64,u8,u32,str,u32,u32,u32
    [BlubContract]
    public class SFindUserAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public byte Unk1 { get; set; }

        [BlubMember(2)]
        public uint Unk2 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(4)]
        public uint Unk3 { get; set; }

        [BlubMember(5)]
        public uint Unk4 { get; set; }

        [BlubMember(6)]
        public uint Unk5 { get; set; }

        public SFindUserAckMessage()
        {
            Nickname = "";
        }
    }

    // 60093: contador int + RoomDto[]
    [BlubContract]
    public class SRoomInfosAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public RoomDto[] Rooms { get; set; }

        public SRoomInfosAckMessage()
        {
            Rooms = Array.Empty<RoomDto>();
        }
    }

    // 60095: sin cuerpo
    [BlubContract]
    public class SStartLoadingAckMessage : IGameMessage
    {
    }

    // 60096: sin cuerpo
    [BlubContract]
    public class SStartIngameAckMessage : IGameMessage
    {
    }

    // 60097: b8
    [BlubContract]
    public class SEndLoadingAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong Unk { get; set; }
    }

    // 60099: b4,b1 + item
    [BlubContract]
    public class SShoppingBasketActionAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Result { get; set; }

        [BlubMember(1)]
        public byte Unk { get; set; }

        [BlubMember(2)]
        public ShoppingBasketItemDto Item { get; set; }

        public SShoppingBasketActionAckMessage()
        {
            Item = new ShoppingBasketItemDto();
        }
    }

    // 60100: contador int + items
    [BlubContract]
    public class SShoppingBasketListAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ShoppingBasketItemDto[] Items { get; set; }

        public SShoppingBasketListAckMessage()
        {
            Items = Array.Empty<ShoppingBasketItemDto>();
        }
    }

    // 60101: sin cuerpo
    [BlubContract]
    public class SRandomShopUpdateRequestAckMessage : IGameMessage
    {
    }

    // 60104: b4 + contador int + entradas
    [BlubContract]
    public class SRandomShopRollingStartAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public RandomShopRollingDto[] Entries { get; set; }

        public SRandomShopRollingStartAckMessage()
        {
            Entries = Array.Empty<RandomShopRollingDto>();
        }
    }

    // 60105: str,b4,b4,b4,b4
    [BlubContract]
    public class SRandomShopWhoGetNiceItemAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(1)]
        public uint Unk1 { get; set; }

        [BlubMember(2)]
        public uint Unk2 { get; set; }

        [BlubMember(3)]
        public uint Unk3 { get; set; }

        [BlubMember(4)]
        public uint Unk4 { get; set; }

        public SRandomShopWhoGetNiceItemAckMessage()
        {
            Nickname = "";
        }
    }

    // 60038: 23 escalares y seis bloques de stats, en el orden del cliente.
    [BlubContract]
    public class SGetPlayerInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public ulong AccountId { get; set; }

        [BlubMember(2)]
        public byte Unk2 { get; set; }

        [BlubMember(3)]
        public uint Unk3 { get; set; }

        [BlubMember(4)]
        public ushort Unk4 { get; set; }

        [BlubMember(5)]
        public ushort Unk5 { get; set; }

        [BlubMember(6)]
        public uint Unk6 { get; set; }

        [BlubMember(7)]
        public byte Unk7 { get; set; }

        [BlubMember(8)]
        public uint Unk8 { get; set; }

        [BlubMember(9)]
        public byte Unk9 { get; set; }

        [BlubMember(10)]
        public ushort Unk10 { get; set; }

        [BlubMember(11)]
        public byte Unk11 { get; set; }

        [BlubMember(12)]
        public uint Unk12 { get; set; }

        [BlubMember(13)]
        public uint Unk13 { get; set; }

        [BlubMember(14)]
        public uint Unk14 { get; set; }

        [BlubMember(15)]
        public uint Unk15 { get; set; }

        [BlubMember(16)]
        public uint Unk16 { get; set; }

        [BlubMember(17)]
        public uint Unk17 { get; set; }

        [BlubMember(18)]
        public uint Unk18 { get; set; }

        [BlubMember(19)]
        public uint Unk19 { get; set; }

        [BlubMember(20)]
        public uint Unk20 { get; set; }

        [BlubMember(21)]
        public uint Unk21 { get; set; }

        [BlubMember(22)]
        public uint Unk22 { get; set; }

        [BlubMember(23)]
        public PlayerInfoStatsADto StatsA { get; set; }

        [BlubMember(24)]
        public PlayerInfoStatsBDto StatsB { get; set; }

        [BlubMember(25)]
        public PlayerInfoStatsCDto StatsC { get; set; }

        [BlubMember(26)]
        public PlayerInfoStatsDDto StatsD1 { get; set; }

        [BlubMember(27)]
        public PlayerInfoStatsDDto StatsD2 { get; set; }

        [BlubMember(28)]
        public PlayerInfoStatsEDto StatsE { get; set; }

        public SGetPlayerInfoAckMessage()
        {
            StatsA = new PlayerInfoStatsADto();
            StatsB = new PlayerInfoStatsBDto();
            StatsC = new PlayerInfoStatsCDto();
            StatsD1 = new PlayerInfoStatsDDto();
            StatsD2 = new PlayerInfoStatsDDto();
            StatsE = new PlayerInfoStatsEDto();
        }
    }
}
