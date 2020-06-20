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
                rb.velocity = new Vector3((hit_power * Mathf.Cos(angle)), (hit_power * Mathf.Sin(angle)), 0);
            }
            if (Input.GetButtonDown("Straight"))
            {

            }
        }

        void FindInitialAngleY()
        {
            //angle is in radians
            angle = (Mathf.PI / 2) - ((Mathf.Asin(((endPointProjection.position.x) * -Physics.gravity.y)/ Mathf.Pow(hit_power, 2))) / 2);

            //max distance can only be achieved at 45 degree angle
            if (angle * Mathf.Rad2Deg < 45.0f)
            {
                angle = 0;
            }
        }

        void FindFinalZForCurve()
        {
            z = (horizontalCurve * Mathf.Pow(endPointProjection.position.x, 2)) / (2 * (hit_power/Mathf.Cos(angle)));
        }

    }
}
