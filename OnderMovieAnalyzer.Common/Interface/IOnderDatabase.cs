using OnderMovieAnalyzer.Common.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnderMovieAnalyzer.Common.Interface
{
    public interface IOnderDatabase
    {
        void Connect(object connection);
        List<Movie> ReadMovieList();
        void AddMovie(Movie movieToAdd);
        void DeleteMovie(Movie movieToDelete);
    }
}
