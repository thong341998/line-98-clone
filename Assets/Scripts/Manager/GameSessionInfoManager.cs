using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonConstants;

public class PlayerInfo
{
    private int score;

}

public class GameSessionInfoManager : MonoBehaviour
{
    public static GameSessionInfoManager Instance = null;


    private int playerScore;
    private int highScore;


    public int HighScore
    {
        get => highScore;
        private set
        {
            highScore = value;
            PlayerPrefs.SetInt(StringConstants.HIGH_SCORE_KEY, highScore);
        }
    }

    PlayerInfo playerInfo;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            DestroyImmediate(gameObject);
        }

        BoardManager.OnBallLineDestroy += BoardManager_OnBallLineDestroy;
    }

    private void Start()
    {
        playerScore = PlayerPrefs.GetInt(StringConstants.PLAYER_SCORE_KEY, 0);
        highScore = PlayerPrefs.GetInt(StringConstants.HIGH_SCORE_KEY, 0);

        UIManager.Instance.UpdateHighScoreText(highScore);
    }

    private void BoardManager_OnBallLineDestroy(int ballAmount)
    {
        playerScore += (GameManager.Instance.scoreAmountPerBall * ballAmount);

        //Update UI;
        UIManager.Instance.UpdatePlayerScoreText(playerScore);

        if (playerScore > highScore)
        {
            highScore = playerScore;

            //Fire high score event
            UIManager.Instance.UpdateHighScoreText(highScore);
        }

        //Save to database
        PlayerPrefs.SetInt(StringConstants.PLAYER_SCORE_KEY, playerScore);
        PlayerPrefs.SetInt(StringConstants.HIGH_SCORE_KEY, highScore);
    }
}
