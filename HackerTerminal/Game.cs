using System.Text.Json;

namespace HackerTerminal;

public class Game
{
    private GameState _state = new();
    private readonly List<QuestFile> _files;
    private readonly Dictionary<string, int> _dirVisibleFromLevel = new()
    {
        ["/home"] = 1,
        ["/gate"] = 1,
        ["/server"] = 2,
        ["/vault"] = 3,
    };

    private const string ServerAddress = "192.168.4.7";
    private const string VaultAddress = "10.0.0.7";

    private const string AccessPlainText = "UNLOCK CODE: RAVEN";
    private const string TruthPlainText = "THE TRUTH IS FREE NOW";

    private const string SavePath = "save.json";

    public Game()
    {
        _files = BuildQuest();
    }

    private static List<QuestFile> BuildQuest()
    {
        return new List<QuestFile>
        {
            new()
            {
                Path = "/readme.txt",
                VisibleFromLevel = 1,
                Content =
                    "БРИФИНГ.\n" +
                    "Тебя наняли, чтобы найти утечку в закрытой сети компании.\n" +
                    "Начни с домашней папки — там могут быть черновики бывшего сотрудника.\n" +
                    "Команда help покажет список доступных операций."
            },
            new()
            {
                Path = "/home/notes.txt",
                VisibleFromLevel = 1,
                Content =
                    "Заметки для себя, не забыть.\n" +
                    "Старый шифр всё ещё используют на воротах.\n" +
                    "Сдвиг, который я всегда забываю: 4."
            },
            new()
            {
                Path = "/gate/access.enc",
                VisibleFromLevel = 1,
                Encrypted = true,
                CipherKey = 4,
                Content = Cipher.Encode(AccessPlainText, 4)
            },
            new()
            {
                Path = "/server/log.txt",
                VisibleFromLevel = 2,
                Content =
                    "[СИСТЕМНЫЙ ЖУРНАЛ]\n" +
                    "Обнаружены подозрительные попытки входа под учёткой 'admin'.\n" +
                    "Проверь файлы проекта — возможно, пароль лежит прямо в них."
            },
            new()
            {
                Path = "/server/project.txt",
                VisibleFromLevel = 2,
                Content =
                    "TODO: сменить пароль администратора.\n" +
                    "Текущий (временный, забыл убрать): blackout"
            },
            new()
            {
                Path = "/vault/keycard.txt",
                VisibleFromLevel = 3,
                Content = "Фрагмент ключа шифрования, найденный на карте доступа: 7"
            },
            new()
            {
                Path = "/vault/truth.enc",
                VisibleFromLevel = 3,
                Encrypted = true,
                CipherKey = 7,
                Content = Cipher.Encode(TruthPlainText, 7)
            },
        };
    }
    
    public void Run()
    {
        PrintBanner();
        StartMenu();
        BootSequence();

        Type("Введите 'help', чтобы увидеть список команд.\n");

        while (!_state.GameOver)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write($"root@{PromptHost()}:{_state.CurrentDir}$ ");
            Console.ForegroundColor = ConsoleColor.Green;

            string raw = Console.ReadLine() ?? "";
            raw = raw.Trim();
            if (raw.Length == 0) continue;

            var parts = raw.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string cmd = parts[0].ToLowerInvariant();
            string arg = parts.Length > 1 ? parts[1].Trim() : "";

            Dispatch(cmd, arg);
        }

