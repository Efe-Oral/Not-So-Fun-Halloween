using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyConfig enemyConfig;

    

    private void Awake()
    {
       
            gameObject.transform.localScale = new Vector3(enemyConfig.scaleX, enemyConfig.scaleY, enemyConfig.scaleZ);
        
    }
}
