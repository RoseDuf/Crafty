using UnityEngine;

using Tenacious.Scenes;

namespace Game.Scenes
{
    public class CreditsScene : MonoBehaviour
    {
        [SerializeField] private RectTransform scrollingArea;
        [SerializeField] [Min(0)] private float movementSpeed = 50f;

        private float scrollSpeed;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;

            scrollingArea.anchoredPosition = new Vector2(scrollingArea.anchoredPosition.x, -scrollingArea.parent.GetComponent<RectTransform>().rect.height);
            scrollSpeed = movementSpeed;
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Submit"))
                AlterScrollSpeed(3);

            if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Submit"))
                AlterScrollSpeed();
        }

        private void AlterScrollSpeed(float speedFactor = 1)
        {
            scrollSpeed = movementSpeed * speedFactor;
        }

        private void Update()
        {
            HandleInput();

            scrollingArea.transform.Translate(0, scrollSpeed * Time.deltaTime, 0);

            if (scrollingArea.anchoredPosition.y > scrollingArea.rect.height)
                SceneLoader.Instance.LoadScene("Welcome", SceneLoader.ETransitionType.Fade);
        }
    }
}
