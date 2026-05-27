using UnityEngine;

public class BatteryItem : MonoBehaviour
{
    public float rechargeAmount = 50f;

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