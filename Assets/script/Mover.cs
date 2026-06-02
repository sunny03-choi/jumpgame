using UnityEngine;

public class Mover : MonoBehaviour
{
    [Header("세팅")]
    public float moveSpeed = 1f; // 이동 속도
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // 기본 게임 속도 + 개별 속도
        float totalSpeed = GameManager.Instance.CalculateGameSpeed() + moveSpeed;
        transform.position += Vector3.left * totalSpeed * Time.deltaTime; // 왼쪽으로 이동
    }
}
