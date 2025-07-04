using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject cardPrefab;
    public Transform gridParent;
    public TextMeshProUGUI scoreText;
    public Sprite[] cardFaceSprites;
    public GameObject winPanel;
    public AudioSource audioSource;
    public AudioClip flipClip;
    public AudioClip matchClip;
    public AudioClip noMatchClip;


    private Card firstFlipped;
    private Card secondFlipped;
    private int score = 0;
    private List<Card> cards = new List<Card>();
    private Queue<Card> cardPool = new Queue<Card>();
    private bool isInputLocked = false;

    public bool IsInputLocked => isInputLocked;

    void Awake() { Instance = this; }

    void Start()
    {
        GenerateGrid(4, 4);
        score = 0;
        UpdateScore();
    }

    private Card GetCardFromPool()
    {
        if (cardPool.Count > 0)
        {
            Card card = cardPool.Dequeue();
            card.gameObject.SetActive(true);
            // Always reset visibility when reusing a card!
            CanvasGroup group = card.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 1;
            return card;
        }
        else
        {
            GameObject cardGO = Instantiate(cardPrefab, gridParent);
            return cardGO.GetComponent<Card>();
        }
    }

    public void GenerateGrid(int rows, int cols)
    {
        // Pool (deactivate) all existing cards
        foreach (Transform child in gridParent)
        {
            child.gameObject.SetActive(false);
            Card card = child.GetComponent<Card>();
            if (card != null)
                cardPool.Enqueue(card);
        }
        cards.Clear();

        // Prepare pairs (8 pairs for 4x4 grid)
        List<int> ids = new List<int>();
        for (int i = 0; i < 8; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }
        ids.Shuffle();

        for (int i = 0; i < rows * cols; i++)
        {
            Card card = GetCardFromPool();
            card.transform.SetParent(gridParent, false);
            card.cardId = ids[i];
            // Safety: only assign sprite if exists
            if (ids[i] < cardFaceSprites.Length)
                card.frontImage.sprite = cardFaceSprites[ids[i]];
            card.isMatched = false;
            card.SetFaceDown(true);
            cards.Add(card);
        }
    }

    public void OnCardFlipped(Card card)
    {
        if (isInputLocked) return;

        if (firstFlipped == null)
        {
            firstFlipped = card;
        }
        else if (secondFlipped == null && card != firstFlipped)
        {
            secondFlipped = card;
            isInputLocked = true;
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator<WaitForSeconds> CheckMatch()
    {
        yield return new WaitForSeconds(0.8f);
        if (firstFlipped.cardId == secondFlipped.cardId)
        {
            firstFlipped.SetMatched();
            secondFlipped.SetMatched();
            score += 10;
            PlayMatchSound();
        }
        else
        {
            PlayNoMatchSound();
            firstFlipped.SetFaceDown();
            secondFlipped.SetFaceDown();
        }
        UpdateScore();
        firstFlipped = null;
        secondFlipped = null;
        isInputLocked = false;

        if (cards.TrueForAll(card => card.isMatched))
        {
            winPanel.SetActive(true);
            isInputLocked = true;
        }
    }

    void UpdateScore() { scoreText.text = $"Score: {score}"; }

    public void RestartGame()
    {
        winPanel.SetActive(false);
        isInputLocked = false;
        GenerateGrid(4, 4); // or your current grid size
        score = 0;
        UpdateScore();
    }

    public void PlayFlipSound()
    {
        if (flipClip != null) audioSource.PlayOneShot(flipClip);
    }
    public void PlayMatchSound()
    {
        if (matchClip != null) audioSource.PlayOneShot(matchClip);
    }
    public void PlayNoMatchSound()
    {
        if (noMatchClip != null) audioSource.PlayOneShot(noMatchClip);
    }
}


public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = Random.Range(i, n);
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}
