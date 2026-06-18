using UnityEngine;
public class EnemyTraceController : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float raycastDistance = 0.2f;
    public float traceDistance = 2f;

    [Header("체력 설정")]
    public int maxHp = 3;
    private int currentHp;

    [Header("피격 이펙트")]
    public float hitFlashDuration = 0.1f;
    private Color originalColor;

    [Header("아이템 드롭 설정")]
    [SerializeField] private GameObject hpItemPrefab;      // 유니티 인스펙터에서 체력 아이템 프리팹 등록
    [SerializeField] private GameObject batteryItemPrefab; // 유니티 인스펙터에서 배터리 아이템 프리팹 등록
    [SerializeField] private GameObject xpItemPrefab;      // ⭐ 여기에 XP 프리팹 변수를 새로 추가합니다!

    private bool hasPlayedChaseSound = false;
    private Transform player;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHp = maxHp; // 체력 초기화
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;

        }
    }

    private void Update()
    {
        Vector2 direction = player.position - transform.position;
        if (direction.magnitude > traceDistance)
        {
            hasPlayedChaseSound = false;
            return;
        }

        if (!hasPlayedChaseSound)
        {
            AudioManager audioManager = FindAnyObjectByType<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.enemy);
            }
            hasPlayedChaseSound = true;
        }

        // 좌우 방향에 따라 스프라이트 뒤집기
        if (direction.x > 0.01f)
            spriteRenderer.flipX = true;
        else if (direction.x < -0.01f)
            spriteRenderer.flipX = false;

        Vector2 directionNormalized = direction.normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, directionNormalized, raycastDistance);
        Debug.DrawRay(transform.position, directionNormalized * raycastDistance, Color.red);

        foreach (RaycastHit2D rHit in hits)
        {
            if (rHit.collider != null && rHit.collider.CompareTag("Wall"))
            {
                Vector3 alternativeDirection = Quaternion.Euler(0f, 0f, -90f) * direction;
                transform.Translate(alternativeDirection * moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(direction * moveSpeed * Time.deltaTime);
            }
        }
    }   

    // 외부(총알, 플레이어 공격 등)에서 호출하는 데미지 처리 함수
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        // 맞았다는 시각적 피드백 (빨갛게 깜빡)
        if (spriteRenderer != null)
            StartCoroutine(HitFlash());

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator HitFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        // 1. [100% 드랍] XP 아이템은 확률과 관계없이 무조건 생성
        if (xpItemPrefab != null)
        {
            Instantiate(xpItemPrefab, transform.position, Quaternion.identity);
        }

        // 2. [확률 드랍] 기존 체력/배터리/None 확률 계산
        float randomValue = Random.Range(0f, 100f);
            
        if (randomValue < 33.33f)
        {
            // 33.33% 확률로 체력 아이템 드롭
            if (hpItemPrefab != null)
            {
                Instantiate(hpItemPrefab, transform.position, Quaternion.identity);
            }
        }
        else if (randomValue < 66.66f)
        {
            // 33.33% 확률로 배터리 아이템 드롭
            if (batteryItemPrefab != null)
            {
                Instantiate(batteryItemPrefab, transform.position, Quaternion.identity);
            }
        }
        else
        {
            // 나머지 33.34% 확률로 아무것도 나오지 않음 (none)
            Debug.Log("적이 아이템을 떨어뜨리지 않았습니다.");
        }

        Destroy(gameObject);
    }
}