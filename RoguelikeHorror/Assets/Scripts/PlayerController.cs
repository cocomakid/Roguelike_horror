using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private float moveInput;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;   // 총알 프리팹 (Rigidbody2D 필요)
    public float bulletSpeed = 10f;
    public float shootDelay = 2f;     // 발사 간격
    public Transform firePoint;       // 총알이 나갈 위치 (없으면 플레이어 위치 사용)

    private float lastShootTime = -999f; // 시작하자마자 한 번 쏠 수 있게 음수로 초기화

    [Header("공격 설정")]
    public int damage = 1; // 기본 공격력 1


    [SerializeField] private UnityEngine.UI.Image[] hearts;
    private int hp = 4;
    public static int currentHp = 4;

    private bool isInvincible = false;

    private SpriteRenderer sr;

    private Animator animator;

    // 🔦 [추가] 회전시킬 손전등 불빛 오브젝트를 담을 변수
    private Transform flashlightTransform;
    private Vector2 lastDirection = Vector2.down;

    AudioManager audioManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
       // audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        hp = currentHp;
        UpdateHearts();

        flashlightTransform = transform.Find("FlashlightAxis");

        if (flashlightTransform == null)
        {
            // 백업용으로 Triangle을 직접 찾던 기존 코드 유지
            flashlightTransform = transform.Find("Triangle");
        }

        // 📂 [기존 세이브 로드 코드]
        SaveManager saveManager = FindAnyObjectByType<SaveManager>();
        if (saveManager != null)
        {
            PlayerSaveData loadedData = saveManager.LoadGame();
            if (loadedData != null)
            {
                this.damage = loadedData.damage;
                Debug.Log("세이브 데이터 로드 성공! 현재 공격력: " + this.damage);
            }
        }
    }

    private void Update()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
        {
            dir.x = -1;
            animator.SetInteger("Direction", 3);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            dir.x = 1;
            animator.SetInteger("Direction", 2);
        }

        if (Input.GetKey(KeyCode.W))
        {
            dir.y = 1;
            animator.SetInteger("Direction", 1);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            dir.y = -1;
            animator.SetInteger("Direction", 0);
        }

        dir.Normalize();
        animator.SetBool("IsMoving", dir.magnitude > 0);

        // ❌ 플레이어 몸통을 돌리던 코드는 삭제하고 이동만 처리합니다.
        GetComponent<Rigidbody2D>().linearVelocity = speed * dir;

        // 🛠️ [수정] 몸통은 가만히 두고, 오직 "손전등(Triangle)"만 회전시킵니다.
        if (dir.sqrMagnitude > 0)
        {
            lastDirection = dir;
        }

        if (flashlightTransform != null)
        {
            // WASD 이동 방향의 각도를 계산 (오른쪽이 0도 기준)
            float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;

            // ⭐ [수정] 아래를 바라보는 삼각형 스프라이트 기준에 맞춰 각도 보정값을 (+ 90f)로 변경합니다!
            flashlightTransform.localRotation = Quaternion.Euler(0f, 0f, angle + 90f);
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastShootTime + shootDelay)
        {
            TryShoot();
        }
    }

    void TryShoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet Prefab이 비어있거나 파괴된 참조입니다! 인스펙터를 확인하세요.");
            return;
        }

        GameObject target = FindNearestEnemy();
        if (target == null) return;

        lastShootTime = Time.time;

        Vector2 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = ((Vector2)target.transform.position - spawnPos).normalized;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetDamage(damage);
        }

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.linearVelocity = direction * bulletSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        GameObject nearest = null;
        float minDistance = float.MaxValue;
        Vector2 myPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(myPos, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance; // 오타 수정 완료
                nearest = enemy;
            }
        }

        return nearest;
    }

    void ResetInvincible()
    {
        isInvincible = false;
    }
    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Die()
    {
        PlayerController.currentHp = 4;
        rb.linearVelocity = Vector2.zero;

        SaveManager saveManager = FindAnyObjectByType<SaveManager>();
        XpManager xpManager = FindAnyObjectByType<XpManager>();
        FlashlightController flashlight = FindAnyObjectByType<FlashlightController>();

        if (saveManager != null)
        {
            int dmg = this.damage;
            float maxBat = (flashlight != null) ? flashlight.maxBattery : 100f;
            int mxXp = (xpManager != null) ? xpManager.GetMaxXp() : 100;

            saveManager.SaveGame(dmg, maxBat, mxXp);
        }

        this.enabled = false;
        Invoke(nameof(RestartScene), 1.5f);
    }

    public void TakeDamage()
    {
        if (isInvincible) return;

        isInvincible = true;

        hp--;
        currentHp = hp;

        UpdateHearts();

        if (hp <= 0)
        {
            Die();
            return;
        }

        Invoke(nameof(ResetInvincible), 2f);
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < hp;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 내 몸통에 있는 Collider2D 컴포넌트를 가져옵니다.
            Collider2D myBodyCollider = GetComponent<Collider2D>();

            // 유니티 공식 기능: "이번에 일어난 충돌이 내 몸통 콜라이더와 일어난 게 아니라면?"
            // 즉, 자식인 손전등(Triangle) 콜라이더에 닿아서 올라온 신호라면 무시하고 끝냅니다!
            if (myBodyCollider != null && !collision.IsTouching(myBodyCollider))
            {
                return; // 억울하게 피 닳는 현상 완벽 차단!
            }
        }

        // ----------------------------------------------------
        // 여기서부터는 오직 '플레이어 진짜 몸통'에 적이 닿았을 때만 실행됩니다!
        // ----------------------------------------------------

        if (collision.CompareTag("Enemy"))
        {
            if (isInvincible) return;

            //audioManager.PlaySFX(audioManager.enemy);

            TakeDamage(); // 이제 진짜 몸에 닿을 때만 대미지를 입습니다.
        }

        // ----------------------------------------------------
        // 이하 다른 트리거 아이템 처리 (기존 코드 유지)
        // ----------------------------------------------------
        if (collision.CompareTag("Respawn"))
        {
            if (audioManager != null) { audioManager.PlaySFX(audioManager.death); }
            Die();
        }

        if (collision.CompareTag("hpItem"))
        {
            if (audioManager != null) { audioManager.PlaySFX(audioManager.Hpitem); }

            if (hp < 4)
            {
                hp++;
                currentHp = hp;
                UpdateHearts();
            }
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Xp"))
        {
            if (audioManager != null) { audioManager.PlaySFX(audioManager.Xpitem); }
            XpManager xpManager = FindAnyObjectByType<XpManager>();
            if (xpManager != null)
            {
                xpManager.AddXp();
            }
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("BatteryItem")) // 인스펙터에 지정한 배터리 아이템 태그
        {
            AudioManager audioManager = FindAnyObjectByType<AudioManager>();
            if (audioManager != null) audioManager.PlaySFX(audioManager.Batteryitem);

            // 배터리 충전 로직 (예: battery += 20;)
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("KeyItem")) // 인스펙터에 지정한 열쇠 아이템 태그
        {
            AudioManager audioManager = FindAnyObjectByType<AudioManager>();
            if (audioManager != null) audioManager.PlaySFX(audioManager.keyitem);

            ScoreManager scoreManager = FindAnyObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddCoin(); // 이 함수가 실행되어야 텍스트가 1/5, 2/5로 바뀝니다!
            }

            // 열쇠 개수 증가 로직 (예: keyCount++;)
            Destroy(collision.gameObject);
        }
    }
}