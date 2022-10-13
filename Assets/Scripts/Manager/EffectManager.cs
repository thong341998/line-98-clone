using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private GameObject ballExplosion;

    

    private void Awake()
    {
        BallController.OnBallDestroyed += BallController_OnBallDestroyed;
    }

    

    private void OnDestroy()
    {
        BallController.OnBallDestroyed -= BallController_OnBallDestroyed;
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void BallController_OnBallDestroyed(BallController ballController)
    {
        var effect = SpawnEffect(ballExplosion, ballController.transform.position, Quaternion.identity, transform).GetComponent<ParticleSystem>();
        var settings = effect.main;
        settings.startColor = ballController.BallColor;
        
    }

    private GameObject SpawnEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var ins = Instantiate(effectPrefab,position,rotation,parent);

        return ins;
    }

    
}
