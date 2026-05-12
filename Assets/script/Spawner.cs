using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("세팅")]
    public float minSpawnDelay;
    public float maxSpawnDelay;
    public float minYOffset = 0f; // 추가: 최소 높이 오프셋
    public float maxYOffset = 0f; // 추가: 최대 높이 오프셋

    [Header("레퍼런스")]
    public GameObject[] gameObjects; // 생성할 프리팹

    void OnEnable()
    {
        Invoke("Spawn", Random.Range(minSpawnDelay, maxSpawnDelay)); // 2초 후에 Spawn 메서드 호출
    }
    private void OnDisable()
    {
        CancelInvoke(); // 스폰 중지
    }


    void Spawn()
    {
        var randomObj = gameObjects[Random.Range(0, gameObjects.Length)]; // 랜덤으로 프리팹 선택
        
        // Y 위치 랜덤 적용
        Vector3 spawnPos = transform.position;
        spawnPos.y += Random.Range(minYOffset, maxYOffset);

        Instantiate(randomObj, spawnPos, Quaternion.identity); // 선택된 프리팹을 랜덤 높이에 생성
        Invoke("Spawn", Random.Range(minSpawnDelay, maxSpawnDelay));
    }
}
