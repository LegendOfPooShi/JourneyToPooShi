using UnityEngine;

public class PooShiToken : MonoBehaviour
{
    private Car car = null;
    private Collider2D carCollider = null;

    private void LateUpdate()
    {
        if (transform.localPosition.x <= -24.0f)
        {
            gameObject.SetActive(false);
        }
    }

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

        car.HitPooShiTokenObject();
        gameObject.SetActive(false);
    }
}