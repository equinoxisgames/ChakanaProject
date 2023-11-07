using System.Collections.Generic;
using System.Linq;
using Assets.FantasyInventory.Scripts.Data;
using Assets.FantasyInventory.Scripts.Enums;
using Assets.FantasyInventory.Scripts.GameData;
using Assets.FantasyInventory.Scripts.Interface.Elements;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.FantasyInventory.Scripts.Interface
{
    /// <summary>
    /// High-level shop interface.
    /// </summary>
    public class Shop : ItemWorkspaceShop
    {
        [SerializeField] Hoyustus player;
        [SerializeField] Text goldTxt;
        public ScrollInventory Trader;
        public ScrollInventory Bag;
        public Button BuyButton;
        public Button SellButton;
        public AudioSource AudioSource;
        public AudioClip TradeSound;
        public AudioClip NoMoney;

        public const int SellRatio = 2;
        public ConversationInteract conv;
        /// <summary>
        /// Initialize owned items and trader items (just for example).
        /// </summary>
        protected void Awake()
        {
            var inventory = new List<Item>
            {
                new Item(ItemId.GoldPieces, 10000)
            };

            NewShop();

            Bag.Initialize(ref inventory);
        }

        public void NewShop()
        {
            var shop = new List<Item>();

            if (PlayerPrefs.GetInt("Boost01") != 1)
            {
                shop.Add(new Item(ItemId.KunturMask, 1));
                print("hola1");
            }

            if (PlayerPrefs.GetInt("Boost02") != 1)
            {
                shop.Add(new Item(ItemId.PachamamaAmulet, 1));
                print("hola2");
            }

            if (PlayerPrefs.GetInt("Boost03") != 1)
            {
                shop.Add(new Item(ItemId.WarriorTearAmulet, 1));
                print("hola3");
            }

            print(PlayerPrefs.GetInt("Boost01"));
            print(PlayerPrefs.GetInt("Boost02"));
            print(PlayerPrefs.GetInt("Boost03"));

            Trader.Initialize(ref shop);
        }

        protected void Start()
        {
            Reset();
            BuyButton.interactable = SellButton.interactable = false;
            goldTxt.text = player.getGold().ToString();
            // TODO: Assigning static callbacks. Don't forget to set null values when UI will be closed. You can also use events instead.
            InventoryItem.OnItemSelected = SelectItem;
            InventoryItem.OnDragStarted = SelectItem;
            InventoryItem.OnDragCompleted = InventoryItem.OnDoubleClick = item => { if (Trader.Items.Contains(item)) Buy(); else Sell(); };

            var shop = new List<Item>();

            if (PlayerPrefs.GetInt("Boost01") != 1)
            {
                shop.Add(new Item(ItemId.KunturMask, 1));
                print("hola1");
            }

            if (PlayerPrefs.GetInt("Boost02") != 1)
            {
                shop.Add(new Item(ItemId.PachamamaAmulet, 1));
                print("hola2");
            }

            if (PlayerPrefs.GetInt("Boost03") != 1)
            {
                shop.Add(new Item(ItemId.WarriorTearAmulet, 1));
                print("hola3");
            }

            print(PlayerPrefs.GetInt("Boost01"));
            print(PlayerPrefs.GetInt("Boost02"));
            print(PlayerPrefs.GetInt("Boost03"));

            Trader.Initialize(ref shop);
            if (shop.Count > 0)
                SelectItem(shop[0].Id);


        }

        public void SelectItem(Item item)
        {
            SelectItem(item.Id);
        }

        public void SelectItem(ItemId itemId)
        {
            SelectedItem = itemId;
            SelectedItemParams = Items.Params[itemId];
            ItemInfo.Initialize(SelectedItem, SelectedItemParams, true);
            Refresh();
        }

        public void Buy()
        {
            if (player.getGold() < SelectedItemParams.Price)
            {
                AudioSource.PlayOneShot(NoMoney);
                Debug.LogWarning("You haven't enough gold!");
                return;
            }

            if(SelectedItem.ToString() == "KunturMask")
            {
                PlayerPrefs.SetInt("Boost01", 1);
            }
            else if (SelectedItem.ToString() == "PachamamaAmulet")
            {
                PlayerPrefs.SetInt("Boost02", 1);
            }
            else if (SelectedItem.ToString() == "WarriorTearAmulet")
            {
                PlayerPrefs.SetInt("Boost03", 1);
            }

            if(PlayerPrefs.HasKey("Boost03") && PlayerPrefs.HasKey("Boost02") && PlayerPrefs.HasKey("Boost01"))
            {
                PlayerPrefs.SetInt("TiendaVacia",1);
            }

            /*AddMoney(Bag, -SelectedItemParams.Price, ItemId.GoldPieces);
            AddMoney(Trader, SelectedItemParams.Price, ItemId.GoldPieces);*/

            player.setGold(-SelectedItemParams.Price);
            goldTxt.text = player.getGold().ToString();

            MoveItem(SelectedItem, Trader, Bag);
            AudioSource.PlayOneShot(TradeSound);

            conv.SelectFirstItem();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                Buy();
            }
        }

        public void Sell()
        {
            if (GetCurrency(Trader, ItemId.GoldPieces) < SelectedItemParams.Price / SellRatio)
            {
                AudioSource.PlayOneShot(NoMoney);
                Debug.LogWarning("Trader hasn't enough gold!");
                return;
            }

            AddMoney(Bag, SelectedItemParams.Price / SellRatio, ItemId.GoldPieces);
            AddMoney(Trader, -SelectedItemParams.Price / SellRatio, ItemId.GoldPieces);
            MoveItem(SelectedItem, Bag, Trader);
            AudioSource.PlayOneShot(TradeSound);
        }

        public override void Refresh()
        {
            if (SelectedItem == ItemId.Undefined)
            {
                ItemInfo.Reset();
                BuyButton.interactable = SellButton.interactable = false;
            }
            else
            {
                var item = Items.Params[SelectedItem];

                if (!item.Tags.Contains(ItemTag.NotForSale))
                {
                    BuyButton.interactable = Trader.Items.Any(i => i.Id == SelectedItem) && GetCurrency(Bag, ItemId.GoldPieces) >= item.Price;
                    SellButton.interactable = Bag.Items.Any(i => i.Id == SelectedItem) && GetCurrency(Trader, ItemId.GoldPieces) >= item.Price / SellRatio;
                }
                else
                {
                    ItemInfo.Price.text = null;
                    BuyButton.interactable = SellButton.interactable = false;
                }
            }
        }

        private static long GetCurrency(ItemContainer bag, ItemId currencyId)
        {
            var currency = bag.Items.SingleOrDefault(i => i.Id == currencyId);

            return currency?.Count ?? 0;
        }

        private static void AddMoney(ItemContainer inventory, int value, ItemId currencyId)
        {
            var currency = inventory.Items.SingleOrDefault(i => i.Id == currencyId);

            if (currency == null)
            {
                inventory.Items.Insert(0, new Item(currencyId, value));
            }
            else
            {
                currency.Count += value;

                if (currency.Count == 0)
                {
                    inventory.Items.Remove(currency);
                }
            }
        }
    }
}