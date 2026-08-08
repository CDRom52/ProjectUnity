using System.Collections;
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

    [Header("Settings")]
    public float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;
    public bool isTyping = false;
    private string currentFullText = "";

    void Awake()
    {
        Instance = this;

        dialoguePanel.SetActive(false);

        currentSprite = nextSprite;
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
    }

    public void Skip()
    {
        StopCoroutine(typingCoroutine);
        dialogueText.text = currentFullText;
        isTyping = false;
    }

    public void Close()
    {
        dialoguePanel.SetActive(false);
        currentSprite = nextSprite;
    }

    public void LastLine()
    {
        currentSprite = endSprite;
    }
}