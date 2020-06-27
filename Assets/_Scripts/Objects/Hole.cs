using UnityEngine;

namespace Game.Objects
{
    public class Hole : MonoBehaviour
    {
        private int holeLayer = -1;
        private int playerLayer = -1;

        private void Awake()
        {
            holeLayer = LayerMask.NameToLayer("Hole");
            playerLayer = LayerMask.NameToLayer("Player");

            for (int i = 0; i < 32; i++)
            {
                if (i != holeLayer && i != playerLayer)
                    Physics.IgnoreLayerCollision(i, holeLayer, true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("Player"))
            {
                other.gameObject.layer = holeLayer;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag.Equals("Player"))
            {
                other.gameObject.layer = playerLayer;
            }
        }
    }
}