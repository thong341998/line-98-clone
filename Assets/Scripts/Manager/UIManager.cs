using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance = null;

    [Header("Object references")]
    [SerializeField] private Text playerScoreText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            DestroyImmediate(gameObject);

    }


    public void UpdatePlayerScoreText(int score)
    {
        playerScoreText.text = score.ToString();
    }


}
