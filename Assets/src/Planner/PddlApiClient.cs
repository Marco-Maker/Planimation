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

    public static IEnumerator RequestPlan(
        string domainPddl,
        string problemPddl,
        Action<List<string>> onSuccess,
        Action<string> onError
    ) {
        var payload = new PlannerRequest {
            domain_pddl = domainPddl,
            problem_pddl = problemPddl
        };

        string json = JsonUtility.ToJson(payload);
        Debug.Log("[DEBUG] Sending JSON to planner:\n" + json);

        using (var req = new UnityWebRequest(PlannerUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");
            req.timeout = 180;

            yield return req.SendWebRequest();

            Debug.Log($"[DEBUG] Request result: {req.result}, Code: {req.responseCode}");

            string responseText = req.downloadHandler.text;

            // ❗ Leggi sempre il JSON, anche in caso di errore HTTP
            if (req.result != UnityWebRequest.Result.Success)
            {
                if (!string.IsNullOrEmpty(responseText))
                {
                    try
                    {
                        var errorResp = JsonUtility.FromJson<PlannerResponse>(responseText);
                        onError?.Invoke($"Planner Error:\n{errorResp.stderr}");
                    }
                    catch (Exception parseErr)
                    {
                        onError?.Invoke($"HTTP Error: {req.error}\n(Parse failed: {parseErr.Message})");
                    }
                }
                else
                {
                    onError?.Invoke($"HTTP Error: {req.error}");
                }
                yield break;
            }

            Debug.Log("[DEBUG] Received response:\n" + responseText);

            PlannerResponse resp;
            try
            {
                resp = JsonUtility.FromJson<PlannerResponse>(responseText);
            }
            catch (Exception e)
            {
                onError?.Invoke("Errore nel parsing della risposta JSON:\n" + e.Message);
                yield break;
            }

            // 🔴 Errore da OPTIC (ma senza crash HTTP)
            if (resp.returncode != 0)
            {
                if (!string.IsNullOrEmpty(resp.stdout) && resp.stdout.Contains("No solution"))
                {
                    onSuccess?.Invoke(new List<string>()); // valido ma senza piano
                }
                else
                {
                    onError?.Invoke($"Planner Error:\n{resp.stderr}");
                }
                yield break;
            }

            // ✅ Parsing del piano
            var steps = new List<string>();
            var lines = resp.stdout.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                Debug.Log("[DEBUG] LINEA:\n" + trimmed);
                if (trimmed.Contains(": ("))
                {
                    steps.Add(trimmed);
                    Debug.Log("[DEBUG] ➕ Step aggiunto: " + trimmed);
                }
            }

            if (steps.Count == 0)
            {
                Debug.LogWarning("[DEBUG] Nessun passo riconosciuto nel piano.");
                onError?.Invoke("OPTIC ha restituito output, ma nessun passo è stato riconosciuto.");
                yield break;
            }

            Debug.Log("[DEBUG] Steps finali trovati:\n" + string.Join("\n", steps));
            onSuccess?.Invoke(steps);
        }
    }
}
