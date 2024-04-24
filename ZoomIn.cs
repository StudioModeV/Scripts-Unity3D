using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomIn : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float targetZoom = 50.0f;
    [SerializeField] private float zoomSpeed = 3.0f;
     
    private float defaultFOV = 60.0f;
   
    // Update is called once per frame
    void Update()
    {
        ZoomAction();
    }

    private void SmoothZoomIn()
    {
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetZoom, Time.deltaTime * zoomSpeed);
    }

    private void SmoothZoomOut()
    {
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultFOV, Time.deltaTime * zoomSpeed);
    }

    private void ZoomAction()
    {
        if (Input.GetMouseButton(2))
        {
            SmoothZoomIn();
            //CAN CHANGE MOUSE SENSITIVITY
        }
        else
        {
            SmoothZoomOut();
            //DEFAULT MOUSE SENSITIVITY
        }
    }


}
