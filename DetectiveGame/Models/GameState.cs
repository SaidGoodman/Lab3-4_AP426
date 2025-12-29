using System.Collections.Generic;

namespace DetectiveGame.Models
{
    public class GameState
    {
        public string CurrentLocationName { get; set; }
        public List<Evidence> CollectedEvidence { get; set; }
        public List<Character> Characters { get; set; }
        public List<Location> Locations { get; set; }

        public GameState()
        {
            CollectedEvidence = new List<Evidence>();
            Characters = new List<Character>();
            Locations = new List<Location>();
        }

        public void AddEvidence(Evidence evidence) => CollectedEvidence.Add(evidence);
        public void AddCharacter(Character character) => Characters.Add(character);
        public void AddLocation(Location location) => Locations.Add(location);
    }
}
