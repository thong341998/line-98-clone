using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallQueueController : BallBase
{
    public event System.Action OnBalLScaleToNormalCompleed = delegate { };
    public event System.Action OnBallScaleToHalfCompleted = delegate { };


    [SerializeField] private float initSizeScale = 0.1f;
    [SerializeField] private float enlargeTime = 0.5f;


    public float EnlargeTime { get => enlargeTime; }

    private Coroutine enlargeCR;
    private Vector3 normalScale;
    private Vector3 halfScale;

    public override void InitValues()
    {
        base.InitValues();
        normalScale = transform.localScale;
        halfScale = new Vector3(initSizeScale, initSizeScale, initSizeScale);
        //transform.localScale = new Vector3(initSizeScale, initSizeScale, initSizeScale);
        EnlargeToHalf();
    }


    void Enlarge(Vector3 startScale, Vector3 endScale, System.Action completed)
    {
        if (enlargeCR != null)
        {
            StopCoroutine(enlargeCR);
        }
        enlargeCR = StartCoroutine(CR_Enlarge(startScale, endScale, completed));
    }

    public void EnlargeToHalf(System.Action completed = null)
    {
        Enlarge(Vector3.zero, halfScale, completed);
    }

    public void EnlargeToNormal(System.Action completed = null)
    {
        Enlarge(halfScale, normalScale, completed);
    }

    IEnumerator CR_Enlarge(Vector3 startScale, Vector3 endScale, System.Action completed)
    {
        float t = 0;
        transform.localScale = startScale;
        while (t < enlargeTime)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, t / enlargeTime);
            yield return null;
        }

        completed?.Invoke();
    }
    

}
