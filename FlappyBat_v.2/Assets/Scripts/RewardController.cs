using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FlappyBatGame.Controllers
{
    public class RewardController : MonoBehaviour
    {
        public float rewardSpeed = 2f;
        
        public GameObject flashLayerObject;
        public Sprite flashSprite1;
        public Sprite flashSprite2;
        public float flashDuration1 = 0.07f;
        public float flashDuration2 = 0.08f;
        private bool isFlashing = false;
        
        private SpriteRenderer sr;
        private SpriteRenderer flashSR;
        
        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null) Debug.LogError("RewardController: SpriteRenderer not found!");
            
            if (flashLayerObject != null) flashSR = flashLayerObject.GetComponent<SpriteRenderer>();
            
            if (flashSR == null) Debug.LogError("RewardController: Flash SpriteRenderer not found!");
        }

        public void SetupReward(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
            if (flashLayerObject != null) flashLayerObject.SetActive(false);
            isFlashing = false;
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

        public void EffectReward()
        {
            if (!isFlashing) EffectRewardAsync().Forget();
        }

        private async UniTaskVoid EffectRewardAsync()
        {
            if (flashLayerObject == null || flashSR == null)
                return;
            
            isFlashing = true;
            
            flashLayerObject.SetActive(true);
            flashSR.sprite = flashSprite1;
            await UniTask.Delay(System.TimeSpan.FromSeconds(flashDuration1));
            
            flashSR.sprite = flashSprite2;
            await UniTask.Delay(System.TimeSpan.FromSeconds(flashDuration2));
            
            flashLayerObject.SetActive(false);
            gameObject.SetActive(false);
            
            isFlashing = false;
        }
    }
}