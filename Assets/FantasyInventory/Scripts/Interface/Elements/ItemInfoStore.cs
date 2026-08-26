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
    public class ItemInfoStore : MonoBehaviour
    {
        public Text Name;
        public Text Description;
        public Text Price;
        public Text ShortDescription;
        public Image Icon;

        public void Reset()
        {
            Name.text = Description.text = Price.text = null;
            Icon.sprite = ImageCollection.Instance.DefaultItemIcon;
        }

        public void Initialize(ItemId itemId, ItemParams itemParams, bool shop = false)
        {
            // FIX: Guardia defensiva. Si alguna referencia UI fue destruida o no está
            // asignada (puede ocurrir si Initialize se llama antes de que el GameObject
            // complete su ciclo OnEnable tras SetActive(true)), se aborta con log de error
            // en lugar de lanzar MissingReferenceException.
            if (Name == null || Description == null || Price == null || ShortDescription == null || Icon == null)
            {
                Debug.LogError("[ItemInfoStore] Initialize abortado: una o más referencias de UI son null. " +
                               "Verifica que todos los campos estén asignados en el Inspector y que el GameObject " +
                               "haya completado su inicialización antes de llamar a Initialize.");
                return;
            }

            Icon.sprite = ImageCollection.Instance.GetIcon(itemId);
            Name.text = SplitName(itemId.ToString());                          // línea 34 original — ahora segura
            Description.text = $"Here will be {itemId} description soon...";

            if (itemParams.Tags.Contains(ItemTag.NotForSale))
            {
                Price.text = null;
            }
            else if (shop)
            {
                // Se comenta esta parte ya que no se implementará la venta de objetos
                //Price.text = $"Buy price: {itemParams.Price}G{Environment.NewLine}Sell price: {itemParams.Price / Shop.SellRatio}G";

                var buyPriceLabel = LocalizationSettings.StringDatabase.GetLocalizedString("ChakanaGameText", "BUY_PRICE_TEXT");
                Price.text = $"{buyPriceLabel}: {itemParams.Price}";
            }

            // Se comenta esta parte ya que no se implementará la venta de objetos
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

            ShortDescription.text = LocalizationSettings.StringDatabase.GetLocalizedString("ChakanaGameText", itemParams.ShortDescLocalKey); // línea 67 original — ahora segura
        }

        public static string SplitName(string name)
        {
            return Regex.Replace(Regex.Replace(name, "[A-Z]", " $0"), "([a-z])([1-9])", "$1 $2").Trim();
        }
    }
}