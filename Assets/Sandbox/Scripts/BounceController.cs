using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceController : MonoBehaviour
{
    private bool grounded;
    public Vector3 posCur { get; set; }
    public float angle { get; set; }

    void FixedUpdate()
    {
        MoveAcrossSurface();
    }

    private void MoveAcrossSurface()
    {
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Ball");

        if (Physics.BoxCast(transform.position + new Vector3(0, 100f, 0), new Vector3(0.5f, 0.5f, 0.5f), -transform.up, out hit, Quaternion.identity, 200f, layerMask) == true)
        {
            posCur = new Vector3(transform.position.x, hit.point.y + 0.5f, transform.position.z);
            angle = Mathf.Acos(hit.normal.y);

            grounded = true;

        }
        else
        {
            grounded = false;
        }


        if (grounded == true)
        {
            transform.position = posCur;
        }
        else
        {
            transform.position = transform.position - Vector3.up * 1f;
        }
    }

}
