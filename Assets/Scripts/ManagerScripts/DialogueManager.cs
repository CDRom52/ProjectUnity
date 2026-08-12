using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image nextImage;
    public Sprite nextSprite;
    public Sprite endSprite;
    private Sprite currentSprite;

    [Header("Choice UI References")]
    public Transform choiceButtonContainer;
    public GameObject choiceButtonPrefab;

    [Header("Settings")]
    public float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;
    public bool isTyping = false;
    private string currentFullText = "";

    // Internal choices tracking
    private List<DialogueChoice> currentChoices;
    private NPCDialogue currentNPC;
    private List<GameObject> activeChoiceButtons = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
        currentSprite = nextSprite;
    }

    public void ShowDialogue(string npcName, DialogueNode node, NPCDialogue npc)
    {
        currentNPC = npc;
        currentChoices = node.choices;

        ClearChoices();

        ShowDialogue(npcName, node.npcText);
    }

    public void ShowDialogue(string npcName, string text)
    {
        nextImage.sprite = currentSprite;
        dialoguePanel.SetActive(true);
        nameText.text = npcName;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeSentence(text));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        currentFullText = sentence;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        DisplayChoices();
    }

    public void Skip()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentFullText;
        isTyping = false;

        DisplayChoices();
    }

    private void DisplayChoices()
    {
        ClearChoices();

        if (currentChoices == null || currentChoices.Count == 0) return;

        if (nextImage != null) nextImage.gameObject.SetActive(false);

        foreach (DialogueChoice choice in currentChoices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            activeChoiceButtons.Add(btnObj);

            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = choice.choiceText;

            Button button = btnObj.GetComponent<Button>();
            int targetIndex = choice.targetLineIndex;

            button.onClick.AddListener(() => OnSelectChoice(targetIndex));
        }
    }

    private void OnSelectChoice(int targetLineIndex)
    {
        ClearChoices();

        if (nextImage != null) nextImage.gameObject.SetActive(true);

        if (currentNPC != null)
        {
            currentNPC.JumpToLine(targetLineIndex);
        }
    }

    private void ClearChoices()
    {
        foreach (GameObject btn in activeChoiceButtons)
        {
            Destroy(btn);
        }
        activeChoiceButtons.Clear();
    }

    public void Close()
    {
        ClearChoices();
        dialoguePanel.SetActive(false);
        currentSprite = nextSprite;
        if (nextImage != null) nextImage.gameObject.SetActive(true);
    }

    public void LastLine()
    {
        currentSprite = endSprite;
    }
}