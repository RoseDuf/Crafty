using UnityEngine;

using System.Collections;

namespace Game.Cameras
{
    public class GameplayCamera : MonoBehaviour
    {
        [Tooltip("The target the camera is interested in")]
        [SerializeField] private Transform target;

        [Range(0, 1)]
        [Tooltip("How fast the camera will snap to the target")]
        [SerializeField] private float smoothing = 0.125f;

        [Tooltip("The camera's offset relative to the target")]
        [SerializeField] private Vector3 offset = Vector3.zero;

        [Range(-80f, 80f)]
        [Tooltip("The minimum angle that the camera can move vertically")]
        [SerializeField] private float minAngleY = 0f;
        [Range(-80f, 80f)]
        [Tooltip("The maximum angle that the camera can move vertically")]
        [SerializeField] private float maxAngleY = 80f;

        [Tooltip("Whether or not to invert our Y axis for mouse input")]
        [SerializeField] private bool invertY = false;

        [Min(1f)]
        [Tooltip("How sensitive the camera is to movement")]
        [SerializeField] private float cameraSensitivity = 100f;

        [Header("UI")]
        [SerializeField] private GameObject ballCamImageObject;
        [SerializeField] private GameObject freeCamImageObject;

        private enum Mode { LockedOnTarget, Roaming }
        private Mode mode;

        private Vector2 mouseMovement;
        private Vector3 positionBeforeRoaming;
        private Vector2 rotation;

        private float maxDistanceToTarget;
        private float minDistanceToTarget;

        private void Awake()
        {
            mode = Mode.LockedOnTarget;
            mouseMovement = Vector2.zero;
            positionBeforeRoaming = Vector3.zero;
            rotation = Vector2.zero;

            maxDistanceToTarget = offset.magnitude;
            minDistanceToTarget = 0;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (mode == Mode.Roaming)
                {
                    // going from Mode.Roaming to Mode.LockedOnTarget
                    this.transform.position = positionBeforeRoaming;
                    this.transform.LookAt(target);

                    ChangeMode(Mode.LockedOnTarget);
                }
                else if (mode == Mode.LockedOnTarget)
                {
                    // going from Mode.LockedOnTarget to Mode.Roaming
                    positionBeforeRoaming = this.transform.position;
                    rotation.x = this.transform.localEulerAngles.y;
                    rotation.y = -this.transform.localEulerAngles.x;

                    ChangeMode(Mode.Roaming);
                }
            }

            if (mode == Mode.LockedOnTarget && target != null)
            {
                if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
                {
                    mouseMovement.x -= Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
                    mouseMovement.y += Input.GetAxis("Mouse Y") * (invertY ? -1 : 1) * cameraSensitivity * Time.deltaTime;
                }

                mouseMovement.x = Mathf.Abs(mouseMovement.x) > 360f ? 0 : mouseMovement.x;
                mouseMovement.y = Mathf.Clamp(mouseMovement.y, minAngleY, maxAngleY);

                Vector3 desiredPosition = target.position + Quaternion.Euler(mouseMovement.y, mouseMovement.x, 0) * offset;

                if (target != null && target.gameObject.layer != LayerMask.NameToLayer("Hole"))
                {
                    // object avoidance
                    if (Physics.Linecast(target.position, desiredPosition, out RaycastHit hit))
                    {
                        if (hit.transform.GetComponentInParent<CameraObstruction>() != null)
                        {
                            float distanceBias = 1e-3f;
                            if ((hit.distance * distanceBias) < maxDistanceToTarget && (hit.distance * distanceBias) > minDistanceToTarget)
                                desiredPosition = hit.point;
                        }
                    }
                }

                Vector3 smoothedPosition = Vector3.Lerp(this.transform.position, desiredPosition, smoothing);

                this.transform.position = smoothedPosition;
                this.transform.LookAt(target);
            }
            else if (mode == Mode.Roaming)
            {
                Vector3 desiredPosition = this.transform.position + GetInputTranslationDirection();
                Vector3 smoothedPosition = Vector3.Lerp(this.transform.position, desiredPosition, smoothing);

                this.transform.position = smoothedPosition;

                rotation.x -= Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
                rotation.y -= Input.GetAxis("Mouse Y") * (invertY ? -1 : 1) * cameraSensitivity * Time.deltaTime;

                rotation.x = Mathf.Abs(rotation.x) > 360f ? 0 : rotation.x;
                rotation.y = Mathf.Clamp(rotation.y, -90, 90);

                Quaternion desiredRotation = Quaternion.AngleAxis(rotation.x, Vector3.up);
                desiredRotation *= Quaternion.AngleAxis(rotation.y, Vector3.left);
                Quaternion smoothedRotation = Quaternion.Lerp(this.transform.localRotation, desiredRotation, smoothing);

                this.transform.localRotation = smoothedRotation;
            }
        }

        private Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
                direction += this.transform.forward;

            if (Input.GetKey(KeyCode.S))
                direction -= this.transform.forward;

            if (Input.GetKey(KeyCode.A))
                direction -= this.transform.right;

            if (Input.GetKey(KeyCode.D))
                direction += this.transform.right;

            if (Input.GetKey(KeyCode.Q))
                direction += Vector3.down;

            if (Input.GetKey(KeyCode.E))
                direction += Vector3.up;

            return direction;
        }

        private void ChangeMode(Mode mode)
        {
            this.mode = mode;

            if (mode == Mode.LockedOnTarget)
            {
                if (ballCamImageObject != null)
                {
                    ballCamImageObject.SetActive(true);
                    StopCoroutine(CRDisableObject(freeCamImageObject, 1f));
                    StartCoroutine(CRDisableObject(ballCamImageObject, 1f));
                }
            }
            else if (mode == Mode.Roaming)
            {
                if (freeCamImageObject != null)
                {
                    freeCamImageObject.SetActive(true);
                    StopCoroutine(CRDisableObject(ballCamImageObject, 1f));
                    StartCoroutine(CRDisableObject(freeCamImageObject, 1f));
                }
            }
        }

        private IEnumerator CRDisableObject(GameObject obj, float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);

            obj.SetActive(false);
        }
    }
}