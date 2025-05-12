using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProblemToGenerator : MonoBehaviour
{
    public static Dictionary<string, List<string>> pddlObjects(string pddlProblemFilePath)
    {
        bool objectStarted = false;
        Dictionary<string, List<string>> objects = new Dictionary<string, List<string>>();
        string[] lines = File.ReadAllLines(pddlProblemFilePath);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.StartsWith("(:objects"))
            {
                objectStarted = true;
                line = line.Replace("(:objects", "").Trim();
            }

            if (objectStarted)
            {
                if (line.StartsWith("(:") || line.StartsWith(")"))
                {
                    // fine della sezione objects
                    break;
                }
                Debug.Log(line);
                // Accumula la linea nel caso ci siano più righe
                string[] parts = line.Split('-');
                if (parts.Length == 2)
                {
                    string[] items = parts[0].Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    string type = parts[1].Trim();

                    if (!objects.ContainsKey(type))
                        objects[type] = new List<string>();

                    objects[type].AddRange(items);
                }
            }
        }

        return objects;
    }

    public static string printDictionary(string problemPath)
    {
        Dictionary<string, List<string>> objects = pddlObjects(problemPath);
        string print = "";
        foreach (string rawLine in objects.Keys)
        {
            print += "Key: " + rawLine + " Value: " + objects[rawLine].ToString() + "\n";
            
        }
        return print;
    }
}
