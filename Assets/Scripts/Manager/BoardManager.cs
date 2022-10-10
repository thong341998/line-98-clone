using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using CommonConstants;

public class BoardManager : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject ballPrefs;
    public static BoardManager Instance = null;

    private Dictionary<Vector2, BallController> ballDictionary = new Dictionary<Vector2, BallController>(); // Use to store ball with its board position

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 worldPos = MapProvider.Instance.gameCam.ScreenToWorldPoint(eventData.position);

        //Snap to board position
        Vector2 boardPosition = UtilMapHelpers.WorldToBoardPosition(worldPos, MapProvider.Instance.cellSize, MapConstants.BOARD_COL, MapConstants.BOARD_ROW);
        
        //Check if has ball then 

        Instantiate(ballPrefs, boardPosition, Quaternion.identity, transform);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != null)
            DestroyImmediate(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Check if the position pass in has a ball
    private bool HasBallAt(Vector2 position)
    {
        return ballDictionary.ContainsKey(position);
    }

   
}
