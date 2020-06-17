using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BallController
{

    public class BallController : MonoBehaviour
    {

        [SerializeField] float hit_power;

        Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        private void FixedUpdate()
        {
            HandleInput();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void HandleInput()
        {
            if (Input.GetButtonDown("Lob"))
            {
                print("hit");
                rb.AddForce(hit_power, hit_power, hit_power, ForceMode.Impulse);
            }
            if (Input.GetButtonDown("Straight"))
            {

            }
        }
    }
}
