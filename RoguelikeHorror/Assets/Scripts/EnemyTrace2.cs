using UnityEngine;

public class EnemyTrace2 : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float raycastDistance = 0.2f;
    public float traceDistance = 2f;

    [Header("체력 설정")]
    public int maxHp = 5;
    private int currentHp;

    [Header("피격 이펙트")]
    public float hitFlashDuration = 0.1f;
    private Color originalColor;

    [Header("아이템 드롭 설정")]
    [SerializeField] private GameObject hpItemPrefab;
    [SerializeField] private GameObject batteryItemPrefab;
    [SerializeField] private GameObject xpItemPrefab;

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

            // ⭐ 처음 시작할 때는 손전등 불빛을 안 받고 있으므로 에너미를 안 보이게 숨깁니다!
            spriteRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (player == null) return;

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

        // 🛠️ [중요 수정] 이동 방향을 결정할 변수를 하나 두고, 루프 안에서는 '결정'만 내립니다.
        Vector3 finalDirection = direction;

        foreach (RaycastHit2D rHit in hits)
        {
            // 내가 아닌 다른 오브젝트 중 "Wall" 태그를 가진 벽을 발견했다면 우회 방향 선택
            if (rHit.collider != null && rHit.collider.gameObject != gameObject && rHit.collider.CompareTag("Wall"))
            {
                finalDirection = Quaternion.Euler(0f, 0f, -90f) * direction;
                break; // 벽을 하나라도 찾았으면 계산 끝내고 루프 탈출
            }
        }

        // ⭐ 루프가 완전히 끝난 뒤, 이번 프레임에 "딱 한 번만" 이동을 실행합니다! (중복 이동 버그 해결)
        transform.Translate(finalDirection * moveSpeed * Time.deltaTime);
    }

    // 💡 손전등 불빛(Trigger) 안에 들어왔을 때 실행
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Flashlight"))
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true; // 에너미 보이기!
            }
        }
    }

    // 💡 손전등 불빛(Trigger) 밖으로 나갔을 때 실행
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Flashlight"))
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false; // 에너미 다시 숨기기!
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
        // 1. [100% 드랍] XP 아이템 생성
        if (xpItemPrefab != null)
        {
            Instantiate(xpItemPrefab, transform.position, Quaternion.identity);
        }

        // 2. [확률 드랍] 체력/배터리/None 확률 계산
        float randomValue = Random.Range(0f, 100f);

        if (randomValue < 33.33f)
        {
            if (hpItemPrefab != null)
            {
                Instantiate(hpItemPrefab, transform.position, Quaternion.identity);
            }
        }
        else if (randomValue < 66.66f)
        {
            if (batteryItemPrefab != null)
            {
                Instantiate(batteryItemPrefab, transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }
}