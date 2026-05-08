using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    private PlayerController player;
    private NPCController grabbedNPC;
    private Transform playerVisual; // Référence au Transform du modèle 3D du joueur

    [Header("Grabbing NPC")]
    public float grabSpeedMultiplier = 0.3f;
    public float grabRadius = 1.5f;
    



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<PlayerController>();
        player.speedMultiplier = grabbedNPC != null ? grabSpeedMultiplier : 1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(playerVisual.position, grabRadius);
        foreach (Collider hit in hits)
        {
            NPCController npc = hit.GetComponentInParent<NPCController>();
            if (npc != null && npc.isRagdoll && !npc.isGrabbed)
            {
                grabbedNPC = npc;
                break;
            }
        }
    }

    void ReleaseGrab()
    {
        if (grabbedNPC == null) return;
        grabbedNPC.Release();
        grabbedNPC = null;
    }

    public void OnGrab()
    {
        if (grabbedNPC == null)
                TryGrab();
            else
                ReleaseGrab();
    }
}
