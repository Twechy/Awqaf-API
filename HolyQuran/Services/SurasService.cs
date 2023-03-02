using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolyQuran.Models;
using HolyQuran.Vms;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HolyQuran.Services
{
    public interface ISurasService
    {
        Task<List<SurahNamesVm>> GetSurahNames(Rawy rawy);

        Task<List<RawySurahVm>> RawySuras();

        Task<SurahVm> Surah(string surahNameEn, Rawy rawy = Rawy.Hafs);

        Task<SurahVm> Surah(int surahId, Rawy rawy = Rawy.Hafs);

        Task<SurahVm> SurahByOrder(int order, Rawy rawy = Rawy.Hafs);

        Task<List<SurahVm>> Suras(Rawy rawy);

        Task AddSurahToDb(int surahOrder = 0);

        Task<List<Ayah>> AddAyaToDb(List<AyahDir> ayat, int surahId);

        Task<(bool existed, SorahDir surahDir)> AddSurah(SorahDir surahDir, Sorah surah);
    }

    public class SurasService : ISurasService
    {
        private readonly QuranDb _quranDb;
        private readonly IStringParserService _fileParserService;
        private readonly IReadingService _readingService;

        public SurasService(QuranDb quranDb, IStringParserService surahService, IReadingService readingService)
        {
            _quranDb = quranDb;
            _fileParserService = surahService;
            _readingService = readingService;
        }

        public async Task<List<SurahNamesVm>> GetSurahNames(Rawy rawy)
        {
            var names = await _quranDb.Suras.Include(x => x.Ayah).ToListAsync();

            var ayat = await _quranDb.Ayat
                .Include(x => x.RawyText)
                .Where(x => x.RawyText.Rawy == rawy)
                .ToListAsync();

            return names.Select(x => new SurahNamesVm
            {
                Id = x.Id,
                Name = x.HolySurahNameAr,
                Order = x.Order,
                AyaCount = ayat.Count(m => m.SorahId == x.Id),
                Description = x.Description,
                Landing = x.Landing
            }).OrderBy(x => x.Order).ToList();
        }

        public async Task<List<RawySurahVm>> RawySuras()
        {
            var soras = new List<RawySurahVm>();

            var sorasList = await _quranDb.Suras.Select(x => x.Id).ToListAsync();

            foreach (var id in sorasList) soras.Add(await RawySurah(id));

            return soras;
        }

        private async Task<RawySurahVm> RawySurah(int surahId)
        {
            var surah = await Surah(surahId);

            return new RawySurahVm
            {
                Id = surah.Id,
                Name = surah.HolySurahNameAr,
                Order = surah.Order,
                Landing = surah.Landing,
                Description = surah.Description,
                QuranRewat = new Dictionary<Rawy, List<AyahVm>>
                {
                    {Rawy.Qalon, surah.Ayah.Where(x => x.RawyText.Rawy == Rawy.Qalon).ToList()},
                    {Rawy.Hafs, surah.Ayah.Where(x => x.RawyText.Rawy == Rawy.Hafs).ToList()},
                    {Rawy.Wersh, surah.Ayah.Where(x => x.RawyText.Rawy == Rawy.Wersh).ToList()},
                    {Rawy.AlBozy, surah.Ayah.Where(x => x.RawyText.Rawy == Rawy.AlBozy).ToList()},
                }
            };
        }

        public async Task<SurahVm> Surah(string surahNameEn, Rawy rawy = Rawy.Hafs)
        {
            var surah = await _quranDb.Suras.FirstOrDefaultAsync(x => x.HolySurahNameEn == surahNameEn);

            if (surah != null) return await Surah(surah.Id, rawy);

            return null;
        }

        private async Task<SurahVm> Surah(int surahId)
        {
            var surah = await _quranDb.Suras
                .Include(x => x.Ayah)
                .FirstOrDefaultAsync(x => x.Id == surahId);

            var readers = await _quranDb.Readers.ToListAsync();

            var ayat = await _quranDb.Ayat
                .Include(x => x.RawyText)
                .Include(x => x.Readings)
                .Where(x => x.SorahId == surahId)
                .ToListAsync();

            foreach (var aya in ayat.Where(x => x.Readings != null && x.Readings.Count >= 1))
            {
                var collection = await _quranDb.Reading
                    .Where(x => x.AyahId == aya.Id && x.AyaNumber == aya.RawyText.AyaNumber)
                    .ToListAsync();

                if (collection.Count >= 1)
                    ayat.FirstOrDefault(x => x.RawyText.AyahId == aya.Id)?.Readings.AddRange(collection);
            }

            return new SurahVm
            {
                Id = surah.Id,
                HolySurahNameAr = surah.HolySurahNameAr,
                HolySurahNameEn = surah.HolySurahNameEn,
                Landing = surah.Landing == PlaceOfLanding.Maka ? "مكية" : "مدنية",
                Description = surah.Description,
                Order = surah.Order,
                Ayah = ayat.Select(x => new AyahVm
                {
                    Id = x.Id,
                    AyaNumber = x.RawyText.AyaNumber,
                    SorahId = x.SorahId,
                    RawyText = x.RawyText,
                    Readings = x.Readings.Distinct().Select(reader => new Vms.ReadingVm
                    {
                        Id = reader.Id,
                        AyahId = reader.AyahId,
                        AyaNumber = reader.AyaNumber,
                        HolyRead = reader.HolyRead,
                        ReadView = reader.ReadView,
                        ReadInfo = reader.ReadInfo,
                        AgreedOn = reader.AgreedOn,
                        Readers = ExtractReader(reader, readers)
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<SurahVm> Surah(int surahId, Rawy rawy = Rawy.Hafs)
        {
            var surah = await _quranDb.Suras.FirstOrDefaultAsync(x => x.Id == surahId);

            var readers = await _quranDb.Readers.ToListAsync();

            var ayat = await _quranDb.Ayat
                .Include(x => x.RawyText)
                .Include(x => x.Readings)
                .Where(x => x.SorahId == surahId && x.RawyText.Rawy == rawy)
                .ToListAsync();

            var ayas = ayat.Where(x => x.Readings != null && x.Readings.Count >= 1).ToList();

            foreach (var aya in ayas)
            {
                var collection = await _quranDb.Reading
                    .Where(x => x.AyahId == aya.Id && x.AyaNumber == aya.RawyText.AyaNumber).ToListAsync();

                if (collection.Count >= 1)
                    ayat.FirstOrDefault(x => x.RawyText.AyahId == aya.Id)?.Readings.AddRange(collection);
            }

            var s = new SurahVm
            {
                Id = surah.Id,
                HolySurahNameAr = surah.HolySurahNameAr,
                HolySurahNameEn = surah.HolySurahNameEn,
                Order = surah.Order,
                Description = surah.Description,
                Landing = surah.Landing == PlaceOfLanding.Maka ? "مكية" : "مدنية",
                Ayah = ayat.Select(x => new AyahVm
                {
                    Id = x.Id,
                    AyaNumber = x.RawyText.AyaNumber,
                    SorahId = x.SorahId,
                    RawyText = x.RawyText,
                    Readings = x.Readings.Distinct().Select(reader => new Vms.ReadingVm
                    {
                        Id = reader.Id,
                        AyahId = reader.AyahId,
                        AyaNumber = reader.AyaNumber,
                        HolyRead = reader.HolyRead,
                        ReadView = reader.ReadView,
                        ReadInfo = reader.ReadInfo,
                        AgreedOn = reader.AgreedOn,
                        Readers = ExtractReader(reader, readers)
                    }).ToList()
                }).ToList()
            };

            return s;
        }

        private static string ExtractReader(HolyReadingModel reader, List<Reader> readers)
        {
            var enumerable = GetReadersFlags(reader.Reader).ToList();

            var lists = enumerable.Select(x => readers.FirstOrDefault(r => r.Read == x)?.NameView).ToList();

            if (!reader.AgreedOn)
                return string.Join(" ,", lists);

            var readersFromEum = ModifyReaders(ReadersService.ListOfReadersFromEnum());

            foreach (var item in enumerable)
            {
                readersFromEum.Remove(item);
            }

            var displayList = readersFromEum.Select(x => readers.FirstOrDefault(r => r.Read == x)?.NameView).ToList();

            var v = $"القراء العشرة عدا: {string.Join(" ", displayList)}";
            return v;
        }

        private static List<Read> ModifyReaders(List<Read> readsList)
        {
            var unNeededReaders = new List<Read>
            {
                Read.AlSosy,
                Read.Sho3ba,
                Read.AlDory,
                Read.Qonbol,
                Read.Hesham,
                Read.Hafs,
                Read.Royaes,
                Read.Qalon,
                Read.Wersh,
                Read.AlBozy,
                Read.AbnDakwan,
                Read.Kalad,
                Read.Laith,
                Read.AbnWardan,
                Read.AbnJamaz,
                Read.Rouh,
                Read.Ashak,
                Read.Adress
            };

            unNeededReaders.ForEach(x => readsList.Remove(x));

            return readsList;
        }

        private static IEnumerable<Read> GetReadersFlags(Read input) =>
            Enum.GetValues(input.GetType()).Cast<Read>()
                .Where(value => Convert.ToInt32(value) != 0 && input.HasFlag(value));

        public async Task<List<SurahVm>> Suras(Rawy rawy)
        {
            var list = new List<SurahVm>();

            var suras = await _quranDb.Suras
                .Include(x => x.Ayah)
                .ThenInclude(x => x.Readings)
                .Include(x => x.Ayah)
                .ThenInclude(x => x.RawyText)
                .ToListAsync();

            foreach (var item in suras)
                list.Add(await Surah(item.Id, rawy));

            return list.OrderBy(x => x.Order).ToList();
        }

        public async Task AddSurahToDb(int surahOrder = 0)
        {
            var surahs = (await _fileParserService.ParseSurahFromDir(surahOrder)).OrderBy(x => x.Order).ToList();

            foreach (var surah in surahs)
            {
                var (surahExist, sorah) = await AddSurahToDb(new Sorah
                {
                    HolySurahNameAr = surah.HolySurahNameAr,
                    HolySurahNameEn = surah.HolySurahNameEn,
                    Order = surah.Order,
                    Description = surah.Description,
                    Landing = surah.Landing
                });

                if (surahExist) continue;
                await AddAyaToDb(surah.Ayah, sorah.Id);

                var reads = _readingService.GetReadingList(surah);

                foreach (var read in reads)
                    await _readingService.ModifyReading(new HolyReadingModel
                    {
                        HolyRead = read.HolyRead.Trim(),
                        ReadView = read.ReadView.Trim(),
                        AyaNumber = read.AyaNumber,
                        AyahId = read.AyaId,
                    }, read.Reader);
            }
        }

        public async Task<List<Ayah>> AddAyaToDb(List<AyahDir> ayat, int surahId)
        {
            var ayatList = new List<Ayah>();
            var ayaToRemove = new List<AyahDir>();

            ayat.ForEach(x =>
            {
                if (string.IsNullOrEmpty(x.HolyText.Text))
                    ayaToRemove.Add(x);
            });

            ayaToRemove.ForEach(x => ayat.Remove(x));

            foreach (var aya in ayat.Where(aya => !string.IsNullOrEmpty(aya.HolyText.Text)))
            {
                aya.HolyText.SurahId = surahId;
                aya.HolyText.Text = aya.HolyText.Text.Trim();

                var entity = new Ayah { SorahId = surahId, RawyText = aya.HolyText };

                await _quranDb.Ayat.AddAsync(entity);
                await _quranDb.SaveChangesAsync();

                ayatList.Add(entity);
            }

            return ayatList;
        }

        public async Task<(bool surahExist, Sorah surah)> AddSurahToDb(Sorah surah)
        {
            var surahExist = await _quranDb.Suras.AnyAsync(x => x.Order == surah.Order);
            if (surahExist) return (true, surah);

            await _quranDb.Suras.AddAsync(surah);
            await _quranDb.SaveChangesAsync();
            return (false, surah);
        }

        public async Task<(bool existed, SorahDir surahDir)> AddSurah(SorahDir surahDir, Sorah surah)
        {
            if (await _quranDb.RawyText.AnyAsync(x => x.Rawy == surah.Ayah.FirstOrDefault().RawyText.Rawy && x.SurahId == surah.Id))
                return (true, surahDir);

            await _quranDb.Suras.AddAsync(surah);
            await _quranDb.SaveChangesAsync();

            return (false, surahDir);
        }

        public async Task<SurahVm> SurahByOrder(int order, Rawy rawy = Rawy.Hafs)
        {
            var surah = await _quranDb.Suras.FirstOrDefaultAsync(x => x.Order == order);

            var readers = await _quranDb.Readers.ToListAsync();

            var ayat = await _quranDb.Ayat
                .Include(x => x.RawyText)
                .Include(x => x.Readings)
                .Where(x => x.RawyText.Rawy == rawy)
                .ToListAsync();

            var ayas = ayat.Where(x => x.Readings != null && x.Readings.Count >= 1).ToList();

            foreach (var aya in ayas)
            {
                var collection = await _quranDb.Reading
                    .Where(x => x.AyahId == aya.Id && x.AyaNumber == aya.RawyText.AyaNumber).ToListAsync();

                if (collection.Count >= 1)
                    ayat.FirstOrDefault(x => x.RawyText.AyahId == aya.Id)?.Readings.AddRange(collection);
            }

            var s = new SurahVm
            {
                Id = surah.Id,
                HolySurahNameAr = surah.HolySurahNameAr,
                HolySurahNameEn = surah.HolySurahNameEn,
                Order = surah.Order,
                Description = surah.Description,
                Landing = surah.Landing == PlaceOfLanding.Maka ? "مكية" : "مدنية",
                Ayah = ayat.Select(x => new AyahVm
                {
                    Id = x.Id,
                    AyaNumber = x.RawyText.AyaNumber,
                    SorahId = x.SorahId,
                    RawyText = x.RawyText,
                    Readings = x.Readings.Distinct().Select(reader => new Vms.ReadingVm
                    {
                        Id = reader.Id,
                        AyahId = reader.AyahId,
                        AyaNumber = reader.AyaNumber,
                        HolyRead = reader.HolyRead,
                        ReadView = reader.ReadView,
                        ReadInfo = reader.ReadInfo,
                        AgreedOn = reader.AgreedOn,
                        Readers = ExtractReader(reader, readers)
                    }).ToList()
                }).ToList()
            };

            return s;
        }
    }
}