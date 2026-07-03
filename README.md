# HackerTerminal

Консольный текстовый квест на C# (.NET 10), стилизованный под терминал хакера.

## Запуск

```
dotnet run
```

или открыть `HackerTerminal.csproj` в Rider и запустить.

## Команды

`help`, `ls`/`dir`, `cd <папка>`, `cat <файл>`/`open`, `decrypt <файл> <ключ>`,
`unlock <код>`, `hack <цель>`, `scan`, `connect <адрес>`, `status`, `save`,
`clear`/`cls`, `exit`/`quit`.

## Прохождение (шпаргалка)

1. `cat readme.txt`, затем `cd home`, `cat notes.txt` — узнать ключ шифра (4).
2. `cd ..`, `cd gate`, `decrypt access.enc 4` — получить код `RAVEN`.
3. `unlock raven` — уровень 2, открыта корпоративная сеть.
4. `scan` — найти адрес сервера, `connect 192.168.4.7`.
5. `cd server`, `cat log.txt`, `cat project.txt` — узнать пароль `blackout`.
6. `hack admin blackout` — уровень 3, открыто хранилище.
7. `scan`, `connect 10.0.0.7`, `cd vault`, `cat keycard.txt` — ключ 7.
8. `decrypt truth.enc 7` — финал.

## Структура проекта

- `Program.cs` — точка входа.
- `Game.cs` — главный цикл, разбор команд, вся логика уровней.
- `GameState.cs` — состояние игрока (уровень, очки, подключения).
- `QuestFile.cs` — модель файла игровой файловой системы.
- `Cipher.cs` — шифр Цезаря.
- `save.json` — создаётся командой `save`, в git не коммитится.
