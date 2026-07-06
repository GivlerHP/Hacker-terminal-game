namespace HackerTerminal;

public class QuestJson
{
    public List<QuestDirDto>? Directories { get; set; }
    public List<QuestFileDto>? Files { get; set; }
    public QuestPuzzleDto? Puzzles { get; set; }
}

public class QuestDirDto
{
    public string Path { get; set; } = "";
    public int VisibleFromLevel { get; set; } = 1;
}

public class QuestFileDto
{
    public string Path { get; set; } = "";
    public int VisibleFromLevel { get; set; } = 1;
    public List<string>? Content { get; set; }
    public bool Encrypted { get; set; } = false;
    public int? CipherKey { get; set; }
    public string? PlainText { get; set; }
}

public class QuestPuzzleDto
{
    public string? BadgeFilePath { get; set; }
    public string? UnlockCode { get; set; }
    public string? HackTarget { get; set; }
    public string? HackPassword { get; set; }
    public string? VictoryFilePath { get; set; }
    public string? StudioNodeAddress { get; set; }
    public string? ArchiveNodeAddress { get; set; }
}