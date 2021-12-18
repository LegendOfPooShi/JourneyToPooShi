using System;
using UnityEngine;

[Serializable]
public struct EnemyGroups
{
    public string name;
    public GameObject enemyPrefab;
    public int totalEnemies;
    public GameObject[] inGameEnemies;
    public Rigidbody2D[] inGameRBEnemies;
    public int nextUpEnemyID;
}

public class EnemyManager : MonoBehaviour
{
    public enum EnemyType { NULL = -1, BOMB, KING_BALD_EAGLE, POOSHI_TOKEN }

    [SerializeField]
    private Transform car = null;

    [SerializeField]
    private EnemyGroups[] enemyGroups = null;
    [SerializeField]
    private float enemySpawnRateSeconds = 3.0f;
    [SerializeField]
    private Vector2 startingEnemyXOffsetLocationRange = Vector2.zero;
    [SerializeField]
    private Vector2 startingEnemyYOffsetLocationRange = Vector2.zero;

    private float currentSpawnEnemySeconds = 0.0f;
    private bool areWeActivatingBombTokenEnemies = false;

    public void ActivateTheBoss()
    {
        ActivateNextEnemy(EnemyType.KING_BALD_EAGLE, new Vector3(740.0f, -3.1f, 1.0f));
        areWeActivatingBombTokenEnemies = false;
    }

    public void DeactivateTheBoss()
    {
        enemyGroups[(int)EnemyType.KING_BALD_EAGLE].inGameEnemies[0].SetActive(false);
    }

    public void ActivateBombTokenSpawner()
    {
        areWeActivatingBombTokenEnemies = true;
    }

    public void DeactivateBombTokenSpawner()
    {
        areWeActivatingBombTokenEnemies = false;
    }

    public void ActivateNextEnemy(EnemyType enemyType, Vector3 startingPosition)
    {
        int enemyGroupID = (int)enemyType;

        int nextEnemyID = enemyGroups[enemyGroupID].nextUpEnemyID;
        enemyGroups[enemyGroupID].inGameEnemies[nextEnemyID].gameObject.SetActive(true);
        enemyGroups[enemyGroupID].inGameEnemies[nextEnemyID].transform.position = startingPosition;

        enemyGroups[enemyGroupID].nextUpEnemyID++;
        if(enemyGroups[enemyGroupID].nextUpEnemyID >= enemyGroups[enemyGroupID].inGameEnemies.Length)
        {
            enemyGroups[enemyGroupID].nextUpEnemyID = 0;
        }
    }

    private void Awake()
    {
        for(int i = 0; i < enemyGroups.Length; i++)
        {
            if (enemyGroups[i].enemyPrefab == null)
            {
                continue;
            }

            enemyGroups[i].inGameEnemies = new GameObject[enemyGroups[i].totalEnemies];
            enemyGroups[i].inGameRBEnemies = new Rigidbody2D[enemyGroups[i].totalEnemies];

            for (int j = 0; j < enemyGroups[i].totalEnemies; j++)
            {
                enemyGroups[i].inGameEnemies[j] = Instantiate(enemyGroups[i].enemyPrefab, transform);
                enemyGroups[i].inGameEnemies[j].name = enemyGroups[i].enemyPrefab.name + j;
                enemyGroups[i].inGameEnemies[j].SetActive(false);
                enemyGroups[i].inGameRBEnemies[j] = enemyGroups[i].inGameEnemies[j].GetComponent<Rigidbody2D>();
            }
        }
    }

    private void Update()
    {
        if(areWeActivatingBombTokenEnemies == false)
        {
            return;
        }

        currentSpawnEnemySeconds += Time.deltaTime;
        if(currentSpawnEnemySeconds < enemySpawnRateSeconds)
        {
            return;
        }

        currentSpawnEnemySeconds = 0.0f;

        Vector3 newStartingPositionOffset = new Vector3(
            UnityEngine.Random.Range(startingEnemyXOffsetLocationRange.x, startingEnemyXOffsetLocationRange.y),
            UnityEngine.Random.Range(startingEnemyYOffsetLocationRange.x, startingEnemyYOffsetLocationRange.y), 1.0f);
        ActivateNextEnemy(EnemyType.BOMB, car.position + newStartingPositionOffset);
        ActivateNextEnemy(EnemyType.POOSHI_TOKEN, car.position + newStartingPositionOffset - (Vector3.left * 3f));
    }

    private void OnDisable()
    {
        currentSpawnEnemySeconds = 0;
        areWeActivatingBombTokenEnemies = false;

        for (int i = 0; i < enemyGroups.Length; i++)
        {
            for (int j = 0; j < enemyGroups[i].inGameEnemies.Length; j++)
            {
                if(enemyGroups[i].inGameEnemies[j] == null)
                {
                    continue;
                }

                if (enemyGroups[i].inGameEnemies[j].gameObject == null)
                {
                    continue;
                }

                enemyGroups[i].inGameEnemies[j].gameObject.SetActive(false);

                if (enemyGroups[i].inGameRBEnemies == null)
                {
                    continue;
                }

                if (j >= enemyGroups[i].inGameRBEnemies.Length)
                {
                    continue;
                }

                if (enemyGroups[i].inGameRBEnemies[j] == null)
                {
                    continue;
                }

                enemyGroups[i].inGameRBEnemies[j].velocity = Vector2.zero;
                enemyGroups[i].inGameRBEnemies[j].transform.position = Vector2.zero;
            }
        }
    }
}