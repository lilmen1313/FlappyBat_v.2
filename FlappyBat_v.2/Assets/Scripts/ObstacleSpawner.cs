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
        public PoolManager poolManager;
        private Camera mainCamera;   // Кэш камеры
        public float GapSize { get; set; }

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        //Ассинхронный цикл спавна препятствий
        public async UniTaskVoid StartSpawning(CancellationToken token)
        {
            if (Camera.main == null)
            {
                Debug.LogError("Camera main is null!");
                return;
            }

            if (poolManager == null)
            {
                Debug.LogError("PoolManager is null!");
                return;
            }
            
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(spawnerInterval), cancellationToken: token);

                GameObject obsObj = poolManager.GetObstacle();
                if (obsObj != null)
                {
                    obsObj.SetActive(true);
                    Debug.Log("Obstacle активирован");

                    float spawnX = SpawnerUtils.GetSpawnX(mainCamera);
                    float gapCenterY = UnityEngine.Random.Range(-2, 2);
                    
                    Vector3 spawnPos = new Vector3(spawnX,0f, 0f);
                    
                    ObstacleController oc = obsObj.GetComponent<ObstacleController>();
                    if (oc != null)
                    {
                        oc.SetupObstacle(spawnPos, gapCenterY, GapSize);
                    }
                }
            }
        }
    }
}