using HackerTerminal;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.BackgroundColor = ConsoleColor.Black;
Console.ForegroundColor = ConsoleColor.Green;
Console.Clear();

var game = new Game();
game.Run();

Console.WriteLine();
Console.WriteLine("Нажмите любую клавишу, чтобы выйти...");
Console.ReadKey();