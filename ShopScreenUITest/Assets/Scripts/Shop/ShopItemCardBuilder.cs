using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shop
{
    public static class ShopItemCardBuilder
    {
        public static VisualElement Build(
            VisualTreeAsset template,
            ShopItemData data,
            Sprite watchIconSprite,
            Sprite buyButtonSprite,
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
            SetBuyButton(card, data, watchIconSprite, buyButtonSprite, onBuyClicked);

            return card;
        }

        private static void SetImage(VisualElement card, ShopItemData data)
        {
            // Frame sprite (Frame_ShopMoney / Frame_ShopCoins) is applied as the card's
            // backgroundImage — it composites the product art, currency strip and beige
            // background into a single asset, so no separate card-image element is needed.
            var itemCard = card.Q<VisualElement>("item-card");
            if (itemCard == null) return;

            if (data.itemSprite != null)
                itemCard.style.backgroundImage = new StyleBackground(data.itemSprite);
        }

        private static void SetCurrencyRow(VisualElement card, ShopItemData data)
        {
            var amountLabel = card.Q<Label>("amount-label");
            if (amountLabel != null)
                amountLabel.text = string.IsNullOrEmpty(data.amountDisplay)
                    ? data.amount.ToString("N0")
                    : data.amountDisplay;

            // Modifier class keeps Money and Coins positions independent — adjust in USS only.
            var coinRow = card.Q<VisualElement>("coin-row");
            if (coinRow != null)
            {
                coinRow.AddToClassList(data.tab == ShopTab.Money
                    ? "item-card__coin-row--money"
                    : "item-card__coin-row--coins");
            }
        }

        private static void SetBuyButton(
            VisualElement card,
            ShopItemData data,
            Sprite watchIconSprite,
            Sprite buyButtonSprite,
            Action<ShopItemData> onBuyClicked)
        {
            var buyBtn = card.Q<Button>("buy-btn");
            if (buyBtn == null) return;

            if (buyButtonSprite != null)
                buyBtn.style.backgroundImage = new StyleBackground(buyButtonSprite);

            if (data.purchaseType == PurchaseType.WatchAd)
            {
                buyBtn.text = "";
                buyBtn.AddToClassList("item-card__buy-btn--watch");

                if (watchIconSprite != null)
                {
                    var icon = new VisualElement();
                    icon.AddToClassList("item-card__watch-icon");
                    icon.style.backgroundImage = new StyleBackground(watchIconSprite);
                    buyBtn.Add(icon);
                }

                var label = new Label("WATCH");
                label.AddToClassList("item-card__watch-label");
                buyBtn.Add(label);
            }
            else
            {
                buyBtn.text = string.IsNullOrEmpty(data.priceDisplay) ? "Buy" : data.priceDisplay;
            }

            buyBtn.clicked += () => onBuyClicked?.Invoke(data);
        }
    }
}
