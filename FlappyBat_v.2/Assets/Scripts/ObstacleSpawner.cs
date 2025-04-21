using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FlappyBatGame.Controllers;
using FlappyBatGame.Managers;
using UnityEngine;

namespace FlappyBatGame.Spawners
{
    public class ObstacleSpawner : MonoBehaviour
    {
        public float spawnerInterval = 2f;
        public float gapSize = 2f;
        public PoolManager poolManager;
            
        
        
        //Ассинхронный цикл спавна препятствий
        public async UniTaskVoid StartSpawning(CancellationToken token)
        {
            if (poolManager == null)
            {
                Debug.LogError("PoolManager is null!");
                return;
            }
            
            if (Camera.main == null)
            {
                Debug.LogError("Camera main is null!");
                return;
            }

            if (poolManager == null)
            {
                Debug.LogError("Spawner: PoolManager is null!");
                return;
            }
            
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(spawnerInterval), cancellationToken: token);

                GameObject obsObj = poolManager.GetObstacle();
                Debug.Log($"[ObstacleSpawner] Попытка получить obstacle: {obsObj}");
                if (obsObj != null)
                {
                    obsObj.SetActive(true);
                    Debug.Log("Obstacle активирован");
                    float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;
                    float spawnX = Camera.main.transform.position.x + screenWidth + 1f;
                    float gapCenterY = UnityEngine.Random.Range(-2f, 2f);
                    Vector3 spawnPos = new Vector3(spawnX, 0f, 0f);
                    
                    ObstacleController oc = obsObj.GetComponent<ObstacleController>();
                    if (oc != null)
                    {
                        oc.SetupObstacle(spawnPos, gapCenterY, gapSize);
                    }
                }
            }
        }
    }
}

