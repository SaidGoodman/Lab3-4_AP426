using DetectiveGame.Models;
using Xunit;

namespace DetectiveGame.Tests
{
    public class CharacterTests
    {
        [Fact]
        public void Interrogate_IncrementsTimesInterrogated()
        {
            var state = new GameState();
            var ch = new Character("Сержант", "Полиция", "Участок");

            Assert.Equal(0, ch.TimesInterrogated);

            _ = ch.Interrogate(state);
            Assert.Equal(1, ch.TimesInterrogated);

            _ = ch.Interrogate(state);
            Assert.Equal(2, ch.TimesInterrogated);
        }

        [Fact]
        public void Interrogate_Gazetchik_FirstTime_ReturnsIntroLine()
        {
            var state = new GameState();
            var ch = new Character("Газетчик", "Свидетель", "Улица");

            var line = ch.Interrogate(state);

            Assert.Contains("Слыхали?", line);
        }

        [Fact]
        public void Interrogate_Gazetchik_WithBloodEvidenceAfterFirst_ReturnsBloodLine()
        {
            var state = new GameState();
            var ch = new Character("Газетчик", "Свидетель", "Улица");

            _ = ch.Interrogate(state); // 1-й раз
            state.AddEvidence(new Evidence("Пятно крови", "Свежее", "Подсказка"));

            var line = ch.Interrogate(state); // 2-й раз

            Assert.Contains("Кровь", line);
            Assert.Contains("заголовок", line);
        }

        [Fact]
        public void Interrogate_DoctorHarrou_WithRecipeEvidenceAfterFirst_ReturnsPaperThreatLine()
        {
            var state = new GameState();
            var ch = new Character("Доктор Харроу", "Врач", "Кабинет доктора", isSuspect: true);

            _ = ch.Interrogate(state); // 1-й раз
            state.AddEvidence(new Evidence("Рецепт с инициалами", "Чернила свежие", "Подсказка"));

            var line = ch.Interrogate(state); // 2-й раз

            Assert.Contains("моих бумагах", line);
            Assert.Contains("приводит к крови", line);
        }
    }
}