using OnderMovieAnalyzer.Common.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace OnderMovieAnalyzer.Common.Utilities
{
    public static class FileHelper
    {
        public static List<Movie> GetMovieListFromFiles(string directory)
        {
            var movieList = new List<Movie>();
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            foreach (var movie in files)
            {
                if (IsVideoFile(movie))
                {
                    var newMovie = GetMovieFromFile(movie);
                    if (newMovie != null)
                        movieList.Add(newMovie);
                }
            }
            return movieList;
        }

        private static string[] mediaExtensions = {
                "MP2", "MP2V",".MP4", "MP4V",
                ".AVI", ".DIVX", ".WMV", "MKV",
                "MPA", "MPE", "MPEG", "MOV"
            };

        private static bool IsVideoFile(string path)
        {
            return -1 != Array.IndexOf(mediaExtensions, Path.GetExtension(path).ToUpperInvariant());
        }

        public static Movie GetMovieFromFile(string fileToConvert)
        {
            try
            {
                var newMovie = new Movie();
                var fileName = Path.GetFileNameWithoutExtension(fileToConvert);
                var directoryName = Path.GetDirectoryName(fileToConvert);
                var detailsName = GetDetails(fileName);
                var detailsDirectory = GetDetails(directoryName);

                newMovie.Guid = Guid.NewGuid().ToString();

                newMovie.Name = GetName(fileName);
                newMovie.Part = ReadPart(detailsName);
                newMovie.Series = ReadSeries(detailsName);
                newMovie.Episode = ReadEpisode(detailsName);
                newMovie.LanguageOriginal = ReadLanguageOriginal(detailsDirectory);
                if (string.IsNullOrWhiteSpace(newMovie.LanguageOriginal))
                    newMovie.LanguageOriginal = ReadLanguageOriginal(detailsName);

                newMovie.LanguageDub = ReadLanguageDub(detailsName);

                if (string.IsNullOrWhiteSpace(newMovie.LanguageDub))
                    newMovie.LanguageDub = newMovie.LanguageOriginal;

                newMovie.Quality = ReadQuality(detailsName);
                newMovie.Source = ReadSource(detailsName);
                newMovie.TvStation = ReadTvStation(detailsName);
                newMovie.Year = ReadYear(detailsDirectory);

                newMovie.Location = System.Environment.MachineName;
                newMovie.Path = fileToConvert;
                newMovie.Size = new FileInfo(fileToConvert).Length;
                newMovie.LastModified = new FileInfo(fileToConvert).LastWriteTime;

                return newMovie;
            }
            catch
            {
                return null;
            }
        }

        private static string ReadEpisode(string details)
        {
            var fullSource = GetTagContent(details, "ep");
            if (string.IsNullOrWhiteSpace(fullSource))
                return string.Empty;

            var tokens = fullSource.Split(new char[] { ',' });
            if (tokens.Length < 1)
                return string.Empty;

            return tokens[0];
        }

        private static string ReadSeries(string details)
        {
            var fullSource = GetTagContent(details, "ep");
            if (string.IsNullOrWhiteSpace(fullSource))
                return string.Empty;

            var tokens = fullSource.Split(new char[] { ',' });
            if (tokens.Length < 2)
                return string.Empty;

            return tokens[1].Replace(",", "");
        }

        private static string ReadTvStation(string details)
        {
            var fullSource = GetTagContent(details, "src");
            if (string.IsNullOrWhiteSpace(fullSource))
                return string.Empty;

            var tokens = fullSource.Split(new char[] { '!' });
            if (tokens.Length < 2)
                return string.Empty;

            return tokens[1].Replace(",", "");
        }

        private static string ReadYear(string details)
        {
            string dateAsString = GetTagContent(details, "date");
            if (dateAsString != string.Empty)
            {
                DateTime dateAsDateTime;
                if (DateTime.TryParse(dateAsString, out dateAsDateTime))
                    return dateAsDateTime.Year.ToString();
            }

            return GetTagContent(details, "year");

        }

        private static string ReadSource(string details)
        {
            var fullSource = GetTagContent(details, "src");
            if (string.IsNullOrWhiteSpace(fullSource))
                return string.Empty;

            var tokens = fullSource.Split(new char[] { '!' });
            if (tokens.Length < 1)
                return string.Empty;

            return tokens[0];
        }

        private static string ReadQuality(string details)
        {
            return GetTagContent(details, "res");
        }

        private static string ReadLanguageDub(string details)
        {
            return GetTagContent(details, "lang");
        }

        private static string ReadLanguageOriginal(string details)
        {
            return GetTagContent(details, "olang");
        }

        private static string ReadPart(string details)
        {
            return GetTagContent(details, "part");
        }

        public static bool DeleteMovie(Movie movieToDelete)
        {
            try
            {

                Thread thread = new Thread(delegate () { DeleteFile(movieToDelete.Path); });
                thread.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void DeleteFile(string fileStringToDelete)
        {
            FileInfo fileToDelete = new FileInfo(fileStringToDelete);
            if (fileToDelete.Exists)
            {
                while (IsFileLocked(fileToDelete))
                    Thread.Sleep(1000);
                fileToDelete.Delete();
            }
        }

        public static bool MovieExists(Movie movieToCheck)
        {
            return false;
        }

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private static string GetDetails(string s)
        {
            if (!s.Contains("["))
                return string.Empty;

            var startTag = "[";
            int startIndex = s.IndexOf(startTag) + startTag.Length;
            int endIndex = s.IndexOf("]", startIndex);
            return s.Substring(startIndex, endIndex - startIndex);
        }

        private static string GetTagContent(string s, string tag)
        {
            if (!s.Contains(tag))
                return string.Empty;

            var startTag = string.Format("{0}=", tag);
            int startIndex = s.IndexOf(startTag) + startTag.Length;
            int endIndex = s.IndexOf(";", startIndex);
            if (endIndex < 0)
                endIndex = s.Length;

            var content = s.Substring(startIndex, endIndex - startIndex);

            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            return content;
        }

        private static string GetName(string s)
        {
            int l = s.IndexOf("[");
            if (l > 0)
                return s.Substring(0, l).Trim();

            return s.Trim();
        }

        internal static List<Movie> GetMovieListFromTexts(string[] txtFiles)
        {
            var movieList = new List<Movie>();

            foreach (var txtFile in txtFiles)
            {
                if (File.Exists(txtFile) && txtFile.Contains(".txt"))
                {
                    var Encoding = EncodingDetector.DetectTextFileEncoding(txtFile);
                    //moviesString += File.ReadAllText(txtFile);
                    var myFile = new StreamReader(txtFile, Encoding);
                    var moviesString = myFile.ReadToEnd();
                    myFile.Close();

                    var fileName = Path.GetFileNameWithoutExtension(txtFile);
                    moviesString = moviesString.Replace("last-modified   |ext |size|name|location", string.Empty);
                    var moviesLines = moviesString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in moviesLines)
                    {
                        if (line != "done" && !string.IsNullOrWhiteSpace(line))
                        {
                            var movie = GetMovieFromLine(line, fileName);
                            if (movie != null)
                                movieList.Add(movie);

                        }
                    }
                }
            }

            return movieList;
        }

        public static Movie GetMovieFromLine(string line, string fileName)
        {
            var lines = line.Split(new string[] { "|" }, StringSplitOptions.None);
            if (lines.Length < 5)
                return null;

            if (-1 == Array.IndexOf(mediaExtensions, lines[1].ToUpperInvariant()))
                return null;

            try
            {
                var newMovie = new Movie();
                var detailsName = GetDetails(lines[3]);
                var detailsDirectory = GetDetails(lines[4]);

                newMovie.Guid = Guid.NewGuid().ToString();

                newMovie.Name = GetName(lines[3]);
                newMovie.Part = ReadPart(detailsName);
                newMovie.Series = ReadSeries(detailsName);
                newMovie.Episode = ReadEpisode(detailsName);
                newMovie.LanguageOriginal = ReadLanguageOriginal(detailsDirectory);
                if (string.IsNullOrWhiteSpace(newMovie.LanguageOriginal))
                    newMovie.LanguageOriginal = ReadLanguageOriginal(detailsName);
                newMovie.LanguageDub = ReadLanguageDub(detailsName);

                if (string.IsNullOrWhiteSpace(newMovie.LanguageDub))
                    newMovie.LanguageDub = newMovie.LanguageOriginal;

                newMovie.Quality = ReadQuality(detailsName);
                newMovie.Source = ReadSource(detailsName);
                newMovie.TvStation = ReadTvStation(detailsName);
                newMovie.Year = ReadYear(detailsDirectory);

                newMovie.Location = fileName;
                newMovie.Path = lines[4] + lines[3] + lines[1];

                newMovie.Size = long.Parse(lines[2]);
                try
                {
                    newMovie.LastModified = DateTime.ParseExact(lines[0], "dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    newMovie.LastModified = DateTime.MinValue;
                }

                return newMovie;
            }
            catch
            {
                return null;
            }
        }

        public static void SaveDuplicatesToFile(List<Movie> duplicates, string file)
        {
            string outputString = "";
            foreach (var movie in duplicates)
            {
                outputString += movie.FullPath + Environment.NewLine;
            }
            using (StreamWriter outfile = new StreamWriter(file))
            {
                outfile.Write(outputString);
            }
        }

        public static void SaveToFile(string file, string[] lines)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            File.WriteAllLines(file, lines);
        }
    }
}
