using UnityEngine;

namespace Shop
{
    public enum ShopTab
    {
        Offers,
        Money,
        Coins
    }

    public enum PurchaseType
    {
        RealMoney,  // In-App Purchase
        WatchAd     // Rewarded ad (free)
    }

    public enum OfferCardType
    {
        Default,      // not an offer card
        StarPass,     // frame + centered chest image
        ResourcePack  // frame + 3 icon/label rows (Starter Pack, Premium Pack, etc.)
    }

    [CreateAssetMenu(fileName = "ShopItem", menuName = "Shop/Shop Item")]
    public class ShopItemData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique product ID (used for IAP and logging).")]
        public string productId;

        [Tooltip("Which shop tab this item belongs to.")]
        public ShopTab tab;

        [Header("Currency")]
        [Tooltip("Numeric amount granted on purchase.")]
        public int amount;

        [Tooltip("Formatted amount shown on the card, e.g. '2.500'.")]
        public string amountDisplay;

        [Header("Purchase")]
        [Tooltip("How the player pays for this item.")]
        public PurchaseType purchaseType;

        [Tooltip("Localised price string shown on the button, e.g. 'R$ 19,99'. Ignored for WatchAd items.")]
        public string priceDisplay;

        [Header("Visuals")]
        [Tooltip("Main product artwork shown in the card image area.")]
        public Sprite itemSprite;

        [Tooltip("Small currency icon shown beside the amount.")]
        public Sprite currencyIconSprite;

        [Header("Offer Card")]
        [Tooltip("Layout variant. Set for items in the Offers tab only.")]
        public OfferCardType offerCardType;

        [Tooltip("Título exibido no header colorido do card (ex: STAR PASS, STARTER PACK).")]
        public string cardTitle;

        [Tooltip("Frame image used as full card background (Offers tab).")]
        public Sprite frameSprite;

        [Tooltip("Center image for StarPass cards (e.g. Chest sprite).")]
        public Sprite centerSprite;

        [Header("Offer Card Resource Rows")]
        [Tooltip("Icon for the first resource row (e.g. Hammer).")]
        public Sprite row1Icon;
        [Tooltip("Label text for the first row, e.g. 'x2'.")]
        public string row1Label;

        [Tooltip("Icon for the second resource row (e.g. Money).")]
        public Sprite row2Icon;
        [Tooltip("Label text for the second row, e.g. 'x20'.")]
        public string row2Label;

        [Tooltip("Icon for the third resource row (e.g. Coin).")]
        public Sprite row3Icon;
        [Tooltip("Label text for the third row, e.g. 'x600'.")]
        public string row3Label;
    }
}
