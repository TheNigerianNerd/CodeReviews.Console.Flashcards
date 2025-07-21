using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flashcards.TheNigerianNerd.Models.DTOs
{
    internal class FlashcardDTO
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string StackName { get; set; }
    }
}
