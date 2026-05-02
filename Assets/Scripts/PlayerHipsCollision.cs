using UnityEngine;

public class PlayerHipsCollision : MonoBehaviour
{
    private PlayerController player;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    void OnCollisionEnter(Collision collision)
    {
        player.OnBoneCollision(collision);
    }
}