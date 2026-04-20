using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.FantasyInventory.Scripts.Data;
using Assets.FantasyInventory.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

namespace Assets.FantasyInventory.Scripts.Interface.Elements
{
    /// <summary>
    /// Represents item when it was selected. Displays icon, name, price and properties.
    /// </summary>
    public class ItemInfo : MonoBehaviour
    {
        public Text Name;
        public Text Description;
        public Text Price;
        public Text ShortDescription;
        public Image Icon;

        // ── PRIORIDAD 6: Re-buscar referencias si fueron destruidas al cambiar de escena ──
        private void Awake()
        {
            if (Name == null)
            {
                var nameObj = transform.Find("NameText");
                if (nameObj != null) Name = nameObj.GetComponent<Text>();
            }
            if (Description == null)
            {
                var descObj = transform.Find("DescriptionText");
                if (descObj != null) Description = descObj.GetComponent<Text>();
            }
            if (Price == null)
            {
                var priceObj = transform.Find("PriceText");
                if (priceObj != null) Price = priceObj.GetComponent<Text>();
            }
            if (ShortDescription == null)
            {
                var shortDescObj = transform.Find("ShortDescriptionText");
                if (shortDescObj != null) ShortDescription = shortDescObj.GetComponent<Text>();
            }
        }

        // ── PRIORIDAD 1: Validar referencias antes de usarlas ──
        private bool ReferencesAreValid()
        {
            if (Name == null || Description == null || Price == null || ShortDescription == null || Icon == null)
            {
                Debug.LogWarning("[ItemInfo] Una o más referencias de UI están destruidas o son nulas. Operación cancelada.");
                return false;
            }
            return true;
        }

        public void Reset()
        {
            // ── PRIORIDAD 1 ──
            if (!ReferencesAreValid()) return;

            Name.text = Description.text = Price.text = ShortDescription.text = null;
            Icon.sprite = ImageCollection.Instance.DefaultItemIcon;
        }

        public void Initialize(ItemId itemId, ItemParams itemParams, bool shop = false)
        {
            // ── PRIORIDAD 1 ──
            if (!ReferencesAreValid()) return;

            Icon.sprite = ImageCollection.Instance.GetIcon(itemId);
            Name.text = SplitName(itemId.ToString());
            Description.text = $"Here will be {itemId} description soon...";

            if (itemParams.Tags.Contains(ItemTag.NotForSale))
            {
                Price.text = null;
                ShortDescription.text = null;
            }
            else if (shop)
            {
                // Se comenta esta parte ya que no se implementara la venta de objetos
                //Price.text = $"Buy price: {itemParams.Price}G{Environment.NewLine}Sell price: {itemParams.Price / Shop.SellRatio}G";

                Price.text = $"Buy price: {itemParams.Price}G";
                ShortDescription.text = LocalizationSettings.StringDatabase.GetLocalizedString("ChakanaGameText", itemParams.ShortDescLocalKey);
            }

            // Se comenta esta parte ya que no se implementara la venta de objetos
            //else
            //{
            //    Price.text = $"Sell price: {itemParams.Price / Shop.SellRatio}G";
            //}

            var description = new List<string> { $"Type: {itemParams.Type}" };

            if (itemParams.Tags.Any())
            {
                description[description.Count - 1] += $" <color=grey>[{string.Join(", ", itemParams.Tags.Select(i => $"{i}").ToArray())}]</color>";
            }

            foreach (var attribute in itemParams.Properties)
            {
                description.Add($"{SplitName(attribute.Id.ToString())}: {attribute.Value}");
            }

            Description.text = string.Join(Environment.NewLine, description.ToArray());

            ShortDescription.text = LocalizationSettings.StringDatabase.GetLocalizedString("ChakanaGameText", itemParams.ShortDescLocalKey);
        }

        public static string SplitName(string name)
        {
            return Regex.Replace(Regex.Replace(name, "[A-Z]", " $0"), "([a-z])([1-9])", "$1 $2").Trim();
        }
    }
}