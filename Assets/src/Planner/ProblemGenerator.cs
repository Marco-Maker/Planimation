/******************************************************
 * PDDLProblemGenerator.cs
 * Crea (e opzionalmente salva) un file PDDL a partire
 * dai dati correnti contenuti in PlanInfo.
 *
 * Aggiungi lo script a un GameObject e collega il
 * metodo GenerateAndSave() a un bottone �GENERA�.
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
    private string problemName = "p";

    [Tooltip("Nome del dominio da usare nella clausola (:domain ...)")]
    private string domainName;

    [Header("Output")]
    [Tooltip("Se true scrive il file sul disco, altrimenti lo ritorna soltanto")]
    public bool saveToFile = true;

    [Tooltip("Percorso relativo al progetto dove salvare *.pddl")]
    private string outputPath = "Assets/Generated/problem.pddl";
    private string domainPath;

    /* ---------------------------------------------------------- */

    /// <summary>
    /// Chiamare questo metodo (es. dal bottone UI) per generare
    /// e, se richiesto, salvare il problema PDDL aggiornato.
    /// </summary>
    public string GenerateAndSave()
    {
        var planInfo = PlanInfo.GetInstance();

        var objects = planInfo.GetObjects();             // List<ObjectToAdd>
        var predicates = planInfo.GetPredicates();       // List<PredicateToAdd>
        var goals = planInfo.GetGoals();                 // List<GoalToAdd>
        var functions = planInfo.GetFunctions();         // ✅ functions
        var domainType = planInfo.GetDomainType();       // ✅ domain type

        string pddl = BuildPddl(problemName, domainName, objects, predicates, goals, functions, domainType);

        if (saveToFile)
        {
            string folder = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(outputPath, pddl, Encoding.UTF8);
        }

        return pddl;
    }


    /* ----------------- LOGICA DI COSTRUZIONE ------------------ */

    private string BuildPddl(
        string problem, string domain,
        IList<ObjectToAdd> objs,
        IList<PredicateToAdd> preds,
        IList<GoalToAdd> goals,
        IList<FunctionToAdd> funcs,   // ✅ nuovo parametro
        float domainType             // ✅ nuovo parametro
    )
    {
        var sb = new StringBuilder();

        sb.AppendLine($"(define (problem {problem})");
        sb.AppendLine($"\t(:domain {domain})");

        /* ----- SEZIONE (:objects ...) ----- */
        sb.AppendLine("\t(:objects");
        // Raggruppa per type cos� da stampare: a b c - tipo
        foreach (var grp in objs.GroupBy(o => o.type))
        {
            string names = string.Join(" ", grp.Select(o => o.name));
            sb.AppendLine($"\t\t{names} - {grp.Key.Split("-")[0]}");
        }
        sb.AppendLine("\t)");

        /* ----- SEZIONE (:init ...) ----- */

        sb.AppendLine("\t(:init");

        // Aggiungi i predicati
        var uniquePreds = preds
            .GroupBy(p => new { p.name, Key = string.Join(",", p.values) })
            .Select(g => g.First())
            .ToList();

        foreach (var p in uniquePreds)
            sb.AppendLine("\t\t" + FormatAtomic(p.name, p.values));

        // Aggiungi le funzioni solo se il dominio è numerico
        if (!Mathf.Approximately(domainType % 1f, 0.0f) && funcs != null)
        {
            var uniqueFuncs = funcs
                .GroupBy(f => f.name + "_" + string.Join(",", f.values))
                .Select(g => g.First());

            foreach (var f in uniqueFuncs)
            {
                sb.AppendLine("\t\t" + FormatFunctionAssignment(f));
            }
        }

        sb.AppendLine("\t)");

        // Sezione goal con deduplicazione dei goal
        var uniqueGoals = goals
            .GroupBy(g => new { g.name, Key = string.Join(",", g.values) })
            .Select(g => g.First())
            .ToList();

        sb.AppendLine("\t(:goal");
        sb.AppendLine("\t\t(and");
        foreach (var g in uniqueGoals)
            sb.AppendLine("\t\t\t" + FormatAtomic(g.name, g.values));
        sb.AppendLine("\t\t)");
        sb.AppendLine("\t)");

        sb.AppendLine(")");
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

        // Predicati �normali�
        return $"({name} {string.Join(" ", values)})";
    }

    /// <summary>
    /// Restituisce un'assegnazione numerica in formato PDDL: (= (nome args) valore)
    /// Es: (= (weight personA) 60)
    /// </summary>
    private string FormatFunctionAssignment(FunctionToAdd func)
    {
        if (func.values == null || func.values.Count < 1)
            return ""; // Ignora assegnazioni mal formate

        string value = func.values.Last(); // l'ultimo è il valore numerico
        var args = func.values.Take(func.values.Count - 1); // tutti gli altri sono argomenti
        string call = $"({func.name} {string.Join(" ", args)})";

        return $"(= {call} {value})";
    }


    public void SetDomainName(string name)
    {
        domainName = name.Replace(".pddl", "");
        //Debug.Log("Domain: " + domainName);
    }
    public string GetDomainName()
    {
        return domainName;
    }

    public string GetDomainPath()
    {
       return domainPath;
    }
    public void SetDomainPath(string path)
    {
        domainPath = path;
        //Debug.Log("Domain Path: " + domainPath);
    }
}
