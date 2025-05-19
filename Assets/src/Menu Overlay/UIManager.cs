using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject planPanel; // assegnalo via Inspector
    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void ShowPlan()
    {
        planPanel.SetActive(true);
    }

}


