using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolyQuran.Models;

namespace HolyQuran.Services
{
    public interface IReadingService
    {
        Task ModifyReading(HolyReadingModel read, string reader = "");

        IEnumerable<ReadingVm> GetReadingList(SorahDir surah);
    }

    public class ReadingService : IReadingService
    {
        private readonly QuranDb _quranDb;

        public ReadingService(QuranDb quranDb)
        {
            _quranDb = quranDb;
        }

        public async Task ModifyReading(HolyReadingModel read, string reader = "")
        {
            const string readerExcept = "القرَّاء العشرة ما عدا:";

            if (!reader.Contains(readerExcept))
            {
                var readerSplit = reader.Split("،");
                if (readerSplit.Length == 1)
                {
                    await AddReading(read, ParseReaderFromString(reader));
                }
                else
                {
                    var readAttributes = readerSplit
                        .Select(item => item.Trim().StartsWith('و') ? item.Trim().Remove(0, 1) : item)
                        .Select(ParseReaderFromString).Aggregate<Read, Read>(default,
                            (current, parsedReader) => current | parsedReader);

                    await AddReading(read, readAttributes);
                }
            }
            else
            {
                var readerExc = reader.Split(":")[1];
                var exceptSplit = readerExc.Split("،");

                Read readAttributes = default;

                if (exceptSplit.Length == 1)
                {
                    GetRemainingReaders(new List<Read> { ParseReaderFromString(readerExc) })
                        .ForEach(x => readAttributes |= x);

                    await AddReading(read, readAttributes, true);
                }
                else
                {
                    var readList = exceptSplit
                        .Select(item => item.Trim().StartsWith('و') ? item.Trim().Remove(0, 1) : item)
                        .Select(ParseReaderFromString).ToList();

                    GetRemainingReaders(readList).ForEach(x => readAttributes |= x);

                    await AddReading(read, readAttributes, true);
                }
            }
        }

        private static List<Read> GetRemainingReaders(IEnumerable<Read> reads)
        {
            var readsList = ListOfReadersFromEnum();

            foreach (var item in reads)
                readsList.Remove(item);

            var readers = ModifyReaders(readsList);

            return readers;
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

        private static List<Read> ListOfReadersFromEnum()
        {
            var vs = Enum.GetNames(typeof(Read)).ToList();

            vs.Remove("None");

            return vs.Select(Enum.Parse<Read>).ToList();
        }

        private async Task AddReading(HolyReadingModel reading, Read reader, bool agreedOn = false)
        {
            reading.AgreedOn = agreedOn;
            reading.Reader = reader;

            await _quranDb.Reading.AddAsync(reading);
            await _quranDb.SaveChangesAsync();
        }

        private static Read ParseReaderFromString(string reader)
        {
            if (reader.Contains("أبو جعفر"))
                return Read.AbuJafer;
            if (reader.Contains("أبو عمرو"))
                return Read.Abu3amro;
            if (reader.Contains("ابن عامر"))
                return Read.Abn3amer;
            if (reader.Contains("يعقوب"))
                return Read.Ya3qob;
            if (reader.Contains("ابن كثير"))
                return Read.AbnKater;
            if (reader.Contains("نافع"))
                return Read.Nafe3;
            if (reader.Contains("الكسائي"))
                return Read.Qesaee;
            if (reader.Contains("عاصم"))
                return Read.Asem;
            if (reader.Contains("خلف عن حمزة") || reader.Contains("خلف"))
                return Read.Kalaf;
            if (reader.Contains("حمزة"))
                return Read.Hamza;

            if (reader.Contains("السوسي"))
                return Read.AlSosy;
            if (reader.Contains("شعبة"))
                return Read.Sho3ba;
            if (reader.Contains("الدوري"))
                return Read.AlDory;
            if (reader.Contains("قنبل"))
                return Read.Qonbol;
            if (reader.Contains("هشام"))
                return Read.Hesham;
            if (reader.Contains("حفص"))
                return Read.Hafs;
            if (reader.Contains("رويس"))
                return Read.Royaes;

            return Read.Abn3amer;
        }

        public IEnumerable<ReadingVm> GetReadingList(SorahDir surah)
        {
            var readingList = new List<ReadingVm>();

            foreach (var ayat in from aya in surah.Ayah
                                 where aya.Readings != null
                                 select surah.Ayah.FirstOrDefault(x => x.HolyText.AyahId == aya.HolyText.AyahId)
                into ayat
                                 where ayat?.Readings != null
                                 select ayat)
            {
                readingList.AddRange(ayat.Readings.Select(read => new ReadingVm
                {
                    AyaNumber = read.AyaNumber,
                    HolyRead = read.HolyRead,
                    Reader = read.Reader,
                    ReadView = read.ReadView,
                    AyaId = ayat.HolyText.AyahId
                }));
            }

            return readingList;
        }
    }
}