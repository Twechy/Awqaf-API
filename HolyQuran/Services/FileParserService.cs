using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using HolyQuran.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HolyQuran.Services
{
    public interface IStringParserService
    {
        string ParseSurahTextFromFile(int surahOrder,
                                      Rawy rewaya,
                                      char seperator = '(',
                                      string inputFile = "C:\\Users\\a.mesbahi\\Source\\Repos\\HolyQuranApi\\HolyQuran\\Files\\Suras_M\\testTxt.txt");

        string ParseSurahFromText(string surah, char seperator = '(');

        Task<SorahDir> ConvertStringSurah(string surahTxt, SurahInfoSetting surahInfo, Rawy rawyType);

        Task<IEnumerable<SorahDir>> ParseSurahFromDir(int surahOrder = 0);
    }

    public class FileParserService : IStringParserService
    {
        private readonly QuranOptions _options;

        public FileParserService(IOptions<QuranOptions> options)
        {
            _options = options?.Value;
        }

        public string ParseSurahTextFromFile(int surahOrder, Rawy rewaya, char seperator = '(', string inputFile = "C:\\Users\\a.mesbahi\\Source\\Repos\\HolyQuranApi\\HolyQuran\\Files\\Suras_M\\testTxt.txt")
        {
            var outputFile = $"C:\\Users\\a.mesbahi\\Source\\Repos\\HolyQuranApi\\HolyQuran\\Files\\Suras_M\\{surahOrder}\\{rewaya}.txt";
            using var sr = File.OpenText(inputFile);

            var parsedText = ParseSurahText(sr.ReadToEnd(), seperator);

            File.WriteAllText(outputFile, parsedText);

            return parsedText;
        }

        public string ParseSurahFromText(string surah, char seperator = '(')
        {
            var parsedText = ParseSurahText(surah, seperator);

            return parsedText;
        }

        private static string ParseSurahText(string text, char seperator)
        {
            var indexList = new List<int>();
            var sb = new StringBuilder(text);

            for (var i = 0; i < sb.Length; i++)
            {
                var charString = text[i];
                if (!charString.Equals(seperator)) continue;
                var index = text.IndexOf(charString, i);
                indexList.Add(index);
            }

            var looperIndex = 0;
            foreach (var index in indexList)
            {
                var innerIndex = index + looperIndex;
                text = text.ReplaceAt(innerIndex, '_');

                var v = 2;
                var startIndex = index + looperIndex + 1;

                if (text[startIndex + 1].Equals(')'))
                {
                    text = text.Remove(startIndex, v);

                    looperIndex -= v;
                }
                else if (text[startIndex + 2].Equals(')'))
                {
                    v = 3;

                    text = text.Remove(startIndex, v);

                    looperIndex -= v;
                }
                else if (text[startIndex + 3].Equals(')'))
                {
                    v = 4;
                    text = text.Remove(startIndex, v);

                    looperIndex -= v;
                }
            }

            return text;
        }

        public async Task<IEnumerable<SorahDir>> ParseSurahFromDir(int surahOrder = 0)
        {
            var suras = new List<SorahDir>();
            var surasFiles = Directory.GetDirectories(_options.FilesLocation).ToList();

            if (surahOrder >= 1)
                surasFiles = surasFiles
                    .Where(x => int.Parse(x.Split("\\").LastOrDefault() ?? string.Empty) == surahOrder).ToList();

            foreach (var file in surasFiles)
            {
                var files = Directory.GetFiles(file).ToList();

                var settingsPath = files.FirstOrDefault(x => x.Contains("settings.json"));

                var settings = JsonConvert
                    .DeserializeObject<SurahInfoSetting>(await File.ReadAllTextAsync(settingsPath));

                files.Remove(settingsPath);

                var rewaya = new Dictionary<Rawy, string>();

                var stringBuilder = new StringBuilder();
                foreach (var surahTxt in files)
                {
                    stringBuilder.Clear();

                    stringBuilder.Append(await File.ReadAllTextAsync(surahTxt));

                    var rawy = Enum.Parse<Rawy>(Path.GetFileName(surahTxt).Split(".")[0]);

                    rewaya.Add(rawy, stringBuilder.ToString());
                }

                var surah = await ParseSurah(settings, rewaya);
                suras.Add(surah);
            }

            return suras;
        }

        public async Task<SorahDir> ConvertStringSurah(string surahTxt, SurahInfoSetting surahInfo, Rawy rawyType) =>
            await ParseSurah(surahTxt, surahInfo, rawyType);

        private async Task<SorahDir> ParseSurah(SurahInfoSetting surahSettings, Dictionary<Rawy, string> rewayat)
        {
            var surah = Parse(surahSettings, rewayat);

            var hafs = await HafsRead(surahSettings.EnName);
            var qalon = await QalonRead(surahSettings.EnName);
            var wersh = await WershRead(surahSettings.EnName);
            var alBozy = await AlBozyRead(surahSettings.EnName);

            foreach (var aya in surah.Ayah)
            {
                var readings = new List<ReadingDir>();

                readings = aya.HolyText.Rawy switch
                {
                    Rawy.Hafs => hafs,
                    Rawy.Qalon => qalon,
                    Rawy.Wersh => wersh,
                    Rawy.AlBozy => alBozy,
                    _ => readings
                };

                if (readings.Count == 0) continue;

                if (readings.Where(x => x.AyaNumber == aya.HolyText.AyaNumber).ToList().Count == 0) continue;

                var ayat = readings.Where(x => x.AyaNumber == aya.HolyText.AyaNumber).ToList();
                if (ayat.Count >= 1)
                    aya.Readings.AddRange(ayat);
            }

            return surah;
        }

        private async Task<SorahDir> ParseSurah(string rewaya, SurahInfoSetting surahSettings, Rawy rawy)
        {
            var surah = Parse(rewaya, surahSettings, rawy);

            var dta = rawy switch
            {
                Rawy.Hafs => await HafsRead(surahSettings.EnName),
                Rawy.Qalon => await QalonRead(surahSettings.EnName),
                Rawy.Wersh => await WershRead(surahSettings.EnName),
                Rawy.AlBozy => await AlBozyRead(surahSettings.EnName),
                _ => null
            };

            if (dta.Count == 0) return surah;

            foreach (var aya in surah.Ayah)
            {
                var readings = new List<ReadingDir>();

                readings = aya.HolyText.Rawy switch
                {
                    Rawy.Hafs => dta,
                    Rawy.Qalon => dta,
                    Rawy.Wersh => dta,
                    Rawy.AlBozy => dta,
                    _ => null
                };

                if (readings.Count == 0) continue;

                if (readings.Where(x => x.AyaNumber == aya.HolyText.AyaNumber).ToList().Count == 0) continue;

                var ayat = readings.Where(x => x.AyaNumber == aya.HolyText.AyaNumber).ToList();
                if (ayat.Count >= 1)
                    aya.Readings.AddRange(ayat);
            }

            return surah;
        }

        private static SorahDir Parse(string rewaya, SurahInfoSetting surahInfo, Rawy rawy)
        {
            var rewatList = new List<HolyText>();

            var index = 0;
            var ayat = rewaya.Split("_");

            foreach (var aya in ayat)
            {
                if (string.IsNullOrEmpty(aya)) continue;
                rewatList.Add(new HolyText { AyaNumber = index + 1, Text = aya.Trim(), Rawy = rawy });
                index++;
            }

            var ayatList = rewatList.Select(item => new AyahDir { HolyText = item }).ToList();

            return new SorahDir
            {
                HolySurahNameAr = surahInfo.ArName,
                HolySurahNameEn = surahInfo.EnName,
                Order = surahInfo.Order,
                Landing = surahInfo.Landing,
                Description = surahInfo.Description,
                Ayah = ayatList,
            };
        }

        private static SorahDir Parse(SurahInfoSetting surahInfo, Dictionary<Rawy, string> rewayat)
        {
            var rewatList = new List<HolyText>();

            foreach (var rawy in rewayat.Keys)
            {
                var index = 0;
                var ayat = rewayat[rawy].Split("_");

                foreach (var aya in ayat)
                {
                    if (string.IsNullOrEmpty(aya)) continue;
                    rewatList.Add(new HolyText { AyaNumber = index + 1, Text = aya.Trim(), Rawy = rawy });
                    index++;
                }
            }

            var ayatList = rewatList.Select(item => new AyahDir { HolyText = item }).ToList();

            return new SorahDir
            {
                HolySurahNameAr = surahInfo.ArName,
                HolySurahNameEn = surahInfo.EnName,
                Order = surahInfo.Order,
                Landing = surahInfo.Landing,
                Description = surahInfo.Description,
                Ayah = ayatList,
            };
        }

        private async Task<List<ReadingDir>> GetReading(string readingFileName, Rawy rawy) =>
            rawy switch
            {
                Rawy.Hafs => await HafsRead(readingFileName),
                Rawy.Qalon => await QalonRead(readingFileName),
                Rawy.Wersh => await WershRead(readingFileName),
                _ => new List<ReadingDir>()
            };

        private async Task<List<ReadingDir>> HafsRead(string readingFileName) =>
            await ParseHolyRead(readingFileName, Rawy.Hafs);

        private async Task<List<ReadingDir>> QalonRead(string readingFileName) =>
            await ParseHolyRead(readingFileName, Rawy.Qalon);

        private async Task<List<ReadingDir>> WershRead(string readingFileName) =>
            await ParseHolyRead(readingFileName, Rawy.Wersh);

        private async Task<List<ReadingDir>> AlBozyRead(string readingFileName) =>
            await ParseHolyRead(readingFileName, Rawy.AlBozy);

        private async Task<List<ReadingDir>> ParseHolyRead(string readingFileName, Rawy rawy)
        {
            try
            {
                var emptyList = new List<ReadingDir>();
                var file = Directory.GetFiles(_options.SheetsLocation, $"*{rawy}.accdb").FirstOrDefault();

                var connectionString = @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};DBQ= " + file;

                if (!File.Exists(file)) return emptyList;

                var connection = new OdbcConnection(connectionString);
                var lists = await connection.QueryAsync<ReadingVm>("SELECT *  From " + readingFileName);

                return lists.Select(x => new ReadingDir
                {
                    AyaNumber = x.AyaNumber,
                    Reader = x.Reader.Trim(),
                    ReadView = x.ReadView.Trim(),
                    HolyRead = x.HolyRead.Trim(),
                }).ToList();
            }
            catch (Exception)
            {
                return new List<ReadingDir>();
            }
        }
    }

    public class SurahInfoSetting
    {
        public string ArName { get; set; }
        public string EnName { get; set; }
        public PlaceOfLanding Landing { get; set; }
        public int Order { get; set; }
        public string Description { get; set; }
    }

    public enum PlaceOfLanding
    {
        Maka,
        Madena
    }

    public class SorahDir
    {
        public SorahDir()
        {
            Ayah = new List<AyahDir>();
        }

        public int Id { get; set; }
        public int Order { get; set; }
        public string HolySurahNameAr { get; set; }
        public string HolySurahNameEn { get; set; }
        public PlaceOfLanding Landing { get; set; }
        public string Description { get; set; }
        public int AyaCount => Ayah.Count;
        public List<AyahDir> Ayah { get; set; }
    }

    public class AyahDir
    {
        public AyahDir()
        {
            Readings = new List<ReadingDir>();
        }

        public int Id { get; set; }
        public int SorahId { get; set; }
        public HolyText HolyText { get; set; }
        public List<ReadingDir> Readings { get; set; }
    }

    public class ReadingDir
    {
        public int AyaNumber { get; set; }
        public string ReadView { get; set; }
        public string Reader { get; set; }
        public string HolyRead { get; set; }
    }

    public static class Extensions
    {
        public static string ReplaceAt(this string value, int index, char newchar) => value.Length <= index
            ? value
            : string.Concat(value.Select((c, i) => i == index ? newchar : c));
    }
}