using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("세팅")]
    public float minSpawnDelay;
    public float maxSpawnDelay;

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
        //  GameObject randomObj = buildings[Random.Range(0, buildings.Length)]; 위와 동일한 코드임
        Instantiate(randomObj, transform.position, Quaternion.identity); // 선택된 프리팹을 현재 위치에 생성
        Invoke("Spawn", Random.Range(minSpawnDelay, maxSpawnDelay));
    }
}
