using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class PddlApiClient
{
    private const string PlannerUrl = "http://localhost:5000/plan";

    [Serializable]
    private class PlannerRequest
    {
        public string domain_pddl;
        public string problem_pddl;
    }

    [Serializable]
    private class PlannerResponse
    {
        public string stdout;
        public string stderr;
        public int returncode;
    }

    /// <summary>
    /// Manda i PDDL al server e restituisce la lista di step del piano.
    /// </summary>
    public static IEnumerator RequestPlan(
        string domainPddl,
        string problemPddl,
        Action<List<string>> onSuccess,
        Action<string> onError
    ) {
        // 1) Usa una classe serializzabile anziché un anonymous type
        var payload = new PlannerRequest {
            domain_pddl  = domainPddl,
            problem_pddl = problemPddl
        };
        string json = JsonUtility.ToJson(payload);
        Debug.Log("[DEBUG] Sending JSON: " + json);

        using (var req = new UnityWebRequest(PlannerUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");
            req.timeout = 60;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"HTTP Error: {req.error}");
                yield break;
            }

            Debug.Log("[DEBUG] Received response: " + req.downloadHandler.text);
            var resp = JsonUtility.FromJson<PlannerResponse>(req.downloadHandler.text);

            if (resp.returncode != 0)
            {
                if (!string.IsNullOrEmpty(resp.stdout) && resp.stdout.Contains("No solution"))
                {
                    onSuccess?.Invoke(new List<string>()); // Lista vuota: nessun piano
                }
                    else
                    {
                        onError?.Invoke($"Planner Error:\n{resp.stderr}");
                    }
                yield break;
            }


            // 5) Estrai righe del piano
            var steps = new List<string>();
            foreach (var line in resp.stdout
                     .Split(new[]{'\n'}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Contains(": ("))
                    steps.Add(line.Trim());
            }
            onSuccess?.Invoke(steps);
        }
    }
}
