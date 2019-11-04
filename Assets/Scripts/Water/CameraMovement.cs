using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]private float _speed;
    [SerializeField]private float _rotSpeedH;
    [SerializeField]private float _rotSpeedV;
    private float pitch;
    private float yaw;

    private void Start()
    {
        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += _rotSpeedH * Input.GetAxis("Mouse X") *  Time.deltaTime;
            pitch -= _rotSpeedV * Input.GetAxis("Mouse Y") *  Time.deltaTime;

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f) ;
        }

        Vector3 newPos = transform.right * Input.GetAxis("Horizontal");
        newPos += transform.forward * Input.GetAxis("Vertical");
        transform.Translate(newPos * Time.deltaTime * _speed);
    }
}
