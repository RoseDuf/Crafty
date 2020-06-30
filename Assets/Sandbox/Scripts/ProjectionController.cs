using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectionController : MonoBehaviour
{
    [SerializeField] float speed;

    private Vector3 input_vector;

    // Start is called before the first frame update
    void Start()
    {
        input_vector = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        MoveProjection();
    }

    void HandleInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        input_vector = new Vector3(horizontal, 0, vertical);
    }

    void MoveProjection()
    {
        transform.Translate(Vector3.right * input_vector.x * Time.deltaTime * speed);
        transform.Translate(Vector3.forward * input_vector.z * Time.deltaTime * speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag != "Floor")
        {
            RaycastHit hitInfo;
            bool obstacleFound = Physics.BoxCast(collision.collider.transform.position + new Vector3(0, 100, 0), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, -1, 0), out hitInfo, Quaternion.identity, 200f);

            if (obstacleFound)
            {

                print("hello");
                transform.position = hitInfo.transform.position;
            }
        }
    }
}
