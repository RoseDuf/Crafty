using UnityEngine;

using System;

namespace Game.Objects.Creatures.Targets
{
    public class Target : MonoBehaviour
    {
        [SerializeField] private GameObject starObject;
        [SerializeField] private GameObject holePrefab;

        public event Action<Target> OnCapture;

        public bool IsCaptured { get; set; }

        private bool adjustPosition;
        private Vector3 aboveHolePosition;

        private void Awake()
        {
            aboveHolePosition = this.transform.position;
            starObject?.SetActive(false);
        }

        private void Update()
        {
            if (adjustPosition)
            {
                starObject.transform.position = Vector3.MoveTowards(starObject.transform.position, aboveHolePosition, Time.deltaTime * 10);
                if (starObject.transform.position == aboveHolePosition)
                    adjustPosition = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("Player"))
                Capture();
        }

        private void Capture()
        {
            IsCaptured = true;
            TurnIntoStar();

            OnCapture(this);
        }

        private void TurnIntoStar()
        {
            this.GetComponent<MeshRenderer>().enabled = false;
            this.GetComponent<MeshCollider>().enabled = false;

            starObject.SetActive(true);
        }

        public void TurnIntoHole()
        {
            TurnIntoStar();

            // do a downwards raycast and create a hole
            if (Physics.Raycast(starObject.transform.position, Vector3.down, out RaycastHit hit))
            {
                Vector3 translation = (hit.point.y - starObject.transform.position.y + 0.5f) * Vector3.up;
                aboveHolePosition = starObject.transform.position + translation;
                adjustPosition = true;

                if (holePrefab != null)
                    Instantiate(holePrefab, hit.point, hit.transform.rotation, this.transform);
            }
        }
    }
}