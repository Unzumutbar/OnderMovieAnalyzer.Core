using OnderMovieAnalyzer.Common.Interface;
using OnderMovieAnalyzer.Common.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnderMovieAnalyzer.Simple
{
    class DummyDatabase : IOnderDatabase
    {
        public void AddMovie(Movie movieToAdd)
        {
        }

        public void Connect(object connection)
        {
        }

        public void DeleteMovie(Movie movieToDelete)
        {
        }

        public List<Movie> ReadMovieList()
        {
            return new List<Movie>();
        }
    }
}
