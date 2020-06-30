#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.EventSystems;

using Tenacious.Scenes;
using Tenacious.UI;

using Game.Scenes;

namespace Game.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject firstSelected;

        private CursorLockMode previousCursorLockModeState;
        private bool previousCursorVisibility;

        private void Awake()
        {
            if (firstSelected != null)
                EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        private void OnEnable()
        {
            previousCursorLockModeState = Cursor.lockState;
            previousCursorVisibility = Cursor.visible;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDisable()
        {
            Cursor.lockState = previousCursorLockModeState;
            Cursor.visible = previousCursorVisibility;
        }

        public void WelcomeMenuBtnClick()
        {
            SceneLoader.Instance.LoadScene("Welcome", SceneLoader.ETransitionType.Random);
        }

        public void QuitBtnClick()
        {
            ConfirmDialog cw = SystemUI.Instance.WindowManager.CreateWindow<ConfirmDialog>();
            cw.SetData("Are you sure you want to quit ?", (ConfirmDialog dialog, bool result) =>
            {
                if (result)
                {
                    Application.Quit();

#if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
#endif
                }
                else
                    SystemUI.Instance.WindowManager.CloseTopWindow();
            });
            SystemUI.Instance.WindowManager.OpenWindow(cw);
        }
    }
}
