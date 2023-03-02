using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HolyQuran.Models;
using Microsoft.EntityFrameworkCore;
using MittDevQA.Utils;
using Utils.Others;

namespace HolyQuran.Services
{
    public sealed class QuranSearcher
    {
        private readonly QuranCacher _quranCacher;

        public QuranSearcher(QuranCacher quranCacher)
        {
            _quranCacher = quranCacher;
        }

        public async Task<SearchedModel> Search(List<string> searchTerms, Rawy rawy = Rawy.Hafs, bool fromCache = true)
        {
            var searchTermsStack = new Dictionary<string, List<AyaModel>>();
            var ayaModels = new List<AyaModel>();
            var mostRepeatedSurch = new List<OccurrencesOrder>();
            var numberOfSurahOccured = new List<(string surahName, int surahId)>();

            try
            {
                if (searchTerms.Count == 1)
                {
                    var searchTerm = searchTerms.FirstOrDefault();

                    if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length <= 2) return new SearchedModel();

                    ayaModels = GetAyaModels(searchTerm,
                        fromCache ? await _quranCacher.CachedAyat(rawy) : await _quranCacher.Ayat(rawy));
                }
                else
                {
                    foreach (var searchTerm in searchTerms)
                    {
                        if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length <= 2) return new SearchedModel();

                        var ayaModel = GetAyaModels(searchTerm, fromCache ? await _quranCacher.CachedAyat(rawy) : await _quranCacher.Ayat(rawy));

                        searchTermsStack.Add(searchTerm, ayaModel);
                    }

                    foreach (var aya in searchTermsStack.SelectMany(searchStack => searchStack.Value))
                    {
                        if (ayaModels.FirstOrDefault(x => x.AyaId == aya.AyaId) != null) continue;
                        ayaModels.Add(aya);
                    }
                }

                numberOfSurahOccured.AddRange(ayaModels.Select(aya => (aya.SurahName, aya.SurahId)));

                var orderedEnumerable = numberOfSurahOccured
                    .GroupBy(s => s)
                    .OrderByDescending(s => s.Count())
                    .ThenBy(x => x.Key.surahName).ToList();

                HandleParsedAyat(numberOfSurahOccured, mostRepeatedSurch, orderedEnumerable,
                    orderedEnumerable.Count >= 6 ? 5 : orderedEnumerable.Count);

