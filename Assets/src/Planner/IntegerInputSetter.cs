using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntegerInputSetter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI number;
    
    public void Add()
    {
        int count = int.Parse(number.text);
        count++;
        number.text = count.ToString();
    }

    public void Subtract()
    {
        int count = int.Parse(number.text);
        if (count > 0)
        {
            count--;
            number.text = count.ToString();
        }
    }

    public int GetValue()
    {
        return int.Parse(number.text);
    }
}
