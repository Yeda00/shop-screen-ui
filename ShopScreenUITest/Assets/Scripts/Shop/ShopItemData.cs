using UnityEngine;

namespace Shop
{
    /// <summary>
    /// Which tab this item belongs to.
    /// </summary>
    public enum ShopTab
    {
        Offers,
        Money,
        Coins
    }

    /// <summary>
    /// How the player acquires this item.
    /// </summary>
    public enum PurchaseType
    {
        RealMoney,  // In-App Purchase
        WatchAd     // Rewarded ad (free)
    }

    /// <summary>
    /// Data model for a single shop item.
    /// Create via Assets → Create → Shop → Shop Item.
    /// </summary>
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
    }
}
