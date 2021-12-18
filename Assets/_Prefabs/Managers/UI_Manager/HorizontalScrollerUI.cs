using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalScrollerUI : MonoBehaviour
{
    [SerializeField]
    private Car car = null;

    [SerializeField]
    private EnemyManager enemyManager = null;

    [SerializeField]
    private BackgroundControl_0 backgroundController = null;
    [SerializeField]
    private ParallaxBackground_0 parallaxController = null;

    [SerializeField]
    private GameObject[] lifeHearts = null;
    [SerializeField]
    private int scoreDeductedForBeingHit = 0;

    [SerializeField]
    private TextMeshProUGUI scoreText = null;
    [SerializeField]
    private TextMeshProUGUI highScoreText = null;

    [SerializeField]
    private Button playButton = null;
    [SerializeField]
    private Button pauseButton = null;

    [SerializeField]
    private GameObject userWonMessage = null;
    [SerializeField]
    private GameObject userLostMessage = null;
    [SerializeField]
    private GameObject gamePausedMessage = null;
    [SerializeField]
    private GameObject gameTitle = null;

    [SerializeField]
    private GameObject areYouSureQuitUI = null;

    [SerializeField]
    private GameObject gameHelpMessageStandalone = null;
    [SerializeField]
    private GameObject gameHelpMessageMobile = null;

    private bool isGamePaused = false;
    private float currentHighScore = 0;

    private const string scorePrefix = "Score: ";
    private const string highScorePrefix = "High Score: ";

    public void UserWon()
    {
        SaveCurrentHighScore();
        enemyManager.DeactivateBombTokenSpawner();
        pauseButton.interactable = false;
        parallaxController.enabled = false;
        userWonMessage.SetActive(true);
        playButton.gameObject.SetActive(true);
        highScoreText.transform.parent.gameObject.SetActive(true);

#if UNITY_ANDROID
        gameHelpMessageMobile.SetActive(true);
#else
        gameHelpMessageStandalone.SetActive(true);
#endif

        car.PauseTires();
    }

    public void UserPressedPlay()
    {
        gameTitle.SetActive(false);
        enemyManager.ActivateBombTokenSpawner();
        pauseButton.interactable = true;
        car.gameObject.SetActive(false);
        userWonMessage.SetActive(false);
        userLostMessage.SetActive(false);
        gamePausedMessage.SetActive(false);
        parallaxController.enabled = true;
        backgroundController.SetBG(0);
        car.gameObject.SetActive(true);
        highScoreText.transform.parent.gameObject.SetActive(false);

#if UNITY_ANDROID
        gameHelpMessageMobile.SetActive(false);
#else
        gameHelpMessageStandalone.SetActive(false);
#endif

        Camera.main.transform.position = new Vector3(0.0f, 0.0f, -10f);
        car.CanUserControlCar = true;
        ResetUI();
    }

    public void UserPressedPause()
    {
        isGamePaused = !isGamePaused;
        gamePausedMessage.SetActive(isGamePaused);
        highScoreText.transform.parent.gameObject.SetActive(isGamePaused);

#if UNITY_ANDROID
        gameHelpMessageMobile.SetActive(isGamePaused);
#else
        gameHelpMessageStandalone.SetActive(isGamePaused);
#endif

        if (isGamePaused == false)
        {
            Time.timeScale = 1.0f;
        }
        else
        {
            Time.timeScale = 0.0f;
        }
    }

    public void SetScoreText(string newScore)
    {
        scoreText.text = scorePrefix + newScore;
    }

    public void SetHighScoreText(string newHighScore)
    {
        highScoreText.text = highScorePrefix + newHighScore;
    }

    public void EnemyHitPlayer()
    {
        for (int i = 0; i < lifeHearts.Length; i++)
        {
            if (lifeHearts[i] == null)
            {
                continue;
            }

            if (lifeHearts[i].activeSelf == false)
            {
                if (i == lifeHearts.Length - 1)
                {
                    UserLost();
                    break;
                }

                continue;
            }

            lifeHearts[i].SetActive(false);
            car.CurrentPlayerScore -= ((scoreDeductedForBeingHit * i) + scoreDeductedForBeingHit);
            break;
        }
    }

    public void UserDecidedToQuitTheGame()
    {
        Application.Quit();
    }

    private void Awake()
    {
        isGamePaused = false;
    }

    private void Start()
    {
        areYouSureQuitUI.SetActive(false);
        pauseButton.interactable = false;
        userWonMessage.SetActive(false);
        userLostMessage.SetActive(false);
        highScoreText.transform.parent.gameObject.SetActive(true);

#if UNITY_ANDROID
        gameHelpMessageMobile.SetActive(true);
        gameHelpMessageStandalone.SetActive(false);
#else
        gameHelpMessageStandalone.SetActive(true);
        gameHelpMessageMobile.SetActive(false);
#endif

        gamePausedMessage.SetActive(false);
        playButton.gameObject.SetActive(true);
        playButton.onClick.AddListener(UserPressedPlay);
        car.gameObject.SetActive(false);
        parallaxController.enabled = false;
        enemyManager.DeactivateBombTokenSpawner();
        gameTitle.SetActive(true);

        currentHighScore = PlayerPrefs.GetFloat("HighScore", 0.0f);
        SetHighScoreText((currentHighScore * 100.0f).ToString("0"));
    }

    private void LateUpdate()
    {
        if(car.CurrentPlayerScore > currentHighScore)
        {
            currentHighScore = car.CurrentPlayerScore;
            SetHighScoreText((currentHighScore * 100.0f).ToString("0"));
        }

        if(Input.GetKeyDown(KeyCode.Escape) == true)
        {
            areYouSureQuitUI.SetActive(true);

            if (isGamePaused == true)
            {
                return;
            }

            UserPressedPause();
        }
    }

    private void UserLost()
    {
        SaveCurrentHighScore();
        pauseButton.interactable = false;
        car.gameObject.SetActive(false);
        playButton.gameObject.SetActive(true);
        parallaxController.enabled = false;
        enemyManager.DeactivateBombTokenSpawner();
        enemyManager.DeactivateTheBoss();
        highScoreText.transform.parent.gameObject.SetActive(true);
#if UNITY_ANDROID
        gameHelpMessageMobile.SetActive(true);
#else
        gameHelpMessageStandalone.SetActive(true);
#endif


        userLostMessage.SetActive(true);
    }

    private void SaveCurrentHighScore()
    {
        PlayerPrefs.SetFloat("HighScore", currentHighScore);
        PlayerPrefs.Save();
    }

    private void ResetUI()
    {
        SetScoreText("0");

        for (int i = 0; i < lifeHearts.Length; i++)
        {
            if (lifeHearts[i] == null)
            {
                continue;
            }

            lifeHearts[i].SetActive(true);
        }

        playButton.gameObject.SetActive(false);
    }
}