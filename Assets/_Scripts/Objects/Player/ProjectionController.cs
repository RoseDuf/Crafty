﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player
{
    public class ProjectionController : MonoBehaviour
    {
        [SerializeField] float speed;

        private Vector3 input_vector;

        private bool grounded;
        private Vector3 posCur;
        public float angle { get; set; }

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
            MoveAcrossSurface();
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

        private void MoveAcrossSurface()
        {
            RaycastHit hit;
            int layerMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Target") | 1 << LayerMask.NameToLayer("Hole") | 1 << LayerMask.NameToLayer("Star"));

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
}
