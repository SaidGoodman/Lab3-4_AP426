using DetectiveGame.Models;
using System;
using System.Linq;
using System.Text;

namespace DetectiveGame.Services
{
    public enum EngineMode
    {
        Normal,
        ChoosingVerdict,
        GameOver
    }

    public class GameEngine
    {
        private readonly SaveLoadService _saveLoad;

        public GameState State { get; private set; }
        public EngineMode Mode { get; private set; } = EngineMode.Normal;

        // Убийца по прошлой версии
        public string KillerName { get; } = "Доктор Харроу";

        public GameEngine(SaveLoadService saveLoad)
        {
            _saveLoad = saveLoad;
            State = _saveLoad.LoadGame();

            if (State.Locations.Count == 0)
                WorldFactory.InitializeNewGame(State);

            if (string.IsNullOrWhiteSpace(State.CurrentLocationName))
                State.CurrentLocationName = "Дом";
        }

        public string GetCommandsForUi() =>
@"осмотр
идти <локация>
взять <предмет>
говорить <имя>
вердикт
сохранить
загрузить";

        public void NewGame()
        {
            State = new GameState();
            WorldFactory.InitializeNewGame(State);
            Mode = EngineMode.Normal;
        }

        public string Save()
        {
            _saveLoad.SaveGame(State);
            return "Игра сохранена.";
        }

        public string Load()
        {
            State = _saveLoad.LoadGame();
            if (State.Locations.Count == 0)
                WorldFactory.InitializeNewGame(State);

            Mode = EngineMode.Normal;
            return "Игра загружена.";
        }

        public string Look()
        {
            var loc = GetCurrentLocation();
            if (loc == null) return "Текущая локация не найдена.";

            var sb = new StringBuilder();
            sb.AppendLine("================================");
            sb.AppendLine($"ЛОКАЦИЯ: {loc.Name}");
            sb.AppendLine(loc.Description);
            sb.AppendLine("================================");

            if (loc.EvidenceList.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Предметы рядом:");
                foreach (var e in loc.EvidenceList)
                    sb.AppendLine($" - {e.Description}");
            }

            var charsHere = State.Characters.Where(c => c.LocationName == loc.Name).ToList();
            if (charsHere.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Здесь находятся:");
                foreach (var c in charsHere)
                    sb.AppendLine($" - {c.Name} ({c.Role})");
            }

            sb.AppendLine();
            sb.AppendLine("Выходы:");
            foreach (var l in State.Locations.Where(l => l.Name != loc.Name))
                sb.AppendLine($" -> {l.Name}");

            return sb.ToString().TrimEnd();
        }

        public string Execute(string raw)
        {
            if (Mode == EngineMode.GameOver)
                return "Игра завершена. Нажмите «Новая игра» или введите «загрузить».";

            if (Mode == EngineMode.ChoosingVerdict)
                return ChooseVerdict(raw);

            if (string.IsNullOrWhiteSpace(raw)) return "";

            var parts = raw.Trim().Split(new[] { ' ' }, 2);
            var cmd = parts[0].ToLower();
            var arg = parts.Length > 1 ? parts[1].Trim() : "";

            return cmd switch
            {
                "осмотр" => Look(),
                "идти" => Move(arg),
                "взять" => Take(arg),
                "говорить" => Talk(arg),
                "вердикт" => StartVerdict(),
                "сохранить" => Save(),
                "загрузить" => Load() + "\n" + Look(),
                _ => "Неизвестная команда. Список команд — слева."
            };
        }

        private Location? GetCurrentLocation() =>
            State.Locations.FirstOrDefault(l => l.Name == State.CurrentLocationName);

        private string Move(string targetName)
        {
            if (string.IsNullOrWhiteSpace(targetName))
                return "Куда идти? (пример: идти Доки)";

            var target = State.Locations.FirstOrDefault(l =>
                l.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

            if (target == null)
                return $"Локация \"{targetName}\" не найдена.";

            if (target.Name == State.CurrentLocationName)
                return "Вы уже находитесь здесь.";

            State.CurrentLocationName = target.Name;
            return $"...Вы переходите в {target.Name}...\n{Look()}";
        }

        private string Take(string itemName)
        {
            var loc = GetCurrentLocation();
            if (loc == null) return "Текущая локация не найдена.";

            if (string.IsNullOrWhiteSpace(itemName))
                return loc.EvidenceList.Count == 0
                    ? "Здесь нечего брать."
                    : "Что взять? Доступно: " + string.Join(", ", loc.EvidenceList.Select(e => e.Description));

            var item = loc.EvidenceList.FirstOrDefault(e =>
                e.Description.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (item == null) return $"Предмет \"{itemName}\" не найден здесь.";

            State.CollectedEvidence.Add(item);
            loc.EvidenceList.Remove(item);
            return $"Вы подобрали: {item.Description}. ({item.Clue})";
        }

        private string Talk(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "С кем говорить? (пример: говорить Газетчик)";

            var character = State.Characters.FirstOrDefault(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                c.LocationName == State.CurrentLocationName);

            if (character == null)
            {
                var somewhere = State.Characters.FirstOrDefault(c =>
                    c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                return somewhere != null
                    ? $"{somewhere.Name} сейчас не здесь."
                    : "Такого персонажа здесь нет.";
            }

            var line = character.Interrogate(State);
            return $"[{character.Name}]: \"{line}\"";
        }

        public string StartVerdict()
        {
            var suspects = State.Characters.Where(c => c.IsSuspect).ToList();
            if (suspects.Count == 0) return "Подозреваемых нет.";

            Mode = EngineMode.ChoosingVerdict;

            var sb = new StringBuilder();
            sb.AppendLine("ВЫНEСТИ ВЕРДИКТ:");
            for (int i = 0; i < suspects.Count; i++)
                sb.AppendLine($"{i + 1}) {suspects[i].Name} — {suspects[i].Role}");
            sb.AppendLine("Введите номер подозреваемого.");
            return sb.ToString().TrimEnd();
        }

        private string ChooseVerdict(string input)
        {
            var suspects = State.Characters.Where(c => c.IsSuspect).ToList();

            if (!int.TryParse(input, out int n) || n < 1 || n > suspects.Count)
                return "Неверный номер. Введите номер из списка.";

            var chosen = suspects[n - 1].Name;
            var win = chosen.Equals(KillerName, StringComparison.OrdinalIgnoreCase);

            Mode = EngineMode.GameOver;

            return win
                ? "Вы называете имя — и пазл сходится.\nПреступник ломается на деталях. Вы поймали убийцу. Дело закрыто."
                : "Вы называете имя — и понимаете, что чего-то не хватило.\nНастоящий преступник ушёл в туман. Вы упустили убийцу.";
        }
    }
}
