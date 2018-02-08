using OnderMovieAnalyzer.Common.Objects;
using OnderMovieAnalyzer.Common.Utilities;
using OnderMovieAnalyzer.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnderMovieAnalyzer.Simple
{
    class Program
    {
        static void Main(string[] args)
        {
            var collection = new MovieCollection(new DummyDatabase());
            collection = new Importer().ImportMoviesFromDumpFiles(Directory.GetFiles(GetDumpDirectory()), collection);
            Console.WriteLine(string.Format("{0} Movies imported", collection.Movies.Count));
            CreateScripts(collection);

        }

        private static void CreateScripts(MovieCollection collection)
        {
            Console.WriteLine("Using default Filter for Duplicates: Name, Episode, Part, Language");
            var duplicates = new DuplicateFinder().FindDuplicates(collection.Movies, _defaultFilter);
            Console.WriteLine(string.Format("{0} duplicates found", duplicates.Count()));
            CreateCopyScriptFile(collection, duplicates);
            CreateDeleteScriptFile(collection, duplicates);
            Console.WriteLine("Script creation finished!...");
            Console.ReadLine();
        }

        private static void CreateCopyScriptFile(MovieCollection collection, IEnumerable<Movie> duplicates)
        {
            var copyScriptLines = new Exporter().GetLines(collection.Movies,
                                              duplicates.ToList(),
                                              new ExportOptions { FileSource = string.Empty, ExportType = Common.Enums.ExportType.CopyScript })
                                              .ToArray();
            Console.WriteLine(string.Format("Found {0} movies to copy", copyScriptLines.Where(l => !String.IsNullOrEmpty(l) && !l.StartsWith("#")).Count()));
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), "output", "OnderMovieCopyScript.txt");
            FileHelper.SaveToFile(outputFile, copyScriptLines);
            Console.WriteLine(string.Format("CopyScript created: {0}", outputFile));
        }

        private static void CreateDeleteScriptFile(MovieCollection collection, IEnumerable<Movie> duplicates)
        {
            var deleteScriptLines = new Exporter().GetLines(collection.Movies,
                                  duplicates.ToList(),
                                  new ExportOptions { FileSource = string.Empty, ExportType = Common.Enums.ExportType.DeletScript })
                                  .ToArray();
            Console.WriteLine(string.Format("Found {0} movies to delete", deleteScriptLines.Where(l => !String.IsNullOrEmpty(l) && !l.StartsWith("#")).Count()));
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), "output", "OnderMovieDeleteScript.txt");
            FileHelper.SaveToFile(outputFile, deleteScriptLines);
            Console.WriteLine(string.Format("DeleteScript created: {0}", outputFile));
        }

        private static string GetDumpDirectory()
        {
            Console.WriteLine("Input directory containing MovieDumps");
            var dumpDirectory = string.Empty;
            while (!Directory.Exists(dumpDirectory))
            {
                dumpDirectory = Console.ReadLine();
                if (!Directory.Exists(dumpDirectory))
                    Console.WriteLine(string.Format("Directory '{0}' does not exist", dumpDirectory));
            }
            return dumpDirectory;
        }

        private static DuplicateFilter _defaultFilter
        {
            get
            {
                return new DuplicateFilter
                {
                    Name = true,
                    Episode = true,
                    Part = true,
                    Language = true
                };
            }
        }
    }
}
