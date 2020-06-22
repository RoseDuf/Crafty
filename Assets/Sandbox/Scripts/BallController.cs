using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BallController
{

    public class BallController : MonoBehaviour
    {

        [SerializeField] float hit_power;
        [SerializeField] GameObject endPointProjection;
        [SerializeField] float horizontalCurve; //temporaty, will get this from meters later

        private Rigidbody rb;
        public float angleTheta; //for x, y, z vector
        public float anglePhi; // for x, z vector
        public float finalZ;
        public float finalX;
        public float time;
        public Vector3 vectorFromBallToProjectionPoint;
        private float deltaZ; //to find final z value if there is horizontal curve

        private float previousHorizontalCurve;

        bool shoot;
        bool thereIsCurve;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Start is called before the first frame update
        void Start()
        {
            shoot = false;
            thereIsCurve = false;
            vectorFromBallToProjectionPoint = endPointProjection.transform.position;
        }

        private void FixedUpdate()
        {
            FindInitialAngles();
            FindFinalXZForCurve();

            previousHorizontalCurve = horizontalCurve;
            
            UpdateVectorFromBallToProjectionPoint();

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
                shoot = true; 

                rb.velocity = new Vector3(x, y, z);
            }

            if (Input.GetButtonDown("Straight"))
            {

            }
            
            if (shoot && horizontalCurve > 0)
            {
                rb.AddForce(Vector3.Normalize(Vector3.Cross(vectorFromBallToProjectionPoint, Vector3.up)) * horizontalCurve, ForceMode.Acceleration);
            }
        }

        void FindInitialAngles()
        {
            float magnitudeOfVectorXZ = Mathf.Sqrt(Mathf.Pow(vectorFromBallToProjectionPoint.x, 2) + Mathf.Pow(vectorFromBallToProjectionPoint.z, 2));

            //angle is in radians
            angleTheta = (Mathf.PI / 2) - ((Mathf.Asin((magnitudeOfVectorXZ * -Physics.gravity.y) / Mathf.Pow(hit_power, 2))) / 2);

            time = (2 / -Physics.gravity.y) * hit_power * Mathf.Sin(angleTheta);

            anglePhi = Mathf.Atan(vectorFromBallToProjectionPoint.z / vectorFromBallToProjectionPoint.x);
        }

        void FindFinalXZForCurve()
        {
            if (horizontalCurve != 0.0f)
            {
                finalX = hit_power * Mathf.Cos(angleTheta) * Mathf.Cos(anglePhi) * time + transform.position.x;
                finalZ = hit_power * Mathf.Cos(angleTheta) * Mathf.Sin(anglePhi) * time + (((horizontalCurve / rb.mass) / 2) * Mathf.Pow(time, 2)) + transform.position.z;
            }
            else
            {
                if (horizontalCurve == 0.0f && previousHorizontalCurve != 0)
                {
                    finalX = vectorFromBallToProjectionPoint.x;
                    finalZ = vectorFromBallToProjectionPoint.z;
                }
                else
                {
                    finalX = endPointProjection.transform.position.x;
                    finalZ = endPointProjection.transform.position.z;
                }
            }
        
            if (!shoot)
            {
                CorrectProjectionPosition();
            }
        }

        void UpdateVectorFromBallToProjectionPoint()
        {
            if (horizontalCurve == 0.0f)
            {
                vectorFromBallToProjectionPoint = endPointProjection.transform.position - transform.position;
            }
        }

        void CorrectProjectionPosition()
        {
            endPointProjection.transform.position = new Vector3(finalX, endPointProjection.transform.position.y, finalZ);
        }
        
    }
}
