using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC Profile")]
    public string npcName;

    [Header("Dialogue Content")]
    [TextArea(3, 5)]
    public string[] lines;
    public bool isTalking = false;

    private int currentLineIndex = 0;

    public void Interact()
    {
        if (lines.Length == 0) return;

        if (currentLineIndex < lines.Length)
        {
            isTalking = true;
            DialogueManager.Instance.ShowDialogue(npcName, lines[currentLineIndex]);
            currentLineIndex++;
        }
        else
        {
            isTalking = false;
            currentLineIndex = 0;
        }

        if (currentLineIndex == lines.Length - 1)
        {
            DialogueManager.Instance.LastLine();
        }

        if (currentLineIndex == 0)
        {
            ObjectiveManager.Instance.AddDeliveryObjective(
                "- Deliver NPC 1's package to its destination.",
                0
            );
        }

    }
}