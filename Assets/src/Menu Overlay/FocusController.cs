using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class FocusController : MonoBehaviour {
    public TMP_Text txtFocus;
    private List<GameObject> elements;     // Lista di nomi o descrizioni
    private int idx = 0;

    void Start() {
        UpdateFocus();
    }

    public void GoToPreviousElement()
    {
        idx = (idx - 1 + elements.Count) % elements.Count; // Assicura che l'indice sia positivo
        if (idx < 0) idx = elements.Count - 1; // Se l'indice è negativo, torna all'ultimo elemento 
        UpdateFocus();
    }

    public void GoToNextElement()
    {
        idx = (idx + 1) % elements.Count;
        UpdateFocus();
    }

    void UpdateFocus()
    {
        txtFocus.text = elements[idx].name;
    // TODO: Aggiungere un effetto visivo per il focus
    }
}
