using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Prepare,
    Start,
    Playing,
    Pause,
    GameOver,
    GameWin
}

public enum ScreenRatio
{
    SuperNarrow,
    VeryNarrow,
    Narrow,
    Normal,
    Broad,
    VeryBroad,
}

public class GameManager : MonoBehaviour
{
    public static event System.Action<GameState,GameState> OnGameStateChanged = delegate { };

    public static GameManager Instance = null;
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private ScreenRatio _screenRatio;
    public ScreenRatio ScreenRatio
    {
        get { return _screenRatio; }
        private set { _screenRatio = value; }
    }

    [SerializeField]
    private GameState _gameState = GameState.Prepare;
    [SerializeField]
    private int targetFrameRate = 60;
    public int scoreAmountPerBall = 10;

    private float responsiveFactor;
    private static readonly float RESPONSIVE_SIZE = 5;
    private static readonly Vector2 RESPONSIVE_SCREEN = new Vector2(480, 800);


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
        responsiveFactor = RESPONSIVE_SIZE / ( RESPONSIVE_SCREEN.y /  RESPONSIVE_SCREEN.x);
        Application.targetFrameRate = targetFrameRate;
        Input.simulateMouseWithTouches = true;
        CheckScreenRatio();
        MakeCameraResponsive();
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

    public void RestartGame()
    {
        GameState = GameState.Playing;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CheckScreenRatio()
    {
        float aspect = (float)Screen.width / Screen.height;
        if (aspect >= 0.72f)
        {
            ScreenRatio = ScreenRatio.VeryBroad;
            return;
        }
        if (aspect >= 0.64f)
        {
            ScreenRatio = ScreenRatio.Broad;
            return;
        }

        if (aspect >= 0.53f)
        {
            ScreenRatio = ScreenRatio.Normal;
            return;
        }

        if (aspect >= 0.48f)
        {
            ScreenRatio = ScreenRatio.Narrow;
            return;
        }

        if (aspect >= 0.44f)
        {
            ScreenRatio = ScreenRatio.VeryNarrow;
            return;
        }
        ScreenRatio = ScreenRatio.SuperNarrow;
    }


    //Responsive size for camera is 5 with aspect ratio 1x2 (800x400)
    void MakeCameraResponsive()
    {
        
        
        gameplayCamera.orthographicSize = responsiveFactor * (1.0f * Screen.height /Screen.width);
    }
}
