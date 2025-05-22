using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Person"))
        {
            Debug.Log("Open");
            GetComponentInParent<Animator>().SetBool("Open", !GetComponentInParent<Animator>().GetBool("Open"));
        }
    }
}
