using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorDragger : MonoBehaviour, InteractionSystem.IInteractable
{
    //SOUNDS
    [Header("SOUNDS")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    private bool bOpenSoundPlayed = false;
    private bool bCloseSoundPlayed = false;
    private bool bHasBeenOpened = false;

    //SETTINGS
    [Header("SETTINGS")]
    private bool bIsDragging = false;
    private Vector3 lastMousePosition;
    private float originalYRotation;
    public float minRotationY = 0.0f;
    public float maxRotationY = 90.0f;
    public float rotationSpeed = 5.0f;

    //VIBRATION SETTINGS
    private float vibrationIntensity;
    private float vibrationDuration;
    private Vector3 initialPosition;
    [SerializeField] private AudioClip vibrationSound;

    private Rigidbody rb;
    InventoryPlayer inventory;
    FPMovement movement;
    AudioSource aS;
    ManageWeapon manageWeapon;
    void Awake()
    {
        inventory = FindObjectOfType<InventoryPlayer>();
        manageWeapon = FindObjectOfType<ManageWeapon>();
        rb = GetComponent<Rigidbody>();
        aS = GetComponent<AudioSource>();
        originalYRotation = transform.eulerAngles.y;

        //VIBRATION SETTINGS
        initialPosition = transform.position;
        vibrationIntensity = 0.005f;
        vibrationDuration = 1.2f;
    }
    void Start()
    {
        movement = FindObjectOfType<FPMovement>();
    }
    public void Interact() { }
    public void StartDrag()
    {
        if (inventory.CheckForKeysAvailable(InventoryPlayer.KeysAvailable.FirstKey))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            lastMousePosition = Input.mousePosition;
            bIsDragging = true;
            rb.isKinematic = true;

            if (manageWeapon != null)
            {
                manageWeapon.SetCanShoot(false); //NO SHOOTING WHILE START DRAGGING
                manageWeapon.SetInteractionState(true);
            }

            if (movement != null)
            {
                movement.SetRotationActive(false);
                movement.SetMovementActive(false);
            }
        }
        else
        {
            VibrateDoor();

            if (manageWeapon != null)
            {
                manageWeapon.SetCanShoot(false);
            }
            Debug.Log("DOOR IS CLOSED YOU NEED THE KEY");
        }
    }
    public void DuringDrag()
    {
        if (bIsDragging)
        {
            RotateDoorWithMouse();
        }
    }

    public void EndDrag()
    {
        bIsDragging = false;
        rb.isKinematic = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (manageWeapon != null)
        {
            manageWeapon.SetCanShoot(true);
            manageWeapon.SetInteractionState(false);
        }

        if (movement != null)
        {
            movement.SetRotationActive(true);
            movement.SetMovementActive(true);
        }
    }

    private void RotateDoorWithMouse()
    {
        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
        float rotationDelta = -mouseDelta.x * rotationSpeed * Time.deltaTime;

        // Calcular la rotación actual y la rotación deseada
        float currentYRotation = rb.transform.eulerAngles.y;
        float desiredYRotation = currentYRotation + rotationDelta;

        // Clampear la rotación deseada dentro de los límites
        desiredYRotation = Mathf.Clamp(desiredYRotation, minRotationY, maxRotationY);

        // Calcular la rotación final permitida
        float clampedRotationY = Mathf.Clamp(desiredYRotation, originalYRotation - maxRotationY, originalYRotation + maxRotationY);

        // Aplicar la rotación
        rb.transform.eulerAngles = new Vector3(0.0f, clampedRotationY, 0.0f);

        //Comprobación para reproducir sonidos
        if (Mathf.Abs(clampedRotationY - 90.0f) < 1.0f && !bOpenSoundPlayed)
        {
            aS.PlayOneShot(openSound, 1.0f);
            bOpenSoundPlayed = true;
            bCloseSoundPlayed = false;  //SE RESTABLECE
            bHasBeenOpened = true;
        }
        else if (Mathf.Abs(clampedRotationY) < 1.0f && !bCloseSoundPlayed && bHasBeenOpened)
        {
            aS.PlayOneShot(closeSound, 1.0f);
            bCloseSoundPlayed = true;
            bOpenSoundPlayed = false;  //SE RESTABLECE
            bHasBeenOpened = false;  //SE RETABLECE
        }
        else if (clampedRotationY > 1.0f && clampedRotationY < 89.0f)
        {
            //SI LA PUERTA ESTÁ ENTRE LOS DOS ÁNGULOS RESTABLECE SONIDO
            bCloseSoundPlayed = false;
            bOpenSoundPlayed = false;
        }

        // Actualizar la posición del ratón
        lastMousePosition = Input.mousePosition;
    }

    private IEnumerator DoorVibration()
    {
        float elapsedTime = 0.0f;

        AudioSource.PlayClipAtPoint(vibrationSound, transform.position, 1.0f);

        while (elapsedTime < vibrationDuration)
        {
            Vector3 randomOffset = Random.insideUnitSphere * vibrationIntensity;
            randomOffset.x = 0.0f;
            transform.position = initialPosition + randomOffset;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = initialPosition;
    }

    public void VibrateDoor()
    {
        StartCoroutine(DoorVibration());
    }
}
