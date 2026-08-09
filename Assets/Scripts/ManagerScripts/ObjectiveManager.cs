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

    public void AddObjective(string description, Object target, string subjectKey)
    {
        Objective newObj = new Objective(description, target, subjectKey);
        activeObjectives.Add(newObj);

        GameObject textObj = Instantiate(objectiveTextPrefab, listContentContainer);
        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        tmpText.text = description;

        uiMap.Add(newObj, tmpText);
    }

    public bool TryCompleteObjective(Object target, string subjectKey)
    {
        foreach (Objective obj in activeObjectives)
        {
            if (obj.isCompleted) continue;

            if (obj.targetObject == target && obj.subjectKey == subjectKey)
            {
                obj.isCompleted = true;

                if (uiMap.TryGetValue(obj, out TextMeshProUGUI tmpText))
                {
                    tmpText.text = $"<s>{obj.description}</s>";
                    tmpText.color = Color.gray;
                }

                Debug.Log($"[Objective System] Completed: {obj.description}");
                return true;
            }
        }
        return false;
    }

    public void ToggleUI()
    {
        objectivePanel.SetActive(!objectivePanel.activeSelf);
    }
}