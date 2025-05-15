using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogisticPlanExecutor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoveForward(transform, 1f);
    }

    // Funzione per muovere in avanti un oggetto avanti 
    public void MoveForward(Transform obj, float distance)
    {
        // Calcola la nuova posizione
        Vector3 newPosition = obj.position + obj.forward * distance;
        // Muovi l'oggetto alla nuova posizione
        obj.position = newPosition;
    }
}
