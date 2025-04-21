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

        //Сброс мыши в начальное состояние
        public void ResetPosition(Vector3 startPosition)
        {
            transform.position = startPosition;
            verticalSpeed = 0f;
            gameObject.SetActive(true);
        }

        //Обновление позиции птички (вызов из GameController.Update)
        public void UpdatePosition()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (Input.GetKeyDown(KeyCode.Space)) //research
            {
                verticalSpeed = flapStrength;
            }

            verticalSpeed -= gravity * Time.deltaTime;
            transform.position += new Vector3(0f, verticalSpeed * Time.deltaTime, 0f);
        }

        //Получение верхней точки спрайта объекта
        public Vector3 GetTopPoint()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            return transform.position + new Vector3(0, sr.bounds.size.y / 2, 0);
        }

        //Получение нижней точки спрайта объекта
        public Vector3 GetBottomPoint()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            return transform.position - new Vector3(0, sr.bounds.size.y / 2, 0);
        }
        
        //Получение левой точки спрайта объекта
        public float GetLeftPoint()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            return sr.bounds.min.x;
        }
    }
}