using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState
{
    Prepare,
    Start,
    Playing,
    Pause,
    GameOver,
    GameWin
}

public class GameManager : MonoBehaviour
{
    public static event System.Action<GameState,GameState> OnGameStateChanged = delegate { };

    public static GameManager Instance = null;

   
    [SerializeField]
    private GameState _gameState = GameState.Prepare;
    [SerializeField]
    private int targetFrameRate = 60;
    public int scoreAmountPerBall = 10;

    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        set
        {
            if (_gameState != value)
            {
                GameState oldState = _gameState;
                _gameState = value;

                OnGameStateChanged?.Invoke(_gameState, oldState);
            }
        }
    }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != null)
            DestroyImmediate(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = targetFrameRate;
        Input.simulateMouseWithTouches = true;
        //PrepareGame();
    }


    public void PrepareGame()
    {
        _gameState = GameState.Prepare;
    }

    public void StartGame()
    {
        _gameState = GameState.Start;
    }

    public void LoseGame()
    {
        _gameState = GameState.GameOver;
    }
}
