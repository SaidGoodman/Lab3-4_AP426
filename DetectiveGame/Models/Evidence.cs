using System;

namespace DetectiveGame.Models
{
    public class Evidence
    {
        public string Description { get; set; }
        public string Clue { get; set; }
        public string Details { get; set; } // Подсказки

        public Evidence() { }

        public Evidence(string description, string clue, string details = null)
        {
            Description = description;
            Clue = clue;
            Details = details ?? "";
        }

        public override string ToString()
        {
            return $"{Description}: {Clue}";
        }
    }
}
