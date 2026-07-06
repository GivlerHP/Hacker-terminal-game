using System.Text.Json;

namespace HackerTerminal;

public class QuestData
{
    public List<QuestFile> Files { get; } = new();
    public Dictionary<string, int> DirVisibleFromLevel { get; } = new();
    public Dictionary<string, string> PlainTextByPath { get; } = new();
}

public static class QuestLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static QuestData Load(string questsDir)
    {
        if (!Directory.Exists(questsDir))
        {
            throw new DirectoryNotFoundException($"Папка квестов не найдена: {questsDir}");
        }

        var jsonFiles = Directory.GetFiles(questsDir, "*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        if (jsonFiles.Count == 0)
        {
            throw new FileNotFoundException($"В папке {questsDir} нет .json файлов квеста.");
        }

        var data = new QuestData();
        foreach (var path in jsonFiles)
        {
            LoadOneFile(path, data);
        }

        return data;
    }

    private static void LoadOneFile(string path, QuestData data)
    {
        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Не удалось прочитать {path}: {ex.Message}", ex);
        }

        QuestJson? dto;
        try
        {
            dto = JsonSerializer.Deserialize<QuestJson>(json, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Некорректный JSON в {path}: {ex.Message}", ex);
        }

        if (dto == null)
        {
            throw new InvalidDataException($"Пустой квест-файл: {path}");
        }

        foreach (var dir in dto.Directories ?? new())
        {
            if (string.IsNullOrWhiteSpace(dir.Path))
            {
                throw new InvalidDataException($"В {path} есть директория без 'path'.");
            }

            data.DirVisibleFromLevel[NormalizePath(dir.Path)] = dir.VisibleFromLevel;
        }

        foreach (var file in dto.Files ?? new())
        {
            if (string.IsNullOrWhiteSpace(file.Path))
            {
                throw new InvalidDataException($"В {path} есть файл без 'path'.");
            }

            string normalizedPath = NormalizePath(file.Path);

            if (data.Files.Any(f => f.Path == normalizedPath))
            {
                throw new InvalidDataException($"Дублирующийся путь файла '{normalizedPath}' ({path}).");
            }

            string content;

            if (file.Encrypted)
            {
                if (file.CipherKey is null)
                {
                    throw new InvalidDataException(
                        $"Файл {normalizedPath} encrypted, но не задан cipherKey ({path}).");
                }

                if (string.IsNullOrEmpty(file.PlainText))
                {
                    throw new InvalidDataException(
                        $"Файл {normalizedPath} encrypted, но не задан plainText ({path}).");
                }

                content = Cipher.Encode(file.PlainText, file.CipherKey.Value);
                data.PlainTextByPath[normalizedPath] = file.PlainText;
            }
            else
            {
                content = file.Content != null ? string.Join("\n", file.Content) : "";
            }

            data.Files.Add(new QuestFile
            {
                Path = normalizedPath,
                VisibleFromLevel = file.VisibleFromLevel,
                Encrypted = file.Encrypted,
                CipherKey = file.CipherKey ?? 0,
                Content = content,
            });
        }
    }

    private static string NormalizePath(string path)
    {
        path = path.Trim();
        if (!path.StartsWith('/')) path = "/" + path;
        return path.Length > 1 ? path.TrimEnd('/') : path;
    }
}