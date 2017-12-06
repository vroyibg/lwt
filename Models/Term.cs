using System;

namespace LWT.Models
{
    public class Term
    {
        public int ID { get; set; }
        public string Content { get; set; }
        public int Level { get; set; }

        public string Meaning { get; set; }

        public Language Language { get; set; }

        public DateTime CreateTime { get; set;}


    }
}