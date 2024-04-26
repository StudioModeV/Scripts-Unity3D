using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private int score = 0;
    private int points = 10;

    EnemyHealth[] enemies;

    private void Start()
    {
        enemies = FindObjectsOfType<EnemyHealth>();

        UpdateScoreText();
    }
    public void AddScore()
    {
        score += points;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString() + ("/") + enemies.Length * points;
        }
    }
}
