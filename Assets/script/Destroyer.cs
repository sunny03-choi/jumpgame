using UnityEngine;

public class Destroyer : MonoBehaviour
{
    public float deltePoint = -18f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < deltePoint) // x좌표가 -10보다 작아지면
        {
            Destroy(gameObject); // 이 게임 오브젝트를 제거
        }
    }
}

