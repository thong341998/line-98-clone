using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance = null;

    [Header("Object references")]
    [SerializeField] private Text playerScoreText;
    [SerializeField] private Text highScoreText;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            DestroyImmediate(gameObject);

    }

    private void Start()
    {
        
    }

    public void UpdatePlayerScoreText(int score)
    {
        playerScoreText.text = score.ToString();
    }

    public void UpdateHighScoreText(int score)
    {
        highScoreText.text = score.ToString();
    }
}
