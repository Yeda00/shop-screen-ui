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
    private const string OfferItemCardUXML  = "Assets/UI/UXML/OfferItemCard.uxml";

    [MenuItem("Shop/Repair Offer Card Types")]
    public static void RepairOfferCardTypes()
    {
        var config = AssetDatabase.LoadAssetAtPath<ShopDataConfig>(ShopConfigPath);
        if (config == null || config.offersItems == null)
        {
            Debug.LogWarning("[ShopSetupWizard] ShopDataConfig not found. Run Setup Shop Scene first.");
            return;
        }

        // offersItems order: [0] StarPass, [1] StarterPack, [2] PremiumPack
        var types = new[] { OfferCardType.StarPass, OfferCardType.ResourcePack, OfferCardType.ResourcePack };
        var chestSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Chest.png");

        for (int i = 0; i < config.offersItems.Length && i < types.Length; i++)
        {
            var item = config.offersItems[i];
            if (item == null) continue;

            var so = new SerializedObject(item);
            so.FindProperty("offerCardType").enumValueIndex = (int)types[i];

            if (types[i] == OfferCardType.StarPass && chestSprite != null)
                so.FindProperty("centerSprite").objectReferenceValue = chestSprite;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(item);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[ShopSetupWizard] ✅ Offer card types repaired.");
    }

    [MenuItem("Shop/Setup Shop Scene")]
    public static void SetupShopScene()
    {
        EnsureFolders();

        var panelSettings  = GetOrCreatePanelSettings();
        var shopConfig     = GetOrCreateShopDataConfig();
        var shopUxml       = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ShopScreenUXML);
        var cardUxml       = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ShopItemCardUXML);
        var offerCardUxml  = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(OfferItemCardUXML);

        GetOrCreateShopUIDocumentObject(panelSettings, shopUxml, cardUxml, offerCardUxml, shopConfig);

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
            CreateOfferItem(0, "offer_starpass", "Star Pass",    "R$ 19,99", OfferCardType.StarPass),
            CreateOfferItem(1, "offer_starter",  "Starter Pack", "R$ 19,99", OfferCardType.ResourcePack),
            CreateOfferItem(2, "offer_premium",  "Premium Pack", "R$ 19,99", OfferCardType.ResourcePack),
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

    private static ShopItemData CreateOfferItem(int idx, string id, string display, string price, OfferCardType cardType)
    {
        var path     = string.Format(OffersItemPathFmt, idx);
        var existing = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);

        ShopItemData item;
        if (existing != null)
        {
            item = existing;
        }
        else
        {
            item               = ScriptableObject.CreateInstance<ShopItemData>();
            item.productId     = id;
            item.tab           = ShopTab.Offers;
            item.amountDisplay = display;
            item.priceDisplay  = price;
            item.purchaseType  = PurchaseType.RealMoney;
            AssetDatabase.CreateAsset(item, path);
        }

        // Always (re)apply the card type so existing assets are also updated
        item.offerCardType = cardType;
        EditorUtility.SetDirty(item);
        return item;
    }

    // ── UIDocument GameObject ─────────────────────────────────────────────

    private static void GetOrCreateShopUIDocumentObject(
        PanelSettings panelSettings,
        VisualTreeAsset shopUxml,
        VisualTreeAsset cardUxml,
        VisualTreeAsset offerCardUxml,
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
        so.FindProperty("offerItemCardTemplate") .objectReferenceValue = offerCardUxml;

        // Try to auto-assign test sprites
        TryAssignSprite(so, "backgroundSprite",  "Assets/Test Assets/image_background.png");
        TryAssignVector(so, "moneyWalletVector", "Assets/Test Assets/Group 570.svg");
        TryAssignVector(so, "coinWalletVector",  "Assets/Test Assets/Group 620.svg");
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

    private static void TryAssignVector(SerializedObject so, string propName, string assetPath)
    {
        var vector = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VectorImage>(assetPath);
        if (vector != null)
            so.FindProperty(propName).objectReferenceValue = vector;
        else
            Debug.LogWarning($"[ShopSetupWizard] VectorImage not found at {assetPath}. " +
                             "Ensure 'com.unity.vectorgraphics' package is installed and the SVG was imported.");
    }

    private static void AssignItemSprites(ShopDataConfig config)
    {
        if (config == null) return;

        var coinSprite   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Frame_ShopCoins.png");
        var moneySprite  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Frame_ShopMoney.png");
        var coinIcon     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Icon_HUD_Coin.png");
        var moneyIcon    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icon_HUD_Money.png");
        var hammerIcon   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Icon_HUD_Hammer.png");
        var chestSprite  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Chest.png");

        SetItemSprites(config.coinItems,  coinSprite,  coinIcon);
        SetItemSprites(config.moneyItems, moneySprite, moneyIcon);

        if (config.offersItems == null) return;

        // offersItems order: [0] StarPass, [1] StarterPack, [2] PremiumPack
        var offerFrames = new[]
        {
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Frame_StarPass.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Frame_StarterPack.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Test Assets/Frame_PremiumPack.png"),
        };

        // Row label values per pack: [0]=StarPass (unused), [1]=StarterPack, [2]=PremiumPack
        var moneyLabels = new[] { "", "x20",    "x50"   };
        var coinLabels  = new[] { "", "x600",   "x2.500" };

        for (int i = 0; i < config.offersItems.Length; i++)
        {
            var item = config.offersItems[i];
            if (item == null) continue;

            var so = new SerializedObject(item);

            if (i < offerFrames.Length && offerFrames[i] != null)
                so.FindProperty("frameSprite").objectReferenceValue = offerFrames[i];

            if (item.offerCardType == OfferCardType.StarPass)
            {
                if (chestSprite != null)
                    so.FindProperty("centerSprite").objectReferenceValue = chestSprite;
            }
            else // ResourcePack
            {
                if (hammerIcon != null)
                {
                    so.FindProperty("row1Icon").objectReferenceValue = hammerIcon;
                    so.FindProperty("row1Label").stringValue = "x2";
                }
                if (moneyIcon != null)
                {
                    so.FindProperty("row2Icon").objectReferenceValue = moneyIcon;
                    so.FindProperty("row2Label").stringValue = moneyLabels[i];
                }
                if (coinIcon != null)
                {
                    so.FindProperty("row3Icon").objectReferenceValue = coinIcon;
                    so.FindProperty("row3Label").stringValue = coinLabels[i];
                }
            }

            so.ApplyModifiedProperties();
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
