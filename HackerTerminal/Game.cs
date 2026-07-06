using System.Text.Json;

namespace HackerTerminal;

public class Game
{
    private GameState _state = new();
    private readonly List<QuestFile> _files;
    private readonly Dictionary<string, int> _dirVisibleFromLevel = new()
    {
        ["/inbox"] = 1,
        ["/reception"] = 1,
        ["/studio"] = 2,
        ["/archive"] = 3,
    };

    private const string StudioAddress = "172.20.14.9";
    private const string ArchiveAddress = "10.55.2.1";

    private const string BadgePlainText = "GUEST CODE: COMPASS";
    private const string TruthPlainText = "Утечка произошла по вине продюсера Марии";

    private const string SavePath = "save.json";

    private const int ScoreForDecrypt = 50;
    private const int ScoreForLevelUp = 50;
    private const int ScoreForConnect = 10;

    private static readonly JsonSerializerOptions SaveJsonOptions = new() { WriteIndented = true };

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
                    "Студию PMR Peak Games наняли тебя, чтобы найти того, кто слил в сеть\n" +
                    "предрелизную сборку игры «Порог» за неделю до релиза.\n" +
                    "Начни с собственной папки — там черновики, которые ты набросал после\n" +
                    "первого созвона с заказчиком.\n" +
                    "Команда help покажет список доступных операций."
            },
            new()
            {
                Path = "/inbox/notes.txt",
                VisibleFromLevel = 1,
                Content =
                    "Мои заметки, чтобы не забыть.\n" +
                    "На проходной всё ещё стоит древний ридер бейджей с шифром Цезаря,\n" +
                    "сдвиг вроде 6."
            },
            new()
            {
                Path = "/inbox/old_grocery_list.txt",
                VisibleFromLevel = 1,
                Content =
                    "Список покупок (черновик, случайно попал не в ту папку):\n" +
                    "- кофе\n" +
                    "- хлеб\n" +
                    "- зарядка для мыши\n" +
                    "- забрать посылку\n" +
                    "(Похоже, это не имеет отношения к делу.)"
            },
            new()
            {
                Path = "/reception/badge.enc",
                VisibleFromLevel = 1,
                Encrypted = true,
                CipherKey = 6,
                Content = Cipher.Encode(BadgePlainText, 6)
            },
            new()
            {
                Path = "/reception/visitor_log.txt",
                VisibleFromLevel = 1,
                Content =
                    "Журнал регистрации посетителей.\n" +
                    "14:02 — курьер, доставка обедов.\n" +
                    "14:15 — техник, обслуживание кондиционера.\n" +
                    "14:40 — курьер, доставка обедов (опять).\n" +
                    "(Ничего полезного, обычный день.)"
            },
            new()
            {
                Path = "/studio/security_log.txt",
                VisibleFromLevel = 2,
                Content =
                    "[ЖУРНАЛ БЕЗОПАСНОСТИ]\n" +
                    "Зафиксирован вход в систему сборки под учёткой 'admin' в необычное время.\n" +
                    "Пароль, похоже, кто-то обсуждал в общем чате команды — глупо,\n" +
                    "но людям свойственно ошибаться."
            },
            new()
            {
                Path = "/studio/team_chat.json",
                VisibleFromLevel = 2,
                Content =
                    "[Экспорт переписки. Общий чат команды «Порог».]\n\n" +
                    "Egor_Dev, 09:14: доброе утро, кто-нибудь трогал билд-сервер ночью?\n" +
                    "Nastya_QA, 09:15: не я, я спала как убитая\n" +
                    "Egor_Dev, 09:16: странно, в логах новый вход под admin\n" +
                    "Egor_Dev, 09:20: так, пароль на билд-сервере опять дефолтный,\n" +
                    "я временно поставил midnight, поменяю вечером\n" +
                    "Nastya_QA, 09:21: ЕГОР ЭТО ОБЩИЙ ЧАТ\n" +
                    "Egor_Dev, 09:21: твою ж...\n" +
                    "Egor_Dev, 09:22: удалил сообщение! никто не видел, да?\n" +
                    "Nastya_QA, 09:22: уже видели, все 40 человек в чате"
            },
            new()
            {
                Path = "/studio/sprint_backlog.txt",
                VisibleFromLevel = 2,
                Content =
                    "Бэклог спринта.\n" +
                    "- поправить баг с текстурами на 3 уровне\n" +
                    "- добавить поддержку геймпада\n" +
                    "- написать тесты (когда-нибудь)\n" +
                    "(К утечке отношения не имеет.)"
            },
            new()
            {
                Path = "/archive/keycard.txt",
                VisibleFromLevel = 3,
                Content = "Фрагмент ключа шифрования, вышитый на старой карте доступа архива: 3"
            },
            new()
            {
                Path = "/archive/truth.enc",
                VisibleFromLevel = 3,
                Encrypted = true,
                CipherKey = 3,
                Content = Cipher.Encode(TruthPlainText, 3)
            },
            new()
            {
                Path = "/archive/coffee_machine_manual.txt",
                VisibleFromLevel = 3,
                Content =
                    "Инструкция к кофемашине в переговорной.\n" +
                    "Режим эспрессо — кнопка 2.\n" +
                    "(Прости, здесь правда нет ничего про утечку.)"
            },
            new()
            {
                Path = "/archive/СЕКРЕТНОЕ_НЕ_ОТКРЫВАТЬ.txt",
                VisibleFromLevel = 3,
                Content = "А что вы тут хотели найти? ;)"
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

            string? raw = Console.ReadLine();
            if (raw == null)
            {
                break;
            }

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
                string? plain = ExpectedPlainText(file.Path);
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
        1 => "reception-net",
        2 => "studio-net",
        _ => "archive-net"
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

    private static void Help()
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
            .Select(kv => kv.Key[(kv.Key.LastIndexOf('/') + 1)..])
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
            Type($"         {f.Name,-30} {tag}", 1);
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

        if (arg == "/")
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

        if (target == "/studio" && !_state.StudioConnected)
        {
            Type("Требуется подключение. Используй scan, затем connect <адрес>.");
            return;
        }

        if (target == "/archive" && !_state.ArchiveConnected)
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
        
        int normalizedInput = ((key % 26) + 26) % 26;
        int normalizedExpected = ((file.CipherKey % 26) + 26) % 26;

        if (normalizedInput == normalizedExpected)
        {
            file.Decrypted = true;
            file.Content = attempt;
            _state.Score += ScoreForDecrypt;
            Type("[Ключ верный. Файл расшифрован.]");

            if (file.Path == "/archive/truth.enc")
            {
                _state.Victory = true;
                _state.GameOver = true;
            }
        }
        else
        {
            Type("[Похоже на бессмыслицу — ключ, скорее всего, неверный.]");
        }
    }
    
    private static string? ExpectedPlainText(string path) => path switch
    {
        "/reception/badge.enc" => BadgePlainText,
        "/archive/truth.enc" => TruthPlainText,
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

        var badge = _files.First(f => f.Path == "/reception/badge.enc");
        if (!badge.Decrypted)
        {
            Type("Сначала нужно узнать код — расшифруй badge.enc.");
            return;
        }

        if (code.Trim().Equals("compass", StringComparison.OrdinalIgnoreCase))
        {
            _state.Level = 2;
            _state.Score += ScoreForLevelUp;
            Type("[Проходная открыта. Обнаружена внутренняя сеть студии.]");
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

        string? password = parts.Length > 1 ? parts[1] : null;

        if (password == null)
        {
            Console.WriteLine($"Взлом учётной записи 'admin'. Осталось попыток: {_state.HackAttemptsLeft}");
            Console.Write("password> ");
            password = Console.ReadLine() ?? "";
        }

        if (password.Trim().Equals("midnight", StringComparison.OrdinalIgnoreCase))
        {
            _state.Level = 3;
            _state.Score += ScoreForLevelUp;
            _state.HackAttemptsLeft = 3;
            Type("[Доступ получен. Обнаружен архив студии.]");
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
        if (_state.Level == 2 && !_state.StudioNodeDiscovered)
        {
            _state.StudioNodeDiscovered = true;
            Type($"Обнаружен новый узел: {StudioAddress} (studio)");
            Type("Используй: connect " + StudioAddress);
            return;
        }

        if (_state.Level == 3 && !_state.ArchiveNodeDiscovered)
        {
            _state.ArchiveNodeDiscovered = true;
            Type($"Обнаружен новый узел: {ArchiveAddress} (archive)");
            Type("Используй: connect " + ArchiveAddress);
            return;
        }

        Type("Новых сигналов не обнаружено.");
    }

    private void Connect(string arg)
    {
        arg = arg.Trim();

        if (arg == StudioAddress && _state.Level >= 2 && _state.StudioNodeDiscovered && !_state.StudioConnected)
        {
            _state.StudioConnected = true;
            _state.Score += ScoreForConnect;
            Type("Соединение установлено: studio");
        }
        else if (arg == ArchiveAddress && _state.Level >= 3 && _state.ArchiveNodeDiscovered && !_state.ArchiveConnected)
        {
            _state.ArchiveConnected = true;
            _state.Score += ScoreForConnect;
            Type("Соединение установлено: archive");
        }
        else
        {
            Type("Не удалось подключиться: адрес неизвестен или недоступен.");
        }
    }

    private void Status()
    {
        Type(
            $"Уровень:      {_state.Level}\n" +
            $"Очки:         {_state.Score}\n" +
            $"Путь:         {_state.CurrentDir}\n" +
            $"Studio:       {(_state.StudioConnected ? "подключен" : (_state.StudioNodeDiscovered ? "обнаружен" : "неизвестен"))}\n" +
            $"Archive:      {(_state.ArchiveConnected ? "подключен" : (_state.ArchiveNodeDiscovered ? "обнаружен" : "неизвестен"))}\n" +
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
            var json = JsonSerializer.Serialize(_state, SaveJsonOptions);
            File.WriteAllText(SavePath, json);
            Type("Прогресс сохранён в " + SavePath);
        }
        catch (Exception ex)
        {
            Type("Не удалось сохранить: " + ex.Message);
        }
    }

    private QuestFile? FindFile(string name)
    {
        name = name.Trim('/');
        string full = _state.CurrentDir == "/" ? "/" + name : _state.CurrentDir + "/" + name;

        return _files.FirstOrDefault(f =>
            f.Path == full && f.VisibleFromLevel <= _state.Level);
    }

    private static string Parent(string path)
    {
        if (path == "/") return "/";
        int idx = path.LastIndexOf('/');
        if (idx <= 0) return "/";
        return path[..idx];
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
        [
            "Booting kernel...",
            "Mounting filesystem...",
            "Loading network drivers...",
            "Establishing anonymous relay...",
            "Spoofing MAC address...",
        ];

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
            Type("Личность утечки установлена. Дело закрыто.");
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