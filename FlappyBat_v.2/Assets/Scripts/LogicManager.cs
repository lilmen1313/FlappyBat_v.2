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
    public enum GameState
    {
        Srart,
        Playing,
        GameOver
    }
    public class LogicManager : MonoBehaviour
    {
        private GameState gameState = GameState.Srart;
        
        [Header("Dynamic Prefab Names (in Prefabs)")]
        public string batPrefabName = "Prefabs/BatPrefab";
        public string poolManagerPrefabName = "Prefabs/PoolManager";
        public string obstacleSpawnerPrefabName = "Prefabs/ObstacleSpawner";
        public string rewardSpawnerPrefabName = "Prefabs/RewardSpawner";
        public string backgroundPrefabName = "Prefabs/GameplayBackgroundPrefab";
        
        [Header("Game settings")]
        public Vector3 batStartPosition = new Vector3(-3f, 0f, 0f);
        public float gapSize = 2f;
        
        [Header("Links to components")]
        private BatController bat;
        private PoolManager poolManager;
        private ObstacleSpawner obstacleSpawner;
        private RewardSpawner rewardSpawner;
        private GameObject background;
        private ParallaxMountains mountainsLayer;
        public UIManager uiManager;
        
        private CancellationTokenSource spawnCTS;
        private int score = 0;
        private Camera mainCamera; // кэширую ссылку на камеру
        
        private void Awake()
        {
            mainCamera = Camera.main;

            CreateBackgrounds();
            CreateMountains();
                
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
                    obstacleSpawner.GapSize = gapSize;
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
            SetGameState(GameState.Srart);
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
            
            // Стартуем геймплейный экран через централизованный метод
            SetGameState(GameState.Playing);
            
            spawnCTS = new CancellationTokenSource();
            CancellationToken token = spawnCTS.Token;
            
            obstacleSpawner.StartSpawning(token).Forget();
            rewardSpawner.StartSpawning(token).Forget();

            Debug.Log("Game started");
        }
        
        //сброс игры - остановка спавнеров и деактивация объектов из пулов
        public void ResetGame()
        {
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
            if (gameState != GameState.Playing)
                return;
            
            bool flapInput = Input.GetKeyDown(KeyCode.Space);

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) flapInput = true;
            }
            
            if (bat != null && flapInput) bat.Flap();
            
            // остальная логика движения и проверки
            if (bat != null && bat.gameObject.activeInHierarchy && poolManager != null)
            {
                bat.UpdatePosition();

                if (IsBatOutOfScreen())
                {
                    GameOver();
                    return;
                }
                
                CheckCollisions();
                UpdateMovingObjects();
            }
        }

        // Перемещает активные объекты
        private void UpdateMovingObjects()
        {
            float screenWidth = mainCamera.orthographicSize * mainCamera.aspect;
            float leftBound = mainCamera.transform.position.x - screenWidth - 1f;
            
            float batLeft = bat.GetLeftPoint();
            
            //обновление препятствий
            foreach (GameObject obs in poolManager.obstaclePool)
            {
                if (obs.activeInHierarchy)
                {
                    ObstacleController oc = obs.GetComponent<ObstacleController>();
                    if (oc != null)
                    {
                        oc.Move(); //двигаем
                        float obstacleRight = oc.GetRightPointMax(); // позиция правой стороны самой шировкой половинки
                        
                        if (obstacleRight < batLeft && !oc.scored)
                        {
                            score++;
                            oc.scored = true;
                            uiManager.UpdateScore(score);
                        }
                    
                        // Если препятствие полностью ушло за левую границу экрана, деактивируем его
                        if (obstacleRight < leftBound) obs.SetActive(false);
                    }
                    else
                    {
                        Debug.LogWarning("Obstacle in pool without ObstacleController!");
                    }
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
                        
                        RewardController rc = rew.GetComponent<RewardController>();
                        if (rc != null) rc.EffectReward();
                    }
                }
            }
        }

        private void GameOver()
        {
            if (bat != null)
                bat.gameObject.SetActive(false);
            
            if (spawnCTS != null)
            {
                spawnCTS.Cancel();
                spawnCTS.Dispose();
                spawnCTS = null;
            }
            
            if (poolManager != null)
                poolManager.ResetPools();
            
            SetGameState(GameState.GameOver);
        }
        
        //метод для смены состояния и вызова нужного экрана/действий
        private void SetGameState(GameState newState)
        {
            gameState = newState;

            switch (gameState)
            {
                case GameState.Srart:
                    uiManager.ShowStartScreen();
                    break;
                case GameState.Playing:
                    uiManager.ShowGameplayScreen();
                    break;
                case GameState.GameOver:
                    uiManager.ShowGameOverScreen();
                    break;
            }
        }

        private bool IsBatOutOfScreen()
        {
            if (bat ==null) return false;
            
            float camY = mainCamera.transform.position.y;
            float camHalfHeight = mainCamera.orthographicSize;
            float screenTop = camY + camHalfHeight;
            float screenBottom = camY - camHalfHeight;
            
            float batTopY = bat.GetTopPoint().y;
            float batBottomY = bat.GetBottomPoint().y;
            
            if (batTopY > screenTop || batBottomY < screenBottom)
                return true;
            
            return false;
        }

        private void CreateBackgrounds()
        {
            if (background != null) return;
            
            GameObject bgPrefab = Resources.Load<GameObject>(backgroundPrefabName);
            if (bgPrefab != null)
            {
                background = Instantiate(bgPrefab);
                background.name = "GameplayBackground";
                background.transform.position = Vector3.zero;
            }
            else
            {
                Debug.LogError("BackgroundPrefab not found in Resources!");
            }
        }

        private void CreateMountains()
        {
            if (mountainsLayer != null) return;

            var prefab = Resources.Load<GameObject>("MountainsPrefab");
            if (prefab == null)
            {
                Debug.LogError("MountainsPrefab not found in Resources!");
                return;
            }

            var go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            mountainsLayer = go.GetComponent<ParallaxMountains>();
        }
    }
}