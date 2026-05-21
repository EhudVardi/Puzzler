using System;

namespace Data.DataModels
{
    public class PuzzleBase
    {
        public string   Type        { get; set; } = "";
        public string   Source      { get; set; } = "";
        public string   Name        { get; set; } = "";
        public DateTime DateCreated { get; set; }
    }
}
