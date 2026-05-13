using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;

    [Header("Audio Clips")]
    public AudioClip introMusic;
    public AudioClip playingMusic;
    public AudioClip gameOverMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(GameState state)
    {
        if (musicSource == null) return;

        musicSource.Stop();
        
        switch (state)
        {
            case GameState.Intro:
                if (introMusic != null)
                {
                    musicSource.clip = introMusic;
                    musicSource.loop = true;
                    musicSource.Play();
                }
                break;
            case GameState.Playing:
                if (playingMusic != null)
                {
                    musicSource.clip = playingMusic;
                    musicSource.loop = true;
                    musicSource.Play();
                }
                break;
            case GameState.GameOver:
                if (gameOverMusic != null)
                {
                    musicSource.clip = gameOverMusic;
                    musicSource.loop = false;
                    musicSource.Play();
                }
                break;
        }
    }
}
