using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPos;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// 카메라를 일정 시간 동안 흔듭니다.
    /// </summary>
    /// <param name="duration">흔들림 지속 시간</param>
    /// <param name="magnitude">흔들림 강도</param>
    public IEnumerator Shake(float duration, float magnitude)
    {
        originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
