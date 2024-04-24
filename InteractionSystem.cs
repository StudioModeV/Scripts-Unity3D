using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;

    private Raycaster raycaster;
    private IInteractable currentlyInteracting;

    private bool bIsGamePaused = false;

    private void Awake()
    {
        raycaster = GetComponent<Raycaster>();
    }

    // Update is called once per frame
    void Update()
    {
        if (bIsGamePaused) return;

        //INTERACT
        if (Input.GetKeyDown(KeyCode.F))
        {
            PerformInteraction();
        }

        //DOOR DRAGGER
        if (Input.GetMouseButtonDown(0))
        {
            if (TryGetInteractable(out IInteractable interactable))
            {
                currentlyInteracting = interactable;
                currentlyInteracting.StartDrag();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentlyInteracting != null)
            {
                currentlyInteracting.EndDrag();
                currentlyInteracting = null;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (currentlyInteracting != null)
            {
                currentlyInteracting.DuringDrag();
            }
        }
    }

    public interface IInteractable
    {
        void Interact();

        //DOOR FUNCTIONS
        void StartDrag();
        void DuringDrag();
        void EndDrag();
    }

    private void PerformInteraction()
    {
        RaycastHit hit;
        if (raycaster != null && raycaster.PerformRaycastFromFPCamera(out hit, 2.0f))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

    bool TryGetInteractable(out IInteractable interactable)
    {
        RaycastHit hit;
        if (raycaster != null && raycaster.PerformRaycastFromFPCamera(out hit, 2.0f))
        {
            interactable = hit.collider.GetComponent<IInteractable>();
            return interactable != null;
        }

        interactable = null;
        return false;
    }

    private void OnEnable()
    {
        PauseGameManager.OnGamePaused += HandleGamePaused;
        PauseGameManager.OnGameResumed += HandleGameResumed;
    }

    private void OnDisable()
    {
        PauseGameManager.OnGamePaused -= HandleGamePaused;
        PauseGameManager.OnGameResumed -= HandleGameResumed;
    }

    private void HandleGamePaused()
    {
        bIsGamePaused = true;
    }

    private void HandleGameResumed()
    {
        bIsGamePaused = false;
    }
}
