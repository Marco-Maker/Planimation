using UnityEngine;

public class ExitApp : MonoBehaviour
{
    // Questo metodo verrà chiamato dal bottone per chiudere l'app.
    public void QuitApplication()
    {
        // Se sei in editor, fermalo qui (utile per test in Play Mode).
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
