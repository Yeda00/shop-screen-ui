using Common.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shop
{
    /// <summary>
    /// Controller for the Shop overlay screen.
    /// Extends UIScreenBase (provides UIDocument wiring, RegisterScreen lifecycle).
    ///
    /// Responsibilities:
    ///   - Tab switching (Offers / Money / Coins)
    ///   - Procedural grid population via ShopItemCardBuilder
    ///   - Mock purchase / watch-ad interactions (Debug.Log)
    ///   - Wallet display updates
    /// </summary>
    public class ShopScreenController : UIScreenBase
    {
        // ── Inspector ────────────────────────────────────────────────────

        [Header("Shop Data")]
        [SerializeField] private ShopDataConfig shopConfig;

        [Header("Card Template")]
        [SerializeField] private VisualTreeAsset shopItemCardTemplate;
        [SerializeField] private VisualTreeAsset offerItemCardTemplate;

        [Header("Background & Branding")]
        [SerializeField] private Sprite backgroundSprite;

        [Header("Wallet Frames (SVG)")]
        [SerializeField] private VectorImage moneyWalletVector;
        [SerializeField] private VectorImage coinWalletVector;

        [Header("Button Icons")]
        [SerializeField] private Sprite closeBtnSprite;
        [SerializeField] private Sprite watchAdIconSprite;

        [Header("Wallet Values (runtime)")]
        [SerializeField] private int initialMoneyAmount = 120000;
        [SerializeField] private int initialCoinAmount  = 120000;

        // ── Private fields ────────────────────────────────────────────────

        private Button         _tabOffersBtn;
        private Button         _tabMoneyBtn;
        private Button         _tabCoinsBtn;
        private Button         _closeBtn;
        private Button         _addMoneyBtn;
        private Button         _addCoinBtn;
        private Label          _moneyAmountLabel;
        private Label          _coinAmountLabel;
        private VisualElement  _itemsGrid;
        private VisualElement  _shopBackground;
        private VisualElement  _moneyWallet;
        private VisualElement  _coinWallet;

        private ShopTab _activeTab = ShopTab.Coins;
        private int     _moneyAmount;
        private int     _coinAmount;

        // ── UIScreenBase overrides ────────────────────────────────────────

        protected override void InitializeUIElements()
        {
            _tabOffersBtn     = QueryElement<Button>("tab-offers");
            _tabMoneyBtn      = QueryElement<Button>("tab-money");
            _tabCoinsBtn      = QueryElement<Button>("tab-coins");
            _closeBtn         = QueryElement<Button>("close-btn");
            _addMoneyBtn      = QueryElement<Button>("add-money-btn");
            _addCoinBtn       = QueryElement<Button>("add-coin-btn");
            _moneyAmountLabel = QueryElement<Label>("money-amount");
            _coinAmountLabel  = QueryElement<Label>("coin-amount");
            _shopBackground   = QueryElement<VisualElement>("shop-background");
            _moneyWallet      = QueryElement<VisualElement>("money-wallet");
            _coinWallet       = QueryElement<VisualElement>("coin-wallet");

            // items-grid lives inside the ScrollView (items-scroll)
            _itemsGrid = root.Q<VisualElement>("items-grid");

            ApplySprites();
        }

        protected override void RegisterCallbacks()
        {
            RegisterButtonCallback(_tabOffersBtn, () => SwitchTab(ShopTab.Offers));
            RegisterButtonCallback(_tabMoneyBtn,  () => SwitchTab(ShopTab.Money));
            RegisterButtonCallback(_tabCoinsBtn,  () => SwitchTab(ShopTab.Coins));
            RegisterButtonCallback(_closeBtn,     OnClose);
            RegisterButtonCallback(_addMoneyBtn,  () => SwitchTab(ShopTab.Money));
            RegisterButtonCallback(_addCoinBtn,   () => SwitchTab(ShopTab.Coins));
        }

        protected override void OnScreenEnabled()
        {
            UINavigationController.RegisterScreen(Screens.Shop, root);

            _moneyAmount = initialMoneyAmount;
            _coinAmount  = initialCoinAmount;
            RefreshWalletLabels();

            SwitchTab(_activeTab);
        }

        protected override void OnScreenDisabled()
        {
            UINavigationController.UnregisterScreen(Screens.Shop);
        }

        // ── Public API ───────────────────────────────────────────────────

        /// <summary>Update the displayed money balance.</summary>
        public void SetMoneyAmount(int amount)
        {
            _moneyAmount = amount;
            RefreshWalletLabels();
        }

        /// <summary>Update the displayed coin balance.</summary>
        public void SetCoinAmount(int amount)
        {
            _coinAmount = amount;
            RefreshWalletLabels();
        }

        // ── Tab Logic ─────────────────────────────────────────────────────

        private void SwitchTab(ShopTab tab)
        {
            _activeTab = tab;
            UpdateTabVisuals();
            PopulateGrid(GetItemsForTab(tab));
        }

        private void UpdateTabVisuals()
        {
            _tabOffersBtn?.EnableInClassList("tab-btn--active", _activeTab == ShopTab.Offers);
            _tabMoneyBtn ?.EnableInClassList("tab-btn--active", _activeTab == ShopTab.Money);
            _tabCoinsBtn ?.EnableInClassList("tab-btn--active", _activeTab == ShopTab.Coins);
        }

        private ShopItemData[] GetItemsForTab(ShopTab tab)
        {
            if (shopConfig == null) return null;

            return tab switch
            {
                ShopTab.Offers => shopConfig.offersItems,
                ShopTab.Money  => shopConfig.moneyItems,
                ShopTab.Coins  => shopConfig.coinItems,
                _              => shopConfig.coinItems
            };
        }

        // ── Grid Population ───────────────────────────────────────────────

        /// <summary>
        /// Clears the current grid and rebuilds it procedurally from <paramref name="items"/>.
        /// Each card is created via ShopItemCardBuilder (SRP / DIP).
        /// </summary>
        private void PopulateGrid(ShopItemData[] items)
        {
            if (_itemsGrid == null)
            {
                Debug.LogError("[ShopScreenController] items-grid element not found in UXML.");
                return;
            }

            if (shopItemCardTemplate == null)
            {
                Debug.LogError("[ShopScreenController] shopItemCardTemplate is not assigned.");
                return;
            }

            _itemsGrid.Clear();

            if (items == null || items.Length == 0)
            {
                Debug.LogWarning($"[ShopScreenController] No items for tab: {_activeTab}");
                return;
            }

            foreach (var item in items)
            {
                if (item == null) continue;

                VisualElement card;

                if (_activeTab == ShopTab.Offers)
                {
                    card = OfferItemCardBuilder.Build(
                        offerItemCardTemplate,
                        item,
                        OnItemPurchased);
                }
                else
                {
                    card = ShopItemCardBuilder.Build(
                        shopItemCardTemplate,
                        item,
                        watchAdIconSprite,
                        OnItemPurchased);
                }

                _itemsGrid.Add(card);
            }
        }

        // ── Event Handlers ────────────────────────────────────────────────

        private void OnItemPurchased(ShopItemData item)
        {
            if (item.purchaseType == PurchaseType.WatchAd)
            {
                Debug.Log($"[ShopScreen] 📺 Watch ad → reward: {item.amountDisplay} {item.tab} (id: {item.productId})");
                // TODO: trigger rewarded ad via IAdProvider
            }
            else
            {
                Debug.Log($"[ShopScreen] 💳 Purchase → product: {item.productId} | amount: {item.amountDisplay} | price: {item.priceDisplay}");
                // TODO: trigger IAP via IIAPService
            }
        }

        private void OnClose()
        {
            Debug.Log("[ShopScreen] ✕ Close pressed.");
            UINavigationController.GoBack();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void ApplySprites()
        {
            if (backgroundSprite != null && _shopBackground != null)
                _shopBackground.style.backgroundImage = new StyleBackground(backgroundSprite);

            if (moneyWalletVector != null && _moneyWallet != null)
                _moneyWallet.style.backgroundImage = new StyleBackground(moneyWalletVector);

            if (coinWalletVector != null && _coinWallet != null)
                _coinWallet.style.backgroundImage = new StyleBackground(coinWalletVector);

            if (closeBtnSprite != null && _closeBtn != null)
            {
                _closeBtn.style.backgroundImage = new StyleBackground(closeBtnSprite);
                _closeBtn.text = "";
            }
        }

        private void RefreshWalletLabels()
        {
            if (_moneyAmountLabel != null)
                _moneyAmountLabel.text = _moneyAmount.ToString("N0").Replace(",", ".");

            if (_coinAmountLabel != null)
                _coinAmountLabel.text = _coinAmount.ToString("N0").Replace(",", ".");
        }
    }
}
