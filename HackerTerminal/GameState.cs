namespace HackerTerminal;

public class GameState
{
    public string CurrentDir { get; set; } = "/";
    public int Level { get; set; } = 1;
    public int Score { get; set; } = 0;

    public bool ServerConnected { get; set; } = false;
    public bool VaultConnected { get; set; } = false;
    public bool ServerNodeDiscovered { get; set; } = false;
    public bool VaultNodeDiscovered { get; set; } = false;

    public int HackAttemptsLeft { get; set; } = 3;

    public bool GameOver { get; set; } = false;
    public bool Victory { get; set; } = false;
}
