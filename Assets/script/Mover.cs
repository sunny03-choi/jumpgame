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
        transform.position += Vector3.left *
            GameManager.Instance.CalculateGameSpeed() * Time.deltaTime; // 왼쪽으로 이동
        // Vector3.left == new Vector3(-1, 0, 0)
    }
}


