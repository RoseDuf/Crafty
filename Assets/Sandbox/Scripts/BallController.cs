using System;
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
        private Vector3 Vo;
        private Vector3 VoBounce;
        private float time;
        private float time2;
        private Vector3 vectorFromBallToProjectionPoint;

        private float previousHorizontalCurve;
        private float previousVerticalCurve;
        private float finalZ; // for horizontal curve
        private float finalX; // for horizontal curve

        private float bounceValueBG; // Ball/ground average for bounce
        private float dynamicFrictionValueBG; // Ball/ground average for bounce
        [SerializeField] PhysicMaterial groundMaterial;
        private Vector3 bounceCoordinates;
        private const float DUFRESNE_CONSTANT = 0.65f; // Mystery friction value that we don't seem to have access to anywhere, so I had to find it myself.

        // Shooting options
        enum ShootingOptions { Straight, Lob};
        private ShootingOptions option;

        float timer;

        bool shoot;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            vectorFromBallToProjectionPoint = endPointProjection.transform.position;
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

            timer = 0.0f;
        }

        private void FixedUpdate()
        {
            FindInitialAngles();

            if (angleTheta >= 0.785398f)
            {
                FindTotalTime();
                FindFinalXZForCurve();
                PredictFirstBouncePosition();

                previousHorizontalCurve = horizontalCurve;
                previousVerticalCurve = verticalCurve;

                HandleInput();
            }

            UpdateVectorFromBallToProjectionPoint();
        }

        // Update is called once per frame
        void Update()
        {
            if (option == ShootingOptions.Straight)
            {
                bouncePointProjection.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                bouncePointProjection.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        void HandleInput()
        {
            FindInitialVector();

            if (Input.GetButtonDown("OptionLob") && !shoot && option != ShootingOptions.Lob)
            {
                option = ShootingOptions.Lob;
                bouncePointProjection.GetComponent<MeshRenderer>().enabled = true;
            }

            if (Input.GetButtonDown("OptionStraight") && !shoot && option != ShootingOptions.Straight)
            {
                option = ShootingOptions.Straight;
                bouncePointProjection.GetComponent<MeshRenderer>().enabled = false;
            }

            if (Input.GetButtonDown("Lob") && !shoot)
            {
                option = ShootingOptions.Lob;
                FindInitialVector();
                shoot = true; 
                rb.velocity = new Vector3(Vo.x, Vo.y, Vo.z);
            }

            if (Input.GetButtonDown("Straight") && !shoot)
            {
                option = ShootingOptions.Straight;
                FindInitialVector();
                shoot = true;
                rb.velocity = new Vector3(Vo.x, 0.0f, Vo.z);
            }
            
            if (shoot && horizontalCurve != 0)
            {
                timer += Time.deltaTime;

                if (timer <= time * 0.8f)
                {
                    rb.AddForce(Vector3.Normalize(Vector3.Cross(vectorFromBallToProjectionPoint, Vector3.up)) * -horizontalCurve, ForceMode.Acceleration);
                }
            }

            if (shoot && verticalCurve != 0)
            {
                timer += Time.deltaTime;

                if (timer <= time * 0.8f)
                {
                    rb.AddForce(Vector3.Normalize(vectorFromBallToProjectionPoint) * -verticalCurve, ForceMode.Acceleration);
                }
            }
        }

        void FindInitialAngles()
        {
            if (!shoot)
            {
                float magnitudeOfVectorXZ = Mathf.Sqrt(Mathf.Pow(vectorFromBallToProjectionPoint.x, 2) + Mathf.Pow(vectorFromBallToProjectionPoint.z, 2));

                // Angles are in radians.

                // Angle between the XZ axis and the Y axis.
                float g = -Physics.gravity.y;
                float a = Mathf.Pow(magnitudeOfVectorXZ, 2) * g / Mathf.Pow(hit_power, 2);
                float b = (2 * vectorFromBallToProjectionPoint.y) + a;
                float c = 2 * magnitudeOfVectorXZ;

                //angleTheta = (Mathf.PI / 2) - ((Mathf.Asin((magnitudeOfVectorXZ * -Physics.gravity.y) / Mathf.Pow(hit_power, 2))) / 2);
                angleTheta = -(Mathf.Atan( (-c - Mathf.Sqrt(Mathf.Pow(c, 2) - (4 * a * b)) ) / (2 * a)));

                // Angle between the X and Z axis.
                anglePhi = Mathf.Abs(Mathf.Atan(vectorFromBallToProjectionPoint.z / vectorFromBallToProjectionPoint.x));

                if (Double.IsNaN(angleTheta))
                {
                    endPointProjection.GetComponent<MeshRenderer>().enabled = false;
                    bouncePointProjection.GetComponent<MeshRenderer>().enabled = false;
                }
                else
                {
                    endPointProjection.GetComponent<MeshRenderer>().enabled = true;
                    bouncePointProjection.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }

        void FindInitialVector()
        {
            Vo.x = hit_power * Mathf.Cos(angleTheta) * Mathf.Cos(anglePhi);
            Vo.y = hit_power * Mathf.Sin(angleTheta);
            Vo.z = hit_power * Mathf.Cos(angleTheta) * Mathf.Sin(anglePhi);

            Vo.x = vectorFromBallToProjectionPoint.x < 0 ? -Vo.x : Vo.x;
            Vo.z = vectorFromBallToProjectionPoint.z < 0 ? -Vo.z : Vo.z;

            float frictionValue;
            
            if (option == ShootingOptions.Straight)
            {
                frictionValue = DUFRESNE_CONSTANT;
            }
            else
            {
                frictionValue = 1f;
            }

            Vo.x /= frictionValue;
            Vo.z /= frictionValue;
        }

        void FindTotalTime()
        {
            if (!shoot)
            {
                //without change in y
                //time = (2 / -Physics.gravity.y) * hit_power * Mathf.Sin(angleTheta);

                time = (-hit_power * Mathf.Sin(angleTheta) - Mathf.Sqrt(Mathf.Pow(hit_power * Mathf.Sin(angleTheta), 2) - (4 * (vectorFromBallToProjectionPoint.y - transform.position.y) * -Physics.gravity.y/2))) / Physics.gravity.y;
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
            if (endPointProjection.GetComponent<ProjectionController>().angle != 0.0f)
            {
                bouncePointProjection.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                bouncePointProjection.GetComponent<MeshRenderer>().enabled = true;
            }

            if (!shoot)
            {
                VoBounce.x = dynamicFrictionValueBG != 0f ? Vo.x * DUFRESNE_CONSTANT : Vo.x;
                VoBounce.z = dynamicFrictionValueBG != 0f ? Vo.z * DUFRESNE_CONSTANT : Vo.z;
                VoBounce.y = Vo.y * bounceValueBG;
                time2 = time * bounceValueBG;

                if (verticalCurve != 0.0f)
                {
                    if (vectorFromBallToProjectionPoint.x < 0)
                    {
                        VoBounce.x = dynamicFrictionValueBG != 0f ? (Vo.x - Mathf.Cos(anglePhi) * (-verticalCurve * time)) * DUFRESNE_CONSTANT : (Vo.x - Mathf.Cos(anglePhi) * (-verticalCurve * time));
                    }
                    else
                    {
                        VoBounce.x = dynamicFrictionValueBG != 0f ? (Vo.x - Mathf.Cos(anglePhi) * (verticalCurve * time)) * DUFRESNE_CONSTANT : (Vo.x - Mathf.Cos(anglePhi) * (verticalCurve * time));
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        VoBounce.z = dynamicFrictionValueBG != 0f ? (Vo.z - Mathf.Sin(anglePhi) * (-verticalCurve * time)) * DUFRESNE_CONSTANT : (Vo.z - Mathf.Sin(anglePhi) * (-verticalCurve * time));
                    }
                    else
                    {
                        VoBounce.z = dynamicFrictionValueBG != 0f ? (Vo.z - Mathf.Sin(anglePhi) * (verticalCurve * time)) * DUFRESNE_CONSTANT : (Vo.z - Mathf.Sin(anglePhi) * (verticalCurve * time));
                    }
                    
                }

                bounceCoordinates.x = endPointProjection.transform.position.x + (VoBounce.x * time2);
                bounceCoordinates.y = endPointProjection.transform.position.y + (VoBounce.y * time2) - (-Physics.gravity.y/2 * Mathf.Pow(time2,2));
                bounceCoordinates.z = endPointProjection.transform.position.z + (VoBounce.z * time2);

                if (verticalCurve != 0.0f)
                {
                    if (vectorFromBallToProjectionPoint.x < 0)
                    {
                        bounceCoordinates.x = bounceCoordinates.x - (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time2, 2));
                    }
                    else
                    {
                        bounceCoordinates.x = bounceCoordinates.x + (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time2, 2));
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        bounceCoordinates.z = bounceCoordinates.z - (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time2, 2));
                    }
                    else
                    {
                        bounceCoordinates.z = bounceCoordinates.z + (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(time2, 2));
                    }
                }

                bouncePointProjection.transform.position = new Vector3(bounceCoordinates.x, bouncePointProjection.transform.position.y, bounceCoordinates.z);
            }
        }
    }
}
