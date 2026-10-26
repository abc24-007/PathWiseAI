using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PathWiseAI;

public class HistoryStore
{
    public string FilePath { get; }

    public HistoryStore(string? filePath = null)
    {
        FilePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "PathWiseAI",
            "history.json");
    }

    public List<TopicResult> Load()
    {
        if (!File.Exists(FilePath))
            return new List<TopicResult>();

        string json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<List<TopicResult>>(json)
            ?? throw new JsonException("History file contains no data.");
    }

    public void Save(List<TopicResult> history)
    {
        string fullPath = Path.GetFullPath(FilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        string json = JsonSerializer.Serialize(
            history,
            new JsonSerializerOptions { WriteIndented = true });

        // Write a temporary file before replacing the saved history.
        string temporaryPath = fullPath + "." +
            Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}