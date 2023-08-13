using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEyeProcess : MonoBehaviour
{
    // Start is called before the first frame update
    Camera cameraComponent;
    void Start()
    {
        cameraComponent = this.GetComponent<Camera>();
        cameraComponent.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
