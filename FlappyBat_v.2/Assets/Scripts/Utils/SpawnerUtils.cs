using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnerUtils
{
    public static float GetSpawnX(Camera cam, float offset = 1f) //генерация позиции спавна
    {
        if (cam == null)
        {
            Debug.LogWarning("SpawnerUtils.GetSpawnX: Camera is null!");
            return 0f;
        }
        
        float screenWidth = cam.orthographicSize * cam.aspect;
        return cam.transform.position.x + screenWidth + offset;;
    }
}
