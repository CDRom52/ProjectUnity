using System.Collections.Generic;

[System.Serializable]
public class ObjectiveData
{
    public string type; // "DeliverPackage" or "GetNPCInfo"
    public string description;
    public int packageID;
    public string lineId;
}

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public string nextNodeId;
    public ObjectiveData addObjective;
    
    public int requiresCompletedPackageID = -1; 
}

[System.Serializable]
public class DialogueNode
{
    public string id;
    public string speaker;
    public string npcText;
    public List<DialogueChoice> choices;
}

[System.Serializable]
public class DialogueContainer
{
    public List<DialogueNode> nodes;
}