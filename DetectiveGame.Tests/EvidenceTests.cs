using Xunit;
using EvidenceModel = DetectiveGame.Models.Evidence;

namespace DetectiveGame.Tests
{
    public class EvidenceTests
    {
        [Fact]
        public void Ctor_WhenDetailsNull_SetsEmptyString()
        {
            var e = new EvidenceModel("Пятно крови", "На ткани", null);

            Assert.NotNull(e.Details);
            Assert.Equal("", e.Details);
        }

        [Fact]
        public void ToString_ReturnsDescriptionAndClue()
        {
            var e = new EvidenceModel("Квитанция ломбарда", "Сумма и дата");

            Assert.Equal("Квитанция ломбарда: Сумма и дата", e.ToString());
        }
    }
}