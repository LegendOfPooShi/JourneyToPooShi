using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField]
    private Vector3 carStartingPosition = Vector3.zero;

    [SerializeField]
    private Vector3[] possibleCarLocations = null;

    [SerializeField]
    private float[] possibleCarLaneHeights = null;

    [SerializeField]
    private AnimationCurve carMovementAnimCurve = null;

    [SerializeField]
    private Transform carShadow = null;
    [SerializeField]
    private Vector3 carShadowOffset = Vector3.zero;

    [SerializeField]
    private float timeToCheckForInput = 0.4f;

    [SerializeField]
    private float carMoveSpeed = 1.0f;

    [SerializeField]
    private float jumpSpeed = 10.0f;
    [SerializeField]
    private float totalJumpSeconds = 2.0f;

    [SerializeField]
    private SpriteRenderer frontBodySpriteRenderer = null;

    [SerializeField]
    private float TimeInBeenHitModeSeconds = 3.0f;

    [SerializeField]
    private HorizontalScrollerUI gameUI = null;

    [SerializeField]
    private EnemyManager enemyManager = null;

    [SerializeField]
    private Animator carAnimator = null;

    [SerializeField]
    private int pointsForHittingPooShiToken = 500;
    [SerializeField]
    private AudioSource carHitTokenAudioSource = null;

    [SerializeField]
    private AudioSource carHitEnemyAudioSource = null;
    [SerializeField]
    private AudioSource carJumpedAudioSource = null;

    [SerializeField]
    private float swipeDistanceToMove = 0.24f;

    private float currentJumpSeconds = 0.0f;

    private Vector3 currentTargetCarPosition = Vector3.zero;
    private int currentCarLocationIndex = 0;
    private int currentCarLaneIndex = 0;
    private float secondsSinceCarJumped = 0.0f;
    private float secondsTraveled = 0.0f;
    private float currentBeenHitModeSeconds = 0.0f;
    private float currentPlayerScore = 0.0f;
    private bool isInBossMode = false;
    private bool isCarJumping = false;
    private float currentSecondsAllowForInput = 0.0f;
    private bool canUserControlCar = false;

    private Vector2 fingerDown = Vector2.zero;
    private Vector2 fingerUp = Vector2.zero;
    public bool detectSwipeOnlyAfterRelease = false;
    public float swipeThershold = 7f;

    public bool CanUserControlCar
    {
        get
        {
            return canUserControlCar;
        }
        set
        {
            canUserControlCar = value;
        }
    }

    public float CurrentPlayerScore
    {
        get
        {
            return currentPlayerScore;
        }
        set
        {
            currentPlayerScore = value;
        }
    }

    public float SecondsSinceCarJumped
    {
        get
        {
            return secondsSinceCarJumped;
        }
        private set
        {
            secondsSinceCarJumped = value;
        }
    }

    public float SecondsTraveled
    {
        get
        {
            return secondsTraveled;
        }
        set
        {
            secondsTraveled = value;
        }
    }

    public void SetCarLocation(Vector3 newLocation)
    {
        currentTargetCarPosition = newLocation;
    }

    public void SetCarLane(float newLaneHeight)
    {
        Vector3 tempPos = currentTargetCarPosition;
        tempPos.y = newLaneHeight;
        currentTargetCarPosition = tempPos;
    }

    public void HitPooShiTokenObject() 
    {
        currentPlayerScore += pointsForHittingPooShiToken;
        carHitTokenAudioSource.Play();
    }

    public void HitEnemyObject()
    {
        if (frontBodySpriteRenderer.enabled == false)
        {
            return;
        }

        frontBodySpriteRenderer.enabled = false;
        gameUI.EnemyHitPlayer();
        carHitEnemyAudioSource.Play();
    }

    public void PauseTires()
    {
        canUserControlCar = false;
        carAnimator.enabled = false;
    }

    private void Awake()
    {
#if UNITY_ANDROID
        carMoveSpeed *= 0.5f;
#endif
    }

    private void OnEnable()
    {
        carShadow.gameObject.SetActive(true);
        canUserControlCar = false;
        carAnimator.enabled = true;
        frontBodySpriteRenderer.enabled = true;
        isCarJumping = false;
        isInBossMode = false;
        currentJumpSeconds = 0.0f;
        secondsTraveled = 0.0f;
        currentPlayerScore = 0.0f;
        currentSecondsAllowForInput = 0.0f;
        currentTargetCarPosition = carStartingPosition;
        transform.localPosition = currentTargetCarPosition;
        transform.GetChild(0).localEulerAngles = Vector3.zero;
        secondsSinceCarJumped = 0.0f;

        currentCarLocationIndex = 0;
        SetCarLocation(possibleCarLocations[currentCarLocationIndex]);

        currentCarLaneIndex = possibleCarLaneHeights.Length / 2;
        SetCarLane(possibleCarLaneHeights[currentCarLaneIndex]);
    }

    private void Update()
    {
        if (canUserControlCar == true)
        {
            CheckForCarLocationShift();
            CheckForCarJumping();
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, currentTargetCarPosition,
            carMovementAnimCurve.Evaluate(Time.deltaTime * carMoveSpeed));

        Vector3 newShadowPosition = transform.localPosition;
        newShadowPosition.x += carShadowOffset.x;
        newShadowPosition.y = (possibleCarLaneHeights[currentCarLaneIndex] - possibleCarLaneHeights[1]) + carShadowOffset.y;
        carShadow.localPosition = newShadowPosition;

        float newScaleX = Mathf.Lerp(1.0f, 0.2f, (transform.localPosition.y + 6.4f) / 8.5f);
        carShadow.localScale = new Vector3(newScaleX, 1.0f, 1.0f);

        if (frontBodySpriteRenderer.enabled == false)
        {
            if(currentBeenHitModeSeconds >= TimeInBeenHitModeSeconds)
            {
                frontBodySpriteRenderer.enabled = true;
                currentBeenHitModeSeconds = 0.0f;
            }
            else
            {
                currentBeenHitModeSeconds += Time.deltaTime;
            }
        }

        if(isInBossMode == true)
        {
            return;
        }

        secondsTraveled += Time.deltaTime;
        currentPlayerScore += Time.deltaTime;

        gameUI.SetScoreText((currentPlayerScore * 100f).ToString("0"));

        if (transform.position.x >= 700f)
        {
            isInBossMode = true;
            enemyManager.ActivateTheBoss();
        }
    }

    private void OnDisable()
    {
        carShadow.gameObject.SetActive(false);
    }

    private void CheckForCarLocationShift()
    {
        currentSecondsAllowForInput += Time.deltaTime;
        if (currentSecondsAllowForInput < timeToCheckForInput)
        {
            return;
        }

        currentSecondsAllowForInput = 0.0f;

        if ((Input.GetKey(KeyCode.A) == true) || (Input.GetKey(KeyCode.LeftArrow) == true))
        {
            currentCarLocationIndex--;
            currentCarLocationIndex = Mathf.Clamp(currentCarLocationIndex, 0, possibleCarLocations.Length - 1);
            SetCarLocation(possibleCarLocations[currentCarLocationIndex]);
            SetCarLane(possibleCarLaneHeights[currentCarLaneIndex]);
        }

        if ((Input.GetKey(KeyCode.D) == true) || (Input.GetKey(KeyCode.RightArrow) == true))
        {
            currentCarLocationIndex++;
            currentCarLocationIndex = Mathf.Clamp(currentCarLocationIndex, 0, possibleCarLocations.Length - 1);
            SetCarLocation(possibleCarLocations[currentCarLocationIndex]);
            SetCarLane(possibleCarLaneHeights[currentCarLaneIndex]);
        }

        if ((Input.GetKey(KeyCode.W) == true) || (Input.GetKey(KeyCode.UpArrow) == true))
        {
            currentCarLaneIndex--;
            currentCarLaneIndex = Mathf.Clamp(currentCarLaneIndex, 0, possibleCarLaneHeights.Length - 1);
            SetCarLane(possibleCarLaneHeights[currentCarLaneIndex]);
        }

        if ((Input.GetKey(KeyCode.S) == true) || (Input.GetKey(KeyCode.DownArrow) == true))
        {
            currentCarLaneIndex++;
            currentCarLaneIndex = Mathf.Clamp(currentCarLaneIndex, 0, possibleCarLaneHeights.Length - 1);
            SetCarLane(possibleCarLaneHeights[currentCarLaneIndex]);
        }

        if (Input.touchCount <= 0)
        {
            return;
        }

        foreach (Touch touch in Input.touches)
        {
            if (touch.tapCount >= 2)
            {
                if (isCarJumping == false)
                {
                    MakeCarJump();
                    continue;
                }
            }

            if (touch.phase == TouchPhase.Began)
            {
                fingerUp = touch.position;
                fingerDown = touch.position;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                if (!detectSwipeOnlyAfterRelease)
                {
                    fingerDown = touch.position;
                    checkSwipe();
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                fingerDown = touch.position;
                checkSwipe();
            }
        }
    }

    private void checkSwipe()
    {
        if ((verticalMove() > swipeThershold) && (verticalMove() > horizontalValMove()))
        {
            if (fingerDown.y - fingerUp.y > 0)
            {
                OnSwipeUp();
            }
            else if (fingerDown.y - fingerUp.y < 0)
            {
                OnSwipeDown();
            }

            fingerUp = fingerDown;
        }
        else if ((horizontalValMove() > swipeThershold) && (horizontalValMove() > verticalMove()))
        {
            if (fingerDown.x - fingerUp.x > 0)
            {
                OnSwipeRight();
            }
            else if (fingerDown.x - fingerUp.x < 0)
            {
                OnSwipeLeft();
            }

            fingerUp = fingerDown;
        }
    }

    private void OnSwipeUp()
    {
        currentCarLaneIndex--;
        currentCarLaneIndex = Mathf.Clamp(currentCarLaneIndex, 0, possibleCarLaneHeights.Length - 1);
        SetCarLane(possibleCarLaneHeights[currentCarLaneIndex]);
    }

    private void OnSwipeDown()
    {
        currentCarLaneIndex++;
        currentCarLaneIndex = Mathf.Clamp(currentCarLaneIndex, 0, possibleCarLaneHeights.Length - 1);
        SetCarLane(possibleCarLaneHeights[currentCarLaneIndex]);
    }

    private void OnSwipeLeft()
    {
        currentCarLocationIndex--;
        currentCarLocationIndex = Mathf.Clamp(currentCarLocationIndex, 0, possibleCarLocations.Length - 1);
        SetCarLocation(possibleCarLocations[currentCarLocationIndex]);
        SetCarLane(possibleCarLaneHeights[currentCarLaneIndex]);
    }

    private void OnSwipeRight()
    {
        currentCarLocationIndex++;
        currentCarLocationIndex = Mathf.Clamp(currentCarLocationIndex, 0, possibleCarLocations.Length - 1);
        SetCarLocation(possibleCarLocations[currentCarLocationIndex]);
        SetCarLane(possibleCarLaneHeights[currentCarLaneIndex]);
    }

    private float verticalMove()
    {
        return Mathf.Abs(fingerDown.y - fingerUp.y);
    }

    private float horizontalValMove()
    {
        return Mathf.Abs(fingerDown.x - fingerUp.x);
    }

    private void CheckForCarJumping()
    {
        if(isCarJumping == true)
        {
            currentJumpSeconds += Time.deltaTime;

            if(currentJumpSeconds >= totalJumpSeconds)
            {
                transform.localPosition -= (Vector3.up * carMovementAnimCurve.Evaluate(Time.deltaTime * jumpSpeed));

                if(transform.localPosition.y <= possibleCarLaneHeights[currentCarLaneIndex])
                {
                    isCarJumping = false;
                    currentJumpSeconds = 0.0f;

                    Vector3 tempPos = transform.localPosition;
                    tempPos.y = possibleCarLaneHeights[currentCarLaneIndex];
                    transform.localPosition = tempPos;
                }
            }
            else
            {
                transform.localPosition += (Vector3.up * carMovementAnimCurve.Evaluate(Time.deltaTime * jumpSpeed));
            }
            
            return;
        }

        secondsSinceCarJumped += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            MakeCarJump();
        }
    }

    private void MakeCarJump()
    {
        carAnimator.SetTrigger("Jump");
        isCarJumping = true;
        secondsSinceCarJumped = 0.0f;
        carJumpedAudioSource.Play();
    }
}