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
                await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), cancellationToken: token);
                GameObject obsObj = poolManager.GetReward();
                Debug.Log($"[RewardSpawner] Попытка получить reward: {obsObj}");
                if (obsObj != null)
                {
                    obsObj.SetActive(true);
                    Debug.Log("Reward активирован");
                    float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;
                    float spawnX = Camera.main.transform.position.x + screenWidth + 1f;
                    float spawnY = UnityEngine.Random.Range(-4f, 4f);
                    Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);
                    
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