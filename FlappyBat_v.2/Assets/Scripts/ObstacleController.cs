using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlappyBatGame.Controllers
{
    public class ObstacleController : MonoBehaviour
    {
        public float obstacleSpeed = 2f;
        public bool scored = false; //флаг, для начисление счетчика пролетевших препятствий

        private void OnEnable()
        {
            scored = false;
        }
        
        //Позиционирование и масштабирование верхней и нижней частей
        public void SetupObstacle(Vector3 spawnPosition, float gapCenterY, float gapSize)
        {
            transform.position = spawnPosition;
            Transform top = transform.Find("ObstacleTop");
            Transform bottom = transform.Find("ObstacleBottom");

            if (top != null && bottom != null && Camera.main != null)
            {
                float screenTop = Camera.main.transform.position.y + Camera.main.orthographicSize;
                float screenBottom = Camera.main.transform.position.y - Camera.main.orthographicSize;
                
                //Верхнее препятствие
                float desiredTopHeight = screenTop - (gapCenterY + gapSize / 2); //Ожидаемая высота
                float topCenterY = desiredTopHeight / 2 + gapCenterY + gapSize / 2; // Позиция центра по Y
                
                SpriteRenderer topSR = top.GetComponent<SpriteRenderer>();
                // float topRightBound = topSR.bounds.max.x;
                topSR.size = new Vector2(topSR.size.x, desiredTopHeight); //тянем
                topSR.transform.position = new Vector3(spawnPosition.x, topCenterY, spawnPosition.z);
                
                //Нижнее препятствие
                float desiredBottomHeight = (gapCenterY - gapSize / 2) - screenBottom; //Ожидаемая высота
                float bottomCenterY = gapCenterY - gapSize / 2 - desiredBottomHeight / 2; // Позиция центра по Y
                
                SpriteRenderer bottomSR = bottom.GetComponent<SpriteRenderer>();
                // float bottomRightBound = bottomSR.bounds.max.x;
                bottomSR.size = new Vector2(bottomSR.size.x, desiredBottomHeight); //тянем
                bottomSR.transform.position = new Vector3(spawnPosition.x, bottomCenterY, spawnPosition.z);
            }
        }
        
        // Перемещение препятствий
        public void Move()
        {
            transform.position += Vector3.left * obstacleSpeed * Time.deltaTime;
        }
        
        public float GetRightPointMax()
        {
            Transform top = transform.Find("ObstacleTop");
            Transform bottom = transform.Find("ObstacleBottom");
            
            if (top != null && bottom != null)
            {
                SpriteRenderer topSR = top.GetComponent<SpriteRenderer>();
                float topRightPoint = topSR.bounds.max.x;
                
                SpriteRenderer bottomSR = bottom.GetComponent<SpriteRenderer>();
                float bottomRightPoint = bottomSR.bounds.max.x;
                
                return Mathf.Max(topRightPoint, bottomRightPoint);
            }

            return float.NegativeInfinity; // Если не найдено, возвращаем минимальное значение
        }
    }
}