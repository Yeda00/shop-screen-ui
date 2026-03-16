using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace Common.UI
{
    /// <summary>
    /// Available UI screens for navigation.
    /// </summary>
    public enum Screens
    {
        Lobby,
        Matchmaking,
        Shop,
        SeasonPass,
        Leaderboard,
        Friends,
        Missions,
        Settings
    }

    /// <summary>
    /// Event-based UI navigation system.
    /// Manages showing/hiding UI screens without scene transitions.
    /// </summary>
    public class UINavigationController : MonoBehaviour
    {
        // Events for screen navigation
        public static event Action<Screens> OnNavigateToScreen;
        public static event Action OnBackPressed;

        // Registered screens
        private static Dictionary<Screens, VisualElement> _registeredScreens = new Dictionary<Screens, VisualElement>();
        private static Screens _currentScreen = Screens.Lobby;
        private static Stack<Screens> _navigationStack = new Stack<Screens>();

        /// <summary>
        /// Register a screen with the navigation controller.
        /// Call this from each screen's OnEnable.
        /// </summary>
        public static void RegisterScreen(Screens screenType, VisualElement screenElement)
        {
            if (!_registeredScreens.ContainsKey(screenType))
                _registeredScreens[screenType] = screenElement;
        }

        /// <summary>
        /// Unregister a screen (called from OnDisable).
        /// </summary>
        public static void UnregisterScreen(Screens screenType)
        {
            if (_registeredScreens.ContainsKey(screenType))
            {
                _registeredScreens.Remove(screenType);
                Debug.Log($"[UINavigationController] Unregistered screen: {screenType}");
            }
        }

        /// <summary>
        /// Navigate to a specific screen.
        /// </summary>
        public static void NavigateTo(Screens screen, bool addToStack = true)
        {
            if (!_registeredScreens.ContainsKey(screen))
            {
                Debug.LogWarning($"[UINavigationController] Screen not registered: {screen}");
                return;
            }

            // Hide current screen
            if (_registeredScreens.ContainsKey(_currentScreen))
            {
                _registeredScreens[_currentScreen].style.display = DisplayStyle.None;
            }

            // Add current to stack if navigating forward
            if (addToStack && _currentScreen != screen)
            {
                _navigationStack.Push(_currentScreen);
            }

            // Show new screen
            _registeredScreens[screen].style.display = DisplayStyle.Flex;
            _currentScreen = screen;

            // Invoke event
            OnNavigateToScreen?.Invoke(screen);

            Debug.Log($"[UINavigationController] Navigated to: {screen}");
        }

        /// <summary>
        /// Go back to previous screen in navigation stack.
        /// </summary>
        public static void GoBack()
        {
            if (_navigationStack.Count > 0)
            {
                Screens previousScreen = _navigationStack.Pop();
                NavigateTo(previousScreen, addToStack: false);
                OnBackPressed?.Invoke();
            }
            else
            {
                Debug.Log("[UINavigationController] No previous screen in stack");
            }
        }

        /// <summary>
        /// Show a screen on top of current (overlay).
        /// </summary>
        public static void ShowOverlay(Screens screen)
        {
            if (!_registeredScreens.ContainsKey(screen))
            {
                Debug.LogWarning($"[UINavigationController] Screen not registered: {screen}");
                return;
            }

            // Keep current screen visible, show overlay on top
            _registeredScreens[screen].style.display = DisplayStyle.Flex;
            _navigationStack.Push(_currentScreen);
            _currentScreen = screen;

            OnNavigateToScreen?.Invoke(screen);
            Debug.Log($"[UINavigationController] Showing overlay: {screen}");
        }

        /// <summary>
        /// Hide overlay and return to previous screen.
        /// </summary>
        public static void HideOverlay()
        {
            GoBack();
        }

        /// <summary>
        /// Reset navigation state (return to lobby).
        /// </summary>
        public static void ResetToLobby()
        {
            _navigationStack.Clear();
            NavigateTo(Screens.Lobby, addToStack: false);
        }

        /// <summary>
        /// Clear all registered screens (call when changing scenes).
        /// </summary>
        public static void Clear()
        {
            _registeredScreens.Clear();
            _navigationStack.Clear();
            _currentScreen = Screens.Lobby;
            Debug.Log("[UINavigationController] Navigation state cleared");
        }

        /// <summary>
        /// Hide all screens (useful when transitioning to game scenes).
        /// </summary>
        public static void HideAllScreens()
        {
            foreach (var screen in _registeredScreens.Values)
            {
                screen.style.display = DisplayStyle.None;
            }
            Debug.Log("[UINavigationController] All screens hidden");
        }

        public static Screens CurrentScreen => _currentScreen;
        public static int NavigationDepth => _navigationStack.Count;
    }
}
