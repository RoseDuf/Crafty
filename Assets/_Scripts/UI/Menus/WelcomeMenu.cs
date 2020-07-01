#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.EventSystems;

using Tenacious.Scenes;
using Tenacious.UI;

namespace Game.UI.Menus
{
    public class WelcomeMenu : MonoBehaviour
    {
        [SerializeField] private GameObject firstSelected;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (firstSelected != null)
                EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        public void PlayBtnClick()
        {
            // TODO
        }

        public void CreditsBtnClick()
        {
            SceneLoader.Instance.LoadScene("Credits", SceneLoader.ETransitionType.Fade);
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
