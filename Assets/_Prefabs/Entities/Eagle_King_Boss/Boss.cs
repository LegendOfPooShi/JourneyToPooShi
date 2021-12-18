using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer bossSR = null;
    [SerializeField]
    private Sprite bossNormalSprite = null;
    [SerializeField]
    private Sprite bossDefeatedSprite = null;
    [SerializeField]
    private Animator bossAnimator = null;
    [SerializeField]
    private Transform bossDefeatedBomb = null;
    [SerializeField]
    private float bossDefeatedDropSpeed = 0.64f;
    [SerializeField]
    private Vector3 booStartingLocalPosition = Vector3.zero;

    [SerializeField]
    private Collider2D bossStateChangeTrigger = null;
    [SerializeField]
    private GameObject remark = null;
    [SerializeField]
    private float totalRemarkSeconds = 5.0f;

    [SerializeField]
    private Transform charactersTransform = null;
    [SerializeField]
    private PooShi pooShi = null;

    [SerializeField]
    private EnemyManager enemyManager = null;

    [SerializeField]
    private Vector3 pooShiStartingLocalPosition = Vector3.zero;

    [SerializeField]
    private Vector3[] targetBossFightLocalPositions = null;
    [SerializeField]
    private float totalSecondsBeforePositionChange = 5.0f;
    [SerializeField]
    private AnimationCurve bossMovementAnimCurve = null;
    [SerializeField]
    private float bossMovementSpeed = 5.0f;
    [SerializeField]
    private float bossRotationSpeed = 3.0f;

    [SerializeField]
    private float secondsToAttackGroundOnlyForNoJumping = 5.0f;
    [SerializeField]
    private float secondsToSurviveForWin = 3.0f;
    [SerializeField]
    private float secondsToWaitForBombDrop = 3.0f;
    [SerializeField]
    private float secondsToWaitForBombHit = 3.0f;
    [SerializeField]
    private float secondsBeforeDeactivatingDefeatedBoss = 18.0f;

    private Car car = null;
    private Collider2D carCollider = null;
    private float currentRemarkSeconds = 0.0f;
    private float currentSecondsForPositionChange = 0.0f;
    private int currentTargetPositionIndex = 0;
    private bool hasStarterTriggerFired = false;
    private float currentSurvivalSeconds = 0.0f;
    private bool eagleIsDefeated = false;

    private void Awake()
    {
        car = GameObject.FindObjectOfType<Car>();
        carCollider = car.GetComponent<Collider2D>();
        remark.SetActive(false);
        bossDefeatedBomb.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        bossDefeatedBomb.gameObject.SetActive(false);
        bossAnimator.enabled = true;
        currentTargetPositionIndex = 1;
        bossSR.sprite = bossNormalSprite;
        bossStateChangeTrigger.enabled = true;
        transform.SetParent(charactersTransform);
        transform.localPosition = booStartingLocalPosition;
        transform.localScale = Vector3.one;
        pooShi.transform.SetParent(transform);
        pooShi.transform.localPosition = pooShiStartingLocalPosition;
        hasStarterTriggerFired = false;
        enemyManager.DeactivateBombTokenSpawner();
    }

    private void Update()
    {
        if (transform.parent != Camera.main.transform)
        {
            if(eagleIsDefeated == false)
            {
                return;
            }

            transform.Translate(Vector3.down * Time.deltaTime * bossDefeatedDropSpeed);
            return;
        }

        if (remark.activeSelf == true)
        {
            EnsureBossFacesCar();

            currentRemarkSeconds += Time.deltaTime;
            if (currentRemarkSeconds >= totalRemarkSeconds)
            {
                currentRemarkSeconds = 0.0f;
                remark.SetActive(false);
                pooShi.transform.SetParent(Camera.main.transform);
            }

            return;
        }

        UpdateMovement();
        UpdateWinCondition();
    }

    private void UpdateWinCondition()
    {
        if (currentSurvivalSeconds <= -1.0f)
        {
            return;
        }

        currentSurvivalSeconds += Time.deltaTime;
        if(currentSurvivalSeconds < secondsToSurviveForWin)
        {
            return;
        }

        currentSurvivalSeconds = -1.0f;
        StartCoroutine(EagleDefeated());
    }

    private void OnDisable()
    {
        currentRemarkSeconds = 0.0f;
        currentSurvivalSeconds = 0.0f;
        eagleIsDefeated = false;
    }

    private void UpdateMovement()
    {
        if (eagleIsDefeated == true)
        {
            return;
        }

        UpdateBossPositioning();

        currentSecondsForPositionChange += Time.deltaTime;
        if (currentSecondsForPositionChange < totalSecondsBeforePositionChange)
        {
            return;
        }

        currentSecondsForPositionChange = 0.0f;
        int previousTargetPositionIndex = currentTargetPositionIndex;

        if(transform.localPosition.x > car.transform.localPosition.x)
        {
            if (car.SecondsSinceCarJumped > secondsToAttackGroundOnlyForNoJumping)
            {
                currentTargetPositionIndex = Random.Range(12, targetBossFightLocalPositions.Length - 1);
            }
            else
            {
                int upOrDown = Random.Range(0, 100);
                if (upOrDown < 64f)
                {
                    currentTargetPositionIndex = Random.Range(12, targetBossFightLocalPositions.Length - 1);
                }
                else
                {
                    currentTargetPositionIndex = Random.Range(4, 8);
                }
            }

            return;
        }
        else if (transform.localPosition.x < car.transform.localPosition.x)
        {
            if (car.SecondsSinceCarJumped > secondsToAttackGroundOnlyForNoJumping)
            {
                currentTargetPositionIndex = Random.Range(8, 12);
            }
            else
            {
                int upOrDown = Random.Range(0, 100);
                if (upOrDown < 64f)
                {
                    currentTargetPositionIndex = Random.Range(8, 12);
                }
                else
                {
                    currentTargetPositionIndex = Random.Range(0, 4);
                }
            }

            return;
        }

        currentTargetPositionIndex = Random.Range(0, targetBossFightLocalPositions.Length - 1);
        if(previousTargetPositionIndex != currentTargetPositionIndex)
        {
            return;
        }

        currentTargetPositionIndex += 1;
        if(currentTargetPositionIndex >= targetBossFightLocalPositions.Length)
        {
            currentTargetPositionIndex = Random.Range(0, targetBossFightLocalPositions.Length - 2);
        }
    }

    private void UpdateBossPositioning()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, 
            targetBossFightLocalPositions[currentTargetPositionIndex],
            bossMovementAnimCurve.Evaluate(Time.deltaTime * bossMovementSpeed));

        Vector3 vectorToTarget = targetBossFightLocalPositions[currentTargetPositionIndex] - transform.localPosition;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * bossRotationSpeed);

        EnsureBossFacesCar();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == carCollider)
        {
            if (hasStarterTriggerFired == false)
            {
                transform.SetParent(Camera.main.transform);
                remark.SetActive(true);
                bossStateChangeTrigger.enabled = false;
                hasStarterTriggerFired = true;

                return;
            }

            car.HitEnemyObject();
        }
    }

    private IEnumerator EagleDefeated()
    {
        eagleIsDefeated = true;
        bossDefeatedBomb.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(secondsToWaitForBombDrop);
        bossAnimator.enabled = false;
        bossSR.sprite = bossDefeatedSprite;

        yield return new WaitForSecondsRealtime(secondsToWaitForBombHit);

        bossSR.sprite = bossDefeatedSprite;
        transform.SetParent(charactersTransform);
        pooShi.transform.SetParent(charactersTransform);

        yield return new WaitForSecondsRealtime(secondsBeforeDeactivatingDefeatedBoss);
        gameObject.SetActive(false);
    }

    private void EnsureBossFacesCar()
    {
        if (transform.localPosition.x < car.transform.localPosition.x)
        {
            Vector3 currentRotation = transform.eulerAngles;
            currentRotation.z = 0.0f;
            transform.eulerAngles = currentRotation;

            Vector3 currentScale = transform.localScale;
            currentScale.x = -1.0f;
            transform.localScale = currentScale;
        }
        else
        {
            Vector3 currentrotation = transform.eulerAngles;
            currentrotation.z = 0.0f;
            transform.eulerAngles = currentrotation;

            Vector3 currentScale = transform.localScale;
            currentScale.x = 1.0f;
            transform.localScale = currentScale;
        }
    }
}