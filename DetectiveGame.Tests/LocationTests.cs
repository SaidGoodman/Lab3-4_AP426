using DetectiveGame.Models;
using Xunit;

namespace DetectiveGame.Tests
{
    public class LocationTests
    {
        [Fact]
        public void Constructor_InitializesEvidenceList()
        {
            var loc = new Location();

            Assert.NotNull(loc.EvidenceList);
            Assert.Empty(loc.EvidenceList);
        }

        [Fact]
        public void AddEvidence_AddsToEvidenceList()
        {
            var loc = new Location("Ломбард", "Пыль и цепочки");
            var e = new Evidence("Квитанция ломбарда", "Номер сделки");

            loc.AddEvidence(e);

            Assert.Single(loc.EvidenceList);
            Assert.Same(e, loc.EvidenceList[0]);
        }
    }
}