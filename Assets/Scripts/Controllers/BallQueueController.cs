using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallQueueController : BallBase
{

    [SerializeField] private float initSizeScale = 0.1f;
    [SerializeField] private float enlareTime = 0.5f;

    public override void InitValues()
    {
        base.InitValues();
        transform.localScale = new Vector3(initSizeScale, initSizeScale, initSizeScale);
    }


}
