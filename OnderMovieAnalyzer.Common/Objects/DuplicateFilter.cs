using System;
using System.Collections.Generic;
using System.Text;

namespace OnderMovieAnalyzer.Common.Objects
{
    public class DuplicateFilter
    {
        public bool Name { get; set; }
        public bool Part { get; set; }
        public bool Episode { get; set; }
        public bool Language { get; set; }
        public bool Year { get; set; }
        public bool Filesize { get; set; }
        public bool Quality { get; set; }
        public bool DLDistance { get; set; }
        public int NumericUpDownDistance { get; set; }
    }
}
