using UnityEngine;

namespace Game.UI.HUDs.Gameplay
{
    public class ShotMetricBars : MonoBehaviour
    {
        [SerializeField] private GameObject verticalMeterObject;
        [SerializeField] private GameObject horizontalMeterObject;
        [SerializeField] private GameObject powerMeterObject;

        private float verticalMeterValue;
        private float horizontalMeterValue;
        private float powerMeterValue;

        public float VerticalMeterValue
        {
            get { return verticalMeterValue; }
            set
            {
                verticalMeterValue = Mathf.Clamp(value, 0f, 1f);

                RectTransform verticalMeterRectTransform = ((RectTransform) verticalMeterObject.transform);
                float height = verticalMeterRectTransform.rect.height;
                Vector3 oldPosition = ((RectTransform) verticalMeterObject.transform.GetChild(0)).position;
                float posY = (verticalMeterValue * height) + 20;
                ((RectTransform) verticalMeterObject.transform.GetChild(0)).position = new Vector3(oldPosition.x, posY, oldPosition.z);
            }
        }

        public float HorizontalMeterValue
        {
            get { return horizontalMeterValue; }
            set
            {
                horizontalMeterValue = Mathf.Clamp(value, 0f, 1f);

                RectTransform horizontalMeterRectTransform = ((RectTransform)horizontalMeterObject.transform);
                float width = horizontalMeterRectTransform.rect.width;
                Vector3 oldPosition = ((RectTransform) horizontalMeterObject.transform.GetChild(0)).position;
                float posX = (horizontalMeterValue * width) + 40;
                ((RectTransform) horizontalMeterObject.transform.GetChild(0)).position = new Vector3(posX, oldPosition.y, oldPosition.z);
            }
        }

        public float PowerMeterValue
        {
            get { return powerMeterValue; }
            set
            {
                powerMeterValue = Mathf.Clamp(value, 0f, 1f);

                Vector3 oldScale = powerMeterObject.transform.GetChild(0).localScale;
                powerMeterObject.transform.GetChild(0).localScale = new Vector3(oldScale.x, powerMeterValue, oldScale.z);
            }
        }

        private void Awake()
        {
            VerticalMeterValue = verticalMeterValue;
            HorizontalMeterValue = horizontalMeterValue;
            PowerMeterValue = powerMeterValue;
        }
    }
}