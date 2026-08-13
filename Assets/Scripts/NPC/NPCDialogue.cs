using UnityEngine;
public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private TextAsset npcDialogueJson;
    [SerializeField] private DialogueManager dialogueManager;

    public void Interact()
    {
        dialogueManager.StartDialogue(this, npcDialogueJson);
    }

}