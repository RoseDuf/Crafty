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
        public float angleTheta; //for x, y, z vector
        public float anglePhi; // for x, z vector
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
            FindInitialAngles();
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
                rb.velocity = new Vector3(hit_power * Mathf.Cos(angleTheta) * Mathf.Cos(anglePhi), hit_power * Mathf.Sin(angleTheta), hit_power * Mathf.Cos(angleTheta) * Mathf.Sin(anglePhi));
            }
            if (Input.GetButtonDown("Straight"))
            {

            }
        }

        void FindInitialAngles()
        {
            float magnitudeOfVectorXZ = Mathf.Sqrt(Mathf.Pow(endPointProjection.position.x, 2) + Mathf.Pow(endPointProjection.position.z, 2));

            //angle is in radians
            angleTheta = (Mathf.PI / 2) - ((Mathf.Asin((magnitudeOfVectorXZ * -Physics.gravity.y)/ Mathf.Pow(hit_power, 2))) / 2);

            //max distance can only be achieved at 45 degree angle
            if (angleTheta * Mathf.Rad2Deg < 45.0f)
            {
                angleTheta = 0;
            }

            anglePhi = Mathf.Acos(endPointProjection.position.x / magnitudeOfVectorXZ);
        }

        void FindFinalZForCurve()
        {
            z = (horizontalCurve * Mathf.Pow(endPointProjection.position.x, 2)) / (2 * (hit_power/Mathf.Cos(angleTheta)));
        }

    }
}
