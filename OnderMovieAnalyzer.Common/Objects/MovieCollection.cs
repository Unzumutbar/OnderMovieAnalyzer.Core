using OnderMovieAnalyzer.Common.Interface;
using OnderMovieAnalyzer.Common.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace OnderMovieAnalyzer.Common.Objects
{
    public class MovieCollection
    {
        private List<Movie> _movies = new List<Movie>();
        private IOnderDatabase _database;

        public MovieCollection(IOnderDatabase database)
        {
            _database = database;
            _movies = GetRefreshedList();
        }

        public List<Movie> Movies
        {
            get { return _movies; }
        }

        public List<Movie> GetList()
        {
            return _movies;
        }

        public List<Movie> GetRefreshedList()
        {
            _movies = _database.ReadMovieList().OrderBy(t => t.Name).ToList();
            return _movies;
        }

        public void AddMovie(Movie movieToAdd)
        {
            if (_movies.Where(p => p.Guid == movieToAdd.Guid ||
                                   (p.Path == movieToAdd.Path && p.Location == movieToAdd.Location))
                                   .FirstOrDefault() != null)
                return;

            _movies.Add(movieToAdd);
            _database.AddMovie(movieToAdd);
        }

        public void DeleteMovie(Movie movieToDelete)
        {
            _movies.Remove(movieToDelete);

            _database.DeleteMovie(movieToDelete);
            FileHelper.DeleteMovie(movieToDelete);
        }
    }
}
