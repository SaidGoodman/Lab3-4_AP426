using System;
using System.Collections.Generic;

namespace DetectiveGame.Models
{
    public class Location
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Evidence> EvidenceList { get; set; }

        public Location()
        {
            EvidenceList = new List<Evidence>();
        }

        public Location(string name, string description)
        {
            Name = name;
            Description = description;
            EvidenceList = new List<Evidence>();
        }

        public void AddEvidence(Evidence evidence)
        {
            EvidenceList.Add(evidence);
        }

        public void DisplayEvidence()
        {
            Console.WriteLine($"Улики в локации {Name}:");
            foreach (var evidence in EvidenceList)
            {
                Console.WriteLine(evidence);
            }
        }
    }
}
