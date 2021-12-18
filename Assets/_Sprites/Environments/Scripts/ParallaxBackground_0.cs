using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground_0 : MonoBehaviour
{
    public bool Camera_Move = false;
    public float Camera_MoveSpeed = 1.5f;

    [Header("Layer Setting")]
    public float[] Layer_Speed = new float[7];
    public GameObject[] Layer_Objects = new GameObject[7];

    private Transform _camera = null;
    private float[] startPos = new float[7];
    private float boundSizeX = 0.0f;
    private float sizeX = 0.0f;

    private void Start()
    {
        _camera = Camera.main.transform;
        sizeX = Layer_Objects[0].transform.localScale.x;
        boundSizeX = Layer_Objects[0].GetComponent<SpriteRenderer>().sprite.bounds.size.x;

        for (int i = 0; i < 5; i++)
        {
            startPos[i] = _camera.position.x;
        }
    }

    private void Update()
    {
        if (Camera_Move)
        {
            _camera.position += Vector3.right * Time.deltaTime * Camera_MoveSpeed;
        }

        for (int i = 0; i < 5; i++)
        {
            float temp = (_camera.position.x * (1 - Layer_Speed[i]));
            float distance = _camera.position.x  * Layer_Speed[i];

            Layer_Objects[i].transform.position = new Vector2(startPos[i] + distance, _camera.position.y);

            if (temp > startPos[i] + boundSizeX*sizeX)
            {
                startPos[i] += boundSizeX*sizeX;
            }
            else if(temp < startPos[i] - boundSizeX*sizeX)
            {
                startPos[i] -= boundSizeX*sizeX;
            }
        }
    }
}