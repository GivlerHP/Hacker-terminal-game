namespace HackerTerminal;

public class GameState
{
    public string CurrentDir { get; set; } = "/";
    public int Level { get; set; } = 1;
    public int Score { get; set; } = 0;

    public bool StudioConnected { get; set; } = false;
    public bool ArchiveConnected { get; set; } = false;
    public bool StudioNodeDiscovered { get; set; } = false;
    public bool ArchiveNodeDiscovered { get; set; } = false;

    public int HackAttemptsLeft { get; set; } = 3;

    public List<string> DecryptedFiles { get; set; } = new();

    public bool GameOver { get; set; } = false;
    public bool Victory { get; set; } = false;
}