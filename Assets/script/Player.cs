using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("세팅")]
    public float jumpForce; //public을 넣으면 코드 외부 인스펙터에서 조정 가능

    [Header("레퍼런스")]
    public Rigidbody2D rb;
    /* rb 라는 이름의 Rigidbody2D 컴포넌트를 참조할 수 있도록 선언
    심볼 까먹으면 안됨! */
    public Animator playerAnimator;
    public BoxCollider2D playerCollider; // <- 이게 추가됨

    private bool isGrounded = true; //땅에 닿아있는지 여부를 저장하는 변수
    private bool canDoubleJump = false; //더블 점프 가능 여부를 저장하는 변수

    public bool isInvincible = false; //무적 상태 여부

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // 시작 위치 저장
    }

    void Update()
    {
        /*
        점프 같은 입력 감지는 Update에서 하는 것이 좋음
        만약 초당 400프레임이 나온다면 유저가 스페이스를 누르는지 초당 400번 확인함

        게임을 많이 플레이를 한다면 입력이 밀리는게 얼마나 기분이 나쁜지 잘 알 것
        */

        // Space 키를 누르면 점프하는 코드

        /*
        linearVelocityY 라는 이름의 Rigidbody2D 컴포넌트의 속성에 jumpForce 값을 할당하는 코드
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocityY = jumpForce; //점프할 때마다 점프포스 만큼의 속도를 주는 코드
        }
        */

        if (GameManager.Instance.state == GameState.Playing && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            if (isGrounded)
            {
                rb.linearVelocityY = 0; //속도를 초기화하여 일정한 점프 높이 유지
                rb.AddForceY(jumpForce, ForceMode2D.Impulse); //점프할 때마다 점프포스 만큼의 힘을 주는 코드
                isGrounded = false;
                canDoubleJump = true;
                playerAnimator.SetInteger("state", 1); //점프 애니메이션으로 전환하는 코드
            }
            else if (canDoubleJump)
            {
                rb.linearVelocityY = 0; //속도를 초기화하여 일정한 점프 높이 유지
                rb.AddForceY(jumpForce, ForceMode2D.Impulse); //더블 점프
                canDoubleJump = false;
                playerAnimator.SetInteger("state", 1); //점프 애니메이션 재시작
            }
        }
        // Impulse는 순간적으로 힘을 주는 것, Force는 지속적으로 힘을 주는 것
    }

    /*
    New Input System을 사용한다면 위의 Update 함수는 필요없음 위 코드와 동일하게 동작함
    public void OnJump()
    {
        rb.linearVelocityY = jumpForce; //점프할 때마다 점프포스 만큼의 속도를 주는 코드
    }
    */

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Platform")
        {
            if (!isGrounded)
            {
                playerAnimator.SetInteger("state", 2);
            }
            isGrounded = true; //땅에 닿으면 점프할 수 있도록 하는 코드
            canDoubleJump = false; //더블 점프 초기화
        }
    }
    public void KillPlayer()
    {
        playerCollider.enabled = false; //플레이어의 콜라이더를 비활성화하는 코드
        playerAnimator.enabled = false; //플레이어의 애니메이터를 비활성화하는 코드
        rb.AddForceY(jumpForce, ForceMode2D.Impulse);
    }

    public void ResetPlayer()
    {
        transform.position = startPosition; // 위치 초기화
        rb.linearVelocity = Vector2.zero;   // 속도 초기화
        isGrounded = true;                  // 상태 초기화
        canDoubleJump = false;              // 더블 점프 초기화
        playerCollider.enabled = true;      // 콜라이더 다시 켜기
        playerAnimator.enabled = true;      // 애니메이터 다시 켜기
        playerAnimator.SetInteger("state", 0); // 애니메이션 상태 초기화
    }

    void Hit()
    {
        GameManager.Instance.lives -= 1;
        
        // 카메라 흔들림 호출 (지속시간 0.2초, 강도 0.2)
        if (CameraShake.Instance != null)
        {
            StartCoroutine(CameraShake.Instance.Shake(0.8f, 1.8f));
        }
    }
    void Heal()
    {
        GameManager.Instance.lives = Mathf.Min(GameManager.Instance.maxLives, GameManager.Instance.lives + 1); //목숨이 최대 개수가 되도록 하는 코드
    }
    void StartInvincible()
    {
        isInvincible = true;
        Invoke("EndInvincible", 5f); //5초 후에 EndInvincible 함수를 호출하는 코드
    }
    void EndInvincible()
    {
        isInvincible = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.gameObject.tag == "enemy"))
        {
            if (!isInvincible)
            {
                Destroy(collision.gameObject);
                Hit();
                //  Debug.Log("플레이어가 적과 충돌했습니다. 남은 목숨: " + GameManager.Instance.lives);
            }

        }
        else if (collision.gameObject.tag == "food")
        {
            int baseScore = 1;
            string message = "";
            int multiplier = 1;

            if (collision.gameObject.name.Contains("garlic"))
            {
                baseScore = 5;
            }

            // 점프 상태에 따른 배율 결정
            if (isGrounded)
            {
                multiplier = 1;
                // 바닥에선 메시지 생략 가능 또는 기본 메시지
            }
            else if (canDoubleJump) // 1단 점프 중 (아직 더블 점프 가능함)
            {
                multiplier = 2;
                message = "JUMP! x2";
            }
            else // 2단 점프 중 (더블 점프 소모함)
            {
                multiplier = 3;
                message = "DOUBLE JUMP!! x3";
            }

            GameManager.Instance.AddScore(baseScore * multiplier);
            if (!string.IsNullOrEmpty(message))
            {
                GameManager.Instance.ShowComboMessage(message);
            }

            if (!collision.gameObject.name.Contains("garlic"))
            {
                Heal();
            }
            
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.tag == "golden")
        {
            Destroy(collision.gameObject);
            StartInvincible();
        }
    }
}
