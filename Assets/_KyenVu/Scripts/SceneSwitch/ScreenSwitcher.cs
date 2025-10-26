using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenSwitcher : MonoBehaviour, I_Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Interact()
    {
        SceneManager.LoadScene("2D scene");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
