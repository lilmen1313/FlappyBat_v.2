using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlappyBatGame.Controllers
{
    public class BatController : MonoBehaviour
    {
        public float gravity = 9.8f;
        public float flapStrength = 5f; //what?
        private float verticalSpeed = 0;
        
        public Sprite[] batFrames;
        public float animationFPS = 10f;
        private int currentFrame = 0;
        private float timer = 0f;

        private SpriteRenderer sr; // кэширую SpriteRenderer
        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null) Debug.LogError("BatController: SpriteRenderer not found!");
        }

        //Сброс мыши в начальное состояние
        public void ResetPosition(Vector3 startPosition)
        {
            transform.position = startPosition;
            verticalSpeed = 0f;
            gameObject.SetActive(true);
        }
        
        //прыжок
        public void Flap()
        {
            verticalSpeed = flapStrength;
        }
        
        //Обновление позиции птички (только гравитация и движение)
        public void UpdatePosition()
        {
            if (!gameObject.activeInHierarchy)
                return;
            
            AnimationBat();

            verticalSpeed -= gravity * Time.deltaTime;
            transform.position += new Vector3(0f, verticalSpeed * Time.deltaTime, 0f);
        }

        //Получение верхней точки бордера объекта
        public Vector3 GetTopPoint()
        {
            if (sr == null) return transform.position;
            float worldMaxY = sr.bounds.max.y;
            float borderTopInWorld = sr.sprite.border.w / sr.sprite.pixelsPerUnit; // размер бордера в мировых
            float borderTopY = worldMaxY - borderTopInWorld * transform.lossyScale.y; 
            
            return new Vector3(transform.position.x, borderTopY, transform.position.z);
        }

        //Получение нижней точки бордера объекта
        public Vector3 GetBottomPoint()
        {
            if (sr == null) return transform.position;
            float worldMinY = sr.bounds.min.y;
            float borderBottomInWorld = sr.sprite.border.y / sr.sprite.pixelsPerUnit; // размер бордера в мировых
            float borderBottomY = worldMinY + borderBottomInWorld * transform.lossyScale.y;

            return new Vector3(transform.position.x, borderBottomY, transform.position.z);
        }
        
        //Получение левой точки спрайта объекта
        public float GetLeftPoint()
        {
            if (sr == null)
            {
               Debug.LogWarning("BatController: SpriteRenderer not found!");
               return transform.position.x;
            }
            return sr.bounds.min.x;
        }

        void AnimationBat()
        {
            if (batFrames == null || batFrames.Length == 0) return;
            
            timer += Time.deltaTime;
            if (timer >= 1f / animationFPS)
            {
                timer = 0f;
                currentFrame = (currentFrame + 1) % batFrames.Length;
                sr.sprite = batFrames[currentFrame];
            }
        }
    }
}