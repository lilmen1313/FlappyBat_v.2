using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlappyBatGame.Controllers
{
    public class ObstacleController : MonoBehaviour
    {
        public float obstacleSpeed = 2f;
        public float minHeightBottomObstacle = 3.85f;
        public float minHeightTopObstacle = 2f;
        public bool scored = false; //флаг, для начисление счетчика пролетевших препятствий

        private Transform _top, _bottom;
        private SpriteRenderer _topSR, _bottomSR;

        private void Awake()
        {
            //один раз ищем нужные компоненты
            _top = transform.Find("ObstacleTop");
            _bottom = transform.Find("ObstacleBottom");
            if (_top != null) _topSR = _top.GetComponent<SpriteRenderer>();
            if (_bottom != null) _bottomSR = _bottom.GetComponent<SpriteRenderer>();
        }
        private void OnEnable()
        {
            scored = false;
        }
        
        //Позиционирование и масштабирование верхней и нижней частей
        public void SetupObstacle(Vector3 spawnPosition, float gapCenterY, float gapSize)
        {
            transform.position = spawnPosition;

            if (_topSR != null && _bottomSR != null && Camera.main != null)
            {
                float screenTop = Camera.main.transform.position.y + Camera.main.orthographicSize;
                float screenBottom = Camera.main.transform.position.y - Camera.main.orthographicSize;
                
                //Верхнее препятствие
                float desiredTopHeight = screenTop - (gapCenterY + (gapSize / 2)); //Ожидаемая высота
                float topHeight = Math.Max(desiredTopHeight, minHeightTopObstacle);
                float topCenterY = screenTop - (topHeight / 2f); // Позиция центра по Y
                
                _topSR.size = new Vector2(_topSR.size.x, topHeight); //тянем
                _topSR.transform.position = new Vector3(spawnPosition.x, topCenterY, spawnPosition.z);
                
                //Нижнее препятствие
                float desiredBottomHeight = (gapCenterY - (gapSize / 2)) - screenBottom; //Ожидаемая высота
                float bottomHeight = Math.Max(desiredBottomHeight, minHeightBottomObstacle);
                float bottomCenterY = screenBottom + (bottomHeight / 2f); // Позиция центра по Y
                
                _bottomSR.size = new Vector2(_bottomSR.size.x, bottomHeight); //тянем
                _bottomSR.transform.position = new Vector3(spawnPosition.x, bottomCenterY, spawnPosition.z);
            }
            else
            {
                Debug.LogWarning("ObstacleController: One or more required components not found!");
            }
        }
        
        // Перемещение препятствий
        public void Move()
        {
            transform.position += Vector3.left * obstacleSpeed * Time.deltaTime;
        }
        
        public float GetRightPointMax()
        {
            if (_topSR != null && _bottomSR != null)
            {
                float topRightPoint = _topSR.bounds.max.x;
                float bottomRightPoint = _bottomSR.bounds.max.x;
                
                return Mathf.Max(topRightPoint, bottomRightPoint);
            }
            
            Debug.LogWarning("ObstacleController: SpriteRenderers not assigned!");
            return float.NegativeInfinity; // Если не найдено, возвращаем минимальное значение
        }
    }
}