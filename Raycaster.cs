using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Raycaster : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask[] obstructionLayers;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Transform rayOrigin;
    
    private LayerMask GetCombinedObstructionLayer()
    {
        LayerMask combinedLayer = 0;
        foreach (LayerMask layer in obstructionLayers)
        {
            combinedLayer |= layer;
        }
        return combinedLayer;
    }

    public bool PerformRaycastFromFPCamera(out RaycastHit hit, float distance)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance, Color.blue, 0.5f);
        LayerMask combinedObstructionLayer = GetCombinedObstructionLayer();
        return Physics.Raycast(ray, out hit, distance, interactableLayer | combinedObstructionLayer);
    }

    public bool PerformRaycastFromRevolver(out RaycastHit hit, float distance)
    {
        //CREATES A RAY FROM THE CENTER OF THE SCREEN
        Ray cameraRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        Debug.DrawRay(cameraRay.origin, cameraRay.direction * distance, Color.red, 0.5f);
        LayerMask combinedObstructionLayer = GetCombinedObstructionLayer();

        //PERFORM THE RAYCAST
        return Physics.Raycast(cameraRay, out hit, distance, interactableLayer | combinedObstructionLayer);
    }

    public bool PerformRaycastFromCamera(Transform cameraSocket, out RaycastHit[] hits, float distance, int rayCount, float coneAngle)
    {
        hits = new RaycastHit[rayCount];
        Vector3 cameraPos = cameraSocket.position;
        Vector3 cameraForward = cameraSocket.forward;

        //CALCULATE RAY ANGLE
        float angleIncrement = coneAngle / (rayCount - 1);

        //CALCULATE FIRST RAY DIRECTION
        Vector3 firstDirection = Quaternion.AngleAxis(-coneAngle / 2, cameraSocket.up) * cameraForward;

        bool hitDetected = false;
        LayerMask combinedObstructionLayer = GetCombinedObstructionLayer();

        for (int i = 0; i < rayCount; i++)
        {
            //CALCULATE RAY ACTUAL DIRECTION
            Vector3 rayDirection = Quaternion.AngleAxis(i * angleIncrement, cameraSocket.up) * firstDirection;

            //CAST RAY
            Ray ray = new Ray(cameraPos, rayDirection);

            Debug.DrawRay(cameraPos, rayDirection * distance, Color.green, 0.5f);

            if (Physics.Raycast(ray, out hits[i], distance, interactableLayer | combinedObstructionLayer))
            {
                hitDetected = true;
            }
        }
        return hitDetected;
    }

    public bool PerformGroundRaycastHit(out RaycastHit hit, float distance)
    {
        Ray ray = new Ray(rayOrigin.transform.position, Vector3.down);
        return Physics.Raycast(ray, out hit, distance, groundLayer);
    }
}
