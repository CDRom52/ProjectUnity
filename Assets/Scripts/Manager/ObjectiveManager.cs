using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [Header("UI References")]
    public GameObject objectivePanel;
    public Transform listContentContainer;
    public GameObject objectiveTextPrefab;

    public List<Objective> activeObjectives = new List<Objective>();
    private Dictionary<Objective, TextMeshProUGUI> uiMap = new Dictionary<Objective, TextMeshProUGUI>();

    private void Awake()
    {
        Instance = this;

        objectivePanel.SetActive(false);
    }

    public void AddDeliveryObjective(string description, int packageID)
    {
        Objective obj = new Objective(description, packageID);
        RegisterObjective(obj);
    }

    public void AddDialogueObjective(string description, NPCDialogue npc, string lineId)
    {
        Objective obj = new Objective(description, npc, lineId);
        RegisterObjective(obj);
    }

    private void RegisterObjective(Objective obj)
    {
        activeObjectives.Add(obj);

        GameObject textObj = Instantiate(objectiveTextPrefab, listContentContainer);
        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        tmpText.text = obj.description;

        uiMap.Add(obj, tmpText);
    }

    public void CheckPackageDelivery(PlatformManager platform, int packageID)
    {
        foreach (Objective obj in activeObjectives)
        {
            if (obj.isCompleted || obj.type != ObjectiveType.DeliverPackage) continue;

            if (obj.packageID == packageID)
            {
                CompleteObjective(obj);
                break;
            }
        }
    }

    public void CheckNPCDialogue(NPCDialogue npc, string lineId)
    {
        foreach (Objective obj in activeObjectives)
        {
            if (obj.isCompleted || obj.type != ObjectiveType.GetNPCInfo) continue;

            if (obj.targetNPC == npc && obj.lineId == lineId)
            {
                CompleteObjective(obj);
                break;
            }
        }
    }

    private void CompleteObjective(Objective obj)
    {
        obj.isCompleted = true;

        if (uiMap.TryGetValue(obj, out TextMeshProUGUI tmpText))
        {
            tmpText.text = $"<s>{obj.description}</s>";
            tmpText.color = Color.gray;
        }

        Debug.Log($"Objective Completed: {obj.description}");
    }

    public void ToggleUI()
    {
        objectivePanel.SetActive(!objectivePanel.activeSelf);
    }
}