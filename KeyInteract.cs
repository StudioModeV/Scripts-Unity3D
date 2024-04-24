using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyInteract : MonoBehaviour, InteractionSystem.IInteractable
{
    public InventoryPlayer.KeysAvailable key;
    InventoryPlayer inventory;

    private void Awake()
    {
        inventory = FindObjectOfType<InventoryPlayer>();
    }
    public void Interact()
    {
        inventory.ObtainKeys(true, key);
        Destroy(gameObject);
    }
    public void StartDrag() { }
    public void DuringDrag() { }
    public void EndDrag() { }
}
