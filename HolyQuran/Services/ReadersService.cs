using HolyQuran.Models;
using HolyQuran.Vms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Vm;

namespace HolyQuran.Services
{
    public class ReadersService
    {
        private readonly QuranDb _quranDb;

        public ReadersService(QuranDb quranDb)
        {
            _quranDb = quranDb;
        }

        public async Task<Reader> Reader(Read reader)
        {
            var readerDb = await _quranDb.Readers.FirstOrDefaultAsync(x => x.Read == reader);
            return readerDb;
        }

        public async Task<List<Reader>> Readers()
        {
            var readers = await _quranDb.Readers.ToListAsync();
            return readers;
        }


        public async Task<List<ReaderVm>> ReadersVm()
        {
            var readersList = new List<ReaderVm>();
            var readers = await _quranDb.Readers.ToListAsync();

            foreach (var reader in readers.Where(x => x.RawyInfo == RawyInfo.Primary))
            {
                var rowatList = new List<ReaderVm>();
                var rowat = reader.Rowat;

                if (rowat != 0)
                {
                    var reads = Enum
                        .GetValues(rowat.GetType())
                        .Cast<Enum>().Where(rowat.HasFlag);

                    rowatList.AddRange(from item in reads
                    select Enum.Parse<Read>(item.ToString())
                    into read
                    where read != Read.None
                    select readers.FirstOrDefault(x => x.Read == read)
                    into rawy
                    select new ReaderVm
                    {
                        Read = rawy.Read,
                        Name = rawy.Name,
                        Location = rawy.Location,
                        BirthDate = rawy.BirthDate,
                        DeathDate = rawy.DeathDate,
                        Info = rawy.Info,
                        NameView = rawy.NameView,
                        RawyInfo = rawy.RawyInfo,
                    });
                }

                readersList.Add(new ReaderVm
                {
                    Read = reader.Read,
                    Name = reader.Name,
                    Location = reader.Location,
                    BirthDate = reader.BirthDate,
                    DeathDate = reader.DeathDate,
                    Info = reader.Info,
                    NameView = reader.NameView,
                    RawyInfo = reader.RawyInfo,
                    Rowat = rowatList
                });
            }

            return readersList;
        }

        public async Task<OperationResult> UpdateRead(UpdateReadDto updateReadDto)
        {
            var reading = await _quranDb.Reading.FirstOrDefaultAsync(x => x.Id == updateReadDto.Id);


            reading.HolyRead = updateReadDto.HolyRead;
            reading.ReadView = updateReadDto.ReadView;
            //reading.Reader = updateReadDto.Reader;
            reading.ReadInfo = updateReadDto.ReadInfo;

            reading.AyaNumber = updateReadDto.AyaNumber;

            _quranDb.Reading.Update(reading);

            var result = await _quranDb.SaveChangesAsync() >= 1;

            return result ? OperationResult.Valid() : OperationResult.UnValid();
        }

