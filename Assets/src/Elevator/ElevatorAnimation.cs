using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorAnimation : MonoBehaviour
{

    public void StartOpenAnimation()
    {
        GetComponent<Animator>().SetBool("Open", true);
    }

    public void StartCloseAnimation()
    {
        GetComponent<Animator>().SetBool("Open", false);
    }
}
