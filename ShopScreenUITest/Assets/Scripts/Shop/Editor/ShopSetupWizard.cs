using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Shop;

/// <summary>
/// One-click setup wizard for the Shop Screen.
/// Menu: Shop → Setup Shop Scene
/// </summary>
public static class ShopSetupWizard
{
    private const string PanelSettingsPath  = "Assets/UI/ShopPanelSettings.asset";
    private const string ShopConfigPath     = "Assets/ScriptableObjects/Shop/ShopDataConfig.asset";
    private const string CoinItemPathFmt    = "Assets/ScriptableObjects/Shop/Coins/CoinItem_{0}.asset";
    private const string MoneyItemPathFmt   = "Assets/ScriptableObjects/Shop/Money/MoneyItem_{0}.asset";
    private const string OffersItemPathFmt  = "Assets/ScriptableObjects/Shop/Offers/OfferItem_{0}.asset";
    private const string ShopScreenUXML     = "Assets/UI/UXML/ShopScreen.uxml";
    private const string ShopItemCardUXML   = "Assets/UI/UXML/ShopItemCard.uxml";

    [MenuItem("Shop/Setup Shop Scene")]
    public static void SetupShopScene()
    {
        EnsureFolders();

        var panelSettings = GetOrCreatePanelSettings();
        var shopConfig    = GetOrCreateShopDataConfig();
        var shopUxml      = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ShopScreenUXML);
        var cardUxml      = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ShopItemCardUXML);

        GetOrCreateShopUIDocumentObject(panelSettings, shopUxml, cardUxml, shopConfig);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ShopSetupWizard] ✅ Shop scene setup complete! " +
                  "Assign sprites in the ShopScreenController inspector to complete the visuals.");
    }

    // ── Folders ───────────────────────────────────────────────────────────

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/UI");
        EnsureFolder("Assets/UI/UXML");
        EnsureFolder("Assets/UI/USS");
        EnsureFolder("Assets/ScriptableObjects");
        EnsureFolder("Assets/ScriptableObjects/Shop");
        EnsureFolder("Assets/ScriptableObjects/Shop/Coins");
        EnsureFolder("Assets/ScriptableObjects/Shop/Money");
        EnsureFolder("Assets/ScriptableObjects/Shop/Offers");
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parts  = path.Split('/');
            var parent = string.Join("/", parts, 0, parts.Length - 1);
            AssetDatabase.CreateFolder(parent, parts[parts.Length - 1]);
        }
    }

    // ── PanelSettings ─────────────────────────────────────────────────────

    private static PanelSettings GetOrCreatePanelSettings()
    {
        var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
        if (existing != null) return existing;

        var ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.scaleMode          = PanelScaleMode.ConstantPhysicalSize;
        ps.referenceDpi       = 96;
        ps.fallbackDpi        = 96;

        AssetDatabase.CreateAsset(ps, PanelSettingsPath);
        Debug.Log($"[ShopSetupWizard] Created PanelSettings at {PanelSettingsPath}");
        return ps;
    }

    // ── ShopDataConfig + Items ────────────────────────────────────────────

    private static ShopDataConfig GetOrCreateShopDataConfig()
    {
        var existing = AssetDatabase.LoadAssetAtPath<ShopDataConfig>(ShopConfigPath);
        if (existing != null) return existing;

        var config = ScriptableObject.CreateInstance<ShopDataConfig>();

        // Coin items (6 normal + 1 watch-ad)
        config.coinItems = new ShopItemData[]
        {
            CreateCoinItem(0, "coins_2500_a",   2500,  "2.500",  "R$ 5,99",   PurchaseType.RealMoney),
            CreateCoinItem(1, "coins_2500_b",   2500,  "2.500",  "R$ 9,99",   PurchaseType.RealMoney),
            CreateCoinItem(2, "coins_5000",     5000,  "5.000",  "R$ 19,99",  PurchaseType.RealMoney),
            CreateCoinItem(3, "coins_10000",    10000, "10.000", "R$ 39,99",  PurchaseType.RealMoney),
            CreateCoinItem(4, "coins_25000",    25000, "25.000", "R$ 69,99",  PurchaseType.RealMoney),
            CreateCoinItem(5, "coins_50000",    50000, "50.000", "R$ 119,99", PurchaseType.RealMoney),
            CreateCoinItem(6, "coins_watch_ad", 2500,  "2.500",  "",          PurchaseType.WatchAd),
        };

        // Money items
        config.moneyItems = new ShopItemData[]
        {
            CreateMoneyItem(0, "money_500",   500,   "500",    "R$ 4,99",  PurchaseType.RealMoney),
            CreateMoneyItem(1, "money_1200",  1200,  "1.200",  "R$ 9,99",  PurchaseType.RealMoney),
            CreateMoneyItem(2, "money_3000",  3000,  "3.000",  "R$ 19,99", PurchaseType.RealMoney),
            CreateMoneyItem(3, "money_7500",  7500,  "7.500",  "R$ 39,99", PurchaseType.RealMoney),
            CreateMoneyItem(4, "money_watch", 500,   "500",    "",         PurchaseType.WatchAd),
        };

        // Offer items
        config.offersItems = new ShopItemData[]
        {
            CreateOfferItem(0, "offer_starter",  5000,  "Starter Pack",  "R$ 9,99",  PurchaseType.RealMoney),
            CreateOfferItem(1, "offer_premium",  15000, "Premium Pack",  "R$ 24,99", PurchaseType.RealMoney),
            CreateOfferItem(2, "offer_starpass", 50000, "Star Pass",     "R$ 49,99", PurchaseType.RealMoney),
        };

        AssetDatabase.CreateAsset(config, ShopConfigPath);
        Debug.Log($"[ShopSetupWizard] Created ShopDataConfig at {ShopConfigPath}");
        return config;
    }

    private static ShopItemData CreateCoinItem(int idx, string id, int amount, string display, string price, PurchaseType pType)
    {
        var path = string.Format(CoinItemPathFmt, idx);
        var existing = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
        if (existing != null) return existing;

        var item = ScriptableObject.CreateInstance<ShopItemData>();
        item.productId     = id;
        item.tab           = ShopTab.Coins;
        item.amount        = amount;
        item.amountDisplay = display;
        item.priceDisplay  = price;
        item.purchaseType  = pType;
        AssetDatabase.CreateAsset(item, path);
        return item;
    }

    private static ShopItemData CreateMoneyItem(int idx, string id, int amount, string display, string price, PurchaseType pType)
    {
        var path = string.Format(MoneyItemPathFmt, idx);
        var existing = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
        if (existing != null) return existing;

        var item = ScriptableObject.CreateInstance<ShopItemData>();
        item.productId     = id;
        item.tab           = ShopTab.Money;
        item.amount        = amount;
        item.amountDisplay = display;
        item.priceDisplay  = price;
        item.purchaseType  = pType;
        AssetDatabase.CreateAsset(item, path);
        return item;
    }

    private static ShopItemData CreateOfferItem(int idx, string id, int amount, string display, string price, PurchaseType pType)
    {
        var path = string.Format(OffersItemPathFmt, idx);
        var existing = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
        if (existing != null) return existing;

        var item = ScriptableObject.CreateInstance<ShopItemData>();
        item.productId     = id;
        item.tab           = ShopTab.Offers;
        item.amount        = amount;
        item.amountDisplay = display;
        item.priceDisplay  = price;
        item.purchaseType  = pType;
        AssetDatabase.CreateAsset(item, path);
        return item;
    }

    // ── UIDocument GameObject ─────────────────────────────────────────────

    private static void GetOrCreateShopUIDocumentObject(
        PanelSettings panelSettings,
        VisualTreeAsset shopUxml,
        VisualTreeAsset cardUxml,
        ShopDataConfig shopConfig)
    {
        // Reuse existing if present
        var existing = GameObject.Find("ShopScreen");
        if (existing != null)
        {
            Debug.Log("[ShopSetupWizard] ShopScreen GameObject already exists.");
            return;
        }

        var go = new GameObject("ShopScreen");

        // UIDocument
        var uiDoc = go.AddComponent<UIDocument>();
        if (panelSettings != null) uiDoc.panelSettings = panelSettings;
        if (shopUxml != null)      uiDoc.visualTreeAsset = shopUxml;
        uiDoc.sortingOrder = 10;

        // ShopScreenController
        var ctrl = go.AddComponent<ShopScreenController>();

        // Wire up serialized fields via SerializedObject
        var so = new SerializedObject(ctrl);

        so.FindProperty("shopConfig")            .objectReferenceValue = shopConfig;
        so.FindProperty("shopItemCardTemplate")  .objectReferenceValue = cardUxml;

        // Try to auto-assign test sprites
        TryAssignSprite(so, "backgroundSprite",  "Assets/Test Assets/Frame_OffersShopBG.png");
        TryAssignSprite(so, "moneyIconSprite",   "Assets/Test Assets/Icon_HUD_Money.png");
        TryAssignSprite(so, "coinIconSprite",    "Assets/Test Assets/Icon_HUD_Coin.png");
        TryAssignSprite(so, "closeBtnSprite",    "Assets/Test Assets/Buttom_Close.png");
        TryAssignSprite(so, "watchAdIconSprite", "Assets/Test Assets/Buttom_Next.png");

        // Try assign item sprites in ShopDataConfig
        AssignItemSprites(shopConfig);

        so.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[ShopSetupWizard] Created ShopScreen GameObject with UIDocument + ShopScreenController.");
    }

    private static void TryAssignSprite(SerializedObject so, string propName, string assetPath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null)
            so.FindProperty(propName).objectReferenceValue = sprite;
    }

    private static void AssignItemSprites(ShopDataConfig config)
    {
        if (config == null) return;

        var coinSprite   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Frame_ShopCoins.png");
        var moneySprite  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Frame_ShopMoney.png");
        var coinIcon     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Icon_HUD_Coin.png");
        var moneyIcon    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Icon_HUD_Money.png");

        var offerSprites = new[]
        {
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Frame_StarterPack.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Frame_PremiumPack.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Frame_StarPass.png"),
        };

        SetItemSprites(config.coinItems,   coinSprite,  coinIcon);
        SetItemSprites(config.moneyItems,  moneySprite, moneyIcon);

        if (config.offersItems != null)
        {
            for (int i = 0; i < config.offersItems.Length; i++)
            {
                var item = config.offersItems[i];
                if (item == null) continue;
                var so = new SerializedObject(item);
                if (i < offerSprites.Length && offerSprites[i] != null)
                    so.FindProperty("itemSprite").objectReferenceValue = offerSprites[i];
                so.FindProperty("currencyIconSprite").objectReferenceValue = coinIcon;
                so.ApplyModifiedProperties();
            }
        }
    }

    private static void SetItemSprites(ShopItemData[] items, Sprite itemSprite, Sprite iconSprite)
    {
        if (items == null) return;
        foreach (var item in items)
        {
            if (item == null) continue;
            var so = new SerializedObject(item);
            if (itemSprite != null)  so.FindProperty("itemSprite").objectReferenceValue = itemSprite;
            if (iconSprite != null)  so.FindProperty("currencyIconSprite").objectReferenceValue = iconSprite;
            so.ApplyModifiedProperties();
        }
    }
}
