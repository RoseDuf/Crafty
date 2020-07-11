using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player
{

    public class BallController : MonoBehaviour
    {

        [SerializeField] float hit_power;
        [SerializeField] GameObject endPointProjection;
        [SerializeField] GameObject bouncePointProjection;
        [SerializeField] float horizontalCurve; //temporaty, will get this from meters later
        [SerializeField] float verticalCurve; //temporaty, will get this from meters later

        private Rigidbody rb;
        private float angleTheta; //for x, y, z vector
        private float anglePhi; // for x, z vector
        private Vector3 Vo;
        private Vector3 VoBounce;
        private float time;
        private float time2;
        private float timeForBounce;
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
        private enum ShootingOptions { Straight, Lob};
        private ShootingOptions option { get; set; }

        [SerializeField] GameObject pointProjectionBall;
        [SerializeField] GameObject bounceProjectionBall;
        private List<GameObject> pointProjectionArrow;
        private List<GameObject> bounceProjectionArrow;
        [SerializeField] private int numberOfBallsForPoint;
        [SerializeField] private int numberOfBallsForBounce;

        private float timer;

        public bool shoot { get; set; }

        private void Awake()
        {
            option = ShootingOptions.Straight;

            rb = GetComponent<Rigidbody>();
            vectorFromBallToProjectionPoint = endPointProjection.transform.localPosition;

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
            FindTotalTime();

            if (angleTheta >= 0.785398f)
            {
                PredictFirstBouncePosition();
            }

            pointProjectionArrow = new List<GameObject>();
            bounceProjectionArrow = new List<GameObject>();
            numberOfBallsForPoint -= 1;
            numberOfBallsForBounce -= 1;

            for (float i = 0; i < numberOfBallsForPoint; i ++)
            {
                pointProjectionArrow.Add(Instantiate(pointProjectionBall, new Vector3(0, 0, 0), Quaternion.identity));
            }

            for (float i = 0; i < numberOfBallsForBounce; i ++)
            {
                bounceProjectionArrow.Add(Instantiate(bounceProjectionBall, new Vector3(0, 0, 0), Quaternion.identity));
            }
        }

        private void FixedUpdate()
        {
            FindInitialAngles();
            FindTotalTime();

            FindFinalXZForCurve();

            if (!Double.IsNaN(angleTheta))
            {
                PredictFirstBouncePosition();

                previousHorizontalCurve = horizontalCurve;
                previousVerticalCurve = verticalCurve;

                HandleExteriorForces();

                UpdateNewProjectionCoordinates();

                if (time != 0f && !shoot)
                {
                    DrawArrowPoint();
                }
                if (timeForBounce != 0 && !shoot)
                {
                    DrawArrowBounce();
                }
            }

            UpdateVectorFromBallToProjectionPoint();
        }

        // Update is called once per frame
        void Update()
        {
            HideProjections();
            HandleInput();
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

                if (timer <= time + timeForBounce)
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

                if (option == ShootingOptions.Straight)
                {
                    if (magnitudeOfVectorXZ == 0f)
                    {
                        angleTheta = 0f;
                    }
                    else
                    {
                        angleTheta = Mathf.Atan(vectorFromBallToProjectionPoint.y / magnitudeOfVectorXZ);
                    }
                }
            }
        }

        void FindInitialVector()
        {
            Vo.x = hit_power * Mathf.Cos(angleTheta) * Mathf.Cos(anglePhi);
            Vo.y = hit_power * Mathf.Sin(angleTheta);
            Vo.z = hit_power * Mathf.Cos(angleTheta) * Mathf.Sin(anglePhi);

            if (option == ShootingOptions.Straight)
            {
                float magnitudeOfVectorXZ = Mathf.Sqrt(Mathf.Pow(vectorFromBallToProjectionPoint.x, 2) + Mathf.Pow(vectorFromBallToProjectionPoint.z, 2));
                Vo.x = magnitudeOfVectorXZ * Mathf.Cos(angleTheta) * Mathf.Cos(anglePhi);
                Vo.z = magnitudeOfVectorXZ * Mathf.Cos(angleTheta) * Mathf.Sin(anglePhi);
            }

            Vo.x = vectorFromBallToProjectionPoint.x < 0 ? -Vo.x : Vo.x;
            Vo.z = vectorFromBallToProjectionPoint.z < 0 ? -Vo.z : Vo.z;
        }

        void FindTotalTime()
        {
            if (!shoot)
            {
                //without change in y
                //time = (2 / -Physics.gravity.y) * hit_power * Mathf.Sin(angleTheta);
                if (option == ShootingOptions.Lob)
                {
                    time = (-hit_power * Mathf.Sin(angleTheta) - Mathf.Sqrt(Mathf.Pow(hit_power * Mathf.Sin(angleTheta), 2) - (4 * (vectorFromBallToProjectionPoint.y) * -Physics.gravity.y / 2))) / Physics.gravity.y;
                    time2 = (-hit_power * Mathf.Sin(angleTheta) + Mathf.Sqrt(Mathf.Pow(hit_power * Mathf.Sin(angleTheta), 2) - (4 * (vectorFromBallToProjectionPoint.y) * -Physics.gravity.y / 2))) / Physics.gravity.y;
                }
                else
                {
                    float magnitudeOfVectorXZ = Mathf.Sqrt(Mathf.Pow(Vo.x, 2) + Mathf.Pow(Vo.z, 2));
                    float frictionForce = dynamicFrictionValueBG != 0.0f && option == ShootingOptions.Straight ? frictionForce = 0.31f + DUFRESNE_CONSTANT : frictionForce = 1f;

                    time = magnitudeOfVectorXZ / frictionForce;
                }
            }
        }

        void UpdateVectorFromBallToProjectionPoint()
        {
            if (horizontalCurve == 0.0f && verticalCurve == 0.0f && !shoot)
            {
                vectorFromBallToProjectionPoint = endPointProjection.transform.localPosition - transform.localPosition;
            }
        }

        void CorrectProjectionPosition()
        {
            endPointProjection.transform.localPosition = new Vector3(finalX, endPointProjection.transform.localPosition.y, finalZ);
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
                        finalX = transform.localPosition.x + (Vo.x * time) - accelerationX;
                    }
                    else
                    {
                        finalX = transform.localPosition.x + (Vo.x * time) + accelerationX;
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        finalZ = transform.localPosition.z + (Vo.z * time) - accelerationZ;
                    }
                    else
                    {
                        finalZ = transform.localPosition.z + (Vo.z * time) + accelerationZ;
                    }
                }

                if (horizontalCurve != 0.0f && option == ShootingOptions.Straight)
                {
                    // Need this for when vectorFromBallToProjectionPoint is diagonal.
                    float hypothenuse = (((-horizontalCurve / rb.mass) / 2) * Mathf.Pow(time, 2));

                    // Adjust to axis.
                    finalX = vectorFromBallToProjectionPoint.z < 0 ? finalX + (Vo.x * time) + (hypothenuse * Mathf.Sin(anglePhi)) : finalX + (Vo.x * time) - (hypothenuse * Mathf.Sin(anglePhi));
                    finalZ = vectorFromBallToProjectionPoint.x < 0 ? finalZ + (Vo.x * time) - (hypothenuse * Mathf.Cos(anglePhi)) : finalZ + (Vo.x * time) + (hypothenuse * Mathf.Cos(anglePhi));
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
                        finalX = endPointProjection.transform.localPosition.x;
                        finalZ = endPointProjection.transform.localPosition.z;
                    }
                }

                CorrectProjectionPosition();
            }
        }

        void PredictFirstBouncePosition()
        {
            if (!shoot)
            {
                VoBounce.x = dynamicFrictionValueBG != 0f ? Vo.x * DUFRESNE_CONSTANT : Vo.x;
                VoBounce.z = dynamicFrictionValueBG != 0f ? Vo.z * DUFRESNE_CONSTANT : Vo.z;
                VoBounce.y = Mathf.Sqrt(Mathf.Pow(Vo.y, 2) - (2* -Physics.gravity.y * vectorFromBallToProjectionPoint.y)) * bounceValueBG;
                timeForBounce = (time - time2) * bounceValueBG;

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

                bounceCoordinates.x = endPointProjection.transform.localPosition.x + (VoBounce.x * timeForBounce);
                bounceCoordinates.y = endPointProjection.transform.localPosition.y + (VoBounce.y * timeForBounce) - (-Physics.gravity.y / 2 * Mathf.Pow(timeForBounce, 2));
                bounceCoordinates.z = endPointProjection.transform.localPosition.z + (VoBounce.z * timeForBounce);

                if (verticalCurve != 0.0f)
                {
                    if (vectorFromBallToProjectionPoint.x < 0)
                    {
                        bounceCoordinates.x = bounceCoordinates.x - (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(timeForBounce, 2));
                    }
                    else
                    {
                        bounceCoordinates.x = bounceCoordinates.x + (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(timeForBounce, 2));
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        bounceCoordinates.z = bounceCoordinates.z - (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(timeForBounce, 2));
                    }
                    else
                    {
                        bounceCoordinates.z = bounceCoordinates.z + (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(timeForBounce, 2));
                    }
                }

                if (Double.IsNaN(bounceCoordinates.y))
                {
                    bounceCoordinates = transform.localPosition;
                }

                bouncePointProjection.transform.localPosition = new Vector3(bounceCoordinates.x, bounceCoordinates.y, bounceCoordinates.z);
            }
        }

        void UpdateNewProjectionCoordinates()
        {
            if (rb.velocity.magnitude == 0f && shoot)
            {
                endPointProjection.transform.localPosition = transform.localPosition;
                bouncePointProjection.transform.localPosition = transform.localPosition;
                for (int arrowBalls = 0; arrowBalls < numberOfBallsForPoint; arrowBalls++)
                {
                    pointProjectionArrow[arrowBalls].transform.localPosition = transform.localPosition;
                }
                for (int arrowBalls = 0; arrowBalls < numberOfBallsForBounce; arrowBalls++)
                {
                    bounceProjectionArrow[arrowBalls].transform.localPosition = transform.localPosition;
                }
                shoot = false;
                timer = 0f;
                time = 0f;
                time2 = 0f;
            }
        }

        void DrawArrowPoint()
        {
            float frictionForce = dynamicFrictionValueBG != 0.0f && option == ShootingOptions.Straight ? frictionForce = 1f + DUFRESNE_CONSTANT : frictionForce = 0f;

            for (int arrowBalls = 0; arrowBalls < numberOfBallsForPoint; arrowBalls++)
            {
                Vector3 arrowBallPosition = pointProjectionArrow[arrowBalls].transform.position;
                float fractionOfTime = (time / 8f) * (arrowBalls + 1);

                arrowBallPosition.x = vectorFromBallToProjectionPoint.x * fractionOfTime / time;
                arrowBallPosition.z = vectorFromBallToProjectionPoint.z * fractionOfTime / time;

                if (verticalCurve != 0.0f)
                {
                    float accelerationX = (Mathf.Cos(anglePhi) * ((-verticalCurve - frictionForce) / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));
                    float accelerationZ = (Mathf.Sin(anglePhi) * ((-verticalCurve - frictionForce) / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));

                    if (vectorFromBallToProjectionPoint.x < 0)
                    {
                        arrowBallPosition.x = (Vo.x * fractionOfTime) - accelerationX;
                    }
                    else
                    {
                        arrowBallPosition.x = (Vo.x * fractionOfTime) + accelerationX;
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        arrowBallPosition.z = (Vo.z * fractionOfTime) - accelerationZ;
                    }
                    else
                    {
                        arrowBallPosition.z = (Vo.z * fractionOfTime) + accelerationZ;
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
                    arrowBallPosition.x = (endPointProjection.transform.position.x - transform.position.x) * fractionOfTime / time;
                    arrowBallPosition.z = (endPointProjection.transform.position.z - transform.position.z) * fractionOfTime / time;
                }

                if (option == ShootingOptions.Lob)
                {
                    arrowBallPosition.y = transform.position.y + (Vo.y * fractionOfTime) + (Physics.gravity.y / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2);
                }
                else
                {
                    float magnitudeOfVectorXZ = Mathf.Sqrt(Mathf.Pow(vectorFromBallToProjectionPoint.x, 2) + Mathf.Pow(vectorFromBallToProjectionPoint.z, 2));

                    arrowBallPosition.y = vectorFromBallToProjectionPoint.y / magnitudeOfVectorXZ * fractionOfTime;
                }

                if (rb.velocity.magnitude == 0f && shoot)
                {
                    arrowBallPosition = transform.position;
                }

                pointProjectionArrow[arrowBalls].transform.position = new Vector3(arrowBallPosition.x, arrowBallPosition.y, arrowBallPosition.z) + transform.position;
            }
        }

        void DrawArrowBounce()
        {
            for (int arrowBalls = 0; arrowBalls < numberOfBallsForBounce; arrowBalls++)
            {
                Vector3 arrowBallPosition = pointProjectionArrow[arrowBalls].transform.position;
                float fractionOfTime = (timeForBounce / 6f) * (arrowBalls + 1);

                arrowBallPosition.x = endPointProjection.transform.position.x + (VoBounce.x * fractionOfTime);
                arrowBallPosition.y = endPointProjection.transform.position.y + (VoBounce.y * fractionOfTime) - (-Physics.gravity.y / 2 * Mathf.Pow(fractionOfTime, 2));
                arrowBallPosition.z = endPointProjection.transform.position.z + (VoBounce.z * fractionOfTime);

                if (verticalCurve != 0.0f)
                {
                    if (vectorFromBallToProjectionPoint.x < 0)
                    {
                        arrowBallPosition.x = arrowBallPosition.x - (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));
                    }
                    else
                    {
                        arrowBallPosition.x = arrowBallPosition.x + (Mathf.Cos(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));
                    }

                    if (vectorFromBallToProjectionPoint.z < 0)
                    {
                        arrowBallPosition.z = arrowBallPosition.z - (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));
                    }
                    else
                    {
                        arrowBallPosition.z = arrowBallPosition.z + (Mathf.Sin(anglePhi) * (-verticalCurve / rb.mass) / 2 * Mathf.Pow(fractionOfTime, 2));
                    }
                }

                if (Double.IsNaN(arrowBallPosition.y))
                {
                    arrowBallPosition = transform.position;
                }

                bounceProjectionArrow[arrowBalls].transform.position = new Vector3(arrowBallPosition.x, arrowBallPosition.y, arrowBallPosition.z);
            }
        }

        void HideProjections()
        {
            if (Double.IsNaN(angleTheta) || shoot || (option == ShootingOptions.Straight && Vo.magnitude >= hit_power))
            {
                //endPointProjection.GetComponent<MeshRenderer>().enabled = false;

                //for (int arrowBalls = 0; arrowBalls < numberOfBallsForPoint; arrowBalls++)
                //{
                //    pointProjectionArrow[arrowBalls].GetComponent<MeshRenderer>().enabled = false;
                //}

                //if (option == ShootingOptions.Lob)
                //{
                //    bouncePointProjection.GetComponent<MeshRenderer>().enabled = false;

                //    for (int arrowBalls = 0; arrowBalls < numberOfBallsForBounce; arrowBalls++)
                //    {
                //        bounceProjectionArrow[arrowBalls].GetComponent<MeshRenderer>().enabled = false;
                //    }
                //}
            }
            else
            {
                endPointProjection.GetComponent<MeshRenderer>().enabled = true;

                for (int arrowBalls = 0; arrowBalls < numberOfBallsForPoint; arrowBalls++)
                {
                    pointProjectionArrow[arrowBalls].GetComponent<MeshRenderer>().enabled = true;
                }

                if (option == ShootingOptions.Lob)
                {
                    if (endPointProjection.GetComponent<ProjectionController>().angle != 0.0f)
                    {
                        bouncePointProjection.GetComponent<MeshRenderer>().enabled = false;

                        for (int arrowBalls = 0; arrowBalls < numberOfBallsForBounce; arrowBalls++)
                        {
                            bounceProjectionArrow[arrowBalls].GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                    else
                    {
                        bouncePointProjection.GetComponent<MeshRenderer>().enabled = true;

                        for (int arrowBalls = 0; arrowBalls < numberOfBallsForBounce; arrowBalls++)
                        {
                            bounceProjectionArrow[arrowBalls].GetComponent<MeshRenderer>().enabled = true;
                        }
                    }
                }

                if (option == ShootingOptions.Straight)
                {
                    bouncePointProjection.GetComponent<MeshRenderer>().enabled = false;

                    for (int arrowBalls = 0; arrowBalls < numberOfBallsForBounce; arrowBalls++)
                    {
                        bounceProjectionArrow[arrowBalls].GetComponent<MeshRenderer>().enabled = false;
                    }
                }
            }
        }
    }
}
