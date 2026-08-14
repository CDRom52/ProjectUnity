using UnityEngine;
public class NPCDialogue : MonoBehaviour
{
    public TextAsset npcDialogueJson;
    public DialogueManager dialogueManager;

    public void Interact()
    {
        dialogueManager.StartDialogue(this, npcDialogueJson);
    }

}