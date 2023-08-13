using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMove : MonoBehaviour
{
    public float minimumY = -90f, maximumY = 90f;
    private float rotationY = 90f, rotationX = 0f;
    public float sensitivity = 10f;
    public float moveSpeed = 5f;
    private float vInput, hInput;
    private bool isMouseRightButtonDown = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isMouseRightButtonDown = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isMouseRightButtonDown = false;
        }

        if (isMouseRightButtonDown)
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivity;
            rotationY += Input.GetAxis("Mouse Y") * sensitivity;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

            vInput = Input.GetAxis("Vertical") * moveSpeed;
            hInput = Input.GetAxis("Horizontal") * moveSpeed;
            transform.Translate(Vector3.forward * vInput * Time.deltaTime);
            transform.Translate(Vector3.right * hInput * Time.deltaTime);
        }
    }
}

