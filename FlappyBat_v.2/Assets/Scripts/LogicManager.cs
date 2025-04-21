using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using FlappyBatGame.Controllers;
using FlappyBatGame.Spawners;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace FlappyBatGame.Managers
{
    public class LogicManager : MonoBehaviour
    {
        [Header("Dynamic Prefab Names (in Prefabs)")]
        public string batPrefabName = "Prefabs/BatPrefab.prefab";
        public string poolManagerPrefabName = "Prefabs/PoolManager.prefab";
        public string obstacleSpawnerPrefabName = "Prefabs/ObstacleSpawner.prefab";
        public string rewardSpawnerPrefabName = "Prefabs/RewardSpawner.prefab";
        
        
        [Header("Game settings")]
        public Vector3 batStartPosition = new Vector3(-3f, 0f, 0f);
        public float gapSize = 2f;
        
        [Header("Links to components")]
        public BatController bat;
        public PoolManager poolManager;
        public ObstacleSpawner obstacleSpawner;
        public RewardSpawner rewardSpawner;
        public UIManager uiManager;
        
        private CancellationTokenSource spawnCTS;
        private bool gameOver = false;
        private int score = 0;

        private void Awake()
        {
            //Создаем PoolManager из префаба
            if (poolManager == null)
            {
                GameObject poolManagerPrefab = Resources.Load<GameObject>(poolManagerPrefabName);
                if (poolManagerPrefab != null)
                {
                    GameObject poolObj = Instantiate(poolManagerPrefab, transform);
                    poolObj.name = "PoolManager";
                    poolManager = poolObj.GetComponent<PoolManager>();
                }
                else
                {
                    Debug.LogError("PoolManagerPrefab not found in Resources!");
                }
            }

            // Создаем ObstacleSpawner из префаба
            if (obstacleSpawner == null)
            {
                GameObject obstacleSpawnerPrefab = Resources.Load<GameObject>(obstacleSpawnerPrefabName);
                if (obstacleSpawnerPrefab != null)
                {
                    GameObject obsSpawnerObj = Instantiate(obstacleSpawnerPrefab, transform);
                    obsSpawnerObj.name = "ObstacleSpawner";
                    obstacleSpawner = obsSpawnerObj.GetComponent<ObstacleSpawner>();
                    obstacleSpawner.gapSize = gapSize;
                    obstacleSpawner.poolManager = poolManager;
                }
                else
                {
                    Debug.LogError("ObstacleSpawnerPrefab not found in Resources!");
                }
            }

            // Создаем RewardSpawner из префаба
            if (rewardSpawner == null)
            {
                GameObject rewardSpawnerPrefab = Resources.Load<GameObject>(rewardSpawnerPrefabName);
                if (rewardSpawnerPrefab != null)
                {
                    GameObject rewSpawnerObj = Instantiate(rewardSpawnerPrefab, transform);
                    rewSpawnerObj.name = "RewardSpawner";
                    rewardSpawner = rewSpawnerObj.GetComponent<RewardSpawner>();
                    rewardSpawner.poolManager = poolManager;
                    Debug.Log($"RewardSpawnerPrefab найден по пути: {rewardSpawnerPrefabName}");
                }
                else
                {
                    Debug.LogError("RewardSpawnerPrefab not found in Resources!");
                }
            }
        }

        //кнопка Start
        public void StartGame()
        {
            // Создаем bat (игровой объект) из префаба
            if (bat == null)
            {
                GameObject batPrefab = Resources.Load<GameObject>(batPrefabName);
                if (batPrefab != null)
                {
                    GameObject batObj = Instantiate(batPrefab, transform);
                    batObj.name = "Bat";
                    bat = batObj.GetComponent<BatController>();
                }
                else
                {
                    Debug.LogError("BatPrefab not found in Resources!");
                    return;
                }
            }
            
            if (bat != null) bat.ResetPosition(batStartPosition);
            
            score = 0;
            uiManager.UpdateScore(score);
            uiManager.ShowGameplayScreen();
            
            spawnCTS = new CancellationTokenSource();
            CancellationToken token = spawnCTS.Token;
            
            obstacleSpawner.StartSpawning(token).Forget();
            rewardSpawner.StartSpawning(token).Forget();

            Debug.Log("Game started");
        }
        
        //сброс игры - остановка спавнеров и деактивация объектов из пулов
        public void ResetGame()
        {
            gameOver = false;

            if (bat != null)
            {
                Destroy(bat.gameObject);
                bat = null;
            }
            
            if (spawnCTS != null)
            {
                spawnCTS.Cancel();
                spawnCTS.Dispose();
                spawnCTS = null;
            }
            
            poolManager.ResetPools();
        }

        private void Update()
        {
            if (gameOver)
                return;
        
            if (bat != null && bat.gameObject.activeInHierarchy && poolManager != null)
            {
                bat.UpdatePosition();
                CheckCollisions();
                UpdateMovingObjects();
            }
        }

        // Перемещает активные объекты
        private void UpdateMovingObjects()
        {
            float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;
            float leftBound = Camera.main.transform.position.x - screenWidth - 1f;
            
            float batLeft = bat.GetLeftPoint();
            
            
            //обновление препятствий
            foreach (GameObject obs in poolManager.obstaclePool)
            {
                if (obs.activeInHierarchy)
                {
                    ObstacleController oc = obs.GetComponent<ObstacleController>();
                    if (oc != null) oc.Move(); //двигаем

                    float obstacleRight = oc.GetRightPointMax(); // позиция правой стороны самой шировкой половинки

                    if (obstacleRight < batLeft && oc != null && !oc.scored)
                    {
                        score++;
                        oc.scored = true;
                        uiManager.UpdateScore(score);
                    }
                    
                    // Если препятствие полностью ушло за левую границу экрана, деактивируем его
                    if (obstacleRight < leftBound && oc != null) obs.SetActive(false);
                }
            }
            
            // Обновление наград
            foreach (GameObject rew in poolManager.rewardPool)
            {
                if (rew.activeInHierarchy)
                {
                    RewardController rc = rew.GetComponent<RewardController>();
                    if (rc != null) rc.Move();

                    float rewardRight = rc.GetRightPoint();
                        
                    if (rewardRight < leftBound)
                    {
                        rew.SetActive(false);
                    }
                }
            }
        }

        private void CheckCollisions()
        {
            Vector3 batTop = bat.GetTopPoint();
            Vector3 batBottom = bat.GetBottomPoint();

            // Проверка столкновений с препятствиями
            foreach (GameObject obs in poolManager.obstaclePool)
            {
                if (obs.activeInHierarchy)
                {
                    foreach (Transform child in obs.transform)
                    {
                        SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                        
                        if (sr != null && (sr.bounds.Contains(batTop) || sr.bounds.Contains(batBottom)))
                        {
                            Debug.Log("Столкновение с препятствием!");
                            GameOver();
                            return;
                        }
                    }
                }
            }

            // Проверка столкновений с наградами
            foreach (GameObject rew in poolManager.rewardPool)
            {
                if (rew.activeInHierarchy)
                {
                    SpriteRenderer sr = rew.GetComponent<SpriteRenderer>();
                    if (sr != null && (sr.bounds.Contains(batTop) || sr.bounds.Contains(batBottom)))
                    {
                        Debug.Log("Награда собрана!");
                        rew.SetActive(false);
                        // RewardController rc = rew.GetComponent<RewardController>();
                        // if (rc != null)
                        //     rc.CollectReward(); // Запускает анимацию и деактивирует награду после анимации
                    }
                }
            }
        }

        private void GameOver()
        {
            gameOver = true;
            if (bat != null)
                bat.gameObject.SetActive(false);

            uiManager.ShowGameOverScreen();
        }
    }
}