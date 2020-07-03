using UnityEngine;

namespace Game.Objects.Powerups
{
    public class Powerup : ScriptableObject
    {
        [SerializeField] private Sprite sprite;

        public Sprite Sprite 
        {
            get { return sprite; }
        }

        public virtual void Acquire(Player player)
        {

        }
    }
}
