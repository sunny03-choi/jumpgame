using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public GameObject enemySpawner;
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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        IntroUI.SetActive(true);
        GameOverUI.SetActive(false); // 시작할 때 GameOverUI는 꺼둡니다.
        if (comboText != null) comboText.gameObject.SetActive(false); // 콤보 텍스트 숨기기

        // 시작 시 스포너와 초기 배치된 건물 비활성화
        if (enemySpawner != null) enemySpawner.SetActive(false);
        if (foodSpawner != null) foodSpawner.SetActive(false);
        if (buildingSpawner != null) buildingSpawner.SetActive(false);

        // 배경 음악 시작
        if (SoundManager.Instance != null) SoundManager.Instance.PlayMusic(GameState.Intro);
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
        if (state == GameState.Playing)
        {
            scoreText.text = "Score: " + Mathf.FloorToInt(CalculateScore()); // 점수 업데이트
        }
        else if (state == GameState.GameOver)
        {
            scoreText.text = "High Score: " + GetHighScore(); // 최고 점수 표시
        }

        if (state == GameState.Intro && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            state = GameState.Playing;
            IntroUI.SetActive(false);
            if (enemySpawner != null) enemySpawner.SetActive(true); //적 스포너 활성화
            if (foodSpawner != null) foodSpawner.SetActive(true); //음식 스포너 활성화
            if (buildingSpawner != null) buildingSpawner.SetActive(true); //건물 스포너 활성화
            playStartTime = Time.time; // 게임 시작 시간 기록
            bonusScore = 0; // 보너스 점수 초기화
            if (SoundManager.Instance != null) SoundManager.Instance.PlayMusic(GameState.Playing);
        }
        if (state == GameState.Playing && lives <= 0)
        {
            playerScript.KillPlayer(); //플레이어 죽이는 코드
            if (enemySpawner != null) enemySpawner.SetActive(false);
            if (foodSpawner != null) foodSpawner.SetActive(false);
            if (buildingSpawner != null) buildingSpawner.SetActive(false);
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

        GameOverUI.SetActive(false);
        IntroUI.SetActive(true);

        if (SoundManager.Instance != null) SoundManager.Instance.PlayMusic(GameState.Intro);

        // 다시 인트로 상태로 갈 때 비활성화
        if (enemySpawner != null) enemySpawner.SetActive(false);
        if (foodSpawner != null) foodSpawner.SetActive(false);
        if (buildingSpawner != null) buildingSpawner.SetActive(false);

        // 플레이어 위치 및 상태 초기화
        playerScript.ResetPlayer();

        // 화면에 남아있는 적들과 음식들 제거
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");
        foreach (GameObject enemy in enemies) Destroy(enemy);

        GameObject[] foods = GameObject.FindGameObjectsWithTag("food");
        foreach (GameObject food in foods) Destroy(food);
    }
}
