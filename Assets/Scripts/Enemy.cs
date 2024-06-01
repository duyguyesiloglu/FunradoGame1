using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    public int level = 1;
    public TextMeshPro levelText;
    public float speed = 2f;
    public Transform[] patrolPoints; 
    private int currentPatrolIndex = 0;
    public float rotationSpeed = 10f; 

    public float viewAngle = 90f; 
    public float viewDistance = 5f; 
    public LayerMask playerLayer; 

    private Transform enemyTransform;
    private Animator animator; 
    private bool isDead = false; 

    private EnemyController enemyController; 

    private void Start()
    {
        enemyTransform = transform; 
        animator = GetComponent<Animator>(); 
        enemyController = FindObjectOfType<EnemyController>();

 
        if (levelText != null)
        {
            levelText.text = "Lv. " + level;
        }
    }

    private void Update()
    {
        if (isDead) return;
        Patrol();
        DetectPlayer();
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        // Patrol
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector3 direction = (targetPoint.position - enemyTransform.position).normalized;
        enemyTransform.position = Vector3.MoveTowards(enemyTransform.position, targetPoint.position, speed * Time.deltaTime);

        // Enemy -> Patrol
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        // Next Patrol
        if (Vector3.Distance(enemyTransform.position, targetPoint.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void DetectPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewDistance, playerLayer);
        foreach (var hitCollider in hitColliders)
        {
            Transform playerTransform = hitCollider.transform;
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < viewAngle / 2)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, viewDistance))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.Log("Player detected!");
                        
                    }
                }
            }
        }
    }

   

    public void PlayDeathAnimation()
    {
        if (animator != null)
        {
            isDead = true;
            animator.SetTrigger("isDead");
            enemyController.EnemyDefeated(this);
        }
    }
}
