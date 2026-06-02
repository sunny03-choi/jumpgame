using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Collections.Generic; // 추가

public enum GameState
{
    Intro,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("레퍼런스")]
    public GameObject IntroUI;
    public GameObject GameOverUI;
    public GameObject[] enemySpawners; // GameObject에서 GameObject[]로 변경
    public GameObject foodSpawner;
    public GameObject buildingSpawner;
    public Player playerScript;
    public TMP_Text scoreText;
    public TMP_Text comboText; // 추가: 콤보 메시지 텍스트

    public float playStartTime;
    private float gameOverTime; // 게임 종료 시점 기록용
    public float comboTextDuration = 0.8f; // 추가: 콤보 텍스트 표시 시간

    public GameState state = GameState.Intro;
    public int maxLives = 8;
    public int lives = 8;
    public int bonusScore = 0; // 아이템으로 얻은 추가 점수

    [Header("화면 반전 설정")]
    public bool isFlipped = false;
    private float nextFlipTime;
    public string uiCameraName = "UI Camera"; // UI 전용 카메라가 있다면 그 이름을 입력

    void Awake()
    {
        // 싱글톤 초기화 개선
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            // 만약 기존 인스턴스에 중요한 레퍼런스가 없고 나에게 있다면 교체
            if (Instance.foodSpawner == null && this.foodSpawner != null)
            {
                Destroy(Instance);
                Instance = this;
            }
            else
            {
                Destroy(this);
                return;
            }
        }
    }

    void OnEnable()
    {
        // URP 카메라 렌더링 콜백 등록
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        // 콜백 해제
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void Start()
    {
        // 내가 현재 인스턴스가 아니면 실행 안 함
        if (Instance != this) return;

        // 자동으로 모든 적 스포너 찾기 (비활성화된 것 포함)
        var allSpawners = FindObjectsByType<Spawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<GameObject> enemyList = new List<GameObject>();
        
        foreach (var s in allSpawners)
        {
            if (s.gameObject == foodSpawner || s.gameObject == buildingSpawner) continue;
            if (s.gameObject.name.Contains("Enemy"))
            {
                enemyList.Add(s.gameObject);
            }
        }
        enemySpawners = enemyList.ToArray();

        IntroUI.SetActive(true);
        GameOverUI.SetActive(false);
        if (comboText != null) comboText.gameObject.SetActive(false);

        // 시작 시 모든 스포너 초기 상태 설정
        SetSpawnersActive(false);

        if (SoundManager.Instance != null) SoundManager.Instance.PlayMusic(GameState.Intro);
        ScheduleNextFlip();
    }

    // 스포너들을 일괄적으로 켜고 끄는 함수
    void SetSpawnersActive(bool active)
    {
        if (enemySpawners != null)
        {
            foreach (var spawner in enemySpawners)
            {
                if (spawner != null) spawner.SetActive(active);
            }
        }
        if (foodSpawner != null) foodSpawner.SetActive(active);
        if (buildingSpawner != null) buildingSpawner.SetActive(active);
    }

    void ScheduleNextFlip()
    {
        // 1초에서 32초 사이의 랜덤한 시간 후 반전
        float delay = Random.Range(1f, 32f);
        nextFlipTime = Time.time + delay;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        // 게임 카메라에만 적용 (에디터 뷰 제외)
        // UI가 포함된 카메라는 반전시키지 않도록 설정하거나, 
        // UI Canvas의 Render Mode가 'Screen Space - Overlay'라면 이 코드의 영향을 받지 않습니다.
        // 만약 UI도 함께 반전된다면, 카메라 이름을 체크하여 제외할 수 있습니다.
        if (camera.cameraType == CameraType.Game && camera.name != uiCameraName)
        {
            camera.ResetProjectionMatrix();
            GL.invertCulling = isFlipped;
            if (isFlipped)
            {
                camera.projectionMatrix = camera.projectionMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
            }
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType == CameraType.Game)
        {
            GL.invertCulling = false;
            // UI 카메라가 따로 없고 한 카메라에서 다 그린다면, 
            // 여기서 Matrix를 리셋해줘야 다음 렌더링(UI 등)에 영향을 주지 않을 수 있습니다.
            camera.ResetProjectionMatrix();
        }
    }

    float CalculateScore()
    {
        return (Time.time - playStartTime) + bonusScore; // 시간 점수 + 보너스 점수
    }

    public void AddScore(int amount)
    {
        bonusScore += amount;
    }

    public void ShowComboMessage(string message)
    {
        if (comboText == null) return;
        
        comboText.text = message;
        comboText.gameObject.SetActive(true);
        
        CancelInvoke("HideComboMessage");
        Invoke("HideComboMessage", comboTextDuration);
    }

    void HideComboMessage()
    {
        if (comboText != null) comboText.gameObject.SetActive(false);
    }

    void SaveHighScore()
    {
        int score = Mathf.FloorToInt(CalculateScore()); // 점수를 정수로 변환
        int currentHighScore = PlayerPrefs.GetInt("highScore"); // 현재 저장된 최고 점수 가져오기
        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt("highScore", score); // 최고 점수 저장
            PlayerPrefs.Save(); // 변경 사항 저장
        }
    }

    public float CalculateGameSpeed()
    {
        if (state != GameState.Playing)
        {
            return 5f; // 게임이 진행 중이 아닐 때는 기본 속도 유지
        }
        float scoreTime = Time.time - playStartTime; // 속도 증가는 순수 시간 기준으로 계산하거나 전체 점수 기준으로 변경 가능
        float speed = 8f + (0.5f * Mathf.Floor(scoreTime / 10f)); 
        return Mathf.Min(speed, 30f); // 최대 속도 제한 ,30f로 설정
    }

    int GetHighScore()
    {
        return PlayerPrefs.GetInt("highScore");
    }

    void Update()
    {
        // 화면 반전 타이머 체크
        if (state == GameState.Playing)
        {
            if (Time.time >= nextFlipTime)
            {
                isFlipped = !isFlipped;
                ScheduleNextFlip();
                UpdateUIFlip(); // UI 반전 상태 업데이트 호출
            }
        }

        if (state == GameState.Playing)
        {
            if (scoreText != null) scoreText.text = "Score: " + Mathf.FloorToInt(CalculateScore()); // 점수 업데이트

            // 속도에 따른 BGM 피치 조절 추가
            if (SoundManager.Instance != null)
            {
                float currentSpeed = CalculateGameSpeed();
                // 기본 속도 8.0을 기준으로, 속도가 1 증가할 때마다 피치를 0.01씩 올림 (최대 30일 때 약 1.22)
                float targetPitch = 1f + (currentSpeed - 8f) * 0.01f;
                SoundManager.Instance.SetMusicPitch(targetPitch);
            }
        }
        else if (state == GameState.GameOver)
        {
            if (scoreText != null) scoreText.text = "High Score: " + GetHighScore(); // 최고 점수 표시
        }

        if (state == GameState.Intro && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            state = GameState.Playing;
            IntroUI.SetActive(false);
            
            SetSpawnersActive(true);
            
            playStartTime = Time.time; // 게임 시작 시간 기록
            bonusScore = 0; // 보너스 점수 초기화
            if (SoundManager.Instance != null) SoundManager.Instance.PlayMusic(GameState.Playing);
        }
        if (state == GameState.Playing && lives <= 0)
        {
            playerScript.KillPlayer(); //플레이어 죽이는 코드
            
            SetSpawnersActive(false);
            
            state = GameState.GameOver;
            gameOverTime = Time.time; // 죽은 시점 기록
            GameOverUI.SetActive(true);
            SaveHighScore(); // 게임 오버 시 최고 점수 저장
            if (SoundManager.Instance != null) SoundManager.Instance.PlayMusic(GameState.GameOver);
        }
        // 죽은 지 최소 0.5초가 지났을 때만 스페이스바 또는 터치 입력 허용
        if (state == GameState.GameOver && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && Time.time > gameOverTime + 0.5f)
        {
            ResetGame(); // 씬을 새로 부르는 대신 UI를 전환하고 게임을 리셋합니다.
        }
    }

    void ResetGame()
    {
        lives = maxLives;
        bonusScore = 0; // 리셋 시 점수 초기화
        state = GameState.Intro;

        isFlipped = false; // 반전 상태 초기화
        UpdateUIFlip();    // UI 스케일 초기화
        ScheduleNextFlip();

        GameOverUI.SetActive(false);
        IntroUI.SetActive(true);

        if (SoundManager.Instance != null) SoundManager.Instance.PlayMusic(GameState.Intro);

        // 다시 인트로 상태로 갈 때 모든 스포너 비활성화
        SetSpawnersActive(false);

        // 플레이어 위치 및 상태 초기화
        playerScript.ResetPlayer();

        // 화면에 남아있는 적들과 음식들 제거
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");
        foreach (GameObject enemy in enemies) Destroy(enemy);

        GameObject[] foods = GameObject.FindGameObjectsWithTag("food");
        foreach (GameObject food in foods) Destroy(food);
    }

    // UI 요소들을 다시 반전시켜 정상적으로 보이게 하는 함수
    void UpdateUIFlip()
    {
        float scaleX = isFlipped ? -1f : 1f;

        // 점수 및 콤보 텍스트 반전 보정
        if (scoreText != null) scoreText.transform.localScale = new Vector3(scaleX, 1, 1);
        if (comboText != null) comboText.transform.localScale = new Vector3(scaleX, 1, 1);

        // UI 패널 반전 보정
        if (IntroUI != null) IntroUI.transform.localScale = new Vector3(scaleX, 1, 1);
        if (GameOverUI != null) GameOverUI.transform.localScale = new Vector3(scaleX, 1, 1);

        // 하트(목숨) UI 오브젝트들도 찾아서 보정
        // 태그를 사용하는 대신 Heart 스크립트를 가진 모든 오브젝트를 직접 찾습니다. (태그 미설정 시 에러 방지)
        Heart[] heartScripts = FindObjectsByType<Heart>(FindObjectsSortMode.None);
        foreach (var h in heartScripts)
        {
            h.transform.localScale = new Vector3(scaleX, 1, 1);
        }
    }
}
