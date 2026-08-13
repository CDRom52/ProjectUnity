using System.Collections.Generic;

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public string nextNodeId;
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