                return new SearchedModel
                {
                    SearcheAyat = ayaModels,
                    NumberOfSurahOccured = numberOfSurahOccured.Distinct().Count(),
                    OccurrencesOrder = mostRepeatedSurch,
                };
            }
            catch (Exception)
            {
                return new SearchedModel
                {
                    SearcheAyat = new List<AyaModel>(),
                    NumberOfSurahOccured = 0,
                    OccurrencesOrder = new List<OccurrencesOrder>()
                };
            }
        }

        private static void HandleParsedAyat(List<(string surahName, int surahId)> numberOfSurahOccured,
            List<OccurrencesOrder> mostRepeatedSurch,
            List<IGrouping<(string surahName, int surahId), (string surahName, int surahId)>> listSurchOrdered,
            int surahCount)
        {
            for (var i = 0; i < surahCount; ++i)
            {
                mostRepeatedSurch.Add(new OccurrencesOrder
                {
                    SurahName = listSurchOrdered[i].Key.surahName,
                    SurahId = listSurchOrdered[i].Key.surahId,
                    Repeated = numberOfSurahOccured.Where(x => x.surahName == listSurchOrdered[i].Key.surahName).ToList().Count
                });
            }
        }

        private List<AyaModel> GetAyaModels(string searchTerm, List<Ayah> ayat) =>
            ayat.Select(x => ConvertAyaToSearchModel(x, _quranCacher))
                .Where(x => x.AyaSearchText.Contains(searchTerm.Normalizer()) &&
                            x.AyaSearchText != "بِسْمِ اللَّهِ الرَّحْمَنِ الرَّحِيمِ".Normalizer())
                .Select(ConvertSearchToAyaModel)
                .ToList();

        private static AyaSearchModel ConvertAyaToSearchModel(Ayah ayah, QuranCacher quranCacher)
        {
            var surah = quranCacher.GetSurah(ayah.SorahId);
            return new AyaSearchModel
            {
                Rawy = ayah.RawyText.Rawy,
                AyaId = ayah.RawyText.AyahId,
                AyaText = ayah.RawyText.Text,
                AyaSearchText = ayah.RawyText.Text.Normalizer(),
                AyaNumber = ayah.RawyText.AyaNumber,
                SurahId = ayah.SorahId,
                SurahName = surah.HolySurahNameAr,
                SurahOrder = surah.Order
            };
        }

        private static AyaModel ConvertSearchToAyaModel(AyaSearchModel searchModel) => new()
        {
            Rawy = searchModel.Rawy,
            AyaId = searchModel.AyaId,
            AyaText = searchModel.AyaText,
            AyaNumber = searchModel.AyaNumber,
            SurahId = searchModel.SurahId,
            SurahName = searchModel.SurahName,
            SurahOrder = searchModel.SurahOrder,
            WordCount = GetWordCount(searchModel.AyaSearchText),
            LetterCount = searchModel.AyaSearchText.ToCharArray().Length
        };

        private static int GetWordCount(string text)
        {
            int wordCount = 0, index = 0;

            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;

            while (index < text.Length)
            {
                while (index < text.Length && !char.IsWhiteSpace(text[index]))
                    index++;

                wordCount++;

                while (index < text.Length && char.IsWhiteSpace(text[index]))
                    index++;
            }

            return wordCount;
        }
    }

    public class QuranCacher
    {
        private const string Key = "QURAN_CACHE_KEY";
        private readonly QuranDb _quranDb;
        private readonly CacheProvider _cacheProvider;

        public QuranCacher(QuranDb quranDb, CacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
            _quranDb = quranDb;
        }

        public async Task<List<Ayah>> CachedAyat(Rawy rawy = Rawy.Hafs)
        {
            var ayat = await _cacheProvider.GetObject<List<Ayah>>(Key);
            if (ayat != null && ayat.Count != 0) return ayat.Where(x => x.RawyText.Rawy == rawy).ToList();

            await AddAyat();
            ayat = await _cacheProvider.GetObject<List<Ayah>>(Key);
            return ayat.Where(x => x.RawyText.Rawy == rawy).ToList();
        }

        public async Task<List<Ayah>> Ayat(Rawy rawy = Rawy.Hafs)
            => await _quranDb.Ayat
                .Include(x => x.RawyText)
                .Where(x => x.RawyText.Rawy == rawy).ToListAsync();

        public async Task AddAyat()
        {
            var ayat = await _quranDb.Ayat.Include(x => x.RawyText).ToListAsync();
            await _cacheProvider.AddToCache(Key, ayat, new TimeSpan(10, 0, 0, 0));
        }

        public Sorah GetSurah(int surahId) => _quranDb.Suras.FirstOrDefault(x => x.Id == surahId);
    }

    public static class QuranNormalizer
    {
        public static string Normalizer(this string text)
        {
            var stringBuilder = new StringBuilder();

            foreach (var c in text.Normalize(NormalizationForm.FormD).Where(c =>
                CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
                switch (c)
                {
                    case 'ٱ':
                        stringBuilder.Append('ا');
                        break;

                    default:
                        stringBuilder.Append(c);
                        break;
                }

            var normalizedText = stringBuilder.ToString().Normalize(NormalizationForm.FormD);

            return Normalize(normalizedText);
        }

        private static string Normalize(string input)
        {
            //Remove honorific sign
            input = input.Replace("\u0610", ""); // ARABIC SIGN SALLALLAHOU ALAYHE WA SALLAM
            input = input.Replace("\u0611", ""); // ARABIC SIGN ALAYHE ASSALLAM
            input = input.Replace("\u0612", ""); // ARABIC SIGN RAHMATULLAH ALAYHE
            input = input.Replace("\u0613", ""); // ARABIC SIGN RADI ALLAHOU ANHU
            input = input.Replace("\u0614", ""); // ARABIC SIGN TAKHALLUS

            //Remove koranic anotation
            input = input.Replace("\u0615", ""); // ARABIC SMALL HIGH TAH
            input = input.Replace("\u0616", ""); // ARABIC SMALL HIGH LIGATURE ALEF WITH LAM WITH YEH
            input = input.Replace("\u0617", ""); // ARABIC SMALL HIGH ZAIN
            input = input.Replace("\u0618", ""); // ARABIC SMALL FATHA
            input = input.Replace("\u0619", ""); // ARABIC SMALL DAMMA
            input = input.Replace("\u061A", ""); // ARABIC SMALL KASRA
            input = input.Replace("\u06D6", ""); // ARABIC SMALL HIGH LIGATURE SAD WITH LAM WITH ALEF MAKSURA
            input = input.Replace("\u06D7", ""); // ARABIC SMALL HIGH LIGATURE QAF WITH LAM WITH ALEF MAKSURA
            input = input.Replace("\u06D8", ""); // ARABIC SMALL HIGH MEEM INITIAL FORM
            input = input.Replace("\u06D9", ""); // ARABIC SMALL HIGH LAM ALEF
            input = input.Replace("\u06DA", ""); // ARABIC SMALL HIGH JEEM
            input = input.Replace("\u06DB", ""); // ARABIC SMALL HIGH THREE DOTS
            input = input.Replace("\u06DC", ""); // ARABIC SMALL HIGH SEEN
            input = input.Replace("\u06DD", ""); // ARABIC END OF AYAH
            input = input.Replace("\u06DE", ""); // ARABIC START OF RUB EL HIZB
            input = input.Replace("\u06DF", ""); // ARABIC SMALL HIGH ROUNDED ZERO
            input = input.Replace("\u06E0", ""); // ARABIC SMALL HIGH UPRIGHT RECTANGULAR ZERO
            input = input.Replace("\u06E1", ""); // ARABIC SMALL HIGH DOTLESS HEAD OF KHAH
            input = input.Replace("\u06E2", ""); // ARABIC SMALL HIGH MEEM ISOLATED FORM
            input = input.Replace("\u06E3", ""); // ARABIC SMALL LOW SEEN
            input = input.Replace("\u06E4", ""); // ARABIC SMALL HIGH MADDA
            input = input.Replace("\u06E5", ""); // ARABIC SMALL WAW
            input = input.Replace("\u06E6", ""); // ARABIC SMALL YEH
            input = input.Replace("\u06E7", ""); // ARABIC SMALL HIGH YEH
            input = input.Replace("\u06E8", ""); // ARABIC SMALL HIGH NOON
            input = input.Replace("\u06E9", ""); // ARABIC PLACE OF SAJDAH
            input = input.Replace("\u06EA", ""); // ARABIC EMPTY CENTRE LOW STOP
            input = input.Replace("\u06EB", ""); // ARABIC EMPTY CENTRE HIGH STOP
            input = input.Replace("\u06EC", ""); // ARABIC ROUNDED HIGH STOP WITH FILLED CENTRE
            input = input.Replace("\u06ED", ""); // ARABIC SMALL LOW MEEM

            //Remove tatweel
            input = input.Replace("\u0640", "");

            //Remove tashkeel
            input = input.Replace("\u064B", ""); // ARABIC FATHATAN
            input = input.Replace("\u064C", ""); // ARABIC DAMMATAN
            input = input.Replace("\u064D", ""); // ARABIC KASRATAN
            input = input.Replace("\u064E", ""); // ARABIC FATHA
            input = input.Replace("\u064F", ""); // ARABIC DAMMA
            input = input.Replace("\u0650", ""); // ARABIC KASRA
            input = input.Replace("\u0651", ""); // ARABIC SHADDA
            input = input.Replace("\u0652", ""); // ARABIC SUKUN
            input = input.Replace("\u0653", ""); // ARABIC MADDAH ABOVE
            input = input.Replace("\u0654", ""); // ARABIC HAMZA ABOVE
            input = input.Replace("\u0655", ""); // ARABIC HAMZA BELOW
            input = input.Replace("\u0656", ""); // ARABIC SUBSCRIPT ALEF
            input = input.Replace("\u0657", ""); // ARABIC INVERTED DAMMA
            input = input.Replace("\u0658", ""); // ARABIC MARK NOON GHUNNA
            input = input.Replace("\u0659", ""); // ARABIC ZWARAKAY
            input = input.Replace("\u065A", ""); // ARABIC VOWEL SIGN SMALL V ABOVE
            input = input.Replace("\u065B", ""); // ARABIC VOWEL SIGN INVERTED SMALL V ABOVE
            input = input.Replace("\u065C", ""); // ARABIC VOWEL SIGN DOT BELOW
            input = input.Replace("\u065D", ""); // ARABIC REVERSED DAMMA
            input = input.Replace("\u065E", ""); // ARABIC FATHA WITH TWO DOTS
            input = input.Replace("\u065F", ""); // ARABIC WAVY HAMZA BELOW
            input = input.Replace("\u0670", ""); // ARABIC LETTER SUPERSCRIPT ALEF

            //Replace Waw Hamza Above by Waw
            input = input.Replace("\u0624", "\u0648");

            //Replace Ta Marbuta by Ha
            input = input.Replace("\u0629", "\u0647");

            //Replace Ya
            // and Ya Hamza Above by Alif Maksura
            input = input.Replace("\u064A", "\u0649");
            input = input.Replace("\u0626", "\u0649");

            //Replace Alifs with Hamza Above / Below
            // and with Madda Above by Alif
            input = input.Replace("\u0622", "\u0627");
            input = input.Replace("\u0623", "\u0627");
            input = input.Replace("\u0625", "\u0627");

            return input;
        }
    }

    public class SearchedModel
    {
        public int NumberOfSurahOccured { get; set; }
        public List<OccurrencesOrder> OccurrencesOrder { get; set; }
        public List<AyaModel> SearcheAyat { get; set; }
    }

    public class OccurrencesOrder
    {
        public string SurahName { get; set; }
        public int Repeated { get; set; }
        public int SurahId { get; set; }
    }

    public class AyaModel
    {
        public int AyaId { get; set; }
        public int SurahId { get; set; }
        public int SurahOrder { get; set; }
        public string SurahName { get; set; }
        public int AyaNumber { get; set; }
        public Rawy Rawy { get; set; }
        public string AyaText { get; set; }
        public int WordCount { get; set; }
        public int LetterCount { get; set; }
    }

    public class AyaSearchModel : AyaModel
    {
        public string AyaSearchText { get; set; }
    }
}