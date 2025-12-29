using DetectiveGame.Models;
using Xunit;

namespace DetectiveGame.Tests
{
    public class GameStateTests
    {
        [Fact]
        public void Constructor_InitializesCollections()
        {
            var state = new GameState();

            Assert.NotNull(state.CollectedEvidence);
            Assert.NotNull(state.Characters);
            Assert.NotNull(state.Locations);
            Assert.Empty(state.CollectedEvidence);
            Assert.Empty(state.Characters);
            Assert.Empty(state.Locations);
        }

        [Fact]
        public void AddEvidence_AddsToCollection()
        {
            var state = new GameState();
            var e = new Evidence("Письмо с печатью", "Пахнет духами");

            state.AddEvidence(e);

            Assert.Single(state.CollectedEvidence);
            Assert.Same(e, state.CollectedEvidence[0]);
        }

        [Fact]
        public void AddCharacter_AddsToCollection()
        {
            var state = new GameState();
            var c = new Character("Газетчик", "Свидетель", "Улица");

            state.AddCharacter(c);

            Assert.Single(state.Characters);
            Assert.Same(c, state.Characters[0]);
        }

        [Fact]
        public void AddLocation_AddsToCollection()
        {
            var state = new GameState();
            var l = new Location("Доки", "Туман и вода");

            state.AddLocation(l);

            Assert.Single(state.Locations);
            Assert.Same(l, state.Locations[0]);
        }
    }
}