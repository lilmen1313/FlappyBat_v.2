using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlappyBatGame.Managers
{
    public class PoolManager : MonoBehaviour
    {
        [Header("Obstacle Pool Settings")]
        public GameObject obstaclePrefab;
        public int obstaclePoolSize = 10;
        
        [Header("Reward Pool Settings")]
        public GameObject rewardPrefab;
        public int rewardPoolSize = 5;
        
        private List<GameObject> _obstaclePool = new List<GameObject>();
        private List<GameObject> _rewardPool = new List<GameObject>();

        public IReadOnlyList<GameObject> obstaclePool => _obstaclePool;
        public IReadOnlyList<GameObject> rewardPool => _rewardPool;
        
        private void Awake()
        {
            Debug.Log(">>> PoolManager.Awake вызван <<<");
            
            if (obstaclePrefab == null)
                Debug.LogError("Obstacle Prefab not assigned!");
            if (rewardPrefab == null)
                Debug.LogError("Reward Prefab not assigned!");
            
            //создание пула препятствий
            for (int i = 0; i < obstaclePoolSize; i++)
            {
                GameObject obs = Instantiate(obstaclePrefab, transform);
                obs.SetActive(false);
                _obstaclePool.Add(obs);
            }
            
            //создание пула наград
            for (int i = 0; i < rewardPoolSize; i++)
            {
                GameObject rew = Instantiate(rewardPrefab, transform);
                rew.SetActive(false);
                _rewardPool.Add(rew);
            }
        }
        
        //возвращает неактивное препятствие
        public GameObject GetObstacle()
        {
            foreach (GameObject obs in _obstaclePool)
            {
                if (!obs.activeInHierarchy) return obs;
            }
            return null;
        }
        
        //возвращает неактивную награду
        public GameObject GetReward()
        {
            foreach (GameObject rew in _rewardPool)
            {
                if (!rew.activeInHierarchy) return rew;
            }
            return null;
        }
        
        //деактивация всех объектов из пула
        public void ResetPools()
        {
            Debug.Log("ResetPools called");
            foreach (GameObject obs in _obstaclePool)
            {
                obs.SetActive(false);
            }

            foreach (GameObject rew in _rewardPool)
            {
                rew.SetActive(false);
            }
        }
    }
}