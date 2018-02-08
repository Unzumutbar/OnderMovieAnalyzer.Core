using OnderMovieAnalyzer.Common.Objects;
using OnderMovieAnalyzer.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnderMovieAnalyzer.Logic
{
    public class DuplicateFinder
    {
        public delegate void ProgressUpdate(int value);
        public event ProgressUpdate OnProgressUpdate;

        public IEnumerable<Movie> FindDuplicates(List<Movie> movieList, DuplicateFilter filter)
        {
            var duplicateList = new List<Movie>();
            int duplicateId = 1;
            int currentMovieIndex = 0;
            foreach (var movie in movieList)
            {
                IEnumerable<Movie> foundDuplicates = movieList.AsEnumerable();
                if (!duplicateList.Any(m => m.Guid == movie.Guid))
                {
                    if (filter.Name)
                        foundDuplicates = foundDuplicates.Where(m => m.Name == movie.Name && m.Guid != movie.Guid);

                    if (filter.Name && filter.Part)
                        foundDuplicates = foundDuplicates.Where(m => m.Name == movie.Name && m.Guid != movie.Guid && movie.Part == m.Part);

                    if (filter.Name && filter.Episode)
                        foundDuplicates = foundDuplicates.Where(m => m.Name == movie.Name && m.Guid != movie.Guid && movie.Episode == m.Episode);

                    if (filter.Language)
                        foundDuplicates = foundDuplicates.Where(m => m.LanguageDub == movie.LanguageDub && m.Guid != movie.Guid);

                    if (filter.Year)
                        foundDuplicates = foundDuplicates.Where(m => m.Year == movie.Year && m.Guid != movie.Guid);

                    if (filter.Filesize)
                        foundDuplicates = foundDuplicates.Where(m => m.Size == movie.Size && m.Guid != movie.Guid);

                    if (filter.Quality)
                        foundDuplicates = foundDuplicates.Where(m => m.Quality == movie.Quality && m.Guid != movie.Guid);

                    if (filter.DLDistance)
                    {
                        var distanceList = new List<Movie>();
                        foreach (var film in foundDuplicates)
                        {
                            if (film.Guid != movie.Guid)
                            {
                                int distance = film.Name.DamerauLevenshteinDistanceTo(movie.Name);
                                if (distance <= filter.NumericUpDownDistance)
                                    distanceList.Add(film);
                            }
                        }
                        foundDuplicates = distanceList.AsEnumerable<Movie>();
                    }

                    if (foundDuplicates.Any())
                    {
                        movie.DuplicateId = duplicateId;
                        duplicateList.Add(movie);
                        foreach (var duplicate in foundDuplicates)
                        {
                            duplicate.DuplicateId = duplicateId;
                            duplicateList.Add(duplicate);
                        }
                        duplicateId++;
                    }
                }
                currentMovieIndex++;
                OnProgressUpdate?.Invoke(currentMovieIndex);
            }
            return duplicateList;
        }
    }
}
