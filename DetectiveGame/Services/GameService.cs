using DetectiveGame.Models;
using System;
using System.Linq;
using System.Threading;

namespace DetectiveGame.Services
{
    public enum GameLoopResult
    {
        Quit,
        Restart
    }

    public class GameService
    {
        private const string KillerName = "Доктор Харроу";

        private GameState _gameState;
        private SaveLoadService _saveLoadService;
        private bool _isGameRunning;
        private GameLoopResult _loopResult;

        public GameService(GameState gameState, SaveLoadService saveLoadService)
        {
            _gameState = gameState;
            _saveLoadService = saveLoadService;
            _isGameRunning = true;
            _loopResult = GameLoopResult.Quit;
        }

        public void ShowIntro()
        {
            Console.Clear();
            PrintSlowly("Лондон, 1889 год. Дождливая осень...", 50);
            PrintSlowly("Вы — частный детектив, чья карьера висит на волоске.", 30);
            PrintSlowly("Сегодня утром к вам в контору ворвалась заплаканная женщина.", 30);
            PrintSlowly("Ее муж пропал три дня назад. Полиция бездействует.", 30);
            PrintSlowly("Единственная зацепка — его старый дом на окраине...", 30);
            PrintSlowly("Ваша цель — найти пропавшего мистера Блэквуда.", 30);
            Console.WriteLine();
            Console.WriteLine("Нажмите Enter...");
            Console.ReadLine();
            Console.Clear();

            ShowHelp();
        }

        public GameLoopResult StartGameLoop()
        {
            if (string.IsNullOrEmpty(_gameState.CurrentLocationName) && _gameState.Locations.Count > 0)
            {
                _gameState.CurrentLocationName = _gameState.Locations[0].Name;
            }

            DisplayCurrentLocation();

            while (_isGameRunning)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Ваша команда > ");
                Console.ResetColor();

                string input = Console.ReadLine();
                ProcessInput(input);
            }

            return _loopResult;
        }

        private void ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            string[] parts = input.Trim().Split(new[] { ' ' }, 2);
            string command = parts[0].ToLower();
            string argument = parts.Length > 1 ? parts[1].Trim() : "";

