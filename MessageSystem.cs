using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageSystem : MonoBehaviour
{
    public static MessageSystem Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private float messageDuration = 3.0f;

    private Coroutine displayCoroutine;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
    public void DisplayMessage(string message)
    {
        if (messageText != null && messagePanel != null)
        {
            messageText.text = message;
            messagePanel.SetActive(true);

            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine); //IF THERE'S A COROUTINE RUNNING STOPS IT
            }

            displayCoroutine = StartCoroutine(HideAfterDelay(messageDuration));
        }
    }

    public void HideMessage()
    {
        if (messageText != null && messagePanel != null)
        {
            messageText.text = "";
            messagePanel.SetActive(false);
        }
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideMessage();
    }
}
