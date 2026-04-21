using System.Collections.Generic;
using Assets.FantasyInventory.Scripts.Data;
using Assets.FantasyInventory.Scripts.Enums;

namespace Assets.FantasyInventory.Scripts.GameData
{
    /// <summary>
    /// Item params are stored here. If you want to store them in any kind of database, please refer to dictionary serialization:
    /// https://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.html
    /// Alternatively, use Newtonsoft JSON (recommended).
    /// </summary>
    public class Items
    {
        public static readonly Dictionary<ItemId, ItemParams> Params = new Dictionary<ItemId, ItemParams>
        {
            {
                ItemId.SupaypaUma,
                new ItemParams
                {
                    Type = ItemType.Ukuku_Object,
                    //Properties = new List<Property> { new Property(PropertyId.MagicDamage, 100) },
                    ShortDescription = "A cursed mask whispered to belong to the servants of Supay. Its hollow gaze seems to watch from the darkness, and those who carry it feel the cold breath of the underworld following every step.",
                    ShortDescLocalKey= "ITEM_3_SHORT_DESCRIPTION_TEXT",

                    Price = 1000
                }
            },
            {
                ItemId.Flute,
                new ItemParams
                {
                    Type = ItemType.Loot,
                    Price = 10
                }
            },
            {
                ItemId.GoldPieces,
                new ItemParams
                {
                    Type = ItemType.Currency,
                    //Tags = new List<ItemTag> { ItemTag.NotForSale }
                }
            },
            {
                ItemId.Suntur,
                new ItemParams
                {
                    Type = ItemType.Weapon,
                    Properties = new List<Property> { new Property(PropertyId.PhysicDamage,50) },
                    ShortDescription = "Ceremonial weapon granted by Viracocha. Its blade carries a fragment of the divine power that shaped the world.",
                    ShortDescLocalKey= "ITEM_1_SHORT_DESCRIPTION_TEXT",
                    Price = 200
                }
            },
            {
                ItemId.Sword,
                new ItemParams
                {
                    Type = ItemType.Weapon,
                    Tags = new List<ItemTag> { ItemTag.Sword },
                    Properties = new List<Property> { new Property(PropertyId.PhysicDamage, 10) },
                    Price = 1000
                }
            },
            {
                ItemId.Bow,
                new ItemParams
                {
                    Type = ItemType.Weapon,
                    Tags = new List<ItemTag> { ItemTag.Bow, ItemTag.TwoHanded },
                    Properties = new List<Property> { new Property(PropertyId.PhysicDamage, 15) },
                    Price = 2000
                }
            },
            {
                ItemId.KunturpaIlla,
                new ItemParams
                {
                    Type = ItemType.Talisman,
                    Properties = new List<Property> { new Property(PropertyId.AbilityDamage, 25) },

                    ShortDescription = "Sacred mask shaped after the Andean condor. Those who wear it are said to feel the gaze of the sky and the ancient strength of the Kuntur.",
                    ShortDescLocalKey= "ITEM_5_SHORT_DESCRIPTION_TEXT",

                    Price = 150
                }
            },
            {
                ItemId.Ayahuasca,
                new ItemParams
                {
                    Type = ItemType.Ukuku_Object,
                    //Properties = new List<Property> { new Property(PropertyId.PhysicDamage, 5) },
                    ShortDescription = "Ancient brew prepared by the sages of the jungle. Its essence opens the mind to visions of the spirit world and the whispers of the gods.",
                    ShortDescLocalKey= "ITEM_4_SHORT_DESCRIPTION_TEXT",

                    Price = 500
                }
            },
            {
                ItemId.AnkasKallampa,
                new ItemParams
                {
                    Type = ItemType.Ukuku_Object,

                    ShortDescription = "Strange blue mushroom that grows in the damp corners of the Andes. The ancients believed its energy was tied to the spirit of the mountains.",
                    ShortDescLocalKey= "ITEM_2_SHORT_DESCRIPTION_TEXT",
                    //Properties = new List<Property> { new Property(PropertyId.RestoreMana, 50) },
                    Price = 200
                }
            },
            {
                ItemId.MirayIlla,
                new ItemParams
                {
                    Type = ItemType.Talisman,

                    ShortDescription = "Sacred stone amulet linked to fertility and abundance. The ancients believed its power helped life grow wherever it was carried.",
                    ShortDescLocalKey= "ITEM_6_SHORT_DESCRIPTION_TEXT",

                    Properties = new List<Property> { new Property(PropertyId.MaxHealth, 50) },
                    Price = 180
                }
            },
            {
                ItemId.SilverRing,
                new ItemParams
                {
                    Type = ItemType.Ring,
                    Properties = new List<Property> { new Property(PropertyId.MagicDefense, 5) },
                    Price = 500
                }
            },
            {
                ItemId.Spear,
                new ItemParams
                {
                    Type = ItemType.Weapon,
                    Tags = new List<ItemTag> { ItemTag.Spear, ItemTag.TwoHanded },
                    Properties = new List<Property> { new Property(PropertyId.PhysicDamage, 15) },
                    Price = 2500
                }
            },
            {
                ItemId.StoneAmulet,
                new ItemParams
                {
                    Type = ItemType.Necklace,
                    Properties = new List<Property> { new Property(PropertyId.MagicDefense, 10) },
                    Price = 1000
                }
            },
            {
                ItemId.TwoHandedSword,
                new ItemParams
                {
                    Type = ItemType.Weapon,
                    Tags = new List<ItemTag> { ItemTag.Sword, ItemTag.TwoHanded },
                    Properties = new List<Property> { new Property(PropertyId.PhysicDamage, 20) },
                    Price = 5000
                }
            },
            {
                ItemId.UturunkuIlla,
                new ItemParams
                {
                    Type = ItemType.Talisman,

                    ShortDescription = "Sacred amulet tied to the spirit of the Andean jaguar. Those who carry it are said to gain the strength and stealth of the great hunter of the jungle.",
                    ShortDescLocalKey= "ITEM_7_SHORT_DESCRIPTION_TEXT",
                    //Tags = new List<ItemTag> { ItemTag.Axe },
                    Properties = new List<Property> { new Property(PropertyId.PhysicDamage, 25) },
                    Price = 200
                }
            }
        };
    }
}