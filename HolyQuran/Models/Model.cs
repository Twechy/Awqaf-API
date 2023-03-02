using System.ComponentModel.DataAnnotations;
using HolyQuran.Services;

namespace HolyQuran.Models
{
    public class Sorah
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string HolySurahNameAr { get; set; }
        public string HolySurahNameEn { get; set; }
        public PlaceOfLanding Landing { get; set; }
        public string Description { get; set; }
        public int AyaCount => Ayah.Count;
        public List<Ayah> Ayah { get; set; }
    }

    public class Ayah
    {
        public int Id { get; set; }
        public int SorahId { get; set; }
        public HolyText RawyText { get; set; }
        public List<HolyReadingModel> Readings { get; set; }
    }

    public class HolyText
    {
        public int Id { get; set; }
        public int SurahId { get; set; }
        public int AyahId { get; set; }
        public int AyaNumber { get; set; }
        public string Text { get; set; }
        public Rawy Rawy { get; set; }
    }

    public class ReadingVm
    {
        public int AyaId { get; set; }
        public int AyaNumber { get; set; }
        public string ReadView { get; set; }
        public Read Read { get; set; }
        public string Reader { get; set; }
        public string HolyRead { get; set; }
    }

    public class HolyReadingModel
    {
        public int Id { get; set; }
        public int AyahId { get; set; }
        public int AyaNumber { get; set; }
        public Read Reader { get; set; }
        public string ReadView { get; set; }
        public string HolyRead { get; set; }
        public string ReadInfo { get; set; } = string.Empty;
        public bool AgreedOn { get; set; }
    }

    public class Reader
    {
        [Key] public Read Read { get; set; }
        public string Name { get; set; }
        public string NameView { get; set; }
        public string Location { get; set; }
        public string BirthDate { get; set; }
        public string DeathDate { get; set; }
        public string Info { get; set; }
        public Read Rowat { get; set; }
        public RawyInfo RawyInfo { get; set; }
    }

    public class ReaderVm
    {
        public Read Read { get; set; }
        public string Name { get; set; }
        public string NameView { get; set; }
        public string Location { get; set; }
        public string BirthDate { get; set; }
        public string DeathDate { get; set; }
        public string Info { get; set; }
        public List<ReaderVm> Rowat { get; set; }
        public RawyInfo RawyInfo { get; set; }
    }

    public enum RawyInfo
    {
        Primary,
        Rawy,
    }

    [Flags]
    public enum Read
    {
        None = 0,
        Nafe3 = 1,
        AbnKater = 2,
        Abu3amro = 4,
        Abn3amer = 8,
        Asem = 16,
        Hamza = 32,
        Qesaee = 64,
        AbuJafer = 128,
        Ya3qob = 256,
        Kalaf = 512,
        AlSosy = Rawy.AlSosy,
        Sho3ba = Rawy.Sho3ba,
        AlDory = Rawy.AlDory,
        Qonbol = Rawy.Qonbol,
        Hesham = Rawy.Hesham,
        Hafs = Rawy.Hafs,
        Royaes = Rawy.Royaes,
        Qalon = Rawy.Qalon,
        Wersh = Rawy.Wersh,
        AlBozy = Rawy.AlBozy,
        AbnDakwan = Rawy.AbnDakwan,
        Kalad = Rawy.Kalad,
        Laith = Rawy.Laith,
        AbnWardan = Rawy.AbnWardan,
        AbnJamaz = Rawy.AbnJamaz,
        Rouh = Rawy.Rouh,
        Ashak = Rawy.Ashak,
        Adress = Rawy.Adress
    }

    [Flags]
    public enum Rawy
    {
        AlSosy = 1024,
        Sho3ba = 2048,
        AlDory = 4096,
        Qonbol = 8192,
        Hesham = 16384,
        Hafs = 32768,
        Royaes = 65536,
        Qalon = 131072,
        Wersh = 262144,
        AlBozy = 524288,
        AbnDakwan = 1048576,
        Kalad = 2097152,
        Laith = 4194304,
        AbnWardan = 8388608,
        AbnJamaz = 16777216,
        Rouh = 33554432,
        Ashak = 67108864,
        Adress = 134217728
    }

    public enum Readers
    {
        AbdAlRashedSofi = 1,
        AliHodify,
        UasenAljazaare,
        AlBozyReader
    }
}