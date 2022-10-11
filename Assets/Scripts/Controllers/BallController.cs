using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum BallState
{
    Idle,
    Bouncing,
    Moving
}

public class BallController : MonoBehaviour, IPointerClickHandler
{

    private Animator ballAnimator;
   
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

    private SpriteRenderer ballSprite;
    private readonly int IS_CLICK_PARAM = Animator.StringToHash("isClick");
    

    // Start is called before the first frame update
    void Start()
    {
        InitValues();
    }

    void InitValues()
    {
        ballState = BallState.Idle;
        ballSprite = GetComponent<SpriteRenderer>();
        ballAnimator = GetComponent<Animator>();
    }


    private void MoveByUnit(float distance)
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BallState != BallState.Bouncing)
        {
            BallState = BallState.Bouncing;
        }
        else
        {
            BallState = BallState.Idle;
        }
    }
}
