using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PredicateInputSetter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private List<string> values;

    public void SetLabel(string value)
    {
        label.text = value;
    }
}
