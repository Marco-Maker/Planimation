using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;                 // <- aggiungi
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

            if (resp.returncode != 0)
            {
                if (!string.IsNullOrEmpty(resp.stdout) && resp.stdout.Contains("No solution"))
                {
                    onSuccess?.Invoke(new List<string>());
                }
                else
                {
                    onError?.Invoke($"Planner Error:\n{resp.stderr}");
                }
                yield break;
            }

            // Parsing del piano
            var steps = new List<string>();
            var lines = resp.stdout.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Contains(": ("))
                    steps.Add(trimmed);
            }

            if (steps.Count == 0)
            {
                onError?.Invoke("OPTIC ha restituito output, ma nessun passo è stato riconosciuto.");
                yield break;
            }

            // ─── Qui salviamo il piano su file ────────────────────────────────────
            try
            {
                // percorso relativo alla cartella di lavoro: "./PDDL/output_plan.txt"
                string folderPath = Application.dataPath + "/PDDL";
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, "output_plan.txt");
                File.WriteAllLines(filePath, steps);
                Debug.Log($"[DEBUG] Piano salvato in: {filePath}");
            }
            catch (Exception fileEx)
            {
                Debug.LogError("[DEBUG] Errore durante il salvataggio del piano:\n" + fileEx);
                // non blocchiamo il callback: proseguiamo comunque
            }
            // ─────────────────────────────────────────────────────────────────────

            onSuccess?.Invoke(steps);
        }
    }
}
