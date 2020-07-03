using UnityEngine;

using Game.Objects.Powerups;

namespace Game.Objects
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Powerup powerup;

        public Powerup Powerup
        {
            get { return powerup; }
            set 
            {
                powerup = value;
                if (powerup != null)
                    powerup.Acquire(this);
            }
        }

        private void Awake()
        {
            Powerup = powerup;
        }
    }
}
