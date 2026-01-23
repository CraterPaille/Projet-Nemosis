using UnityEngine;
using System.IO;

public class ChartLoader : MonoBehaviour
{
    public string chartFileName = "chart_holyground_easy.json";
    public ChartData loadedChart;

    void Awake()
    {
        LoadChart();
    }

    public void LoadChart()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, chartFileName);
        if (!File.Exists(filePath))
        {
            Debug.LogError("Chart introuvable : " + filePath);
            return;
        }

        string jsonData = File.ReadAllText(filePath);
        loadedChart = JsonUtility.FromJson<ChartData>(jsonData);

        Debug.Log($"Chart chargée : {loadedChart.songName}, BPM: {loadedChart.bpm}, Notes: {loadedChart.notes.Count}");
    }
}
