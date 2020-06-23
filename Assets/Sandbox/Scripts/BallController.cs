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
        public float finalZ; // for horizontal curve
        public float finalX; // for horizontal curve
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
            FindTotalTime();
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
            z = vectorFromBallToProjectionPoint.z < 0 ? -z : z;

            if (Input.GetButtonDown("Lob"))
            {
                shoot = true; 
                rb.velocity = new Vector3(x, y, z);
            }

            if (Input.GetButtonDown("Straight"))
            {

            }
            
            if (shoot && horizontalCurve != 0)
            {
                rb.AddForce(Vector3.Normalize(Vector3.Cross(vectorFromBallToProjectionPoint, Vector3.up)) * -horizontalCurve, ForceMode.Acceleration);
            }
        }

        void FindInitialAngles()
        {
            float magnitudeOfVectorXZ = Mathf.Sqrt(Mathf.Pow(vectorFromBallToProjectionPoint.x, 2) + Mathf.Pow(vectorFromBallToProjectionPoint.z, 2));

            // Angles are in radians.

            // Angle between the XZ axis and the Y axis.
            angleTheta = (Mathf.PI / 2) - ((Mathf.Asin((magnitudeOfVectorXZ * -Physics.gravity.y) / Mathf.Pow(hit_power, 2))) / 2);

            // Angle between the X and Z axis.
            anglePhi = Mathf.Abs(Mathf.Atan(vectorFromBallToProjectionPoint.z / vectorFromBallToProjectionPoint.x));
        }

        void FindTotalTime()
        {
            time = (2 / -Physics.gravity.y) * hit_power * Mathf.Sin(angleTheta);
        }

        void FindFinalXZForCurve()
        {
            if (horizontalCurve != 0.0f)
            {
                // Need this for when vectorFromBallToProjectionPoint is diagonal.
                float hypothenuse = (((-horizontalCurve / rb.mass) / 2) * Mathf.Pow(time, 2));

                finalX = vectorFromBallToProjectionPoint.x;
                // Adjust to axis.
                finalX = vectorFromBallToProjectionPoint.z < 0 ? finalX + (hypothenuse * Mathf.Sin(anglePhi)) : finalX - (hypothenuse * Mathf.Sin(anglePhi));

                finalZ = vectorFromBallToProjectionPoint.z;
                // Adjust to axis.
                finalZ = vectorFromBallToProjectionPoint.x < 0 ? finalZ - (hypothenuse * Mathf.Cos(anglePhi)) : finalZ + (hypothenuse * Mathf.Cos(anglePhi));
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
        
            CorrectProjectionPosition();
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
