namespace HackerTerminal;

public class QuestFile
{
    public string Path { get; init; } = "";
    public string Content { get; set; } = "";
    public bool Encrypted { get; init; }
    public int CipherKey { get; init; }
    public int VisibleFromLevel { get; init; } = 1;

    public bool Decrypted { get; set; }

    public string Name => Path.Substring(Path.LastIndexOf('/') + 1);

    public string Dir
    {
        get
        {
            int idx = Path.LastIndexOf('/');
            if (idx <= 0) return "/";
            return Path.Substring(0, idx);
        }
    }
}
