using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class PddlApiClient
{
    // Indirizzo del tuo server Docker (modifica se serve)
    private const string PlannerUrl = "http://localhost:5000/plan";

    /// <summary>
    /// Manda i PDDL al server e restituisce la lista di step del piano.
    /// </summary>
    /// <param name="domainPddl">Testo completo del dominio PDDL</param>
    /// <param name="problemPddl">Testo completo del problema PDDL</param>
    /// <param name="onSuccess">Callback con la lista di step (strings)</param>
    /// <param name="onError">Callback in caso di errore (messaggio)</param>
    public static IEnumerator RequestPlan(
        string domainPddl,
        string problemPddl,
        Action<List<string>> onSuccess,
        Action<string> onError
    ) {
        // 1) Costruisci l'oggetto anonimo e serializza in JSON
        var payload = new {
            domain_pddl  = domainPddl,
            problem_pddl = problemPddl
        };
        string json = JsonUtility.ToJson(payload);

        // 2) Prepara la richiesta HTTP
        using (var req = new UnityWebRequest(PlannerUrl, "POST")) {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 60;  // secondi, regola se serve

            // 3) Manda e aspetta
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success) {
                onError?.Invoke($"HTTP Error: {req.error}");
                yield break;
            }

            // 4) Deserializza la risposta JSON
            var resp = JsonUtility.FromJson<PlannerResponse>(req.downloadHandler.text);

            if (resp.returncode != 0 || !string.IsNullOrEmpty(resp.stderr)) {
                onError?.Invoke($"Planner Error:\n{resp.stderr}");
                yield break;
            }

            // 5) Estrai solo le righe di piano
            var lines = resp.stdout.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            var steps = new List<string>();
            foreach (var line in lines) {
                if (line.Contains(": (")) {
                    // facciamo un trim per sicurezza
                    steps.Add(line.Trim());
                }
            }

            onSuccess?.Invoke(steps);
        }
    }

    [Serializable]
    private class PlannerResponse
    {
        public string stdout;
        public string stderr;
        public int    returncode;
    }
}
