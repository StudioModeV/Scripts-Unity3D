using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPMovement : MonoBehaviour
{
    //CONFIGURATION
    [Header("Configuration")]
    [SerializeField] private float mouseSensitivity = 200.0f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float staminaDecreaseSpeed = 10.0f;
    [SerializeField] private float defaultSpeed = 2.5f;
    [SerializeField] private float sprintSpeed = 5.0f;
    [SerializeField] private float lowStaminaSpeed = 1.5f;
    [SerializeField] private float recoveryRate = 5.0f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float rayDistance = 0.5f;
    [SerializeField] private Image staminaBarImage;

    //JUMP SOUNDS
    [Header("TILE JUMP & LAND SOUNDS")]
    [SerializeField] private AudioClip[] tileJumpSound;
    [SerializeField] private AudioClip[] tileLandSound;

    [Header("WOOD JUMP & LAND SOUNDS")]
    [SerializeField] private AudioClip[] woodJumpSound;
    [SerializeField] private AudioClip[] woodLandSound;

    [Header("DIRTY GROUND JUMP & LAND SOUNDS")]
    [SerializeField] private AudioClip[] dirtyGroundJumpSound;
    [SerializeField] private AudioClip[] dirtyGroundLandSound;

    //STATE
    private float stamina = 100.0f;
    private float speed;
    private Vector3 moveDirection;
    private float yRotation = 0.0f;
    private float xRotation = 0.0f;
    private bool bIsInAir = false;
    private bool allowLandignCheck = false;
    private float landingCheckDelay = 0.2f;
    private Rigidbody rb;
    private AudioSource aS;

    //FLAGS
    public bool bCanMove { get; set; } = true;
    public bool bCanRotate { get; set; } = true;
    public bool bIsInteracting { get; set; } = false;
    public bool bSprinting { get; set; } = false;
    public bool bCanSprint { get; set; } = true;
    public bool bIsTired { get; set; } = false;
    public bool bIsMovingForwardOrBackward { get; private set; }
    public bool bIsMovingSideWards { get; private set; }

    ManageWeapon manageWeapon;
    private bool bIsGamePaused = false;

    // Start is called before the first frame update
    void Start()
    {
        manageWeapon = GameObject.Find("Weapon").GetComponent<ManageWeapon>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        InitializeComponents();
    }

    // Update is called once per frame
    void Update()
    {
        if (bIsGamePaused) return;

        if (bCanMove && !bIsInteracting)
        {
            HandleMovementInput();
            HandleRotationInput();
            HandleJumpInput();
            UpdateStamina();
        }

        CheckLanding();
        manageWeapon.bIsAirborne = bIsInAir;

        // Set the amplitude and frequency of the bob based on the state of motion and fatigue
        if ((IsSprinting() && !bIsTired))
        {
            // If the player is sprinting and is not tired, apply the sprinting bob
            manageWeapon.bobFrequency = 10.0f;
            manageWeapon.bobHorizontalAmplitude = 0.2f;
            manageWeapon.bobVerticalAmplitude = 0.2f;
            manageWeapon.ApplyWeaponBob();
        }
        else if (bIsTired || IsWalking())
        {
            // If the player is tired or simply walking, apply walking weapon bob
            manageWeapon.bobFrequency = 6.2f;
            manageWeapon.bobHorizontalAmplitude = 0.1f;
            manageWeapon.bobVerticalAmplitude = 0.1f;
            manageWeapon.ApplyWeaponBob();
        }
        else
        {
            // If the player isn't moving, reset weapon position
            manageWeapon.ResetWeaponPosition();
        }
    }

    private void FixedUpdate()
    {
        if (bCanMove && !bIsInteracting)
        {
            PerformMovement();
        }
    }

    private void InitializeComponents()
    {
        aS = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        speed = defaultSpeed;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void HandleMovementInput()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        moveDirection = transform.right * x + transform.forward * z;

        /*//CHECK IF PLAYER IS MOVING FORWARD OR BACKWARD
        bIsMovingForwardOrBackward = Mathf.Abs(z) > 0 && Mathf.Abs(x) < 0.1f;

        //CHECK IF PLAYER IS MOVING SIDEWARDS 
        bIsMovingSideWards = Mathf.Abs(x) > 0 && Mathf.Abs(z) < 0.1f;*/

        bIsMovingForwardOrBackward = Mathf.Abs(z) > 0;
        bIsMovingSideWards = Mathf.Abs(x) > 0;

        if (bCanSprint && Input.GetKey(KeyCode.LeftShift) && stamina > 0.0f && !bIsTired)
        {
            bSprinting = true;
            speed = sprintSpeed;
        }
        else if (bIsTired)
        {
            speed = lowStaminaSpeed;
        }
        else
        {
            bSprinting = false;
            speed = defaultSpeed;
        }
    }

    private void HandleRotationInput()
    {
        if (!bCanRotate) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        transform.Rotate(0.0f, mouseX, 0.0f);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90.0f, 90.0f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0.0f, 0.0f);
    }

    private void PerformMovement()
    {
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.Euler(0.0f, yRotation, 0.0f));
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(groundCheck.position, -Vector3.up, rayDistance) && stamina > 0;
    }

    private GameObject CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(groundCheck.position, -Vector3.up, out hit, rayDistance))
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    private void EnableLandingCheck()
    {
        allowLandignCheck = true;
    }

    private void HandleJumpInput()
    {
        GameObject ground = CheckGround();

        if (ground != null && Input.GetButtonDown("Jump")) 
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            PlayJumpSound(ground);
            bIsInAir = true;
            allowLandignCheck = false;
            Invoke("EnableLandingCheck", landingCheckDelay);
        }
    }

    private void CheckLanding()
    {
        if (!allowLandignCheck) return;

        GameObject ground = CheckGround();

        if (bIsInAir && IsGrounded())
        {
            bIsInAir = false;
            PlayLandSound(ground);
        }
    }

    public bool IsWalking()
    {
        return moveDirection.sqrMagnitude > 0.0f && !Input.GetKey(KeyCode.LeftShift) || bIsTired;
    }

    public bool IsSprinting()
    {
        return moveDirection.sqrMagnitude > 0.0f && Input.GetKey(KeyCode.LeftShift) && !bIsTired;
    }

    private void DecreaseStamina()
    {
        if (IsSprinting())
        {
            stamina -= staminaDecreaseSpeed * Time.deltaTime;
            stamina = Mathf.Max(stamina, 0.0f);

            if (stamina <= 0.0f)
            {
                bIsTired = true;
                speed = lowStaminaSpeed;
            }
        }
    }

    private void IncreaseStamina()
    {
        if (!IsSprinting() && stamina < 100.0f)
        {
            stamina += recoveryRate * Time.deltaTime;
            stamina = Mathf.Min(stamina, 100.0f);

            if (stamina > 5.0f)
            {
                bIsTired = false;
                speed = defaultSpeed;
            }
        }
    }

    private void UpdateStamina()
    {
        if (IsSprinting())
        {
            DecreaseStamina();
        }
        else
        {
            IncreaseStamina();
        }
        staminaBarImage.fillAmount = stamina / 100.0f;
    }

    public void SetRotationActive(bool isActive)
    {
        bCanRotate = isActive;
    }

    public void SetMovementActive(bool isActive)
    {
        bCanMove = isActive;
    }

    public void SetSprintActive(bool isActive)
    {
        bCanSprint = isActive;

        if (!isActive)
        {
            bSprinting = false;
            speed = defaultSpeed;
        }
    }

    private void PlayJumpSound(GameObject ground)
    {
        if (ground.layer == LayerMask.NameToLayer("Tile") || ground.layer == LayerMask.NameToLayer("Default"))
        {
            aS.PlayOneShot(tileJumpSound[Random.Range(0, tileJumpSound.Length)]);   
        }
        else if (ground.layer == LayerMask.NameToLayer("Wood"))
        {
            aS.PlayOneShot(woodJumpSound[Random.Range(0, woodJumpSound.Length)]);
        }
        else if (ground.layer == LayerMask.NameToLayer("DirtyGround"))
        {
            aS.PlayOneShot(dirtyGroundJumpSound[Random.Range(0, dirtyGroundJumpSound.Length)]);
        }
    }

    private void PlayLandSound(GameObject ground)
    {
        if (ground.layer == LayerMask.NameToLayer("Tile") || ground.layer == LayerMask.NameToLayer("Default"))
        {
            aS.PlayOneShot(tileLandSound[Random.Range(0, tileLandSound.Length)]);
        }
        else if (ground.layer == LayerMask.NameToLayer("Wood"))
        {
            aS.PlayOneShot(woodLandSound[Random.Range(0, woodLandSound.Length)]);
        }
        else if (ground.layer == LayerMask.NameToLayer("DirtyGround"))
        {
            aS.PlayOneShot(woodLandSound[Random.Range(0, woodLandSound.Length)]);
        }
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

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
}
