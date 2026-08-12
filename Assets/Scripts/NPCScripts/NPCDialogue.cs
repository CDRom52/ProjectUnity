using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public int targetLineIndex;
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(3, 5)]
    public string npcText;
    public List<DialogueChoice> choices;
}

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC Profile")]
    public string npcName;

    [Header("Dialogue Content")]
    public DialogueNode[] nodes;
    public bool isTalking = false;

    private int currentLineIndex = 0;

    public void Interact()
    {
        if (nodes == null || nodes.Length == 0) return;

        if (DialogueManager.Instance.isTyping)
        {
            DialogueManager.Instance.Skip();
            return;
        }

        if (currentLineIndex < nodes.Length)
        {
            isTalking = true;
            
            ObjectiveManager.Instance.CheckNPCDialogue(this, currentLineIndex);

            DialogueManager.Instance.ShowDialogue(npcName, nodes[currentLineIndex], this);

            if (currentLineIndex == nodes.Length - 1)
            {
                DialogueManager.Instance.LastLine();
            }

            currentLineIndex++;
        }
        else
        {
            CloseDialogue();
        }
    }

    public void JumpToLine(int lineIndex)
    {
        currentLineIndex = lineIndex;
        
        if (currentLineIndex < nodes.Length)
        {
            isTalking = true;
            
            ObjectiveManager.Instance.CheckNPCDialogue(this, currentLineIndex);

            DialogueManager.Instance.ShowDialogue(npcName, nodes[currentLineIndex], this);

            if (currentLineIndex == nodes.Length - 1)
            {
                DialogueManager.Instance.LastLine();
            }

            currentLineIndex++;
        }
        else
        {
            CloseDialogue();
        }
    }

    public void CloseDialogue()
    {
        isTalking = false;
        currentLineIndex = 0;
        DialogueManager.Instance.Close();
    }
}