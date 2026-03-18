using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(UIDocument))]
    public abstract class UIScreenBase : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] protected UIDocument uiDocument;
        [SerializeField] protected Font globalFont;

        protected VisualElement root;

        protected virtual void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
        }

        protected virtual void OnEnable()
        {
            if (uiDocument == null)
            {
                Debug.LogError($"[{GetType().Name}] UIDocument component is missing!");
                return;
            }

            root = uiDocument.rootVisualElement;

            if (root == null)
            {
                Debug.LogError($"[{GetType().Name}] Root visual element is null!");
                return;
            }

            InitializeUIElements();

            if (Application.isPlaying)
                RegisterCallbacks();

            OnScreenEnabled();

            if (globalFont != null)
                ApplyFontToTree(root);
        }

        protected virtual void OnDisable()
        {
            UnregisterCallbacks();
            OnScreenDisabled();
        }

        protected abstract void InitializeUIElements();
        protected virtual void RegisterCallbacks() { }
        protected virtual void UnregisterCallbacks() { }
        protected virtual void OnScreenEnabled() { }
        protected virtual void OnScreenDisabled() { }

        /// <summary>
        /// Recursively applies globalFont to every element in the given subtree.
        /// Call this after dynamically adding elements to the visual tree.
        /// </summary>
        protected void ApplyFontToTree(VisualElement element)
        {
            if (element == null || globalFont == null) return;
            element.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(globalFont));
            foreach (var child in element.Children())
                ApplyFontToTree(child);
        }

        protected T QueryElement<T>(string elementName) where T : VisualElement
        {
            var element = root.Q<T>(elementName);
            if (element == null)
                Debug.LogWarning($"[{GetType().Name}] UI element '{elementName}' of type {typeof(T).Name} not found");
            return element;
        }

        protected void RegisterButtonCallback(Button button, System.Action callback)
        {
            if (button != null && callback != null)
                button.RegisterCallback<ClickEvent>(evt => callback());
        }

        protected void UnregisterButtonCallback(Button button, System.Action callback)
        {
            if (button != null && callback != null)
                button.UnregisterCallback<ClickEvent>(evt => callback());
        }
    }
}
