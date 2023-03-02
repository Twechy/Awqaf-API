using HolyQuran.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyQuran.Services
{
    public class PaginatorService
    {
        private readonly ISurasService _surasService;

        public PaginatorService(ISurasService surasService)
        {
            _surasService = surasService;
        }

        public async Task<List<PagingModel>> Pagination(Rawy rawy)
        {
            var pages = Paginate();
            return await PaginateWithData(pages, rawy);
        }

        private async Task<List<PagingModel>> PaginateWithData(List<PagingModel> pages, Rawy rawy = Rawy.Hafs)
        {
            var suras = await _surasService.RawySuras();

            foreach (var page in pages)
            {
                foreach (var chunk in page.SurahChunks)
                {
                    var surah = suras.FirstOrDefault(x => x.Order == chunk.SurahOrder);
                    if (surah == null) continue;
                    {
                        var rewaya = surah.QuranRewat[rawy];

                        if (chunk.StartedAt == 0)
                        {
                            var aya = rewaya.FirstOrDefault(x => x.AyaNumber == chunk.EndedAt);
                            chunk.Ayah.Add(aya);
                        }
                        else
                        {
                            for (var ayaNumber = chunk.StartedAt; ayaNumber <= chunk.EndedAt; ayaNumber++)
                            {
                                var aya = rewaya.FirstOrDefault(x => x.AyaNumber == ayaNumber);
                                chunk.Ayah.Add(aya);
                            }
                        }
                    }
                }
            }

            return pages;
        }

        #region paginator

        public static List<PagingModel> Paginate()
        {
            return new List<PagingModel> {
                PagingModel.Create(1, new List<SurahChunk> { new SurahChunk

                    {
                        StartedAt =1,
                        EndedAt = 7,
                        SurahOrder =1
                    }}),
            PagingModel.Create(2, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =1,
                        EndedAt = 5,
                        SurahOrder =2
                    }}),
            PagingModel.Create(3, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =6,
                        EndedAt = 16,
                        SurahOrder =2
                    }}),
            PagingModel.Create(4, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =17,
                        EndedAt = 24,
                        SurahOrder =2
                    }}),
            PagingModel.Create(5, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =25,
                        EndedAt = 29,
                        SurahOrder =2
                    }}),
            PagingModel.Create(6, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =30,
                        EndedAt = 37,
                        SurahOrder =2
                    }}),
            PagingModel.Create(7, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =38,
                        EndedAt = 48,
                        SurahOrder =2
                    }}),
            PagingModel.Create(8, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =49,
                        EndedAt = 57,
                        SurahOrder =2
                    }}),
            PagingModel.Create(9, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =58,
                        EndedAt = 61,
                        SurahOrder =2
                    }}),
            PagingModel.Create(10, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =62,
                        EndedAt = 69,
                        SurahOrder =2
                    }}),
            PagingModel.Create(11, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =70,
                        EndedAt = 76,
                        SurahOrder =2
                    }}),
            PagingModel.Create(12, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =77,
                        EndedAt = 83,
                        SurahOrder =2
                    }}),
            PagingModel.Create(13, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =84,
                        EndedAt = 88,
                        SurahOrder =2
                    }}),
            PagingModel.Create(14, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =89,
                        EndedAt = 93,
                        SurahOrder =2
                    }}),
            PagingModel.Create(15, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =94,
                        EndedAt = 101,
                        SurahOrder =2
                    }}),
            PagingModel.Create(16, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =102,
                        EndedAt = 105,
                        SurahOrder =2
                    }}),
            PagingModel.Create(17, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =106,
                        EndedAt = 116,
                        SurahOrder =2
                    }}),
            PagingModel.Create(18, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =113,
                        EndedAt = 119,
                        SurahOrder =2
                    }}),
            PagingModel.Create(19, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =120,
                        EndedAt = 126,
                        SurahOrder =2
                    }}),
            PagingModel.Create(20, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =127,
                        EndedAt = 134,
                        SurahOrder =2
                    }}),
            PagingModel.Create(21, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =135,
                        EndedAt = 141,
                        SurahOrder =2
                    }}),
            PagingModel.Create(22, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =142,
                        EndedAt = 145,
                        SurahOrder =2
                    }}),
            PagingModel.Create(23, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =146,
                        EndedAt = 153,
                        SurahOrder =2
                    }}),
            PagingModel.Create(24, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =154,
                        EndedAt = 163,
                        SurahOrder =2
                    }}),
            PagingModel.Create(25, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =164,
                        EndedAt = 169,
                        SurahOrder =2
                    }}),
            PagingModel.Create(26, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =170,
                        EndedAt = 176,
                        SurahOrder =2
                    }}),
            PagingModel.Create(27, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =177,
                        EndedAt = 181,
                        SurahOrder =2
                    }}),
            PagingModel.Create(28, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =182,
                        EndedAt = 186,
                        SurahOrder =2
                    }}),
            PagingModel.Create(29, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =187,
                        EndedAt = 190,
                        SurahOrder =2
                    }}),
            PagingModel.Create(30, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =191,
                        EndedAt = 196,
                        SurahOrder =2
                    }}),
            PagingModel.Create(31, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =197,
                        EndedAt = 202,
                        SurahOrder =2
                    }}),
            PagingModel.Create(32, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =203,
                        EndedAt = 210,
                        SurahOrder =2
                    }}),
            PagingModel.Create(33, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =211,
                        EndedAt = 215,
                        SurahOrder =2
                    }}),
            PagingModel.Create(34, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =216,
                        EndedAt = 219,
                        SurahOrder =2
                    }}),
            PagingModel.Create(35, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =220,
                        EndedAt = 224,
                        SurahOrder =2
                    }}),
            PagingModel.Create(36, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =225,
                        EndedAt = 230,
                        SurahOrder =2
                    }}),
            PagingModel.Create(37, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =231,
                        EndedAt = 233,
                        SurahOrder =2
                    }}),
            PagingModel.Create(38, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =234,
                        EndedAt = 237,
                        SurahOrder =2
                    }}),
            PagingModel.Create(39, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =238,
                        EndedAt = 245,
                        SurahOrder =2
                    }}),
            PagingModel.Create(40, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =246,
                        EndedAt = 248,
                        SurahOrder =2
                    }}),
            PagingModel.Create(41, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =249,
                        EndedAt = 252,
                        SurahOrder =2
                    }}),
            PagingModel.Create(42, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =253,
                        EndedAt = 256,
                        SurahOrder =2
                    }}),
            PagingModel.Create(43, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =257,
                        EndedAt = 259,
                        SurahOrder =2
                    }}),
            PagingModel.Create(44, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =260,
                        EndedAt = 264,
                        SurahOrder =2
                    }}),
            PagingModel.Create(45, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =265,
                        EndedAt = 269,
                        SurahOrder =2
                    }}),
            PagingModel.Create(46, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =270,
                        EndedAt = 274,
                        SurahOrder =2
                    }}),
            PagingModel.Create(47, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =275,
                        EndedAt = 281,
                        SurahOrder =2
                    }}),
            PagingModel.Create(48, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =0,
                        EndedAt = 282,
                        SurahOrder =2
                    }}),
            PagingModel.Create(49, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =283,
                        EndedAt = 286,
                        SurahOrder =2
                    }}),
            PagingModel.Create(50, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =1,
                        EndedAt = 9,
                        SurahOrder =3
                    }}),
            PagingModel.Create(51, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =10,
                        EndedAt = 15,
                        SurahOrder =3
                    }}),
            PagingModel.Create(52, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =16,
                        EndedAt = 22,
                        SurahOrder =3
                    }}),
            PagingModel.Create(53, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =23,
                        EndedAt = 23,
                        SurahOrder =3
                    }}),
            PagingModel.Create(54, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =30,
                        EndedAt = 37,
                        SurahOrder =3
                    }}),
            PagingModel.Create(55, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =38,
                        EndedAt = 45,
                        SurahOrder =3
                    }}),
            PagingModel.Create(56, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =36,
                        EndedAt = 52,
                        SurahOrder =3
                    }}),
            PagingModel.Create(57, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =53,
                        EndedAt = 61,
                        SurahOrder =3
                    }}),
            PagingModel.Create(58, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =62,
                        EndedAt = 70,
                        SurahOrder =3
                    }}),
            PagingModel.Create(59, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =71,
                        EndedAt = 77,
                        SurahOrder =3
                    }}),
            PagingModel.Create(60, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =78,
                        EndedAt = 83,
                        SurahOrder =3
                    }}),

            PagingModel.Create(61, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =84,
                        EndedAt = 91,
                        SurahOrder =3
                    }}),
            PagingModel.Create(62, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =92,
                        EndedAt = 100,
                        SurahOrder =3
                    }}),
            PagingModel.Create(63, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =101,
                        EndedAt = 108,
                        SurahOrder =3
                    }}),
            PagingModel.Create(64, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =109,
                        EndedAt = 115,
                        SurahOrder =3
                    }}),
            PagingModel.Create(65, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =116,
                        EndedAt = 121,
                        SurahOrder =3
                    }}),
            PagingModel.Create(66, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =122,
                        EndedAt = 132,
                        SurahOrder =3
                    }}),
            PagingModel.Create(67, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =133,
                        EndedAt = 140,
                        SurahOrder =3
                    }}),
            PagingModel.Create(68, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =141,
                        EndedAt = 148,
                        SurahOrder =3
                    }}),
            PagingModel.Create(69, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =149,
                        EndedAt = 153,
                        SurahOrder =3
                    }}),
            PagingModel.Create(70, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =154,
                        EndedAt = 157,
                        SurahOrder =3
                    }}),
            PagingModel.Create(71, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =158,
                        EndedAt = 165,
                        SurahOrder =3
                    }}),
            PagingModel.Create(72, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =166,
                        EndedAt = 173,
                        SurahOrder =3
                    }}),
            PagingModel.Create(73, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =174,
                        EndedAt = 180,
                        SurahOrder =3
                    }}),
            PagingModel.Create(74, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =181,
                        EndedAt = 186,
                        SurahOrder =3
                    }}),
            PagingModel.Create(75, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =187,
                        EndedAt = 194,
                        SurahOrder =3
                    }}),
            PagingModel.Create(76, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =195,
                        EndedAt = 200,
                        SurahOrder =3
                    }}),
            PagingModel.Create(77, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =1,
                        EndedAt = 6,
                        SurahOrder =4
                    }}),
            PagingModel.Create(78, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =7,
                        EndedAt = 11,
                        SurahOrder =4
                    }}),
            PagingModel.Create(79, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =12,
                        EndedAt = 14,
                        SurahOrder =4
                    }}),
            PagingModel.Create(80, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =15,
                        EndedAt = 19,
                        SurahOrder =4
                    }}),
            PagingModel.Create(81, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =20,
                        EndedAt = 23,
                        SurahOrder =4
                    }}),
            PagingModel.Create(82, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =24,
                        EndedAt = 26,
                        SurahOrder =4
                    }}),
            PagingModel.Create(83, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =27,
                        EndedAt = 33,
                        SurahOrder =4
                    }}),
            PagingModel.Create(84, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =34,
                        EndedAt = 37,
                        SurahOrder =4
                    }}),
            PagingModel.Create(85, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =38,
                        EndedAt = 44,
                        SurahOrder =4
                    }}),
            PagingModel.Create(86, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =45,
                        EndedAt = 51,
                        SurahOrder =4
                    }}),
            PagingModel.Create(87, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =52,
                        EndedAt = 59,
                        SurahOrder =4
                    }}),
            PagingModel.Create(88, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =60,
                        EndedAt = 65,
                        SurahOrder =4
                    }}),
            PagingModel.Create(89, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =66,
                        EndedAt = 74,
                        SurahOrder =4
                    }}),
            PagingModel.Create(90, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =75,
                        EndedAt = 79,
                        SurahOrder =4
                    }}),
            PagingModel.Create(91, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =80,
                        EndedAt = 86,
                        SurahOrder =4
                    }}),
            PagingModel.Create(92, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =87,
                        EndedAt = 91,
                        SurahOrder =4
                    }}),
            PagingModel.Create(93, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =87,
                        EndedAt = 91,
                        SurahOrder =4
                    }}),
            PagingModel.Create(94, new List<SurahChunk> { new SurahChunk
                    {
                        StartedAt =87,
                        EndedAt = 91,
                        SurahOrder =4
                    }}),
        };
        }

        #endregion paginator
    }
}