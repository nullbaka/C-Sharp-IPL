using System;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Data;
using System.Collections.Generic;
using System.Linq;


namespace IPLProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var matches = ReadMatches();
            var deliveries = ReadDeliveries();
            //GetMatchColumnNames(matches);
            //GetDeliveryColumnNames(deliveries);
            Console.WriteLine("-----Number of matches each season.-----");
            MatchesAllSeasons(matches);
            Console.WriteLine();
            Console.WriteLine("-----Number of wins by teams each season.-----");
            WinsInSeasons(matches);
            Console.WriteLine();
            Console.WriteLine("-----Most economic bowlers of 2015.-----");
            EconomicBowlersOf2015(matches, deliveries);
            Console.WriteLine();
            Console.WriteLine("-----Extra runs by each team in 2016.-----");
            ExtraRunsOf2016(matches, deliveries);
        }

        static DataTable ReadMatches()
        {
            string filePath = Path.GetFullPath(@"IPLDataset/matches.csv");
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                using (var dr = new CsvDataReader(csv))
                {
                    var matches = new DataTable();
                    matches.Load(dr);
                    return matches;
                }
            }
        }

        static DataTable ReadDeliveries()
        {
            string filePath = Path.GetFullPath(@"IPLDataset/deliveries.csv");
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                using (var dr = new CsvDataReader(csv))
                {
                    var deliveries = new DataTable();
                    deliveries.Columns.Add("wide_runs", typeof(int));
                    deliveries.Columns.Add("noball_runs", typeof(int));
                    deliveries.Columns.Add("extra_runs", typeof(int));
                    deliveries.Columns.Add("total_runs", typeof(int));
                    deliveries.Load(dr);
                    return deliveries;
                }
            }
        }

        static void GetMatchColumnNames(DataTable matches)
        {
            foreach (DataColumn column in matches.Columns)
            {
                Console.WriteLine(column.ColumnName);
            }
        }

        static void GetDeliveryColumnNames(DataTable deliveries)
        {
            foreach (DataColumn column in deliveries.Columns)
            {
                Console.WriteLine(column.ColumnName);
            }
        }

        static void MatchesAllSeasons(DataTable matches)
        {
            //var result = matches.AsEnumerable()
            //.GroupBy(r => r.Field<string>("season"))
            //.Select(r => new
            //    {
            //        season = r.Key,
            //        matches = r.Count()
            //    });
            var result2 = from match in matches.AsEnumerable()
                          group match by match.Field<string>("season") into matchesEachSeason
                          orderby matchesEachSeason.Key
                          select new
                          {
                              season = matchesEachSeason.Key,
                              matches = matchesEachSeason.Count()
                          };
            foreach (var item in result2)
            {
                Console.WriteLine($"{item.season}, {item.matches}");
            }
        }

        static void WinsInSeasons(DataTable matches)
        {
            var result = matches.AsEnumerable()
            .GroupBy(r => new { v = r.Field<string>("season"), w = r.Field<string>("winner") })
            .Select(r => new
            {
                season = r.Key.v,
                winner = r.Key.w,
                matches = r.Count()
            });
            foreach (var item in result)
            {
                Console.WriteLine($"{item.season}, {item.winner}, {item.matches}");
            }
        }

        static void ExtraRunsOf2016(DataTable matches, DataTable deliveries)
        {
            var result = from delivery in deliveries.AsEnumerable()
                         join match in matches.AsEnumerable()
                         on delivery.Field<string>("match_id") equals match.Field<string>("Iid")
                         where match.Field<string>("season") == "2016"
                         group delivery by delivery.Field<string>("bowling_team") into teams
                         select new
                         {
                             bowling_team = teams.Key,
                             extra_runs = teams.Sum((r)=> r.Field<int>("extra_runs"))
                         };
            foreach (var item in result)
            {
                Console.WriteLine($"{item.bowling_team}, {item.extra_runs}");
            }
        }

        static void EconomicBowlersOf2015(DataTable matches, DataTable deliveries)
        {
            var result = ((from delivery in deliveries.AsEnumerable()
                         join match in matches.AsEnumerable()
                         on delivery.Field<string>("match_id") equals match.Field<string>("Iid")
                         where match.Field<string>("season") == "2015" && Equals(delivery.Field<string>("is_super_over"), "0")
                         group delivery by delivery.Field<string>("bowler") into bowler
                         select new
                         {
                             bowler = bowler.Key,
                             economy = bowler.Sum(r => r.Field<int>("total_runs")) * 6 / ((float)bowler.Count() - (float)bowler.Count(s => s.Field<int>("wide_runs") == 1 || s.Field<int>("noball_runs") == 1))
                         }).OrderBy(x => x.economy)).Take(10);
            foreach (var item in result)
            {
                Console.WriteLine($"{item.bowler}, {item.economy}");
            }
        }
    }
}
