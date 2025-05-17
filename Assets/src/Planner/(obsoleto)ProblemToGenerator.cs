using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
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
                    break;
                }
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

    public static Dictionary<string, List<string>> pddlInit(string pddlProblemFilePath)
    {
        Dictionary<string, List<string>> atoms = new Dictionary<string, List<string>>();
        bool initStarted = false;
        string[] lines = File.ReadAllLines(pddlProblemFilePath);
        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.StartsWith("(:init"))
            {
                initStarted = true;
                line = line.Replace("(:init", "").Trim();
            }

            if (initStarted)
            {
                if (line.StartsWith("(:") || line.StartsWith(")"))
                {
                    break;
                }
                string[] parts = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    string atom = parts[0].Trim();
                    if (!atoms.ContainsKey(atom))
                        atoms[atom] = new List<string>();

                    atoms[atom].AddRange(parts);
                    atoms[atom].Remove(atom);
                }
            }
        }
        return atoms;
    }


    public static string printDictionary(string problemPath)
    {
        Dictionary<string, List<string>> objects = pddlObjects(problemPath);
        string print = "";
        foreach (string rawLine in objects.Keys)
        {
            print += "Key: " + rawLine + " Value: ";
            foreach (string value in objects[rawLine])
            {
                print += value + " ";
            }
            print += "\n";
        }

        return print;
    }

    public static string printDictionaryInit(string problemPath)
    {
        Dictionary<string, List<string>> objects = pddlInit(problemPath);
        string print = "";
        foreach (string rawLine in objects.Keys)
        {
            print += "Key: " + rawLine + " Value: ";
            foreach (string value in objects[rawLine])
            {
                print += value + " ";
            }
            print += "\n";
        }

        return print;
    }
}
