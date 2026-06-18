using UnityEngine;
using UnityEngine.SceneManagement;

public class Collectable : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 90f;

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {   
            FindAnyObjectByType<ScoreManager>().AddCoin();
            //FindAnyObjectByType<AudioManager>().PlaySFX(FindAnyObjectByType<AudioManager>().Score);
            Destroy(gameObject);
        }
    }

}