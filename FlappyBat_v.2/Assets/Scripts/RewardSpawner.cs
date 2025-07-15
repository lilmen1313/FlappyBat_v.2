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
    public class RewardSpawner : MonoBehaviour
    {
        public float spawnInterval = 2f;
        public float rewardSpawnOffsetX = 2f;
        public PoolManager poolManager;
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

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
            
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), cancellationToken: token);
                GameObject obsObj = poolManager.GetReward();
                
                if (obsObj != null)
                {
                    obsObj.SetActive(true);
                    Debug.Log("Reward активирован");
                    
                    float spawnX = SpawnerUtils.GetSpawnX(mainCamera);
                    float spawnY = UnityEngine.Random.Range(-4f, 4f);
                    
                    Vector3 spawnPos = new Vector3(spawnX + rewardSpawnOffsetX, spawnY, 0f);
                    
                    RewardController rc = obsObj.GetComponent<RewardController>();
                    if (rc != null)
                    {
                        rc.SetupReward(spawnPos);
                    }
                }
            }
        }
    }
}