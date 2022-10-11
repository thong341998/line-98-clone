using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BallState
{
    Idle,
    Bouncing,
    Moving
}

public class BallController : MonoBehaviour
{
    public event System.Action OnBallMoveCompleted = delegate { };

    [SerializeField] private float moveTime = 0.5f;
    [SerializeField] private float moveSpeed = 5f;

    
    private BallState ballState;
    public BallState BallState
    {
        get => ballState;
        private set
        {
            if (ballState != value)
            {
                ballState = value;
                if (ballState == BallState.Bouncing)
                {
                    ballAnimator.SetBool(IS_CLICK_PARAM, true);
                }
                else
                {
                    ballAnimator.SetBool(IS_CLICK_PARAM, false);
                }
            }
        }
    }

    //Components
    private SpriteRenderer ballSprite;
    private Animator ballAnimator;
    private Coroutine moveByUnitCR;

    //Data sructure
    private Queue<Vector2> path;
    private Vector2 currentWaypoint;

    //Data types
    public bool pathCompleted = false;


    private readonly int IS_CLICK_PARAM = Animator.StringToHash("isClick");

    
    

    void Start()
    {
        InitValues();
    }

    public void SetPath(Queue<Vector2> boardPath)
    {
        path = boardPath;
        currentWaypoint = path.Dequeue();
        pathCompleted = true;
        BallState = BallState.Moving;// Ball is moving
    }

    public void OnBallSelected()
    {
        BallState = BallState.Bouncing;
    }

    public void OnBallCancelSelected()
    {
        BallState = BallState.Idle;
    }

    public void SetData(Color color)
    {
        ballSprite.color = color;
    }

    public void InitValues()
    {
        ballState = BallState.Idle;
        ballSprite = GetComponent<SpriteRenderer>();
        ballAnimator = GetComponent<Animator>();
    }


    private void Update()
    {
        if (pathCompleted)
        {
           FollowPath();
        }
    }

    private void MoveByUnit(float distance)
    {

    }

    private void FollowPath()
    {
        
        //Move towards waypoint
        //var waypoint = path.Dequeue();
        transform.position = Vector2.MoveTowards(transform.position, currentWaypoint, Time.deltaTime * moveSpeed);

        //Check if ball reach the current way point
        float distSqr = Vector2.SqrMagnitude((Vector2)transform.position - currentWaypoint);
        if (distSqr < float.Epsilon )
        {
            //If this is the last waypoint then return
            if (path.Count == 0)
            {
                //Ball move completed, Reset values for the next path;
                BallState = BallState.Idle;
                pathCompleted = false;
                OnBallMoveCompleted?.Invoke();
                return;
            }
               
            //Move to next waypoint
            currentWaypoint = path.Dequeue();
        }
    }

    
   

    IEnumerator CR_MoveByUnit(Vector2 src, Vector2 dest, System.Action completed = null)
    {
        float t = 0f;
        float cur = 0f;
        while (cur < moveTime)
        {
            cur += Time.deltaTime;
            t = cur / moveTime;
            Vector2 newPos = Vector2.Lerp(src, dest, t);
            transform.position = newPos;
            yield return null;
        }

        transform.position = dest;
        completed?.Invoke();
    }
}
