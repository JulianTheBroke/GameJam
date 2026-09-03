using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string sceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        // Load the game scene
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        // Quit the application
        Application.Quit();
    }
}
