using UnityEngine;

public class PooShi : MonoBehaviour
{
    [SerializeField]
    private HorizontalScrollerUI gameUI = null;

    private Car car = null;
    private Collider2D carCollider = null;

    private void Awake()
    {
        car = GameObject.FindObjectOfType<Car>();
        carCollider = car.GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider != carCollider)
        {
            return;
        }

        gameUI.UserWon();
    }
}