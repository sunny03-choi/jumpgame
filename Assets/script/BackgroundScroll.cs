using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [Header("스크롤 세팅")]
    [Tooltip("텍스처의 속도를 얼마나 빠르게 해야 하는지 조정")]
    public float scrollSpeed;

    [Header("레퍼런스")]
    public MeshRenderer meshRenderer;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        meshRenderer.material.mainTextureOffset += new Vector2(GameManager.Instance.CalculateGameSpeed() / 20 * scrollSpeed * Time.deltaTime, 0);
    }
}

