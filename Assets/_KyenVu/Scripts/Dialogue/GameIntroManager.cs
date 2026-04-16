using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class GameIntroManager : MonoBehaviour
{
    [Header("Part 1: Gustaff's Dialogue")]
    public string gustaffName = "Mr. Gustaff";
    public Sprite gustaffPortrait;
    [Tooltip("Put the first 3 sentences where Gustaff is talking here.")]
    public DialogueNode[] gustaffNodes;

    [Header("Part 2: Maelle's Dialogue")]
    public string maelleName = "Maelle";
    public Sprite maellePortrait;
    [Tooltip("Put Maelle's final sentence here!")]
    public DialogueNode[] maelleNodes;

    [Header("Testing")]
    [Tooltip("Check this box to force the intro to play every time you press Play in Unity")]
    public bool forcePlayIntro = false;

    private void Start()
    {
        int hasPlayed = PlayerPrefs.GetInt("HasPlayedIntro", 0);

        if (hasPlayed == 0 || forcePlayIntro)
        {
            StartCoroutine(PlayIntroSequence());
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        // Wait half a second for the game to start
        yield return new WaitForSeconds(0.5f);

        // 1. Create an event that fires the exact moment Gustaff finishes talking
        UnityEvent onGustaffDone = new UnityEvent();
        onGustaffDone.AddListener(PlayMaelleResponse);

        // 2. Start Gustaff's portion of the intro
        DialogueManager.Instance.StartDialogue(gustaffName, gustaffPortrait, gustaffNodes, onGustaffDone);
    }

    private void PlayMaelleResponse()
    {
        // 3. Create an event for when the ENTIRE intro is over
        UnityEvent onIntroEnd = new UnityEvent();
        onIntroEnd.AddListener(() =>
        {
            Debug.Log("Intro finished! Let the fishing begin.");

            // Save the fact that we watched the intro
            PlayerPrefs.SetInt("HasPlayedIntro", 1);
            PlayerPrefs.Save();
        });

        // 4. Start Maelle's response immediately!
        DialogueManager.Instance.StartDialogue(maelleName, maellePortrait, maelleNodes, onIntroEnd);
    }

    [ContextMenu("Reset Intro Save")]
    public void ResetIntro()
    {
        PlayerPrefs.SetInt("HasPlayedIntro", 0);
        PlayerPrefs.Save();
        Debug.Log("🧹 Intro save wiped! The intro will play again next time you hit Play.");
    }
}