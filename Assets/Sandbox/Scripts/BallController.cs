using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BallController
{

    public class BallController : MonoBehaviour
    {

        [SerializeField] float hit_power;
        [SerializeField] GameObject endPointProjectionBall;
        [SerializeField] GameObject bouncePointProjectionBall;
        private GameObject endPointProjection;
        private GameObject bouncePointProjection;
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
        private const float DUFRESNE_CONSTANT = 0.67f; // Mystery friction value that we don't seem to have access to anywhere, so I had to find it myself.

        // Shooting options
        enum ShootingOptions { Straight, Lob };
        private ShootingOptions option;

        [SerializeField] GameObject pointProjectionBall;
        [SerializeField] GameObject bounceProjectionBall;
        private List<GameObject> pointProjectionArrow;
        private List<GameObject> bounceProjectionArrow;
        private int numberOfBallsForPoint;
        private int numberOfBallsForBounce;

        float timer;

        bool shoot;

        private void Awake()
        {
            endPointProjection = Instantiate(endPointProjectionBall, new Vector3(0, 0, 0), Quaternion.identity);
            bouncePointProjection = Instantiate(bouncePointProjectionBall, new Vector3(0, 0, 0), Quaternion.identity);

            rb = GetComponent<Rigidbody>();
            vectorFromBallToProjectionPoint = endPointProjection.transform.position;

            float bounceValueBall = GetComponent<Collider>().material.bounciness;
            float dynamicFrictionValueBall = GetComponent<Collider>().material.dynamicFriction;
            float bounceValueGround = groundMaterial.bounciness;
            float dynamicFrictionValueGround = groundMaterial.dynamicFriction;

            // Average bounce and friction values
            bounceValueBG = (bounceValueBall + bounceValueGround) / 2;
            dynamicFrictionValueBG = (dynamicFrictionValueBall + dynamicFrictionValueGround) / 2;
        }

        // Start is called before the first frame update
        void Start()
        {
            shoot = false;
            timer = 0.0f;

            FindInitialAngles();

            if (angleTheta >= 0.785398f)
            {
                FindTotalTime();
                PredictFirstBouncePosition();
            }

            int arrowBalls = 0;
            pointProjectionArrow = new List<GameObject>();
            bounceProjectionArrow = new List<GameObject>();

            for (float i = time / 8f; i < time; i += time/8f)
            {
                pointProjectionArrow.Add(Instantiate(pointProjectionBall, new Vector3(0, 0, 0), Quaternion.identity));
                arrowBalls++;
            }

            numberOfBallsForPoint = arrowBalls;

            arrowBalls = 0;
            for (float i = time2 / 6f; i < time2; i += time2 / 6f)
            {
                bounceProjectionArrow.Add(Instantiate(bounceProjectionBall, new Vector3(0, 0, 0), Quaternion.identity));
                arrowBalls++;
            }

            numberOfBallsForBounce = arrowBalls;
        }

        private void FixedUpdate()
        {
            FindInitialAngles();

            if (angleTheta >= 0.785398f)
            {
                FindTotalTime();
                FindFinalXZForCurve();
                DrawArrowPoint();
                DrawArrowBounce();

                PredictFirstBouncePosition();

                previousHorizontalCurve = horizontalCurve;
                previousVerticalCurve = verticalCurve;

                HandleExteriorForces();
            }

            UpdateNewProjectionCoordinates();
            UpdateVectorFromBallToProjectionPoint();
        }

        // Update is called once per frame
        void Update()
        {
            if (Double.IsNaN(angleTheta) || shoot)
            {
                endPointProjection.GetComponent<MeshRenderer>().enabled = false;
                if (option == ShootingOptions.Lob)
                {
                    bouncePointProjection.GetComponent<MeshRenderer>().enabled = false;
                }
            }
            else
            {
                endPointProjection.GetComponent<MeshRenderer>().enabled = true;
                if (option == ShootingOptions.Lob)
                {
                    bouncePointProjection.GetComponent<MeshRenderer>().enabled = true;
                }

                if (option == ShootingOptions.Straight)
                {
                    bouncePointProjection.GetComponent<MeshRenderer>().enabled = false;
                }
                else
                {
                    if (endPointProjection.GetComponent<ProjectionController>().angle != 0.0f)
                    {
                        bouncePointProjection.GetComponent<MeshRenderer>().enabled = false;
                    }
                    else
                    {
                        bouncePointProjection.GetComponent<MeshRenderer>().enabled = true;
                    }
                }
            }

            if (angleTheta >= 0.785398f)
            {
                HandleInput();
            }
        }

        void HandleInput()
        {
            FindInitialVector();

            if (Input.GetButtonDown("Options") && !shoot)
            {
                option = option != ShootingOptions.Lob ? ShootingOptions.Lob : ShootingOptions.Straight;
                bouncePointProjection.GetComponent<MeshRenderer>().enabled = !bouncePointProjection.GetComponent<MeshRenderer>().enabled;
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
        }

        void HandleExteriorForces()
        {
            if (shoot)
            {
                timer += Time.deltaTime;

                if (timer <= time + time2)
                {
                    if (horizontalCurve != 0)
                    {
                        rb.AddForce(Vector3.Normalize(Vector3.Cross(vectorFromBallToProjectionPoint, Vector3.up)) * -horizontalCurve, ForceMode.Acceleration);
                    }
                    if (verticalCurve != 0)
                    {
                        rb.AddForce(Vector3.Normalize(vectorFromBallToProjectionPoint) * -verticalCurve, ForceMode.Acceleration);
                    }
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
                angleTheta = -(Mathf.Atan((-c - Mathf.Sqrt(Mathf.Pow(c, 2) - (4 * a * b))) / (2 * a)));

                // Angle between the X and Z axis.
                anglePhi = Mathf.Abs(Mathf.Atan(vectorFromBallToProjectionPoint.z / vectorFromBallToProjectionPoint.x));

                if (magnitudeOfVectorXZ == 0f)
                {
                    angleTheta = Mathf.PI / 2;
                    anglePhi = 0f;
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

            if (option == ShootingOptions.Straight)
            {
                Vo.x /= DUFRESNE_CONSTANT;
                Vo.z /= DUFRESNE_CONSTANT;
            }
            else
            {
                Vo.x /= 1f;
                Vo.z /= 1f;
            }
        }

        void FindTotalTime()
        {
            if (!shoot)
            {
                //without change in y
                //time = (2 / -Physics.gravity.y) * hit_power * Mathf.Sin(angleTheta);

                time = (-hit_power * Mathf.Sin(angleTheta) - Mathf.Sqrt(Mathf.Pow(hit_power * Mathf.Sin(angleTheta), 2) - (4 * (vectorFromBallToProjectionPoint.y - transform.position.y) * -Physics.gravity.y / 2))) / Physics.gravity.y;
            }
        }

        void FindFinalXZForCurve()
        {
            if (!shoot)
            {
                finalX = vectorFromBallToProjectionPoint.x;
                finalZ = vectorFromBallToProjectionPoint.z;

                float frictionForce = dynamicFrictionValueBG != 0.0f && option == ShootingOptions.Straight ? frictionForce = 1f + DUFRESNE_CONSTANT : frictionForce = 0f;

                if (verticalCurve != 0.0f)
                {
                    float accelerationX = (Mathf.Cos(anglePhi) * ((-verticalCurve - frictionForce) / rb.mass) / 2 * Mathf.Pow(time, 2));
                    float accelerationZ = (Mathf.Sin(anglePhi) * ((-verticalCurve - frictionForce) / rb.mass) / 2 * Mathf.Pow(time, 2));

                    if (vectorFromBallToProjectionPoint.x < 0)
                    {
                        finalX = transform.position.x + (Vo.x * time) - accelerationX;
                    }
                    else
                    {
                        finalX = transform.position.x + (Vo.x * time) + accelerationX;
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        finalZ = transform.position.z + (Vo.z * time) - accelerationZ;
                    }
                    else
                    {
                        finalZ = transform.position.z + (Vo.z * time) + accelerationZ;
                    }
                }

                if (horizontalCurve != 0.0f && option == ShootingOptions.Straight)
                {
                    // Need this for when vectorFromBallToProjectionPoint is diagonal.
                    float hypothenuse = (((-horizontalCurve / rb.mass) / 2) * Mathf.Pow(time, 2));

                    // Adjust to axis.
                    finalX = vectorFromBallToProjectionPoint.z < 0 ? finalX + (hypothenuse * Mathf.Sin(anglePhi)) : finalX - (hypothenuse * Mathf.Sin(anglePhi));
                    finalZ = vectorFromBallToProjectionPoint.x < 0 ? finalZ - (hypothenuse * Mathf.Cos(anglePhi)) : finalZ + (hypothenuse * Mathf.Cos(anglePhi));
                }

                if (verticalCurve == 0.0f && horizontalCurve == 0.0f)
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
                bounceCoordinates.y = endPointProjection.transform.position.y + (VoBounce.y * time2) - (-Physics.gravity.y / 2 * Mathf.Pow(time2, 2));
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

                bouncePointProjection.transform.position = new Vector3(bounceCoordinates.x, bounceCoordinates.y, bounceCoordinates.z);
            }
        }

        void UpdateNewProjectionCoordinates()
        {
            if (rb.velocity.magnitude == 0f && shoot)
            {
                endPointProjection.transform.position = transform.position;
                bouncePointProjection.transform.position = transform.position;
                shoot = false;
                timer = 0f;
            }
        }

        void DrawArrowPoint()
        {
            float frictionForce = dynamicFrictionValueBG != 0.0f && option == ShootingOptions.Straight ? frictionForce = 1f + DUFRESNE_CONSTANT : frictionForce = 0f;

            for (int arrowBalls = 0; arrowBalls < numberOfBallsForPoint; arrowBalls++)
            {
                Vector3 arrowBallPosition = pointProjectionArrow[arrowBalls].transform.position;
                float fractionOfTime = (time / 8f) * (arrowBalls + 1);

                if (verticalCurve != 0.0f)
                {
                    float accelerationX = (Mathf.Cos(anglePhi) * ((-verticalCurve - frictionForce) / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));
                    float accelerationZ = (Mathf.Sin(anglePhi) * ((-verticalCurve - frictionForce) / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));

                    if (vectorFromBallToProjectionPoint.x < 0)
                    {
                        arrowBallPosition.x = transform.position.x + (Vo.x * fractionOfTime) - accelerationX;
                    }
                    else
                    {
                        arrowBallPosition.x = transform.position.x + (Vo.x * fractionOfTime) + accelerationX;
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        arrowBallPosition.z = transform.position.z + (Vo.x * fractionOfTime) - accelerationZ;
                    }
                    else
                    {
                        arrowBallPosition.z = transform.position.z + (Vo.x * fractionOfTime) - accelerationZ;
                    }
                }

                if (horizontalCurve != 0.0f && option == ShootingOptions.Straight)
                {
                    // Need this for when vectorFromBallToProjectionPoint is diagonal.
                    float hypothenuse = (((-horizontalCurve / rb.mass) / 2) * Mathf.Pow(fractionOfTime, 2));

                    // Adjust to axis.
                    arrowBallPosition.x = vectorFromBallToProjectionPoint.z < 0 ? arrowBallPosition.x + (hypothenuse * Mathf.Sin(anglePhi)) : arrowBallPosition.x - (hypothenuse * Mathf.Sin(anglePhi));
                    arrowBallPosition.z = vectorFromBallToProjectionPoint.x < 0 ? arrowBallPosition.z - (hypothenuse * Mathf.Cos(anglePhi)) : arrowBallPosition.z + (hypothenuse * Mathf.Cos(anglePhi));
                }

                if (verticalCurve == 0.0f && horizontalCurve == 0.0f)
                {
                    arrowBallPosition.x = transform.position.x + (Vo.x * fractionOfTime);
                    arrowBallPosition.z = transform.position.z + (Vo.z * fractionOfTime);
                }

                if (option == ShootingOptions.Lob)
                {
                    arrowBallPosition.y = transform.position.y + (Vo.y * fractionOfTime) + (Physics.gravity.y / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2);
                }
                else
                {
                    arrowBallPosition.y = endPointProjection.transform.position.y;
                }

                pointProjectionArrow[arrowBalls].transform.position = new Vector3(arrowBallPosition.x, arrowBallPosition.y, arrowBallPosition.z);
            }
        }

        void DrawArrowBounce()
        {
            for (int arrowBalls = 0; arrowBalls < numberOfBallsForBounce; arrowBalls++)
            {
                Vector3 arrowBallPosition = pointProjectionArrow[arrowBalls].transform.position;
                float fractionOfTime = (time2 / 6f) * (arrowBalls + 1);

                arrowBallPosition.x = endPointProjection.transform.position.x + (VoBounce.x * fractionOfTime);
                arrowBallPosition.y = endPointProjection.transform.position.y + (VoBounce.y * fractionOfTime) - (-Physics.gravity.y / 2 * Mathf.Pow(fractionOfTime, 2));
                arrowBallPosition.z = endPointProjection.transform.position.z + (VoBounce.z * fractionOfTime);

                if (verticalCurve != 0.0f)
                {
                    if (vectorFromBallToProjectionPoint.x < 0)
                    {
                        arrowBallPosition.x = bounceCoordinates.x - (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));
                    }
                    else
                    {
                        arrowBallPosition.x = bounceCoordinates.x + (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        arrowBallPosition.z = bounceCoordinates.z - (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));
                    }
                    else
                    {
                        arrowBallPosition.z = bounceCoordinates.z + (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));
                    }
                }

                bounceProjectionArrow[arrowBalls].transform.position = new Vector3(arrowBallPosition.x, arrowBallPosition.y, arrowBallPosition.z);
            }
        }
    }
}
