using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace Common.UI
{
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

    public class UINavigationController : MonoBehaviour
    {
        public static event Action<Screens> OnNavigateToScreen;
        public static event Action OnBackPressed;

        private static Dictionary<Screens, VisualElement> _registeredScreens = new Dictionary<Screens, VisualElement>();
        private static Screens _currentScreen = Screens.Lobby;
        private static Stack<Screens> _navigationStack = new Stack<Screens>();

        public static void RegisterScreen(Screens screenType, VisualElement screenElement)
        {
            if (!_registeredScreens.ContainsKey(screenType))
                _registeredScreens[screenType] = screenElement;
        }

        public static void UnregisterScreen(Screens screenType)
        {
            if (_registeredScreens.ContainsKey(screenType))
                _registeredScreens.Remove(screenType);
        }

        public static void NavigateTo(Screens screen, bool addToStack = true)
        {
            if (!_registeredScreens.ContainsKey(screen))
            {
                Debug.LogWarning($"[UINavigationController] Screen not registered: {screen}");
                return;
            }

            if (_registeredScreens.ContainsKey(_currentScreen))
                _registeredScreens[_currentScreen].style.display = DisplayStyle.None;

            if (addToStack && _currentScreen != screen)
                _navigationStack.Push(_currentScreen);

            _registeredScreens[screen].style.display = DisplayStyle.Flex;
            _currentScreen = screen;

            OnNavigateToScreen?.Invoke(screen);
        }

        public static void GoBack()
        {
            if (_navigationStack.Count > 0)
            {
                Screens previousScreen = _navigationStack.Pop();
                NavigateTo(previousScreen, addToStack: false);
                OnBackPressed?.Invoke();
            }
        }

        public static void ShowOverlay(Screens screen)
        {
            if (!_registeredScreens.ContainsKey(screen))
            {
                Debug.LogWarning($"[UINavigationController] Screen not registered: {screen}");
                return;
            }

            _registeredScreens[screen].style.display = DisplayStyle.Flex;
            _navigationStack.Push(_currentScreen);
            _currentScreen = screen;

            OnNavigateToScreen?.Invoke(screen);
        }

        public static void HideOverlay() => GoBack();

        public static void ResetToLobby()
        {
            _navigationStack.Clear();
            NavigateTo(Screens.Lobby, addToStack: false);
        }

        public static void Clear()
        {
            _registeredScreens.Clear();
            _navigationStack.Clear();
            _currentScreen = Screens.Lobby;
        }

        public static void HideAllScreens()
        {
            foreach (var screen in _registeredScreens.Values)
                screen.style.display = DisplayStyle.None;
        }

        public static Screens CurrentScreen => _currentScreen;
        public static int NavigationDepth => _navigationStack.Count;
    }
}
