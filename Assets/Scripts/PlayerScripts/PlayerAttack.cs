using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerController player;
    private Animator anim;

    [Header("Punch Settings")]
    public float punchRange = 2.5f;
    public float punchAngle = 60f;
    public float knockbackForce = 25f;
    public LayerMask npcLayerMask;

    void Start()
    {
        player = GetComponent<PlayerController>();
        anim = GetComponentInChildren<Animator>();
    }

    public void Attack()
    {
        anim.SetTrigger("Punch");
        Vector3 punchOrigin = transform.position + transform.forward * (punchRange * 0.5f);
        Collider[] hitColliders = Physics.OverlapSphere(punchOrigin, punchRange, npcLayerMask);

        foreach (Collider hitCollider in hitColliders)
        {
            Vector3 directionToTarget = (hitCollider.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < punchAngle / 2f)
            {
                if (hitCollider.TryGetComponent<NPCController>(out NPCController npc))
                {
                    Vector3 knockbackDir = (hitCollider.transform.position - transform.position).normalized;
                    
                    npc.Impact(knockbackDir, knockbackForce);
                }
            }
        }
    }
}
