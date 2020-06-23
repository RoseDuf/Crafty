using UnityEngine;

using Tenacious.Scenes;

namespace Game.Scenes
{
    public class LoadingScene : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform loadingBar;

        [Tooltip("Time to wait until Canvas is shown")]
        [SerializeField] private float canvasShowTime = 1;
        private float canvasTimer;

        private void Awake()
        {
            SceneLoader.Instance.loadingBar = loadingBar;
            canvas.gameObject.SetActive(false);
            canvasTimer = 0;
        }

        private void Update()
        {
            canvasTimer += Time.deltaTime;
            if (canvasTimer >= canvasShowTime)
                canvas.gameObject.SetActive(true);
        }
    }
}