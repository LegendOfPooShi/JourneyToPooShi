using UnityEngine;

public class Bomb : MonoBehaviour
{
    private Rigidbody2D rigBody = null;
    private Car car = null;
    private Collider2D carCollider = null;

    private void Awake()
    {
        rigBody = GetComponent<Rigidbody2D>();
        car = GameObject.FindObjectOfType<Car>();
        carCollider = car.GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        rigBody.velocity = Vector2.zero;    
    }

    private void LateUpdate()
    {
        if((transform.localPosition.x <= -24.0f) || (transform.localPosition.y >= 16.0f))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider != carCollider)
        {
            return;
        }

        car.HitEnemyObject();
    }
}