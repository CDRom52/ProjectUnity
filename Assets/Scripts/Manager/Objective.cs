using UnityEngine;

public enum ObjectiveType
{
    DeliverPackage,
    GetNPCInfo
}

[System.Serializable]
public class Objective
{
    public string description;
    public ObjectiveType type;
    public bool isCompleted;

    [Header("Delivery Target")]
    public int packageID;

    [Header("NPC Target")]
    public NPCDialogue targetNPC;
    public string lineId;

    public Objective(string description, int packageID)
    {
        this.description = description;
        type = ObjectiveType.DeliverPackage;
        this.packageID = packageID;
        isCompleted = false;
    }

    public Objective(string description, NPCDialogue targetNPC, string lineId)
    {
        this.description = description;
        type = ObjectiveType.GetNPCInfo;
        this.targetNPC = targetNPC;
        this.lineId = lineId;
        isCompleted = false;
    }
}