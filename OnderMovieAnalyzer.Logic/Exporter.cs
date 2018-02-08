using OnderMovieAnalyzer.Common.Enums;
using OnderMovieAnalyzer.Common.Extensions;
using OnderMovieAnalyzer.Common.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnderMovieAnalyzer.Logic
{
    public class Exporter
    {
        public delegate void ProgressUpdate(string description, int value);
        public event ProgressUpdate OnProgressUpdate;

        public IEnumerable<string> GetLines(List<Movie> movieList, List<Movie> duplicatesList, ExportOptions exportOptions)
        {
            switch (exportOptions.ExportType)
            {
                case ExportType.CopyScript:
                    return GenerateCopyScript(movieList, duplicatesList, exportOptions);
                case ExportType.DeletScript:
                    return GenerateDeleteScript(duplicatesList, exportOptions);
                default: return new string[0].AsEnumerable();
            }
        }

        private IEnumerable<string> GenerateDeleteScript(List<Movie> duplicatesList, ExportOptions exportOptions)
        {
            var moviesToDelete = GetMoviesToDelete(duplicatesList, exportOptions);
            var lastLocation = string.Empty;
            foreach (var movie in moviesToDelete.OrderBy(mov => mov.Location))
            {
                var line = string.Empty;
                if (!lastLocation.Equals(movie.Location))
                {
                    lastLocation = movie.Location;
                    line += string.Format("{1}{1}# {0}", movie.Location, Environment.NewLine);
                }

                line += string.Format("rm {0}", movie.Path);
                yield return line;
            }
        }

        private IEnumerable<Movie> GetMoviesToDelete(List<Movie> duplicatesList, ExportOptions exportOptions)
        {
            var moviesToDelete = duplicatesList.Except(GetMoviesToKeep(duplicatesList, exportOptions));
            return moviesToDelete;
        }

        private IEnumerable<string> GenerateCopyScript(List<Movie> movieList, List<Movie> duplicatesList, ExportOptions exportOptions)
        {
            var moviesToCopy = GetMoviesToKeep(movieList, duplicatesList, exportOptions);
            var lastLocation = string.Empty;
            foreach (var movie in moviesToCopy.OrderBy(mov => mov.Location))
            {
                var line = string.Empty;
                if (!lastLocation.Equals(movie.Location))
                {
                    lastLocation = movie.Location;
                    line += string.Format("{1}{1}# {0}", movie.Location, Environment.NewLine);
                }

                var year = movie.Year;
                if (string.IsNullOrEmpty(year))
                    year = "1900";

                year = year.Substring(0, 3) + "0";
                line = string.Format(@"cp {0} DISK://{1}", movie.Path, year);
                yield return line;
            }
        }

        private IEnumerable<Movie> GetMoviesToKeep(List<Movie> movieList, List<Movie> duplicatesList, ExportOptions exportOptions)
        {
            var moviesToKeep = movieList.Except(duplicatesList).ToList();
            moviesToKeep.AddRange(GetMoviesToKeep(duplicatesList, exportOptions));
            return moviesToKeep;
        }

        private IEnumerable<Movie> GetMoviesToKeep(List<Movie> duplicatesList, ExportOptions exportOptions)
        {
            foreach (var duplicateId in duplicatesList.DistinctBy(mov => mov.DuplicateId).Select(mov => mov.DuplicateId))
            {
                var duplicates = duplicatesList.Where(mov => mov.DuplicateId == duplicateId);
                var bestQuality = duplicates.OrderByDescending(mov => mov.QualityType).FirstOrDefault().QualityType;
                var duplicatesToKeep = duplicates.Where(mov => mov.QualityType == bestQuality);
                var movieToKeep = duplicatesToKeep.FirstOrDefault();
                if (duplicatesToKeep.Any(mov => mov.Location == exportOptions.FileSource))
                    movieToKeep = duplicatesToKeep.FirstOrDefault(mov => mov.Location == exportOptions.FileSource);

                yield return movieToKeep;
            }
        }
    }
}
