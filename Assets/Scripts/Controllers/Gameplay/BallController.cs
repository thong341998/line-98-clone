using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BallState
{
    Idle,
    Bouncing,
    Moving
}

public class BallController : BallBase
{
    public static event System.Action<BallController> OnBallDestroyed = delegate { };
    public event System.Action OnBallMoveCompleted = delegate { };
   

    [SerializeField] protected float moveSpeed = 5f;

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
    private Animator ballAnimator;
    private Coroutine moveByUnitCR;

    //Data sructure
    private Queue<Vector2> path;
    private Vector2 currentWaypoint;

    //Data types
    public bool pathCompleted = false;


    private readonly int IS_CLICK_PARAM = Animator.StringToHash("isClick");

   

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

    public override void SetData(Color color)
    {
        ballSprite.color = color;
    }

    public override void InitValues()
    {
        base.InitValues();
        
        ballState = BallState.Idle;
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


    private void OnDestroy()
    {
        OnBallDestroyed?.Invoke(this);
    }

}
