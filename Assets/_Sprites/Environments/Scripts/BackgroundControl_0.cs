using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundControl_0 : MonoBehaviour
{
    [SerializeField]
    private float secondsBeforeNextBG = 0.0f;

    [SerializeField]
    private bool allowDebugControls = true;

    [SerializeField]
    private Car car = null;

    [Header("BackgroundNum 0 -> 3")]
    public int backgroundNum;
    public Sprite[] Layer_Sprites;
    private GameObject[] Layer_Object = new GameObject[5];
    private int max_backgroundNum = 3;

    public void NextBG()
    {
        backgroundNum++;

        if (backgroundNum > max_backgroundNum)
        {
            backgroundNum = 0;
        }

        ChangeSprite();
    }

    public void BackBG()
    {
        backgroundNum--;

        if (backgroundNum < 0)
        {
            backgroundNum = max_backgroundNum;
        }

        ChangeSprite();
    }

    public void SetBG(int backgroundID)
    {
        backgroundNum = backgroundID;

        if (backgroundNum < 0)
        {
            backgroundNum = 0;
        }
        else if(backgroundNum > 3)
        {
            backgroundNum = 3;
        }

        ChangeSprite();
    }

    private void Start()
    {
        for (int i = 0; i < Layer_Object.Length; i++)
        {
            Layer_Object[i] = GameObject.Find("Layer_" + i);
        }
        
        ChangeSprite();
    }

    private void Update()
    {
        if(car.SecondsTraveled >= secondsBeforeNextBG)
        {
            car.SecondsTraveled = 0;
            NextBG();
        }

        if(allowDebugControls == false)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            NextBG();
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            BackBG();
        }
    }

    private void ChangeSprite()
    {
        Layer_Object[0].GetComponent<SpriteRenderer>().sprite = Layer_Sprites[backgroundNum * 5];

        for (int i = 1; i < Layer_Object.Length; i++)
        {
            Sprite changeSprite = Layer_Sprites[backgroundNum * 5 + i];
            Layer_Object[i].GetComponent<SpriteRenderer>().sprite = changeSprite;
            Layer_Object[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = changeSprite;
            Layer_Object[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = changeSprite;
        }
    }
}