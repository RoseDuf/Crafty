using UnityEngine;
using UnityEngine.UI;

using Tenacious.Scenes;

using DG.Tweening;

namespace Game.Scenes
{
    public class SplashScene : MonoBehaviour
    {
        [SerializeField] private Image logo;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;

            logo.rectTransform.DOScale(1, 2).SetEase(Ease.Linear).OnComplete(() => 
            {
                SceneLoader.Instance.LoadScene("Welcome", SceneLoader.ETransitionType.Box);
            });
        }
    }
}