using DetectiveGame.Models;
using DetectiveGame.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DetectiveGame.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly SaveLoadService _saveLoad = new SaveLoadService();
        private GameEngine _engine;

        private readonly StringBuilder _log = new StringBuilder();

        private List<Character> _suspectsForVerdict = new List<Character>();

        public MainWindow()
        {
            InitializeComponent();

            _engine = new GameEngine(_saveLoad);

            CommandsText.Text =
@"Кнопки управления:

Осмотр — показывает описание текущей локации, предметы, персонажей и выходы.
Перейти — перемещает в выбранную локацию из списка «Выходы».
Подобрать — берёт выбранный предмет из списка «Предметы рядом».
Поговорить — начинает диалог с выбранным персонажем из списка «Персонажи».
Вердикт — открывает выбор подозреваемого.
Обвинить — подтверждает выбранного подозреваемого.
Сохранить — сохраняет прогресс.
Загрузить — загружает сохранение.
Новая игра — начинает игру заново.";

            RefreshAll();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ShowIntroAsync();
            Append(_engine.Look());
            RefreshAll();
        }

        private async Task ShowIntroAsync()
        {
            await TypeLineAsync("Лондон, 1889 год. Дождливая осень...", 35);
            await TypeLineAsync("Вы — частный детектив, чья карьера висит на волоске.", 25);
            await TypeLineAsync("Сегодня утром к вам в контору ворвалась заплаканная женщина.", 25);
            await TypeLineAsync("Ее муж пропал три дня назад. Полиция бездействует.", 25);
            await TypeLineAsync("Единственная зацепка — его старый дом на окраине...", 25);
            await TypeLineAsync("Ваша цель — найти пропавшего мистера Блэквуда.", 25);
            Append("");
        }

        private async Task TypeLineAsync(string line, int delayMs)
        {
            if (_log.Length > 0)
                _log.AppendLine();

            foreach (var ch in line)
            {
                _log.Append(ch);
                LogText.Text = _log.ToString();
                LogScroll.ScrollToEnd();
                await Task.Delay(delayMs);
            }

            _log.AppendLine();
            LogText.Text = _log.ToString();
            LogScroll.ScrollToEnd();
        }

        private void Append(string text)
        {
            if (text == null) return;

            if (_log.Length > 0)
                _log.AppendLine();

            _log.AppendLine(text);
            LogText.Text = _log.ToString();
            LogScroll.ScrollToEnd();
        }

        private void ClearLog()
        {
            _log.Clear();
            LogText.Text = "";
            LogScroll.ScrollToTop();
        }

        private void RefreshAll()
        {
            RefreshInventory();
            RefreshContext();
            UpdateUiMode();
        }

        private void RefreshInventory()
        {
            InventoryList.ItemsSource = null;
            InventoryList.ItemsSource = _engine.State.CollectedEvidence;
        }

        private void RefreshContext()
        {
            var state = _engine.State;

            var current = state.Locations.FirstOrDefault(l => l.Name == state.CurrentLocationName);
            var exits = state.Locations.Where(l => l.Name != state.CurrentLocationName).ToList();
            var evidenceHere = current?.EvidenceList?.ToList() ?? new List<Evidence>();
            var charsHere = state.Characters.Where(c => c.LocationName == state.CurrentLocationName).ToList();

            ExitsList.ItemsSource = exits;
            EvidenceHereList.ItemsSource = evidenceHere;
            CharactersHereList.ItemsSource = charsHere;

            if (_engine.Mode == EngineMode.ChoosingVerdict)
            {
                _suspectsForVerdict = state.Characters.Where(c => c.IsSuspect).ToList();
                SuspectsList.ItemsSource = _suspectsForVerdict;
                VerdictPanel.Visibility = Visibility.Visible;
            }
            else
            {
                _suspectsForVerdict = new List<Character>();
                SuspectsList.ItemsSource = null;
                SuspectsList.SelectedItem = null;
                VerdictPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateUiMode()
        {
            var isVerdict = _engine.Mode == EngineMode.ChoosingVerdict;
            var isGameOver = _engine.Mode == EngineMode.GameOver;

            ActionsPanel.IsEnabled = !isVerdict && !isGameOver;

            LookButton.IsEnabled = !isVerdict && !isGameOver;
            SaveButton.IsEnabled = !isVerdict && !isGameOver;
            VerdictButton.IsEnabled = !isVerdict && !isGameOver;

            // В конце игры по тексту движка остаются «Новая игра» и «Загрузить».
            LoadButton.IsEnabled = true;
            NewGameButton.IsEnabled = true;

            MoveButton.IsEnabled = ActionsPanel.IsEnabled && ExitsList.SelectedItem != null;
            TakeButton.IsEnabled = ActionsPanel.IsEnabled && EvidenceHereList.SelectedItem != null;
            TalkButton.IsEnabled = ActionsPanel.IsEnabled && CharactersHereList.SelectedItem != null;

            AccuseButton.IsEnabled = isVerdict && SuspectsList.SelectedItem != null;
        }

        private void ContextSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) =>
            UpdateUiMode();

        private void Look_Click(object sender, RoutedEventArgs e)
        {
            Append(_engine.Look());
            RefreshAll();
        }

        private void Save_Click(object sender, RoutedEventArgs e) => Append(_engine.Save());

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Append(_engine.Load());
            Append(_engine.Look());
            RefreshAll();
        }

        private void Verdict_Click(object sender, RoutedEventArgs e)
        {
            Append(_engine.StartVerdict());
            RefreshAll();
        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            if (ExitsList.SelectedItem is Location loc)
            {
                var answer = _engine.Execute($"идти {loc.Name}");
                if (!string.IsNullOrWhiteSpace(answer))
                    Append(answer);
                RefreshAll();
            }
        }

        private void Take_Click(object sender, RoutedEventArgs e)
        {
            if (EvidenceHereList.SelectedItem is Evidence ev)
            {
                var answer = _engine.Execute($"взять {ev.Description}");
                if (!string.IsNullOrWhiteSpace(answer))
                    Append(answer);
                RefreshAll();
            }
        }

        private void Talk_Click(object sender, RoutedEventArgs e)
        {
            if (CharactersHereList.SelectedItem is Character ch)
            {
                var answer = _engine.Execute($"говорить {ch.Name}");
                if (!string.IsNullOrWhiteSpace(answer))
                    Append(answer);
                RefreshAll();
            }
        }

        private void Accuse_Click(object sender, RoutedEventArgs e)
        {
            if (_engine.Mode != EngineMode.ChoosingVerdict)
                return;

            if (SuspectsList.SelectedIndex < 0)
                return;

            // В движке вердикт выбирается по номеру в списке подозреваемых (с 1).
            var answer = _engine.Execute((SuspectsList.SelectedIndex + 1).ToString());
            if (!string.IsNullOrWhiteSpace(answer))
                Append(answer);

            RefreshAll();
        }

        private async void NewGame_Click(object sender, RoutedEventArgs e)
        {
            ClearLog();
            InvClue.Text = "";
            InvDetails.Text = "";

            _engine.NewGame();
            RefreshAll();

            await ShowIntroAsync();
            Append(_engine.Look());
            RefreshAll();
        }

        private void InventoryList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (InventoryList.SelectedItem is Evidence ev)
            {
                InvClue.Text = ev.Clue ?? "";
                InvDetails.Text = ev.Details ?? "";
            }
        }
    }
}