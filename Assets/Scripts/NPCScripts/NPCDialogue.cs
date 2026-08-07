using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC Profile")]
    public string npcName;

    [Header("Dialogue Content")]
    [TextArea(3, 5)]
    public string[] lines;

    private int currentLineIndex = 0;

    public void Interact()
    {
        if (lines.Length == 0) return;

        if (currentLineIndex < lines.Length)
        {
            DialogueManager.Instance.ShowDialogue(npcName, lines[currentLineIndex]);
            currentLineIndex++;
        }
        else
        {
            currentLineIndex = 0;
            DialogueManager.Instance.SkipOrClose();
        }
    }
}