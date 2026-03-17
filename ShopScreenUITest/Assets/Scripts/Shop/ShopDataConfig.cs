using UnityEngine;

namespace Shop
{
    /// <summary>
    /// Central configuration asset that holds all shop item lists, grouped by tab.
    /// Create via Assets → Create → Shop → Shop Data Config.
    ///
    /// Assign this single asset to ShopScreenController to drive all three tabs.
    /// Adding or reordering items requires no code changes (Open/Closed Principle).
    /// </summary>
    [CreateAssetMenu(fileName = "ShopDataConfig", menuName = "Shop/Shop Data Config")]
    public class ShopDataConfig : ScriptableObject
    {
        [Header("Offers Tab")]
        public ShopItemData[] offersItems;

        [Header("Money Tab")]
        public ShopItemData[] moneyItems;

        [Header("Coins Tab")]
        public ShopItemData[] coinItems;
    }
}