            switch (command)
            {
                case "помощь":
                    ShowHelp();
                    break;
                case "осмотр":
                    DisplayCurrentLocation();
                    break;
                case "идти":
                    MoveToLocation(argument);
                    break;
                case "взять":
                    TakeItem(argument);
                    break;
                case "говорить":
                    TalkToCharacter(argument);
                    break;
                case "инвентарь":
                    ShowInventory();
                    break;
                case "вердикт":
                    MakeVerdict();
                    break;
                case "вынести":
                    if (argument.Equals("вердикт", StringComparison.OrdinalIgnoreCase) ||
                        argument.StartsWith("вердикт", StringComparison.OrdinalIgnoreCase))
                        MakeVerdict();
                    else
                        Console.WriteLine("Неизвестная команда. Введите 'помощь'.");
                    break;
                case "сохранить":
                    _saveLoadService.SaveGame(_gameState);
                    break;
                case "выход":
                    _loopResult = GameLoopResult.Quit;
                    _isGameRunning = false;
                    break;
                default:
                    Console.WriteLine("Неизвестная команда. Введите 'помощь'.");
                    break;
            }
        }

        private void DisplayCurrentLocation()
        {
            var loc = GetCurrentLocation();
            if (loc == null) return;

            Console.WriteLine("==============================");
            Console.WriteLine($"ЛОКАЦИЯ: {loc.Name}");
            Console.WriteLine(loc.Description);
            Console.WriteLine("==============================");

            if (loc.EvidenceList.Count > 0)
            {
                Console.WriteLine("Предметы рядом:");
                foreach (var item in loc.EvidenceList)
                {
                    Console.WriteLine($" - {item.Description}");
                }
            }
            else
            {
                Console.WriteLine("Здесь нет предметов, которые можно взять.");
            }

            var charactersHere = _gameState.Characters
                .Where(c => c.LocationName == loc.Name)
                .ToList();

            if (charactersHere.Count > 0)
            {
                Console.WriteLine("Здесь находятся:");
                foreach (var c in charactersHere)
                {
                    Console.WriteLine($" - {c.Name} ({c.Role})");
                }
            }

            Console.WriteLine("Выходы (куда можно пойти):");
            foreach (var l in _gameState.Locations)
            {
                if (l.Name != loc.Name)
                {
                    Console.WriteLine($" -> {l.Name}");
                }
            }
        }

        private void MoveToLocation(string targetName)
        {
            if (string.IsNullOrEmpty(targetName))
            {
                Console.WriteLine("Куда идти? Укажите название локации. (Например: 'идти Бар')");
                return;
            }

            var targetLoc = _gameState.Locations
                .FirstOrDefault(l => l.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

            if (targetLoc != null)
            {
                if (targetLoc.Name == _gameState.CurrentLocationName)
                {
                    Console.WriteLine("Вы уже находитесь здесь.");
                    return;
                }

                _gameState.CurrentLocationName = targetLoc.Name;
                Console.WriteLine($"...Вы переходите в {targetLoc.Name}...");
                DisplayCurrentLocation();
            }
            else
            {
                Console.WriteLine($"Локация \"{targetName}\" не найдена. Посмотрите доступные выходы через 'осмотр'.");
            }
        }

        private void TakeItem(string itemName)
        {
            var loc = GetCurrentLocation();
            if (loc == null) return;

            if (string.IsNullOrEmpty(itemName))
            {
                Console.WriteLine("Что взять? Укажите название предмета. (Например: 'взять Записка')");
                if (loc.EvidenceList.Count > 0)
                {
                    Console.WriteLine("Доступно: " + string.Join(", ", loc.EvidenceList.Select(e => e.Description)));
                }
                return;
            }

            var item = loc.EvidenceList
                .FirstOrDefault(e => e.Description.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                _gameState.CollectedEvidence.Add(item);
                loc.EvidenceList.Remove(item);

                string shortClue = string.IsNullOrWhiteSpace(item.Clue) ? "Без отметок." : item.Clue;
                Console.WriteLine($"Вы подобрали: {item.Description}. ({shortClue})");
            }
            else
            {
                Console.WriteLine($"Предмет \"{itemName}\" не найден здесь.");
            }
        }

        private void TalkToCharacter(string charName)
        {
            if (string.IsNullOrEmpty(charName))
            {
                Console.WriteLine("С кем говорить? Введите имя.");
                return;
            }

            var character = _gameState.Characters
                .FirstOrDefault(c =>
                    c.Name.Equals(charName, StringComparison.OrdinalIgnoreCase) &&
                    c.LocationName == _gameState.CurrentLocationName);

            if (character != null)
            {
                Console.WriteLine($"[{character.Name}]: \"{character.Interrogate(_gameState)}\"");
            }
            else
            {
                var charSomewhere = _gameState.Characters
                    .FirstOrDefault(c => c.Name.Equals(charName, StringComparison.OrdinalIgnoreCase));

                if (charSomewhere != null)
                    Console.WriteLine($"{charSomewhere.Name} сейчас не здесь. Поищите в других локациях.");
                else
                    Console.WriteLine("Такого персонажа здесь нет.");
            }
        }

        private void ShowInventory()
        {
            Console.WriteLine("--- ИНВЕНТАРЬ ---");
            if (_gameState.CollectedEvidence.Count == 0)
            {
                Console.WriteLine("(пусто)");
                return;
            }

            for (int i = 0; i < _gameState.CollectedEvidence.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_gameState.CollectedEvidence[i].Description}");
            }

            Console.WriteLine("Введите номер, чтобы прочитать описание, или Enter, чтобы выйти.");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("> ");
                Console.ResetColor();

                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) break;

                if (!int.TryParse(input, out int index) || index < 1 || index > _gameState.CollectedEvidence.Count)
                {
                    Console.WriteLine("Неверный номер.");
                    continue;
                }

                var item = _gameState.CollectedEvidence[index - 1];
                Console.WriteLine("------------------------------");
                Console.WriteLine(item.Description);
                if (!string.IsNullOrWhiteSpace(item.Clue))
                    Console.WriteLine(item.Clue);

                string details = item.Details ?? "";
                if (string.IsNullOrWhiteSpace(details))
                    details = "На предмете нет ничего примечательного. Или вы просто не смотрите достаточно внимательно.";

                Console.WriteLine();
                Console.WriteLine(details);
                Console.WriteLine("------------------------------");
            }
        }

        private void MakeVerdict()
        {
            Console.Clear();
            PrintSlowly("Вы раскладываете улики на столе. Тишина звучит громче дождя...", 25);
            Console.WriteLine();

            var suspects = _gameState.Characters.Where(c => c.IsSuspect).ToList();
            if (suspects.Count == 0)
                suspects = _gameState.Characters.ToList();

            if (suspects.Count == 0)
            {
                Console.WriteLine("Некого обвинять. Мир пуст.");
                return;
            }

            Console.WriteLine("Подозреваемые:");
            for (int i = 0; i < suspects.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {suspects[i].Name} ({suspects[i].Role})");
            }

            Console.WriteLine("Кого вы обвиняете? (введите номер)");
            int pick = ReadNumber(1, suspects.Count);
            var chosen = suspects[pick - 1];

            Console.WriteLine();
            if (chosen.Name.Equals(KillerName, StringComparison.OrdinalIgnoreCase))
            {
                PrintSlowly($"Вы называете имя: {chosen.Name}.", 20);
                PrintSlowly("Слова падают в комнату, как тяжелые капли.", 20);
                PrintSlowly("На миг он замирает... затем взгляд выдает всё.", 20);
                PrintSlowly("Полиция успевает вовремя. Преступник схвачен, а туман наконец отступает.", 20);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("ПОБЕДА: вы поймали убийцу. Лондон запомнит ваше имя — хотя бы до следующей ночи.");
                Console.ResetColor();
            }
            else
            {
                PrintSlowly($"Вы называете имя: {chosen.Name}.", 20);
                PrintSlowly("Слишком быстро. Слишком уверенно.", 20);
                PrintSlowly("Пока вы строите обвинение, настоящий убийца уже растворяется в тумане.", 20);
                PrintSlowly("Утром в газетах будет новый заголовок. И в нем — не ваша победа.", 20);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ПОРАЖЕНИЕ: вы упустили преступника. Следы смыты дождем, а дело закрыто чужими руками.");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("Что дальше?");
            Console.WriteLine(" 1) Начать сначала");
            Console.WriteLine(" 2) Закрыть игру");

            int choice = ReadNumber(1, 2);
            _loopResult = (choice == 1) ? GameLoopResult.Restart : GameLoopResult.Quit;
            _isGameRunning = false;
        }

        private int ReadNumber(int min, int max)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("> ");
                Console.ResetColor();

                var input = Console.ReadLine();
                if (int.TryParse(input, out int n) && n >= min && n <= max)
                    return n;

                Console.WriteLine("Введите корректный номер.");
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("СПИСОК КОМАНД:");
            Console.WriteLine(" осмотр             - посмотреть описание, предметы и выходы");
            Console.WriteLine(" идти [локация]     - перейти (например: 'идти Бар')");
            Console.WriteLine(" взять [предмет]    - подобрать (например: 'взять Записка')");
            Console.WriteLine(" говорить [имя]     - допросить (например: 'говорить Сержант')");
            Console.WriteLine(" инвентарь          - ваши улики (можно читать описания)");
            Console.WriteLine(" вердикт            - вынести вердикт");
            Console.WriteLine(" сохранить          - записать игру");
            Console.WriteLine(" выход              - закрыть игру");
        }

        private Location GetCurrentLocation()
        {
            return _gameState.Locations.FirstOrDefault(l => l.Name == _gameState.CurrentLocationName);
        }

        private void PrintSlowly(string text, int delay)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delay);
            }
            Console.WriteLine();
        }
    }
}
