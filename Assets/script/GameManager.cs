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

    public float playStartTime;
    private float gameOverTime; // 게임 종료 시점 기록용

    public GameState state = GameState.Intro;
    public int maxLives = 8;
    public int lives = 8;
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

        // 시작 시 스포너와 초기 배치된 건물 비활성화
        if (enemySpawner != null) enemySpawner.SetActive(false);
        if (foodSpawner != null) foodSpawner.SetActive(false);
        if (buildingSpawner != null) buildingSpawner.SetActive(false);
    }

    float CalculateScore()
    {
        return Time.time - playStartTime; // 게임이 시작된 후 경과한 시간 계산
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
        float speed = 8f + (0.5f * Mathf.Floor(CalculateScore() / 10f)); // 점수에 따라 게임 속도 증가
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

        if (state == GameState.Intro && Input.GetKeyDown(KeyCode.Space))
        {
            state = GameState.Playing;
            IntroUI.SetActive(false);
            if (enemySpawner != null) enemySpawner.SetActive(true); //적 스포너 활성화
            if (foodSpawner != null) foodSpawner.SetActive(true); //음식 스포너 활성화
            if (buildingSpawner != null) buildingSpawner.SetActive(true); //건물 스포너 활성화
            playStartTime = Time.time; // 게임 시작 시간 기록
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
        }
        // 죽은 지 최소 0.5초가 지났을 때만 스페이스바 입력 허용
        if (state == GameState.GameOver && Input.GetKeyDown(KeyCode.Space) && Time.time > gameOverTime + 0.5f)
        {
            ResetGame(); // 씬을 새로 부르는 대신 UI를 전환하고 게임을 리셋합니다.
        }
    }

    void ResetGame()
    {
        lives = maxLives;
        state = GameState.Intro;

        GameOverUI.SetActive(false);
        IntroUI.SetActive(true);

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
