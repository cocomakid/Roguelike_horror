using UnityEngine;

public class BatteryItem : MonoBehaviour
{
    public float rechargeAmount = 50f;
    [SerializeField] float rotationSpeed = 90f;
    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FlashlightController flashlight = collision.GetComponent<FlashlightController>();

            if (flashlight != null)
            {
                flashlight.RechargeBattery(rechargeAmount);
                Destroy(gameObject);
            }
        }
    }
}