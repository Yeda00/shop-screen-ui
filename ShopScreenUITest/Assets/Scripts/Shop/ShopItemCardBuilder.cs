using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shop
{
    /// <summary>
    /// Stateless factory that builds a shop item card VisualElement
    /// from a UXML template and a ShopItemData instance.
    ///
    /// Single Responsibility: only knows how to turn data into a card.
    /// </summary>
    public static class ShopItemCardBuilder
    {
        /// <summary>
        /// Instantiates the card template and populates it with <paramref name="data"/>.
        /// </summary>
        /// <param name="template">ShopItemCard.uxml asset.</param>
        /// <param name="data">Item to display.</param>
        /// <param name="watchIconSprite">Icon shown on the button for WatchAd items.</param>
        /// <param name="onBuyClicked">Callback invoked when the CTA button is pressed.</param>
        /// <returns>Root VisualElement of the populated card.</returns>
        public static VisualElement Build(
            VisualTreeAsset template,
            ShopItemData data,
            Sprite watchIconSprite,
            Action<ShopItemData> onBuyClicked)
        {
            if (template == null)
            {
                Debug.LogError("[ShopItemCardBuilder] VisualTreeAsset template is null.");
                return new VisualElement();
            }

            var card = template.Instantiate();

            SetImage(card, data);
            SetCurrencyRow(card, data);
            SetBuyButton(card, data, watchIconSprite, onBuyClicked);

            return card;
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static void SetImage(VisualElement card, ShopItemData data)
        {
            var cardImage = card.Q<VisualElement>("card-image");
            if (cardImage == null) return;

            if (data.itemSprite != null)
                cardImage.style.backgroundImage = new StyleBackground(data.itemSprite);
        }

        private static void SetCurrencyRow(VisualElement card, ShopItemData data)
        {
            var amountLabel = card.Q<Label>("amount-label");
            if (amountLabel != null)
                amountLabel.text = string.IsNullOrEmpty(data.amountDisplay)
                    ? data.amount.ToString("N0")
                    : data.amountDisplay;

            var coinIcon = card.Q<VisualElement>("coin-icon");
            if (coinIcon != null && data.currencyIconSprite != null)
                coinIcon.style.backgroundImage = new StyleBackground(data.currencyIconSprite);
        }

        private static void SetBuyButton(
            VisualElement card,
            ShopItemData data,
            Sprite watchIconSprite,
            Action<ShopItemData> onBuyClicked)
        {
            var buyBtn = card.Q<Button>("buy-btn");
            if (buyBtn == null) return;

            if (data.purchaseType == PurchaseType.WatchAd)
            {
                // Insert small video icon before the text
                if (watchIconSprite != null)
                {
                    var icon = new VisualElement();
                    icon.AddToClassList("item-card__watch-icon");
                    icon.style.backgroundImage = new StyleBackground(watchIconSprite);
                    buyBtn.Insert(0, icon);
                }

                buyBtn.text = " WATCH";
                buyBtn.AddToClassList("item-card__buy-btn--watch");
            }
            else
            {
                buyBtn.text = string.IsNullOrEmpty(data.priceDisplay) ? "Buy" : data.priceDisplay;
            }

            buyBtn.clicked += () => onBuyClicked?.Invoke(data);
        }
    }
}
