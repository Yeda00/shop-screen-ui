using UnityEngine;
using UnityEngine.UIElements;

namespace Common.UI
{
    /// <summary>
    /// Base class for all UI screens.
    /// Provides common functionality for UI document management and element initialization.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public abstract class UIScreenBase : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] protected UIDocument uiDocument;

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
            RegisterCallbacks();
            OnScreenEnabled();
        }

        protected virtual void OnDisable()
        {
            UnregisterCallbacks();
            OnScreenDisabled();
        }

        /// <summary>
        /// Initialize UI elements by querying the root visual element.
        /// Override this to cache references to UI elements.
        /// </summary>
        protected abstract void InitializeUIElements();

        /// <summary>
        /// Register button callbacks and event listeners.
        /// Override this to set up event handlers.
        /// </summary>
        protected virtual void RegisterCallbacks() { }

        /// <summary>
        /// Unregister button callbacks and event listeners.
        /// Override this to clean up event handlers.
        /// </summary>
        protected virtual void UnregisterCallbacks() { }

        /// <summary>
        /// Called after UI elements are initialized and callbacks are registered.
        /// Override this for screen-specific initialization logic.
        /// </summary>
        protected virtual void OnScreenEnabled() { }

        /// <summary>
        /// Called before callbacks are unregistered.
        /// Override this for screen-specific cleanup logic.
        /// </summary>
        protected virtual void OnScreenDisabled() { }

        /// <summary>
        /// Helper method to safely query UI elements with error logging.
        /// </summary>
        protected T QueryElement<T>(string elementName) where T : VisualElement
        {
            var element = root.Q<T>(elementName);
            if (element == null)
            {
                Debug.LogWarning($"[{GetType().Name}] UI element '{elementName}' of type {typeof(T).Name} not found");
            }
            return element;
        }

        /// <summary>
        /// Helper method to register button click callbacks.
        /// </summary>
        protected void RegisterButtonCallback(Button button, System.Action callback)
        {
            if (button != null && callback != null)
            {
                button.RegisterCallback<ClickEvent>(evt => callback());
            }
        }

        /// <summary>
        /// Helper method to unregister button click callbacks.
        /// </summary>
        protected void UnregisterButtonCallback(Button button, System.Action callback)
        {
            if (button != null && callback != null)
            {
                button.UnregisterCallback<ClickEvent>(evt => callback());
            }
        }
    }
}
