using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BallController
{

    public class BallController : MonoBehaviour
    {

        [SerializeField] float hit_power;
        [SerializeField] Transform endPointProjection;
        [SerializeField] float horizontalCurve; //temporaty, will get this from meters later

        private Rigidbody rb;
        private const float GRAVITY = 9.8f;
        private float angle;
        private float z;

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
            FindInitialAngleY();
            FindFinalZForCurve();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void HandleInput()
        {
            if (Input.GetButtonDown("Lob"))
            {
                rb.AddForce((hit_power / Mathf.Cos(angle)), (hit_power / Mathf.Sin(angle)), 0.0f, ForceMode.Impulse);
            }
            if (Input.GetButtonDown("Straight"))
            {

            }
        }

        void FindInitialAngleY()
        {
            angle = (Mathf.Asin((endPointProjection.position.x * -Physics.gravity.y)/ Mathf.Pow(hit_power, 2))) / 2;
            angle = angle * Mathf.Rad2Deg;
            print(angle);
        }

        void FindFinalZForCurve()
        {
            z = (horizontalCurve * Mathf.Pow(endPointProjection.position.x, 2)) / (2 * (hit_power/Mathf.Cos(angle)));
        }

    }
}
