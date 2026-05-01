using UnityEngine;

public class RagdollBone : MonoBehaviour
{
    private NPCController npc;

    void Start()
    {
        npc = GetComponentInParent<NPCController>();
    }

    void OnCollisionEnter(Collision collision)
    {
        npc.OnBoneCollision(collision);
    }
}