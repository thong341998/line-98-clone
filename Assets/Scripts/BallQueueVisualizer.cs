using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallQueueVisualizer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] ballSprites;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Visualize( Dictionary<Vector2Int,BallQueueController> ballQueue)
    {
        int cur = 0;
       foreach (var item in ballQueue)
        {
            ballSprites[cur++].color = item.Value.BallColor;
        }
    }
}
