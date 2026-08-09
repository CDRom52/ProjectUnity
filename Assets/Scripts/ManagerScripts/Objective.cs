using UnityEngine;

public enum ObjectiveType
{
    DeliverPackage,
    TriggerNPCDialogue
}

[System.Serializable]
public class Objective
{
    public string description;
    public ObjectiveType type;
    public bool isCompleted;

    [Header("Delivery Target (If DeliverPackage)")]
    public int packageID;

    [Header("NPC Target (If TriggerNPCDialogue)")]
    public NPCDialogue targetNPC;
    public int dialogueLineIndex;

    public Objective(string description, int packageID)
    {
        this.description = description;
        type = ObjectiveType.DeliverPackage;
        this.packageID = packageID;
        isCompleted = false;
    }

    public Objective(string description, NPCDialogue targetNPC, int dialogueLineIndex)
    {
        this.description = description;
        type = ObjectiveType.TriggerNPCDialogue;
        this.targetNPC = targetNPC;
        this.dialogueLineIndex = dialogueLineIndex;
        isCompleted = false;
    }
}