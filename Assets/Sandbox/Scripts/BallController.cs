using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BallController
{

    public class BallController : MonoBehaviour
    {

        [SerializeField] float hit_power;
        [SerializeField] GameObject endPointProjection;
        [SerializeField] GameObject bouncePointProjection1;
        [SerializeField] float horizontalCurve; //temporaty, will get this from meters later

        private Rigidbody rb;
        public float angleTheta; //for x, y, z vector
        public float anglePhi; // for x, z vector
        public float x;
        public float y;
        public float z;
        public float time;
        private Vector3 vectorFromBallToProjectionPoint;

        private float previousHorizontalCurve;
        private float finalZ; // for horizontal curve
        private float finalX; // for horizontal curve

        private float bounceValueBG; // Ball/ground average
        private float dynamicFrictionValueBG; // Ball/ground average
        [SerializeField] PhysicMaterial groundMaterial;
        public float bounceX1;
        public float bounceY1;
        public float bounceZ1;
        public float bounceX2;
        public float bounceY2;
        public float bounceZ2;
        private const float DUFRESNE_CONSTANT = 0.7f; // Mystery friction value that we don't seem to have access to anywhere, so I had to find it myself.

        bool shoot;
        bool thereIsCurve;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Start is called before the first frame update
        void Start()
        {
            float bounceValueBall = GetComponent<Collider>().material.bounciness;
            float dynamicFrictionValueBall = GetComponent<Collider>().material.dynamicFriction;
            float bounceValueGround = groundMaterial.bounciness;
            float dynamicFrictionValueGround = groundMaterial.dynamicFriction;

            // Average bounce and friction values
            bounceValueBG = (bounceValueBall + bounceValueGround) / 2;
            dynamicFrictionValueBG = (dynamicFrictionValueBall + dynamicFrictionValueGround) / 2;

            shoot = false;
            thereIsCurve = false;
            vectorFromBallToProjectionPoint = endPointProjection.transform.position;
        }

        private void FixedUpdate()
        {
            FindInitialAngles();
            FindTotalTime();
            FindFinalXZForCurve();
            PredictFirstBouncePosition();
            //PredictSecondBouncePosition();

            previousHorizontalCurve = horizontalCurve;
            
            UpdateVectorFromBallToProjectionPoint();

            HandleInput();

            if (shoot)
            {
                //print("x: " + transform.position.x + " y: " + transform.position.y + " z: " + transform.position.z + " time: " + Time.time);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        void HandleInput()
        {
            FindInitialVector();

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

        void FindInitialVector()
        {
            x = hit_power * Mathf.Cos(angleTheta) * Mathf.Cos(anglePhi);
            y = hit_power * Mathf.Sin(angleTheta);
            z = hit_power * Mathf.Cos(angleTheta) * Mathf.Sin(anglePhi);

            x = vectorFromBallToProjectionPoint.x < 0 ? -x : x;
            z = vectorFromBallToProjectionPoint.z < 0 ? -z : z;
        }

        void FindTotalTime()
        {
            if (!shoot)
            {
                time = (2 / -Physics.gravity.y) * hit_power * Mathf.Sin(angleTheta);
            }
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

        void PredictFirstBouncePosition()
        {
            if (!shoot)
            {
                float initialVelocityX = dynamicFrictionValueBG != 0f ? x * DUFRESNE_CONSTANT : x;
                float initialVelocityY = y * (bounceValueBG);
                float initialVelocityZ = dynamicFrictionValueBG != 0f ? z * DUFRESNE_CONSTANT : z;
                float time2 = time * (bounceValueBG);

                bounceX1 = endPointProjection.transform.position.x + (initialVelocityX * time2);
                bounceY1 = endPointProjection.transform.position.y + (initialVelocityY * time2) - (-Physics.gravity.y/2 * Mathf.Pow(time2,2));
                bounceZ1 = endPointProjection.transform.position.z + (initialVelocityZ * time2);

                bouncePointProjection1.transform.position = new Vector3(bounceX1, bounceY1, bounceZ1);
            }
        }

        //void PredictSecondBouncePosition()
        //{
        //    if (!shoot)
        //    {
        //        float initialVelocityX = dynamicFrictionValueBG != 0f ? Mathf.Pow(x * DUFRESNE_CONSTANT, 2) : x;
        //        float initialVelocityY = Mathf.Pow(y * (bounceValueBG), 2);
        //        float initialVelocityZ = dynamicFrictionValueBG != 0f ? Mathf.Pow(z * DUFRESNE_CONSTANT, 2) : z;
        //        float time2 = Mathf.Pow(time * (bounceValueBG), 2);

        //        bounceX2 = bounceX1 + (initialVelocityX * time);
        //        bounceY2 = bounceY1 + (initialVelocityY * time) - (-Physics.gravity.y / 2 * Mathf.Pow(time, 2));
        //        bounceZ2 = bounceZ1 + (initialVelocityZ * time);

        //        bouncePointProjection2.transform.position = new Vector3(bounceX2, bounceY2, bounceZ2);
        //    }
        //}

    }
}
