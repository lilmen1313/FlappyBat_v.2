using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMountains : MonoBehaviour
{
    [Tooltip("Скорость движения гор (обычно = скорости препятствий)")]
    public float speed = 2f;

    [Tooltip("Ширина одной горы в world units (например, 8.35)")]
    public float spriteWidth = 14.64f;

    void Start()
    {
        // При запуске ставим обе горы, чтобы занимали ширину фона (2*width)
        // Обычно уже выставлено руками, но можно проконтролировать:
        if (transform.childCount >= 2)
        {
            transform.GetChild(0).localPosition = Vector3.zero;
            transform.GetChild(1).localPosition = new Vector3(spriteWidth, 0, 0);
        }
    }

    void Update()
    {
        // Двигаем весь слой гор влево
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Как только сдвиг ушёл на одну "половинку" (spriteWidth), возвращаем слой вправо на spriteWidth
        if (transform.position.x <= -spriteWidth)
        {
            transform.position += new Vector3(spriteWidth, 0, 0);
        }
    }
}
