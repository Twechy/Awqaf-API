using HolyQuran.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyQuran.Services
{
    public interface IManagementSurasService
    {
        Task AddSurah(string surahText, SurahInfoSetting surahInfo, Rawy rawy);

        Task RemoveRewaya(int order, Rawy rawy);

        Task<SurahCounter> SurasCounter();
    }

    public partial class ManagementSurasService : IManagementSurasService
    {
        private readonly IStringParserService _stringParserService;
        private readonly ISurasService _surasService;
        private readonly IReadingService _readingService;
        private readonly QuranDb _quranDb;

        public ManagementSurasService(IStringParserService fileParserService, ISurasService surasService, IReadingService readingService, QuranDb quranDb)
        {
            _stringParserService = fileParserService;
            _surasService = surasService;
            _readingService = readingService;
            _quranDb = quranDb;
        }

        public async Task AddSurah(string surahText, SurahInfoSetting surahInfo, Rawy rawy)
        {
            var (_, surahDir) = await ParseAndAddSuras(surahText, surahInfo, rawy);

            var ayat = surahDir.Ayah.Select(x => new Ayah
            {
                RawyText = x.HolyText,
                Readings = x.Readings?.Select(r => new HolyReadingModel
                {
                    AyaNumber = r.AyaNumber,
                    HolyRead = r.HolyRead,
                    Reader = Enum.Parse<Read>(r.Reader),
                    ReadView = r.ReadView,
                }).ToList(),
                SorahId = x.SorahId
            }).ToList();

            await AddAyat(ayat.Select(x => new AyahDir
            {
                HolyText = x.RawyText,
                Readings = x.Readings.Select(r => new ReadingDir
                {
                    AyaNumber = r.AyaNumber,
                    HolyRead = r.HolyRead,
                    Reader = r.Reader.ToString(),
                    ReadView = r.ReadView,
                }).ToList(),
                SorahId = x.SorahId
            }).ToList());
        }

        public async Task RemoveRewaya(int order, Rawy rawy)
        {
            var surah = await _quranDb.Suras.FirstOrDefaultAsync(x => x.Order == order);

            if (surah is null) return;

            var ayat = await _quranDb.RawyText.Where(x => x.SurahId == surah.Id && x.Rawy == rawy).ToListAsync();

            foreach (var aya in ayat)
            {
                _quranDb.RawyText.Remove(aya);
            }

            await _quranDb.SaveChangesAsync();
        }

        public async Task<SurahCounter> SurasCounter()
        {
            var counter = new List<SurahCounterData>();

            var suras = await _quranDb.Suras.ToListAsync();
            var readers = await _quranDb.Readers.ToListAsync();

            foreach (var surah in suras)
            {
                var readersList = new List<Reader>();

                var hasHafs = await _quranDb.RawyText.Where(x => x.SurahId == surah.Id && x.Rawy == Rawy.Hafs).AnyAsync();
                if (hasHafs)
                {
                    var reader = readers.FirstOrDefault(x => (int)x.Read == (int)Rawy.Hafs);
                    readersList.Add(reader);
                }

                var hasQalon = await _quranDb.RawyText.Where(x => x.SurahId == surah.Id && x.Rawy == Rawy.Qalon).AnyAsync();
                if (hasQalon)
                {
                    var reader = readers.FirstOrDefault(x => (int)x.Read == (int)Rawy.Qalon);
                    readersList.Add(reader);
                }

                var hasWersh = await _quranDb.RawyText.Where(x => x.SurahId == surah.Id && x.Rawy == Rawy.Wersh).AnyAsync();
                if (hasWersh)
                {
                    var reader = readers.FirstOrDefault(x => (int)x.Read == (int)Rawy.Wersh);
                    readersList.Add(reader);
                }

                var hasAlBozy = await _quranDb.RawyText.Where(x => x.SurahId == surah.Id && x.Rawy == Rawy.AlBozy).AnyAsync();
                if (hasAlBozy)
                {
                    var reader = readers.FirstOrDefault(x => (int)x.Read == (int)Rawy.AlBozy);
                    readersList.Add(reader);
                }

                counter.Add(new SurahCounterData
                {
                    ArName = surah.HolySurahNameAr,
                    EnName = surah.HolySurahNameEn,
                    Description = surah.Description,
                    Order = surah.Order,
                    Rawy = readersList
                });
            }

            return new SurahCounter { SurahCounterData = counter.OrderBy(x => x.Order).ToList() };
        }
    }

    public partial class ManagementSurasService
    {
        private async Task AddAyat(List<AyahDir> managementAyat) => await _surasService.AddAyaToDb(managementAyat, managementAyat.FirstOrDefault().SorahId);

        private async Task AddReading(SorahDir sorah)
        {
            var readings = _readingService.GetReadingList(sorah);
            foreach (var read in readings)
            {
                await _readingService.ModifyReading(new HolyReadingModel
                {
                    HolyRead = read.HolyRead.Trim(),
                    ReadView = read.ReadView.Trim(),
                    AyaNumber = read.AyaNumber,
                    AyahId = read.AyaId,
                }, read.Reader);
            }
        }

        private async Task<(bool existed, SorahDir surahDir)> ParseAndAddSuras(string surahTxt, SurahInfoSetting surahInfo, Rawy rawy)
        {
            var surah = await _stringParserService.ConvertStringSurah(_stringParserService.ParseSurahFromText(surahTxt), surahInfo, rawy);

            return await _surasService.AddSurah(surah, new Sorah
            {
                HolySurahNameAr = surah.HolySurahNameAr,
                HolySurahNameEn = surah.HolySurahNameEn,
                Order = surah.Order,
                Description = surah.Description,
                Landing = surah.Landing,
                Ayah = surah.AyatMapper()
            });
        }
    }

    public static class Ext
    {
        public static List<Ayah> AyatMapper(this SorahDir surah) => surah.Ayah.Select(a => new Ayah
        {
            RawyText = a.HolyText,
            Readings = a.Readings.Select(r => new HolyReadingModel
            {
                AyaNumber = r.AyaNumber,
                HolyRead = r.HolyRead,
                Reader = Enum.Parse<Read>(r.Reader),
                ReadView = r.ReadView
            }).ToList(),
        }).ToList();
    }

    public class SurahCounter
    {
        public int SurahCount => SurahCounterData.Count;
        public List<SurahCounterData> SurahCounterData { get; set; }
    }

    public class SurahCounterData
    {
        public int Order { get; set; }
        public string EnName { get; set; }
        public string ArName { get; set; }
        public string Description { get; set; }
        public List<Reader> Rawy { get; set; }
    }
}