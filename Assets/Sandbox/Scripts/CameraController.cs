using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject cameraObject;

    [SerializeField] GameObject ballFocus;
    [SerializeField] GameObject projectionFocus;

    [SerializeField] float cameraMoveSpeed;
    [SerializeField] float clampAngle; //how high/low we want to be able to look
    [SerializeField] float mouseSensitivity; 

    private Vector3 followPosition;
    private Vector3 followRotation;

    private float finalInputX;
    private float finalInputZ;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        followPosition = transform.position;
        followRotation = transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        followRotation.y += mouseX * mouseSensitivity * Time.deltaTime;
        followRotation.x += mouseY * mouseSensitivity * Time.deltaTime;

        followRotation.x = Mathf.Clamp(followRotation.x, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(followRotation.x, followRotation.y, 0.0f);
        transform.rotation = localRotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Transform target = ballFocus.transform;

        float step = cameraMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }
}