        public async Task<List<Reader>> PersistReadersData()
        {
            if (await _quranDb.Readers.AnyAsync()) return new List<Reader>();

            var readerTypes = ListOfReadersFromEnum();
            var realData = new List<Reader>();


            foreach (var item in readerTypes)
            {
                switch (item)
                {
                    case Read.Nafe3:
                        realData.Add(new Reader
                        {
                            Name = "نافع بن عبد الرحمٰن بن أبي نعيم الليثي",
                            NameView = "نافع",
                            BirthDate = "غير متوفر",
                            Location = "المدينة المنورة",
                            Read = Read.Nafe3,
                            DeathDate = "سنة 169هـ",
                            Info = "أحد القراء السبعة المشهورين أخذ على سبعين من التابعين",
                            Rowat = Read.Qalon | Read.Wersh,
                            RawyInfo = RawyInfo.Primary
                        });
                        break;
                    case Read.Qalon:
                        realData.Add(new Reader
                        {
                            Name = "عيسى بن مينا بن وردان بن عيسى المدني الملقب بقالون",
                            NameView = "قالون عن نافع",
                            BirthDate = "سنة 120هـ",
                            DeathDate = "سنة 220هـ",
                            Location = "المدينة المنورة",
                            Read = Read.Qalon,
                            Info =
                                " أحد القراء المشهورين من أهل المدينة  وكان أصم يُقرأ عليه القرءان وهو ينظر إلى شفتي القارئ فيرد عليه اللحن والخطأ",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Wersh:
                        realData.Add(new Reader
                        {
                            Name = "عثمان بن سعيد بن عبد الله المصري",
                            NameView = "ورش عن نافع",
                            BirthDate = "سنة 45هـ",
                            DeathDate = "سنة 110هـ",
                            Location = "مصر",
                            Read = Read.Wersh,
                            Info = "أحد كبار القراء المشهورين وانتهت إليه رئاسة الإقراء بالديار المصرية في زمانه",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.AbnKater:
                        realData.Add(new Reader
                        {
                            Name = "عبد الله بن كثير بن عمرو بن عبد الله الدّاري المكي",
                            NameView = "ابن كثير",
                            BirthDate = "سنة 45هـ",
                            DeathDate = "سنة 120هـ",
                            Location = "مكة",
                            Read = Read.AbnKater,
                            Info = "أحد القراء السبعة",
                            Rowat = Read.AlBozy | Read.Qonbol,
                            RawyInfo = RawyInfo.Primary
                        });
                        break;
                    case Read.AlBozy:
                        realData.Add(new Reader
                        {
                            Name = "أحمد بن محمد بن عبد الله بن القاسم بن نافع ابن أبي بَزّة",
                            NameView = "البزي عن ابن كثير",
                            BirthDate = "سنة 170هـ",
                            DeathDate = "سنة 250هـ",
                            Location = "مكة",
                            Read = Read.AlBozy,
                            Info = "أكبر من روى قراءة ابن كثير وانتهت إليه مشيخة الإقراء بمكة وكان مؤذن المسجد الحرام",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Qonbol:
                        realData.Add(new Reader
                        {
                            Name = "محمد بن عبد الرحمٰن بن محمد بن خالد بن سعيد المخزومي",
                            NameView = "قُنبل",
                            BirthDate = "سنة 195هـ",
                            DeathDate = "سنة 291هـ",
                            Location = "مكة",
                            Read = Read.Qonbol,
                            Info =
                                "أحد القراء السبعة انتهت إليه مشيخة الإقراء بالحجاز، ورحل إليه الناس من جميع الأقطار",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Abu3amro:
                        realData.Add(new Reader
                        {
                            Name = "زَبّان بن العلاء بن عمار التميمي المازني البصري",
                            NameView = "أبو عمرو",
                            BirthDate = "سنة 68هـ",
                            DeathDate = "سنة 154هـ",
                            Location = "البصرة",
                            Read = Read.Abu3amro,
                            Info = "أحد القراء السبعة ومن أئمة اللغة والأدب",
                            Rowat = Read.AlDory | Read.AlSosy,
                            RawyInfo = RawyInfo.Primary
                        });
                        break;
                    case Read.AlDory:
                        var dory = new Reader
                        {
                            Name = "حفص بن عمر بن عبد العزيز بن صهبان بن عدي الدوري",
                            NameView = "الدُّوري",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 246هـ",
                            Location = "البصرة",
                            Read = Read.AlDory,
                            Info = "إمام القراءة في عصره",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        };
                        realData.Add(dory);
                        break;
                    case Read.AlSosy:
                        realData.Add(new Reader
                        {
                            Name = "صالح بن زياد بن عبد الله بن إسماعيل بن إبراهيم بن الجارود السوسي",
                            NameView = "السُّوسي",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 261هـ",
                            Location = "الرقة",
                            Read = Read.AlSosy,
                            Info = "كان مقرئًا ضابطًا ثقة",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Abn3amer:
                        realData.Add(new Reader
                        {
                            Name = "عبد الله بن عامر بن يزيد بن تميم بن ربيعة اليَحصبي المُكنى بأبي عمرو",
                            NameView = "بن عامر",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 118هـ",
                            Location = "الشام",
                            Read = Read.Abn3amer,
                            Info =
                                "كان إمام أهل الشام، أمَّ المسلمين بالجامع الأموي سنين كثيرة في أيام الخليفة عمر ابن عبد العزيز رضي الله عنه وكان الخليفة يأتم به. جمع بين الإمامة والقضاء، ومشيخة الإقراء بدمشق",
                            Rowat = Read.Hesham | Read.AbnDakwan,
                            RawyInfo = RawyInfo.Primary
                        });
                        break;
                    case Read.Hesham:
                        realData.Add(new Reader
                        {
                            Name = "هشام بن عمار بن نُصَير بن مَيسَرَة السُّلَمي الدمشقي",
                            NameView = "هشام",
                            BirthDate = "سنة 153هـ",
                            DeathDate = "سنة 245هـ",
                            Location = "الشام",
                            Read = Read.Hesham,
                            Info = "له كتاب فضائل القرءان",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.AbnDakwan:
                        realData.Add(new Reader
                        {
                            Name = "عبد الله بن أحمد بن بشر ـ ويقال: بشير ـ ابن ذكوان القرشي، الدمشقي",
                            NameView = "ابن ذَكْوان",
                            BirthDate = "سنة 173هـ",
                            DeathDate = "سنة 242هـ",
                            Location = "الشام",
                            Read = Read.AbnDakwan,
                            Info = "كان شيخ الإقراء بالشام، وإمام الجامع الأموي وانتهت إليه مشيخة الإقراء بدمشق",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Asem:
                        realData.Add(new Reader
                        {
                            Name = "عاصم بن أبي النَّجُود الكوفي، الأسدي أبو بكر",
                            NameView = "عاصم",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 127هـ",
                            Location = "الكوفة",
                            Read = Read.Asem,
                            Info =
                                " أحد التابعين والقراء السبعة المشهورين، انتهت إليه رئاسة الإقراء بالكوفة، ورحل إليه الناس للقراءة",
                            Rowat = Read.Hafs | Read.Sho3ba,
                            RawyInfo = RawyInfo.Primary
                        });
                        break;
                    case Read.Sho3ba:
                        realData.Add(new Reader
                        {
                            Name = "شعبة بن عَيَّاش بن سالم الأَسَدِي الكوفي أبو بكر",
                            NameView = "شعبة",
                            BirthDate = "سنة 95 هـ",
                            DeathDate = "سنة 193هـ",
                            Location = "الكوفة",
                            Read = Read.Sho3ba,
                            Info = "من مشاهير القراء عرض القراءة على عاصم أكثر من مرة، وعلى عطاء بن السائب",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Hafs:
                        realData.Add(new Reader
                        {
                            Name = "حفص بن سليمان بن المغيرة بن أبي داود الأَسَدي الكوفي",
                            NameView = "حفص عن عاصم",
                            BirthDate = "سنة 90هـ",
                            DeathDate = "سنة 180هـ",
                            Location = "الكوفة",
                            Read = Read.Hafs,
                            Info = "قارئ أهل الكوفة وكان أعلمَ أصحاب عاصم بقراءة عاصم",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Hamza:
                        realData.Add(new Reader
                        {
                            Name = "حمزة بن حبيب بن عُمَارة بن إسماعيل الكوفي",
                            NameView = "حمزة",
                            BirthDate = "سنة 80هـ",
                            DeathDate = "سنة 156هـ",
                            Location = "الكوفة",
                            Read = Read.Hamza,
                            Info = " أحد القراء السبعة وأدرك بعض الصحابة بالسن فلعله رأى بعضهم",
                            Rowat = Read.Kalaf | Read.Kalad,
                            RawyInfo = RawyInfo.Primary
                        });
                        break;
                    case Read.Kalaf:
                        realData.Add(new Reader
                        {
                            Name = "خلف بن هشام بن ثَعْلب الأسدي البغدادي أبو محمد",
                            NameView = "خَلَف",
                            BirthDate = "سنة 150هـ",
                            DeathDate = "سنة 229هـ",
                            Location = "بغداد",
                            Read = Read.Kalaf,
                            Info =
                                "أخذ القراءة عرضًا عن سُليم بن عيسى وعبد الرحمٰن بن حماد عن حمزة، وقد اختار لنفسه قراءة انفرد بها، فيعد من القراء العشرة",
                            Rowat = Read.Ashak | Read.Adress,
                            RawyInfo = RawyInfo.Primary
                        });
                        break;
                    case Read.Kalad:
                        realData.Add(new Reader
                        {
                            Name = "خلاد بن خالد الشيباني الصَّيرَفي",
                            NameView = "خَلاَّد",
                            BirthDate = "سنة 119هـ",
                            DeathDate = "سنة 220هـ",
                            Location = "الكوفة",
                            Read = Read.Kalad,
                            Info = "كان إمامًا في القراءة ثقة عارفًا",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Qesaee:
                        realData.Add(new Reader
                        {
                            Name = "علي بن حمزة بن عبد الله الكسائي",
                            NameView = "الكِسَائي",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 189هـ",
                            Location = "الكوفة",
                            Read = Read.Qesaee,
                            Info = "أحد أئمة اللغة والنحو وأحد القراء السبعة المشهورين",
                            Rowat = Read.AlDory | Read.Laith,
                            RawyInfo = RawyInfo.Primary
                        });
                        break;
                    case Read.Laith:
                        realData.Add(new Reader
                        {
                            Name = "الليث بن خالد المَرْوَزِي البغدادي أبو الحارث",
                            NameView = "الليث",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 240هـ",
                            Location = "الكوفة",
                            Read = Read.Laith,
                            Info = "وهو من أجل أصحاب الكِسَائي، كان ثقة ضابطًا",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.AbuJafer:
                        realData.Add(new Reader
                        {
                            Name = "يزيد بن القَعقَاع المخزومي المدني أبو جعفر",
                            NameView = "أبو جعفر",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 130هـ، وقيل 132هـ",
                            Location = "المدينة",
                            Read = Read.AbuJafer,
                            Info = " أحد القراء العشرة ومن التابعين. كان إمام أهل المدينة في القراءة",
                            Rowat = Read.AbnWardan | Read.AbnJamaz,
                            RawyInfo = RawyInfo.Primary
                        });
                        break;
                    case Read.AbnWardan:
                        realData.Add(new Reader
                        {
                            Name = "عيسى بن وَردَان المدني أبو الحارث",
                            NameView = "بن وَردَان",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 160هـ",
                            Location = "المدينة",
                            Read = Read.AbnWardan,
                            Info = "من قدماء أصحاب نافع، قرأ عليه ثم عرض القراءة على أبي جعفر",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.AbnJamaz:
                        realData.Add(new Reader
                        {
                            Name = "سليمان بن مسلم بن جَمَّاز المدني",
                            NameView = "ابن جَمَّاز",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 170هـ",
                            Location = "المدينة",
                            Read = Read.AbnJamaz,
                            Info = "قرأ القراءة عرضًا على أبي جعفر، ثم عرض على نافع",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Ya3qob:
                        realData.Add(new Reader
                        {
                            Name = "يعقوب بن إسحق بن زيد الحضرمي البصري أبو محمد",
                            NameView = "يعقوب",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 205هـ",
                            Location = "البصرة",
                            Read = Read.Ya3qob,
                            Info = "أحد القراء العشرة. ولد بالبصرة كان مقرئ البصرة، وله تصانيف عديدة",
                            Rowat = Read.Royaes | Read.Rouh,
                            RawyInfo = RawyInfo.Primary
                        });
                        break;
                    case Read.Royaes:
                        realData.Add(new Reader
                        {
                            Name = "محمد بن المتوكل اللؤلؤي البصري أبو عبد الله",
                            NameView = "رُوَيس",
                            BirthDate = "غير متوفر",
                            DeathDate = " سنة 238هـ",
                            Location = "البصرة",
                            Read = Read.Royaes,
                            Info = "من أكبر أصحاب يعقوب. كان حاذقًا وإمامًا في القراءة، ضابطًا",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Rouh:
                        realData.Add(new Reader
                        {
                            Name = "روح بن عبد المؤمن الهُذَلي البصري النحوي، أبو الحسن",
                            NameView = "رَوْح",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 234هـ وقيل 235هـ",
                            Location = "البصرة",
                            Read = Read.Rouh,
                            Info = "كان من أجل أصحاب يعقوب وأوثقهم",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Ashak:
                        realData.Add(new Reader
                        {
                            Name = " إسحاق بن إبراهيم بن عثمان بن عبد الله الوَرَّاق المروزي، أبو يعقوب",
                            NameView = "إسحق",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 256هـ",
                            Location = "البصرة",
                            Read = Read.Ashak,
                            Info = " قرأ على خلف وقام به بعده",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.Adress:
                        realData.Add(new Reader
                        {
                            Name = "إدريس بن عبد الكريم الحدَّاد البغدادي، أبو الحسن",
                            NameView = "إدريس",
                            BirthDate = "غير متوفر",
                            DeathDate = "سنة 292هـ",
                            Location = "البصرة",
                            Read = Read.Adress,
                            Info = "قرأ على خلف روايته. وهو إمام متقن ثقة",
                            Rowat = Read.None,
                            RawyInfo = RawyInfo.Rawy
                        });
                        break;
                    case Read.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var items = realData.OrderBy(x => (int)x.Read).Distinct().ToList();

            foreach (var item in items)
            {
                await _quranDb.Readers.AddAsync(item);
                await _quranDb.SaveChangesAsync();
            }

            return realData;
        }

        public async Task<List<ReyawaVm>> Reyawat()
        {
            var lists = new List<ReadersVm>();
            var rowat = new List<Read>
            {
                Read.Qalon,
                Read.Hafs,
                Read.Wersh,
                Read.AlBozy
            };

            var rowatVmList = new List<ReyawaVm>();

            foreach (var rawy in rowat)
            {
                var r = await Reader(rawy);

                lists = rawy switch
                {
                    Read.Hafs => new List<ReadersVm>
                    {
                        new ReadersVm {Reader = Models.Readers.AbdAlRashedSofi, Name = "عبد الرشيد صوفي"}
                    },
                    Read.Wersh => new List<ReadersVm>
                    {
                        new ReadersVm {Reader = Models.Readers.UasenAljazaare, Name = "ياسين الجزائري"}
                    },
                    Read.Qalon => new List<ReadersVm>
                    {
                        new ReadersVm {Reader = Models.Readers.AliHodify, Name = "علي الحذيفي"}
                    },
                    Read.AlBozy => new List<ReadersVm>
                    {
                        new ReadersVm {Reader = Models.Readers.AlBozyReader, Name = "قارئ البزي"}
                    },
                    _ => lists
                };

                var item = new ReyawaVm { Read = rawy, NameView = r.NameView, Readers = lists };

                rowatVmList.Add(item);
            }

            return rowatVmList;
        }

        public static List<Read> ListOfReadersFromEnum()
        {
            var vs = Enum.GetNames(typeof(Read)).ToList();
            vs.Remove("None");
            return vs.Select(Enum.Parse<Read>).ToList();
        }



        public static List<Read> ListOfReaders()
        {
            var readers = new List<Read>
            {
                Read.Qalon,
                Read.Hafs
            };
            //Array read = Enum.GetValues(typeof(Read));

            //foreach (Read item in read)
            //{
            //    int index = (int)item;

            //    if (index > 0 && index <= 512)
            //        readers.Add(item);
            //}
            return readers;
        }
    }
}