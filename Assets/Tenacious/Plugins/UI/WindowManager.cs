using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tenacious.UI
{
    public class WindowManager : MonoBehaviour
    {
        [SerializeField] private EventSystem eventSystem;

        [Tooltip("List of Window prefabs")]
        [SerializeField] private List<Window> windowPrefabList;

        private Stack<Window> windowStack;

        private void Awake()
        {
            if (eventSystem == null) eventSystem = EventSystem.current;

            windowStack = new Stack<Window>();
            if (windowPrefabList == null) windowPrefabList = new List<Window>();
        }

        public T CreateWindow<T>() where T : Window
        {
            T windowPrefab = GetWindowPrefab<T>();
            T window = Instantiate(windowPrefab, this.transform);
            return window;
        }

        private T GetWindowPrefab<T>() where T : Window
        {
            foreach (Window windowPrefab in windowPrefabList)
            {
                if (windowPrefab != null)
                    return windowPrefab.GetComponent<T>();
            }

            throw new MissingReferenceException("[" + nameof(WindowManager) + "] Cannot find prefab script for window of type : " + typeof(T));
        }

        public T OpenWindow<T>() where T : Window
        {
            return (T)OpenWindow(CreateWindow<T>());
        }

        public Window OpenWindow(Window window)
        {
            if (windowStack.Count > 0)
            {
                foreach (Window win in windowStack)
                {
                    win.DisableWindow();

                    if (window.hideWindowsUnderneath)
                        win.Hide();

                    if (win.hideWindowsUnderneath) break;
                }

                // make sure new window is on top of previous in the Scene
                Canvas top = window.GetComponent<Canvas>();
                Canvas previous = windowStack.Peek().GetComponent<Canvas>();
                top.sortingOrder = previous.sortingOrder + 1;
            }

            window.gameObject.name = window.gameObject.name + " [" + (windowStack.Count).ToString() + "]";
            windowStack.Push(window);
            window.gameObject.SetActive(true);
            window.Open();

            return windowStack.Peek();
        }

        public void CloseTopWindow()
        {
            if (windowStack.Count == 0) return;

            Window window = windowStack.Pop();

            window.Close();
            if (window.destroyWhenClosed)
            {
                Destroy(window.gameObject);
            }
            else
            {
                window.gameObject.SetActive(false);
                window.gameObject.name = Regex.Replace(window.gameObject.name, @"\[\d+\](?!.*\[\d+\])", "");
            }

            foreach (Window win in windowStack)
            {
                win.Show();
                if (win.hideWindowsUnderneath) break;
            }

            if (windowStack.Count > 0)
                windowStack.Peek().Focus();
        }

        public void CloseAllWindows()
        {
            do CloseTopWindow();
            while (windowStack.Count > 0);
        }

        public Window TopWindow
        {
            get { return (windowStack != null || windowStack.Count > 0) ? windowStack.Peek() : null; }
        }
    }
}
