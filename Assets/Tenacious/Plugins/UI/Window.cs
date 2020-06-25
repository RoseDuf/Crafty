using UnityEngine;
using UnityEngine.EventSystems;

namespace Tenacious.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class Window : MonoBehaviour
    {
        [Tooltip("Destroy the GameObject when this window is closed (saves memory but, if false, then you have to manually manage this window instance after closing it)")]
        [SerializeField] public bool destroyWhenClosed = true;

        [Tooltip("Hide windows that are under this one")]
        [SerializeField] public bool hideWindowsUnderneath = true;

        [Tooltip("The game object to focus on when this window is focused")]
        [SerializeField] public GameObject focusTarget;

        // TODO: put these methods in an interface
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual void OnEnableWindow() { }
        protected virtual void OnDisableWindow() { }
        protected virtual void OnFocus() { }
        protected virtual void OnUpdate() { }

        private bool windowEnabled;

        protected virtual void Awake()
        {
            windowEnabled = true;
        }

        public void Open()
        {
            Focus();
            OnOpen();
        }

        public void Close()
        {
            DisableWindow();
            OnClose();
        }

        public void Show()
        {
            this.gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
            OnHide();
        }

        public void EnableWindow()
        {
            this.GetComponent<CanvasGroup>().interactable = windowEnabled = true;
            OnEnableWindow();
        }

        public void DisableWindow()
        {
            this.GetComponent<CanvasGroup>().interactable = windowEnabled = false;
            OnDisableWindow();
        }

        public void Focus(EventSystem event_system = null)
        {
            EnableWindow();
            if (event_system == null) event_system = EventSystem.current;
            event_system.SetSelectedGameObject(focusTarget);
            OnFocus();
        }

        public bool isEnabled()
        {
            return windowEnabled;
        }

        private void Update()
        {
            if (windowEnabled) OnUpdate();
        }
    }
}
