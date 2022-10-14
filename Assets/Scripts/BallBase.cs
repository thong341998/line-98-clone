using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBase : MonoBehaviour
{
   
    protected SpriteRenderer ballSprite;
    public Color BallColor { get => ballSprite.color; }
    public Sprite BallSprite { get => ballSprite.sprite; }

    public virtual void InitValues()
    {
        ballSprite = GetComponent<SpriteRenderer>();
    }

    public virtual void SetData(Color color)
    {
        ballSprite.color = color;
    }

}