        EndScreen();
    }

    private void StartMenu()
    {
        if (!File.Exists(SavePath)) return;

        Console.WriteLine("Обнаружен файл предыдущего взлома: " + SavePath);
        Console.WriteLine("1 - Начать заново");
        Console.WriteLine("2 - Продолжить взлом (загрузить сохранение)");

        while (true)
        {
            Console.Write("> ");
            string? raw = Console.ReadLine();
            if (raw == null)
            {
                Type("Начинаем новый взлом.\n");
                return;
            }
            raw = raw.Trim();

            if (raw == "1")
            {
                Type("Начинаем новый взлом.\n");
                return;
            }

            if (raw == "2")
            {
                if (TryLoadState())
                {
                    Type($"Сохранение загружено. Уровень {_state.Level}, очки {_state.Score}.\n");
                }
                else
                {
                    Type("Не удалось прочитать сохранение — начинаем заново.\n");
                }
                return;
            }

            Type("Введите 1 или 2.");
        }
    }

    private bool TryLoadState()
    {
        try
        {
            string json = File.ReadAllText(SavePath);
            var loaded = JsonSerializer.Deserialize<GameState>(json);
            if (loaded == null) return false;

            _state = loaded;

            foreach (var path in _state.DecryptedFiles)
            {
                var file = _files.FirstOrDefault(f => f.Path == path);
                if (file == null) continue;

                file.Decrypted = true;
                string plain = ExpectedPlainText(file.Path);
                if (plain != null) file.Content = plain;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string PromptHost() => _state.Level switch
    {
        1 => "gate-net",
        2 => "corp-net",
        _ => "core-net"
    };

    private void Dispatch(string cmd, string arg)
    {
        switch (cmd)
        {
            case "help": Help(); break;
            case "ls":
            case "dir": Ls(); break;
            case "cd": Cd(arg); break;
            case "cat":
            case "open": Cat(arg); break;
            case "decrypt": Decrypt(arg); break;
            case "unlock": Unlock(arg); break;
            case "hack": Hack(arg); break;
            case "scan": Scan(); break;
            case "connect": Connect(arg); break;
            case "status": Status(); break;
            case "save": Save(); break;
            case "clear":
            case "cls": Console.Clear(); break;
            case "exit":
            case "quit":
                Type("Завершение сеанса...");
                _state.GameOver = true;
                break;
            default:
                Type($"Неизвестная команда: '{cmd}'. Введите 'help'.");
                break;
        }
    }

    private void Help()
    {
        Type(
            "Доступные команды:\n" +
            "  help                 - показать эту справку\n" +
            "  ls / dir             - показать содержимое текущей папки\n" +
            "  cd <папка>           - перейти в папку, cd .. - назад\n" +
            "  cat <файл>           - прочитать файл (open тоже работает)\n" +
            "  decrypt <файл> <key> - расшифровать файл шифром Цезаря\n" +
            "  unlock <код>         - ввести код разблокировки\n" +
            "  hack <цель>          - взломать цель (запросит пароль)\n" +
            "  scan                 - просканировать сеть на новые узлы\n" +
            "  connect <адрес>      - подключиться к обнаруженному узлу\n" +
            "  status               - прогресс, очки, состояние подключений\n" +
            "  save                 - сохранить прогресс в save.json\n" +
            "  clear / cls          - очистить экран\n" +
            "  exit / quit          - выйти из игры"
        );
    }

    private void Ls()
    {
        var dirs = _dirVisibleFromLevel
            .Where(kv => Parent(kv.Key) == _state.CurrentDir && kv.Value <= _state.Level)
            .Select(kv => kv.Key.Substring(kv.Key.LastIndexOf('/') + 1))
            .OrderBy(x => x);

        var files = _files
            .Where(f => f.Dir == _state.CurrentDir && f.VisibleFromLevel <= _state.Level)
            .OrderBy(f => f.Name);

        bool any = false;
        foreach (var d in dirs)
        {
            Type($"  <DIR>  {d}/", 1);
            any = true;
        }
        foreach (var f in files)
        {
            string tag = f.Encrypted && !f.Decrypted ? "[ENCRYPTED]" : "[OK]";
            Type($"         {f.Name,-20} {tag}", 1);
            any = true;
        }

        if (!any) Type("  (пусто)");
    }

    private void Cd(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Type("Использование: cd <папка> | cd ..");
            return;
        }

        if (arg == "..")
        {
            _state.CurrentDir = Parent(_state.CurrentDir);
            return;
        }

        if (arg == "/" )
        {
            _state.CurrentDir = "/";
            return;
        }

        string target = _state.CurrentDir == "/"
            ? "/" + arg.Trim('/')
            : _state.CurrentDir + "/" + arg.Trim('/');

        if (!_dirVisibleFromLevel.TryGetValue(target, out int visibleFrom))
        {
            Type("Такой папки не существует.");
            return;
        }

        if (visibleFrom > _state.Level)
        {
            Type("Доступ запрещён: этот узел ещё не обнаружен.");
            return;
        }

        if (target == "/server" && !_state.ServerConnected)
        {
            Type("Требуется подключение. Используй scan, затем connect <адрес>.");
            return;
        }

        if (target == "/vault" && !_state.VaultConnected)
        {
            Type("Требуется подключение. Используй scan, затем connect <адрес>.");
            return;
        }

        _state.CurrentDir = target;
    }

    private void Cat(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Type("Использование: cat <файл>");
            return;
        }

        var file = FindFile(arg);
        if (file == null)
        {
            Type("Файл не найден.");
            return;
        }

        if (file.Encrypted && !file.Decrypted)
        {
            Type("Файл зашифрован. Используй: decrypt <файл> <ключ>");
            return;
        }

        Type(file.Content);
    }

    private void Decrypt(string arg)
    {
        var parts = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !int.TryParse(parts[1], out int key))
        {
            Type("Использование: decrypt <файл> <ключ>");
            return;
        }

        var file = FindFile(parts[0]);
        if (file == null)
        {
            Type("Файл не найден.");
            return;
        }

        if (!file.Encrypted)
        {
            Type("Этот файл не зашифрован.");
            return;
        }

        string attempt = Cipher.Decode(file.Content, key);
        Type("Результат расшифровки:");
        Type(attempt);

        string expected = ExpectedPlainText(file.Path);

        if (expected != null && attempt == expected)
        {
            file.Decrypted = true;
            file.Content = attempt;
            _state.Score += 50;
            Type("[Ключ верный. Файл расшифрован.]");

            if (file.Path == "/vault/truth.enc")
            {
                _state.Victory = true;
                _state.GameOver = true;
            }
        }
        else if (expected != null)
        {
            Type("[Похоже на бессмыслицу — ключ, скорее всего, неверный.]");
        }
    }

    private static string ExpectedPlainText(string path) => path switch
    {
        "/gate/access.enc" => AccessPlainText,
        "/vault/truth.enc" => TruthPlainText,
        _ => null
    };

    private void Unlock(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            Type("Использование: unlock <код>");
            return;
        }

        if (_state.Level != 1)
        {
            Type("Разблокировывать здесь нечего.");
            return;
        }

        var access = _files.First(f => f.Path == "/gate/access.enc");
        if (!access.Decrypted)
        {
            Type("Сначала нужно узнать код — расшифруй access.enc.");
            return;
        }

        if (code.Trim().Equals("raven", StringComparison.OrdinalIgnoreCase))
        {
            _state.Level = 2;
            _state.Score += 50;
            Type("[Ворота открыты. Обнаружена корпоративная сеть.]");
            Type("Уровень повышен: 2. Используй scan, чтобы найти новый узел.");
        }
        else
        {
            Type("Неверный код.");
        }
    }

    private void Hack(string arg)
    {
        var parts = arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string target = parts.Length > 0 ? parts[0].ToLowerInvariant() : "";

        if (target != "admin")
        {
            Type("Неизвестная цель. Сейчас доступен только 'admin'.");
            return;
        }

        if (_state.Level != 2)
        {
            Type("Цель недоступна на этом этапе.");
            return;
        }

        string password = parts.Length > 1 ? parts[1] : null;

        if (password == null)
        {
            Console.WriteLine($"Взлом учётной записи 'admin'. Осталось попыток: {_state.HackAttemptsLeft}");
            Console.Write("password> ");
            password = Console.ReadLine() ?? "";
        }

        if (password.Trim().Equals("blackout", StringComparison.OrdinalIgnoreCase))
        {
            _state.Level = 3;
            _state.Score += 50;
            _state.HackAttemptsLeft = 3;
            Type("[Доступ получен. Обнаружено хранилище (vault).]");
            Type("Уровень повышен: 3. Используй scan, чтобы найти новый узел.");
        }
        else
        {
            _state.HackAttemptsLeft--;
            if (_state.HackAttemptsLeft <= 0)
            {
                Type("[Соединение отслежено. Сессия прервана.]");
                _state.GameOver = true;
                _state.Victory = false;
            }
            else
            {
                Type($"Неверный пароль. Осталось попыток: {_state.HackAttemptsLeft}");
            }
        }
    }

    private void Scan()
    {
        if (_state.Level == 2 && !_state.ServerNodeDiscovered)
        {
            _state.ServerNodeDiscovered = true;
            Type($"Обнаружен новый узел: {ServerAddress} (server)");
            Type("Используй: connect " + ServerAddress);
            return;
        }

        if (_state.Level == 3 && !_state.VaultNodeDiscovered)
        {
            _state.VaultNodeDiscovered = true;
            Type($"Обнаружен новый узел: {VaultAddress} (vault)");
            Type("Используй: connect " + VaultAddress);
            return;
        }

        Type("Новых сигналов не обнаружено.");
    }

    private void Connect(string arg)
    {
        arg = arg.Trim();

        if (arg == ServerAddress && _state.Level >= 2 && _state.ServerNodeDiscovered)
        {
            _state.ServerConnected = true;
            _state.Score += 10;
            Type("Соединение установлено: server");
        }
        else if (arg == VaultAddress && _state.Level >= 3 && _state.VaultNodeDiscovered)
        {
            _state.VaultConnected = true;
            _state.Score += 10;
            Type("Соединение установлено: vault");
        }
        else
        {
            Type("Не удалось подключиться: адрес неизвестен или недоступен.");
        }
    }

    private void Status()
    {
        Type(
            $"Уровень:    {_state.Level}\n" +
            $"Очки:       {_state.Score}\n" +
            $"Путь:       {_state.CurrentDir}\n" +
            $"Server:     {(_state.ServerConnected ? "подключен" : (_state.ServerNodeDiscovered ? "обнаружен" : "неизвестен"))}\n" +
            $"Vault:      {(_state.VaultConnected ? "подключен" : (_state.VaultNodeDiscovered ? "обнаружен" : "неизвестен"))}\n" +
            $"Попыток hack: {_state.HackAttemptsLeft}"
        );
    }

    private void Save()
    {
        if (_state.GameOver)
        {
            Type("Игра уже завершена — сохранять нечего.");
            return;
        }

        try
        {
            _state.DecryptedFiles = _files.Where(f => f.Decrypted).Select(f => f.Path).ToList();
            var json = JsonSerializer.Serialize(_state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SavePath, json);
            Type("Прогресс сохранён в " + SavePath);
        }
        catch (Exception ex)
        {
            Type("Не удалось сохранить: " + ex.Message);
        }
    }
    
    private QuestFile FindFile(string name)
    {
        name = name.Trim('/');
        string full = _state.CurrentDir == "/" ? "/" + name : _state.CurrentDir + "/" + name;

        return _files.FirstOrDefault(f =>
            (f.Path == full || f.Name == name) && f.VisibleFromLevel <= _state.Level);
    }

    private static string Parent(string path)
    {
        if (path == "/") return "/";
        int idx = path.LastIndexOf('/');
        if (idx <= 0) return "/";
        return path.Substring(0, idx);
    }

    private static void Type(string text, int delayMs = 4)
    {
        foreach (char c in text)
        {
            Console.Write(c);
            Console.Out.Flush();
            if (delayMs > 0) Thread.Sleep(delayMs);
        }
        Console.WriteLine();
    }

    private static void PrintBanner()
    {
        Console.WriteLine(@"
 _   _    _    ____ _  _______ ____
| | | |  / \  / ___| |/ / ____|  _ \
| |_| | / _ \| |   | ' /|  _| | |_) |
|  _  |/ ___ \ |___| . \| |___|  _ <
|_| |_/_/   \_\____|_|\_\_____|_| \_\
        T E R M I N A L   v1.0
");
        Console.WriteLine();
    }

    private static void BootSequence()
    {
        string[] steps =
        {
            "Booting kernel...",
            "Mounting filesystem...",
            "Loading network drivers...",
            "Establishing anonymous relay...",
            "Spoofing MAC address...",
        };

        foreach (var s in steps)
        {
            Console.Write(s);
            Thread.Sleep(150);
            Console.WriteLine(" [OK]");
        }

        Console.WriteLine();
    }

    private void EndScreen()
    {
        Console.WriteLine();
        if (_state.Victory)
        {
            Type("=====================================");
            Type("           МИССИЯ ВЫПОЛНЕНА");
            Type("=====================================");
            Type("Хранилище вскрыто. Правда свободна.");
            Type($"Итоговый счёт: {_state.Score}");
        }
        else
        {
            Type("=====================================");
            Type("           СЕССИЯ ЗАВЕРШЕНА");
            Type("=====================================");
            Type($"Итоговый счёт: {_state.Score}");
        }
        Console.ResetColor();
    }
}