using UnityEngine;

public class Heart : MonoBehaviour
{
    [Header("레퍼런스")]
    public Sprite OnHeart; //체력이 있을 때 하트 이미지
    public Sprite OffHeart; //체력이 없을 때 하트 이미지
    public SpriteRenderer spriteRenderer; //스프라이트 렌더러

    [Header("세팅")]
    public int heartIndex; //하트의 번호 (1, 2, 3)
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.lives >= heartIndex)
        {
            spriteRenderer.sprite = OnHeart; //체력이 있을 때 하트 이미지로 변경
        }
        else
        {
            spriteRenderer.sprite = OffHeart; //체력이 없을 때 하트 이미지로 변경
        }
    }
}
