using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryPlayer : MonoBehaviour
{
    //KEYS
    [Header("Keys")]
    public bool bHasFirstKey = false;
    public bool bHasSecondKey = false;

    //HEALTH HEAL
    [Header("Health Potion in Inventory")]
    [SerializeField] private TextMeshProUGUI healingPotionsAvailable;
    public int healingPotions = 0;
    private int maxHealingPotions = 10;

    //SANITY HEAL
    [Header("Sanity Pills in Inventory")]
    [SerializeField] private TextMeshProUGUI sanityPillAvailable;
    public int sanityPill = 0;
    private int maxSanityPill = 10;

    //AMMO
    [Header("Ammo in Inventory")]
    public int revolverAmmoInventory = 10;
    private int maxRevolverAmmoInventory = 35;

    public int cameraAmmoInventory = 10;
    private int maxCameraAmmoInventory = 25;

    //SOUNDS
    [Header("Sounds")]
    [SerializeField] private AudioSource audiosourceInventory;
    [SerializeField] private AudioClip healingUseSound;
    [SerializeField] private AudioClip sanityUseSound;
    [SerializeField] private AudioClip consumablePickupSound;
    [SerializeField] private AudioClip pickupAmmoSound;
    [SerializeField] private AudioClip pickupKeySound;

    //
    ManageWeapon manageWeapon;
    PlayerHealth playerHealth;
    PlayerSanity playerSanity;

    private bool bIsGamePaused = false;
   
    public enum KeysAvailable
    {
        FirstKey,
        SecondKey,
        ThirdKey
    }
    public enum AmmoType
    {
        Revolver,
        Camera
    }

    public enum HealingType
    {
        HealingPotion,
        SanityPill
    }

    private void Awake()
    {
        manageWeapon = FindObjectOfType<ManageWeapon>();
        playerHealth = GetComponent<PlayerHealth>();
        playerSanity = GetComponent<PlayerSanity>();
        UpdateConsumables();
    }

    private void Update()
    {
        if (bIsGamePaused) return;

        CheckForUserInput();
    }
    private void UpdateConsumables()
    {
        healingPotionsAvailable.text = healingPotions.ToString();
        sanityPillAvailable.text = sanityPill.ToString();
    }

    private void CheckForUserInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseHealingHerb();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            UseSanityPill();
        }
    }

    public void ObtainKeys(bool gotKey, KeysAvailable key)
    {
        switch (key)
        {
            case KeysAvailable.FirstKey:
                bHasFirstKey = gotKey;
                break;
            case KeysAvailable.SecondKey:
                bHasSecondKey = gotKey;
                break;
        }
        if (gotKey)
        {
            audiosourceInventory.PlayOneShot(pickupKeySound, 1.0f);
        }
    }

    public bool CheckForKeysAvailable(KeysAvailable key)
    {
        switch (key)
        {
            case KeysAvailable.FirstKey:
                return bHasFirstKey;
            case KeysAvailable.SecondKey:
                return bHasSecondKey;
            default:
                return false;
        }
    }

    public void UseHealingHerb()
    {
        if (healingPotions >= 1 && playerHealth.CurrentHealth < 100.0f && healingUseSound != null)
        {
            Debug.Log("PRESSED Q ");
            playerHealth.Heal(Random.Range(10.0f, 35.0f));
            healingPotions--;
            audiosourceInventory.PlayOneShot(healingUseSound, 1.0f);

            UpdateConsumables();
        }
        else if (healingPotions >= 1 && playerHealth.CurrentHealth >= 100.0f)
        {
            Debug.Log("HEALTH: " + playerHealth.CurrentHealth + " | HEALING HERBS: " + healingPotions);
            //AUDIO
        }
    }

    public void UseSanityPill()
    {
        if (sanityPill >= 1 && playerSanity.CurrentSanity < 100.0f && sanityUseSound != null)
        {
            Debug.Log("PRESSED E");
            playerSanity.RestoreSanity(Random.Range(10.0f, 25.0f));
            sanityPill--;
            audiosourceInventory.PlayOneShot(sanityUseSound, 1.0f);

            UpdateConsumables();
        }
        else if (sanityPill >= 1 && playerSanity.CurrentSanity >= 100.0f)
        {
            Debug.Log("SANITY: " + playerSanity.CurrentSanity + " | SANITY PILLS: " + sanityPill);
            //AUDIO
        }
    }

    public void AddWeaponAmmo(int amount, AmmoType type)
    {
        switch (type)
        {
            case AmmoType.Revolver:
                revolverAmmoInventory = Mathf.Min(revolverAmmoInventory + amount, maxRevolverAmmoInventory);
                break;
            case AmmoType.Camera:
                cameraAmmoInventory = Mathf.Min(cameraAmmoInventory + amount, maxCameraAmmoInventory);
                break;
        }
        manageWeapon.UpdateAmmoUI();
        audiosourceInventory.PlayOneShot(pickupAmmoSound, 1.0f);
    }

    public void AddHealingObject(int amount, HealingType type)
    {
        switch (type)
        {
            case HealingType.HealingPotion:
                healingPotions = Mathf.Min(healingPotions + amount, maxHealingPotions);
                break;
            case HealingType.SanityPill:
                sanityPill = Mathf.Min(sanityPill + amount, maxSanityPill);
                break;
                
        }
        UpdateConsumables();
        audiosourceInventory.PlayOneShot(consumablePickupSound, 1.0f);
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
