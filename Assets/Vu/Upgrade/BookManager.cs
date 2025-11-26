using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    public List<PageBase> pages;
    private int currentIndex = 0;
    private bool switching = false;

    void Start()
    {
        // Show only page 0
        for (int i = 0; i < pages.Count; i++)
            pages[i].gameObject.SetActive(i == currentIndex);
    }

    public void SwitchToPage(int index)
    {
        if (switching || index == currentIndex)
            return;

        StartCoroutine(SwitchRoutine(index));
    }

    private IEnumerator SwitchRoutine(int newIndex)
    {
        switching = true;

        // Reverse old page
        yield return pages[currentIndex].PlayHideAnimation();

        // Switch
        currentIndex = newIndex;

        // Play show animation
        yield return pages[currentIndex].PlayShowAnimation();

        switching = false;
    }
}
