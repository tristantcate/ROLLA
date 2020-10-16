using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSystem : MonoBehaviour
{
    private int m_score;

    private void Awake()
    {
        m_score = 0;
        AddScore(0);
    }
    public void AddScore(int scoreToAdd)
    {
        m_score += scoreToAdd;
        scoreText.text = string.Format("Score: {0}", m_score);
    }

    public int GetScore() => m_score;

    [SerializeField] private Text scoreText;


}
