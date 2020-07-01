using UnityEngine;

namespace Game.UI.HUDs.Gameplay
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject heartPrefab;

        [Min(0)]
        [SerializeField] private int health = 4;

        public int Health
        {
            get { return health; }
            set
            {
                health = value;
                AdjustHearts();
            }
        }

        private void Awake()
        {
            Health = health;
        }

        private void AdjustHearts()
        {
            int heartCount = this.transform.childCount;
            while (heartCount != health)
            {
                if (heartCount - health < 0)
                {
                    Instantiate(heartPrefab, this.transform);
                    heartCount++;
                }
                else
                {
                    Destroy(this.transform.GetChild(0).gameObject);
                    this.transform.GetChild(0).SetParent(this.transform.parent);
                    heartCount--;
                }
            }
        }
    }
}