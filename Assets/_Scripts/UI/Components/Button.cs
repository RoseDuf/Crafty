using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.UI.Components
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class Button : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Color Overrides (transparent = no override)")]
        [SerializeField] private Color textColor = new Color(21f / 255f, 21f / 255f, 21f / 255f, 0f);
        [SerializeField] private Color textHoverColor = new Color(21f / 255f, 21f / 255f, 21f / 255f, 0f);

        private bool isHoveredOrSelected;
        private Vector2 initialShadowEffectDistance;
        private Color initialTextColor;

        private void Awake()
        {
            initialShadowEffectDistance = this.GetComponent<Shadow>().effectDistance;

            if (!isHoveredOrSelected && textColor.a > 0f)
                this.GetComponentInChildren<Text>().color = textColor;

            initialTextColor = this.GetComponentInChildren<Text>().color;
        }

        public void InvokeClick()
        {
            this.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        }

        public void OnPointerEnter()
        {
            if (textHoverColor.a > 0f)
                this.GetComponentInChildren<Text>().color = textHoverColor;

            isHoveredOrSelected = true;
        }

        public void OnPointerExit()
        {
            if (textColor.a > 0f)
                this.GetComponentInChildren<Text>().color = textColor;
            else
                this.GetComponentInChildren<Text>().color = initialTextColor;

            isHoveredOrSelected = false;
        }

        public void OnSelect()
        {
            if (textHoverColor.a > 0f)
                this.GetComponentInChildren<Text>().color = textHoverColor;

            isHoveredOrSelected = true;
        }

        public void OnDeselect()
        {
            if (textColor.a > 0f)
                this.GetComponentInChildren<Text>().color = textColor;
            else
                this.GetComponentInChildren<Text>().color = initialTextColor;

            isHoveredOrSelected = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            this.GetComponent<Shadow>().effectDistance = Vector2.zero;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            this.GetComponent<Shadow>().effectDistance = initialShadowEffectDistance;
        }
    }
}
