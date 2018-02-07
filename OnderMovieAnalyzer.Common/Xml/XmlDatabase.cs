using OnderMovieAnalyzer.Common.Interface;
using OnderMovieAnalyzer.Common.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace OnderMovieAnalyzer.Common.Xml
{
    public class XmlDatabase : IOnderDatabase
    {
        private string _databaseFile;
        public void Connect(object databaseXmlFile)
        {
            _databaseFile = (string)databaseXmlFile;
            WriteEmptyDatabase();
        }

        private void WriteEmptyDatabase()
        {
            if (File.Exists(_databaseFile))
                File.Delete(_databaseFile);

            using (XmlWriter writer = XmlWriter.Create(_databaseFile))
            {
                writer.WriteStartElement("OnderMovieDatabase");

                writer.WriteStartElement("Movies");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.Flush();
            }
        }

        public List<Movie> ReadMovieList()
        {
            XDocument xdoc = XDocument.Load(_databaseFile);
            return (from _movie in xdoc.Root.Element("Movies").Elements("Movie")
                    select new Movie
                    {
                        Guid = _movie.Element("Guid").Value,

                        Name = _movie.Element("Name").Value,
                        Series = _movie.Element("Series").Value,
                        Episode = _movie.Element("Episode").Value,
                        LanguageOriginal = _movie.Element("LanguageOriginal").Value,
                        LanguageDub = _movie.Element("LanguageDub").Value,
                        Quality = _movie.Element("Quality").Value,
                        Source = _movie.Element("Source").Value,
                        TvStation = _movie.Element("TvStation").Value,
                        Year = _movie.Element("Year").Value,

                        Location = _movie.Element("Location").Value,
                        Path = _movie.Element("Path").Value,
                        Size = Int64.Parse(_movie.Element("Size").Value),
                        LastModified = DateTime.ParseExact(_movie.Element("LastModified").Value, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)

                    }).ToList();
        }

        public void AddMovie(Movie movieToAdd)
        {
            XDocument doc = XDocument.Load(_databaseFile);

            doc.Root.Element("Movies").Add(
                 new XElement("Movie",
                        new XElement("Guid", movieToAdd.Guid),

                        new XElement("Name", movieToAdd.Name),
                        new XElement("Series", movieToAdd.Series),
                        new XElement("Episode", movieToAdd.Episode),
                        new XElement("LanguageOriginal", movieToAdd.LanguageOriginal),
                        new XElement("LanguageDub", movieToAdd.LanguageDub),
                        new XElement("Quality", movieToAdd.Quality),
                        new XElement("Source", movieToAdd.Source),
                        new XElement("TvStation", movieToAdd.TvStation),
                        new XElement("Year", movieToAdd.Year),

                        new XElement("Location", movieToAdd.Location),
                        new XElement("Path", movieToAdd.Path),
                        new XElement("Size", movieToAdd.Size),
                        new XElement("LastModified", movieToAdd.LastModified.ToString("yyyy-MM-dd HH:mm:ss"))
                        )
                 );

            doc.Save(_databaseFile);
        }

        public void DeleteMovie(Movie movieToDelete)
        {
            XDocument doc = XDocument.Load(_databaseFile);

            doc.Root.Element("Movies").Elements("Movie").Where(e => e.Element("Guid").Value.Equals(movieToDelete.Guid)).Select(e => e).Single().Remove();

            doc.Save(_databaseFile);
        }
    }
}

