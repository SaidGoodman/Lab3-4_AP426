using DetectiveGame.Models;
using DetectiveGame.Services;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DetectiveGame.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly SaveLoadService _saveLoad = new SaveLoadService();
        private GameEngine _engine;

        private readonly StringBuilder _log = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();

            _engine = new GameEngine(_saveLoad);
            CommandsText.Text = _engine.GetCommandsForUi();
            RefreshInventory();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ShowIntroAsync();
            Append(_engine.Look());
            RefreshInventory();
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

        private void RefreshInventory()
        {
            InventoryList.ItemsSource = null;
            InventoryList.ItemsSource = _engine.State.CollectedEvidence;
        }

        private void Send()
        {
            var input = InputBox.Text.Trim();
            InputBox.Text = "";

            if (string.IsNullOrWhiteSpace(input))
                return;

            var answer = _engine.Execute(input);
            if (!string.IsNullOrWhiteSpace(answer))
                Append(answer);

            RefreshInventory();
        }

        private void Send_Click(object sender, RoutedEventArgs e) => Send();

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Send();
        }

        private void Look_Click(object sender, RoutedEventArgs e)
        {
            Append(_engine.Look());
            RefreshInventory();
        }

        private void Save_Click(object sender, RoutedEventArgs e) => Append(_engine.Save());

        private void Verdict_Click(object sender, RoutedEventArgs e) => Append(_engine.StartVerdict());

        private async void NewGame_Click(object sender, RoutedEventArgs e)
        {
            ClearLog();
            InvClue.Text = "";
            InvDetails.Text = "";

            _engine.NewGame();
            RefreshInventory();

            await ShowIntroAsync();
            Append(_engine.Look());
            RefreshInventory();
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
