using OnderMovieAnalyzer.Common.Enums;
using OnderMovieAnalyzer.Common.Interface;
using OnderMovieAnalyzer.Common.Objects;
using OnderMovieAnalyzer.Common.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OnderMovieAnalyzer.Logic
{
    public class Importer
    {
        public delegate void ProgressUpdate(ImportProgressType progresstype, string description, int value);
        public event ProgressUpdate OnProgressUpdate;

        public MovieCollection ImportMoviesFromDumpFiles(string[] filesToRead, MovieCollection movieCollection)
        {
            var movieList = new List<Movie>();
            foreach (var txtFile in filesToRead)
            {
                if (File.Exists(txtFile) && txtFile.Contains(".txt"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(txtFile);
                    var Encoding = EncodingDetector.DetectTextFileEncoding(txtFile);

                    var myFile = new StreamReader(txtFile, Encoding);
                    var moviesString = myFile.ReadToEnd();
                    myFile.Close();

                    moviesString = moviesString.Replace("last-modified   |ext |size|name|location", string.Empty);
                    var moviesLines = moviesString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    OnProgressUpdate?.Invoke(ImportProgressType.NewFile, fileName, moviesLines.Count());
                    foreach (var line in moviesLines)
                    {
                        if (line != "done" && !string.IsNullOrWhiteSpace(line))
                        {
                            var movie = FileHelper.GetMovieFromLine(line, fileName);
                            if (movie != null)
                                movieCollection.AddMovie(movie);
                        }
                        OnProgressUpdate?.Invoke(ImportProgressType.NewLine, string.Empty, 0);
                    }
                }
            }
            return movieCollection;
        }
    }
}
