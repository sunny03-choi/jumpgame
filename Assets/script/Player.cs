using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("세팅")]
    public float jumpForce; //public을 넣으면 코드 외부 인스펙터에서 조정 가능
    public float moveSpeed = 5f;       // 좌우 이동 속도
    public float dashForce = 20f;      // 대시 힘
    public float dashDuration = 0.2f;   // 대시 지속 시간
    public float dashCooldown = 1f;     // 대시 쿨타임

    [Header("레퍼런스")]
    public Rigidbody2D rb;
    /* rb 라는 이름의 Rigidbody2D 컴포넌트를 참조할 수 있도록 선언
    심볼 까먹으면 안됨! */
    public Animator playerAnimator;
    public BoxCollider2D playerCollider; // <- 이게 추가됨

    private bool isGrounded = true; //땅에 닿아있는지 여부를 저장하는 변수
    private bool canDoubleJump = false; //더블 점프 가능 여부를 저장하는 변수

    public bool isInvincible = false; //무적 상태 여부
    private bool canDash = true;        // 대시 가능 여부
    private bool isDashing = false;     // 현재 대시 중인지

    private Vector3 startPosition;
    private float horizontalInput;

    void Start()
    {
        startPosition = transform.position; // 시작 위치 저장
    }

    void Update()
    {
        if (GameManager.Instance.state != GameState.Playing) return;

        // 입력 감지 (키보드 + 터치/마우스 클릭)
        HandleInput();

        // 좌우 이동 처리 (대시 중이 아닐 때)
        if (!isDashing)
        {
            rb.linearVelocityX = horizontalInput * moveSpeed;
        }

        // 화면 밖으로 나가지 않도록 제한 (Clamp)
        ClampPosition();

        // 대시 입력 (Left Shift 또는 마우스 우클릭 / 모바일은 추후 제스처나 버튼 고려 가능)
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1)) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    void HandleInput()
    {
        // 1. 키보드 입력 확인
        float keyboardInput = Input.GetAxisRaw("Horizontal");
        if (keyboardInput != 0)
        {
            horizontalInput = keyboardInput;
        }
        else
        {
            horizontalInput = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleJump();
        }

        // 2. 모바일 터치/마우스 클릭 대응 (영역 분할)
        if (Input.GetMouseButtonDown(0)) // 클릭/터치 순간
        {
            float mouseY = Input.mousePosition.y;
            float screenHalfHeight = Screen.height / 2f;

            // 화면 위쪽 절반 터치 시 점프
            if (mouseY > screenHalfHeight)
            {
                HandleJump();
            }
        }

        if (Input.GetMouseButton(0)) // 누르고 있는 동안 (이동)
        {
            float mouseX = Input.mousePosition.x;
            float mouseY = Input.mousePosition.y;
            float screenHalfWidth = Screen.width / 2f;
            float screenHalfHeight = Screen.height / 2f;

            // 화면 아래쪽 절반을 누르고 있을 때만 이동 처리
            if (mouseY <= screenHalfHeight)
            {
                if (mouseX < screenHalfWidth)
                {
                    horizontalInput = -1f; // 왼쪽 이동
                }
                else
                {
                    horizontalInput = 1f;  // 오른쪽 이동
                }
            }
        }
    }

    void HandleJump()
    {
        if (isGrounded)
        {
            PerformJump();
            canDoubleJump = true;
        }
        else if (canDoubleJump)
        {
            PerformJump();
            canDoubleJump = false;
        }
    }

    void ClampPosition()
    {
        // 월드 좌표를 뷰포트 좌표(0~1)로 변환
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        
        // 뷰포트 범위를 0.05 ~ 0.95 정도로 제한하여 캐릭터가 화면 끝에 걸리게 함
        pos.x = Mathf.Clamp(pos.x, 0.05f, 0.95f);
        
        // 다시 월드 좌표로 변환하여 적용
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }

    void PerformJump()
    {
        rb.linearVelocityY = 0; //속도를 초기화하여 일정한 점프 높이 유지
        rb.AddForceY(jumpForce, ForceMode2D.Impulse); //점프할 때마다 점프포스 만큼의 힘을 주는 코드
        isGrounded = false;
        playerAnimator.SetInteger("state", 1); //점프 애니메이션으로 전환하는 코드
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        bool wasInvincible = isInvincible;
        isInvincible = true; // 대시 중 무적

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f; // 대시 중에는 중력의 영향을 받지 않음
        
        // 입력 방향으로 대시, 입력이 없으면 오른쪽으로 대시
        float dashDir = horizontalInput != 0 ? horizontalInput : 1f;
        rb.linearVelocity = new Vector2(dashDir * dashForce, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocityY); 
        isDashing = false;
        isInvincible = wasInvincible;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
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
