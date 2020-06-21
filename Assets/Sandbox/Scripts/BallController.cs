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
        private float time;
        private Vector3 vectorFromBallToProjectionPoint;
        private float deltaZ; //to find final z value if there is horizontal curve

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Start is called before the first frame update
        void Start()
        {
            vectorFromBallToProjectionPoint = new Vector3(0f, 0f, 0f);
        }

        private void FixedUpdate()
        {
            UpdateVectorFromBallToProjectionPoint();
            FindInitialAngles();

            HandleInput();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void HandleInput()
        {
            float x = hit_power * Mathf.Cos(angleTheta) * Mathf.Cos(anglePhi);
            float y = hit_power * Mathf.Sin(angleTheta);
            float z = hit_power * Mathf.Cos(angleTheta) * Mathf.Sin(anglePhi);

            x = vectorFromBallToProjectionPoint.x < 0 ? -x : x;

            if (Input.GetButtonDown("Lob"))
            {
                rb.velocity = new Vector3(x, y, z);

                if (horizontalCurve > 0)
                {
                    print("curve");
                    rb.AddForce(Vector3.Cross(vectorFromBallToProjectionPoint, Vector3.up), ForceMode.Acceleration);
                }
            }
            if (Input.GetButtonDown("Straight"))
            {

            }
        }

        void FindInitialAngles()
        {
            float magnitudeOfVectorXZ = Mathf.Sqrt(Mathf.Pow(vectorFromBallToProjectionPoint.x, 2) + Mathf.Pow(vectorFromBallToProjectionPoint.z, 2));

            //angle is in radians
            angleTheta = (Mathf.PI / 2) - ((Mathf.Asin((magnitudeOfVectorXZ * -Physics.gravity.y)/ Mathf.Pow(hit_power, 2))) / 2);

            time = (2 / -Physics.gravity.y) * hit_power * Mathf.Sin(angleTheta);
            
            anglePhi = Mathf.Asin((vectorFromBallToProjectionPoint.z - ((horizontalCurve / 2) * Mathf.Pow(time, 2))) / (hit_power * Mathf.Cos(angleTheta) * time));
        }

        void FindFinalZForCurve()
        {
            //z = (horizontalCurve * Mathf.Pow(endPointProjection.position.x, 2)) / (2 * (hit_power / Mathf.Cos(angleTheta)));
        }

        void UpdateVectorFromBallToProjectionPoint()
        {
            vectorFromBallToProjectionPoint = endPointProjection.position - transform.position;
        }

    }
}
