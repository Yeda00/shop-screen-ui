using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shop
{
    public static class OfferItemCardBuilder
    {
        public static VisualElement Build(
            VisualTreeAsset template,
            ShopItemData data,
            Sprite buyButtonSprite,
            Action<ShopItemData> onBuyClicked)
        {
            if (template == null)
            {
                Debug.LogError("[OfferItemCardBuilder] VisualTreeAsset template is null.");
                return new VisualElement();
            }

            var card = template.Instantiate();

            SetFrame(card, data);
            SetTitle(card, data);
            SetLayout(card, data);
            SetBuyButton(card, data, buyButtonSprite, onBuyClicked);

            return card;
        }

        private static void SetTitle(VisualElement card, ShopItemData data)
        {
            var title = card.Q<Label>("offer-title");
            if (title == null) return;

            title.text = string.IsNullOrEmpty(data.cardTitle) ? string.Empty : data.cardTitle;

            // StarPass uses larger text and a dark brown to match its light frame background.
            // ResourcePacks use smaller text and beige to sit on the dark frame.
            title.style.fontSize = data.offerCardType == OfferCardType.StarPass ? 22f : 18f;
            title.style.color = data.offerCardType == OfferCardType.StarPass
                ? new StyleColor(new Color32(0x5F, 0x31, 0x24, 0xFF))
                : new StyleColor(new Color32(0xE4, 0xDD, 0xB5, 0xFF));
        }

        private static void SetFrame(VisualElement card, ShopItemData data)
        {
            var root = card.Q<VisualElement>("offer-card");
            if (root == null) return;

            if (data.frameSprite != null)
                root.style.backgroundImage = new StyleBackground(data.frameSprite);
        }

        private static void SetLayout(VisualElement card, ShopItemData data)
        {
            var starPassContent = card.Q<VisualElement>("star-pass-content");
            var packContent     = card.Q<VisualElement>("pack-content");

            if (data.offerCardType == OfferCardType.StarPass)
            {
                if (packContent     != null) packContent.style.display     = DisplayStyle.None;
                if (starPassContent != null) starPassContent.style.display = DisplayStyle.Flex;

                var chestImage = card.Q<VisualElement>("chest-image");
                if (chestImage != null && data.centerSprite != null)
                    chestImage.style.backgroundImage = new StyleBackground(data.centerSprite);
            }
            else
            {
                if (starPassContent != null) starPassContent.style.display = DisplayStyle.None;
                if (packContent     != null) packContent.style.display     = DisplayStyle.Flex;

                SetRow(card, "row1-icon", "row1-label", data.row1Icon, data.row1Label);
                SetRow(card, "row2-icon", "row2-label", data.row2Icon, data.row2Label);
                SetRow(card, "row3-icon", "row3-label", data.row3Icon, data.row3Label);
            }
        }

        private static void SetRow(VisualElement card, string iconName, string labelName, Sprite icon, string label)
        {
            var iconEl  = card.Q<VisualElement>(iconName);
            var labelEl = card.Q<Label>(labelName);

            if (iconEl != null && icon != null)
                iconEl.style.backgroundImage = new StyleBackground(icon);

            if (labelEl != null)
                labelEl.text = label ?? string.Empty;
        }

        private static void SetBuyButton(VisualElement card, ShopItemData data, Sprite buyButtonSprite, Action<ShopItemData> onBuyClicked)
        {
            var buyBtn = card.Q<Button>("buy-btn");
            if (buyBtn == null) return;

            if (buyButtonSprite != null)
                buyBtn.style.backgroundImage = new StyleBackground(buyButtonSprite);

            buyBtn.text = string.IsNullOrEmpty(data.priceDisplay) ? "Comprar" : data.priceDisplay;
            buyBtn.clicked += () => onBuyClicked?.Invoke(data);
        }
    }
}
