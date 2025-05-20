/******************************************************
 * PDDLProblemGenerator.cs
 * Crea (e opzionalmente salva) un file PDDL a partire
 * dai dati correnti contenuti in PlanInfo.
 *
 * Aggiungi lo script a un GameObject e collega il
 * metodo GenerateAndSave() a un bottone “GENERA”.
 *****************************************************/

using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using UnityEngine;

public class ProblemGenerator : MonoBehaviour
{
    /* ---------- Parametri configurabili da Inspector --------- */
    [Header("Metadata del problema")]
    [Tooltip("Nome del problema dentro al file PDDL")]
    private string problemName = "problem";

    [Tooltip("Nome del dominio da usare nella clausola (:domain ...)")]
    private string domainName;

    [Header("Output")]
    [Tooltip("Se true scrive il file sul disco, altrimenti lo ritorna soltanto")]
    public bool saveToFile = true;

    [Tooltip("Percorso relativo al progetto dove salvare *.pddl")]
    private string outputPath = "Assets/Generated/problem.pddl";

    /* ---------------------------------------------------------- */

    public void SetDomain()
    {
        domainName = PlanInfo.GetInstance().GetDomainName();
    }

    /// <summary>
    /// Chiamare questo metodo (es. dal bottone UI) per generare
    /// e, se richiesto, salvare il problema PDDL aggiornato.
    /// </summary>
    public string GenerateAndSave()
    {
        // 1) Recupera le liste correnti
        var objects = PlanInfo.GetInstance().GetObjects();     // List<ObjectToAdd>
        var predicates = PlanInfo.GetInstance().GetPredicates();  // List<PredicateToAdd>
        var goals = PlanInfo.GetInstance().GetGoals();       // List<GoalToAdd>

        // 2) Costruisce la stringa PDDL
        string pddl = BuildPddl(problemName, domainName, objects, predicates, goals);

        // 3) Salva su file se richiesto
        if (saveToFile)
        {
            // Garantiamo che la directory esista
            string folder = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(outputPath, pddl, Encoding.UTF8);
            Debug.Log($"[PDDLGenerator] File creato: {outputPath}");
        }

        return pddl; // Utile se vuoi mostrarlo in un’area di testo o copiarlo altrove
    }

    /* ----------------- LOGICA DI COSTRUZIONE ------------------ */

    private string BuildPddl(
        string problem, string domain,
        IList<ObjectToAdd> objs,
        IList<PredicateToAdd> preds,
        IList<GoalToAdd> goals)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"(define (problem {problem})");
        sb.AppendLine($"\t(:domain {domain})");

        /* ----- SEZIONE (:objects ...) ----- */
        sb.AppendLine("\t(:objects");
        // Raggruppa per type così da stampare: a b c - tipo
        foreach (var grp in objs.GroupBy(o => o.type))
        {
            string names = string.Join(" ", grp.Select(o => o.name));
            sb.AppendLine($"\t\t{names} - {grp.Key.Split("-")[0]}");
        }
        sb.AppendLine("\t)");

        /* ----- SEZIONE (:init ...) ----- */
        sb.AppendLine("\t(:init");
        foreach (var p in preds)
            sb.AppendLine("\t\t" + FormatAtomic(p.name, p.values));
        sb.AppendLine("\t)");

        /* ----- SEZIONE (:goal (and ...)) ----- */
        sb.AppendLine("\t(:goal");
        sb.AppendLine("\t\t(and");
        foreach (var g in goals)
            sb.AppendLine("\t\t\t" + FormatAtomic(g.name, g.values));
        sb.AppendLine("\t\t)");
        sb.AppendLine("\t)");

        sb.AppendLine(")"); // chiude define
        return sb.ToString();
    }

    /// <summary>
    /// Restituisce la forma PDDL atomica di nome + valori:
    ///   (pred val1 val2 ...)
    /// Gestisce anche gli assegnamenti numerici di tipo (= ...).
    /// </summary>
    private string FormatAtomic(string name, IList<string> values)
    {
        // Caso particolare: assegnamenti numerici es. (= (floors) 3)
        if (name == "=" || name.Equals("assign"))
        {
            // valori[0] = "floors"  valori[1] = "3"   (supporto minimo generico)
            string functionCall = values.Count > 2
                ? $"({values[0]} {string.Join(" ", values.Skip(1).Take(values.Count - 2))})"
                : $"({values[0]})";
            string value = values.Last();
            return $"(= {functionCall} {value})";
        }

        // Predicati “normali”
        return $"({name} {string.Join(" ", values)})";
    }

    public void SetDomainName(string name)
    {
        domainName = name;
    }
    public string GetDomainName()
    {
        return domainName;
    }
}
