using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepsAudio : MonoBehaviour
{
    //SOUNDS
    [Header("TILE Foostep Sounds")]
    [SerializeField] private AudioClip[] tileSoundsWalking;
    [SerializeField] private AudioClip[] tileSoundsRunning;

    [Header("WOOD Footstep Sounds")]
    [SerializeField] private AudioClip[] woodSoundsWalking;
    [SerializeField] private AudioClip[] woodSoundsRunning;

    [Header("DIRTY GROUND Footstep Sounds")]
    [SerializeField] private AudioClip[] dirtyGroundWalking;
    [SerializeField] private AudioClip[] dirtyGroundRunning;
    public AudioSource audiosourceFootsteps;

    //SETTINGS
    [Header("Footstep Intervals")]
    [SerializeField] private float tiredInterval = 0.65f;
    [SerializeField] private float defaultInterval = 0.5f;
    [SerializeField] private float sprintInterval = 0.3f;
    private float stepsCooldown;

    Raycaster raycaster;
    FPMovement movement;
    // Start is called before the first frame update
    void Start()
    {
        raycaster = GetComponent<Raycaster>();
        movement = GetComponent<FPMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isMoving = movement.IsWalking() || movement.IsSprinting();
        bool shouldPlaySprintSound = !movement.bIsTired && movement.IsSprinting();
        bool shouldPlayWalkSound = movement.IsWalking() || movement.bIsTired || (movement.IsSprinting());

        if (isMoving)
        {
            stepsCooldown -= Time.deltaTime;

            if (stepsCooldown <= 0)
            {
                PlayFootStepSound();

                // Ajusta el intervalo de sonido según el estado.
                if (shouldPlaySprintSound)
                {
                    stepsCooldown = sprintInterval;
                }
                else if (shouldPlayWalkSound)
                {
                    stepsCooldown = movement.bIsTired ? tiredInterval : defaultInterval;
                }
            }
        }
        else
        {
            ResetFootstepCooldown();
        }
    }

    private void PlayFootStepSound()
    {
        RaycastHit hit;
        if (raycaster.PerformGroundRaycastHit(out hit, 0.4f))
        {
            int layer = hit.collider.gameObject.layer;

            switch (layer)
            {
                case int l when l == LayerMask.NameToLayer("Tile"):
                    PlayMaterialSpecificSounds(tileSoundsWalking, tileSoundsRunning);
                    break;
                case int l when l == LayerMask.NameToLayer("Wood"):
                    PlayMaterialSpecificSounds(woodSoundsWalking, woodSoundsRunning);
                    break;
                case int l when l == LayerMask.NameToLayer("DirtyGround"):
                    PlayMaterialSpecificSounds(dirtyGroundWalking, dirtyGroundRunning);
                    break;
                default:
                    PlayMaterialSpecificSounds(tileSoundsWalking, tileSoundsRunning);
                    break;
            }
        }
    }

    private void ResetFootstepCooldown()
    {
        stepsCooldown = defaultInterval;
        if (audiosourceFootsteps.isPlaying)
        {
            audiosourceFootsteps.Stop();
        }
    }

    private void PlayMaterialSpecificSounds(AudioClip[] walkingSounds, AudioClip[] runningSounds)
    {
        AudioClip[] soundArray = movement.IsSprinting() && !movement.bIsTired ? runningSounds : walkingSounds;
        if (soundArray != null && soundArray.Length > 0)
        {
            int randomIndex = Random.Range(0, soundArray.Length);
            audiosourceFootsteps.PlayOneShot(soundArray[randomIndex], 0.3f);
        }
    }
}
