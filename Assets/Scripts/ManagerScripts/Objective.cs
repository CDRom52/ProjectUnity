using UnityEngine;

[System.Serializable]
public class Objective
{
    public string description;
    public bool isCompleted;
    public Object targetObject;
    public string subjectKey;

    public Objective(string description, Object targetObject, string subjectKey)
    {
        this.description = description;
        this.targetObject = targetObject;
        this.subjectKey = subjectKey;
        this.isCompleted = false;
    }
}