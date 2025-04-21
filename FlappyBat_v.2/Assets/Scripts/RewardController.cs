using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlappyBatGame.Controllers
{
    public class RewardController : MonoBehaviour
    {
        public float rewardSpeed = 2f;
        private SpriteRenderer sr;
        
        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        public void SetupReward(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
        }

        public void Move()
        {
            transform.position += Vector3.left * rewardSpeed * Time.deltaTime;
        }
        
        public float GetRightPoint()
        {
            if (sr != null) return sr.bounds.max.x;
            return float.NegativeInfinity;
        }
    }
}