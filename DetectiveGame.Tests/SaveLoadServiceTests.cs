using DetectiveGame.Models;
using DetectiveGame.Services;
using System.IO;
using Xunit;

namespace DetectiveGame.Tests
{
    public class SaveLoadServiceTests
    {
        [Fact]
        public void LoadGame_WhenNoSave_ReturnsNewGameState()
        {
            using var env = new TempCwdScope();
            var sls = new SaveLoadService();

            var state = sls.LoadGame();

            Assert.NotNull(state);
            Assert.NotNull(state.CollectedEvidence);
            Assert.NotNull(state.Characters);
            Assert.NotNull(state.Locations);
        }

        [Fact]
        public void SaveGame_ThenLoadGame_RoundTripsState()
        {
            using var env = new TempCwdScope();
            var sls = new SaveLoadService();

            var original = new GameState
            {
                CurrentLocationName = "Доки"
            };
            original.AddEvidence(new Evidence("Квитанция ломбарда", "Номер сделки", "Подсказка"));
            original.AddCharacter(new Character("Газетчик", "Свидетель", "Улица"));
            original.AddLocation(new Location("Доки", "Туман"));

            sls.SaveGame(original);

            Assert.True(File.Exists(Path.Combine("Data", "SaveGame.json")));

            var loaded = sls.LoadGame();

            Assert.Equal("Доки", loaded.CurrentLocationName);
            Assert.Single(loaded.CollectedEvidence);
            Assert.Single(loaded.Characters);
            Assert.Single(loaded.Locations);

            Assert.Equal("Квитанция ломбарда", loaded.CollectedEvidence[0].Description);
            Assert.Equal("Номер сделки", loaded.CollectedEvidence[0].Clue);
            Assert.Equal("Подсказка", loaded.CollectedEvidence[0].Details);
        }

        [Fact]
        public void DeleteSave_RemovesSaveFile()
        {
            using var env = new TempCwdScope();
            var sls = new SaveLoadService();

            var state = new GameState { CurrentLocationName = "Особняк" };
            sls.SaveGame(state);

            var path = Path.Combine("Data", "SaveGame.json");
            Assert.True(File.Exists(path));

            sls.DeleteSave();

            Assert.False(File.Exists(path));
        }
    }
}