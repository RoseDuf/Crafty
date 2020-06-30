using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BallController
{

    public class BallController : MonoBehaviour
    {

        [SerializeField] float hit_power;
        [SerializeField] GameObject endPointProjection;
        [SerializeField] GameObject bouncePointProjection;
        [SerializeField] float horizontalCurve; //temporaty, will get this from meters later
        [SerializeField] float verticalCurve; //temporaty, will get this from meters later

        private Rigidbody rb;
        public float angleTheta; //for x, y, z vector
        public float anglePhi; // for x, z vector
        public Vector3 Vo;
        public Vector3 initialVelocity;
        public float time;
        public float time2;
        private Vector3 vectorFromBallToProjectionPoint;
        private Vector3 vectorFromFirstPointToBouncePoint;

        private float previousHorizontalCurve;
        private float previousVerticalCurve;
        private float finalZ; // for horizontal curve
        private float finalX; // for horizontal curve

        private float bounceValueBG; // Ball/ground average
        private float dynamicFrictionValueBG; // Ball/ground average
        [SerializeField] PhysicMaterial groundMaterial;
        private Vector3 bounceVector;
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

            previousHorizontalCurve = horizontalCurve;
            previousVerticalCurve = verticalCurve;
            
            UpdateVectorFromBallToProjectionPoint();

            HandleInput();
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
                rb.velocity = new Vector3(Vo.x, Vo.y, Vo.z);
            }

            if (Input.GetButtonDown("Straight"))
            {

            }
            
            if (shoot && horizontalCurve != 0)
            {
                rb.AddForce(Vector3.Normalize(Vector3.Cross(vectorFromBallToProjectionPoint, Vector3.up)) * -horizontalCurve, ForceMode.Acceleration);
            }

            if (shoot && verticalCurve != 0)
            {
                rb.AddForce(Vector3.Normalize(vectorFromBallToProjectionPoint) * -verticalCurve, ForceMode.Acceleration);
            }
        }

        void FindInitialAngles()
        {
            if (!shoot)
            {
                float magnitudeOfVectorXZ = Mathf.Sqrt(Mathf.Pow(vectorFromBallToProjectionPoint.x, 2) + Mathf.Pow(vectorFromBallToProjectionPoint.z, 2));

                // Angles are in radians.

                // Angle between the XZ axis and the Y axis.
                angleTheta = (Mathf.PI / 2) - ((Mathf.Asin((magnitudeOfVectorXZ * -Physics.gravity.y) / Mathf.Pow(hit_power, 2))) / 2);

                // Angle between the X and Z axis.
                anglePhi = Mathf.Abs(Mathf.Atan(vectorFromBallToProjectionPoint.z / vectorFromBallToProjectionPoint.x));
            }
        }

        void FindInitialVector()
        {
            Vo.x = hit_power * Mathf.Cos(angleTheta) * Mathf.Cos(anglePhi);
            Vo.y = hit_power * Mathf.Sin(angleTheta);
            Vo.z = hit_power * Mathf.Cos(angleTheta) * Mathf.Sin(anglePhi);

            Vo.x = vectorFromBallToProjectionPoint.x < 0 ? -Vo.x : Vo.x;
            Vo.z = vectorFromBallToProjectionPoint.z < 0 ? -Vo.z : Vo.z;
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

            else if (verticalCurve != 0.0f)
            {
                finalX = (Vo.x * time);
                finalZ = (Vo.z * time);

                finalX = vectorFromBallToProjectionPoint.x < 0 ? finalX - (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time, 2)) : finalX + (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time, 2));
                finalZ = vectorFromBallToProjectionPoint.z < 0 ? finalZ - (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time, 2)) : finalZ + (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time, 2));
            }

            else
            {
                if ((horizontalCurve == 0.0f && previousHorizontalCurve != 0.0f) || (verticalCurve == 0.0f && previousVerticalCurve != 0.0f))
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
            if (horizontalCurve == 0.0f && verticalCurve == 0.0f)
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
                initialVelocity.x = dynamicFrictionValueBG != 0f ? Vo.x * DUFRESNE_CONSTANT : Vo.x;
                initialVelocity.y = Vo.y * bounceValueBG;
                initialVelocity.z = dynamicFrictionValueBG != 0f ? Vo.z * DUFRESNE_CONSTANT : Vo.z;
                time2 = time * bounceValueBG;

                if (verticalCurve != 0.0f)
                {
                    if (vectorFromBallToProjectionPoint.x < 0)
                    {
                        initialVelocity.x = dynamicFrictionValueBG != 0f ? (Vo.x - Mathf.Cos(anglePhi) * (-verticalCurve * time)) * DUFRESNE_CONSTANT : (Vo.x - Mathf.Cos(anglePhi) * (-verticalCurve * time));
                    }
                    else
                    {
                        initialVelocity.x = dynamicFrictionValueBG != 0f ? (Vo.x - Mathf.Cos(anglePhi) * (verticalCurve * time)) * DUFRESNE_CONSTANT : (Vo.x - Mathf.Cos(anglePhi) * (verticalCurve * time));
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        initialVelocity.z = dynamicFrictionValueBG != 0f ? (Vo.z - Mathf.Sin(anglePhi) * (-verticalCurve * time)) * DUFRESNE_CONSTANT : (Vo.z - Mathf.Sin(anglePhi) * (-verticalCurve * time));
                    }
                    else
                    {
                        initialVelocity.z = dynamicFrictionValueBG != 0f ? (Vo.z - Mathf.Sin(anglePhi) * (verticalCurve * time)) * DUFRESNE_CONSTANT : (Vo.z - Mathf.Sin(anglePhi) * (verticalCurve * time));
                    }
                    
                }

                bounceVector.x = endPointProjection.transform.position.x + (initialVelocity.x * time2);
                bounceVector.y = endPointProjection.transform.position.y + (initialVelocity.y * time2) - (-Physics.gravity.y/2 * Mathf.Pow(time2,2));
                bounceVector.z = endPointProjection.transform.position.z + (initialVelocity.z * time2);

                if (verticalCurve != 0.0f)
                {
                    if (vectorFromBallToProjectionPoint.x < 0)
                    {
                        bounceVector.x = bounceVector.x - (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time2, 2));
                    }
                    else
                    {
                        bounceVector.x = bounceVector.x + (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time2, 2));
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        bounceVector.z = bounceVector.z - (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time2, 2));
                    }
                    else
                    {
                        bounceVector.z = bounceVector.z + (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time2, 2));
                    }
                }

                bouncePointProjection.transform.position = new Vector3(bounceVector.x, bounceVector.y, bounceVector.z);
            }
        }
    }
}
