using HolyQuran.Services;
using System;
using System.Collections.Generic;
using System.Text;
using HolyQuran.Models;

namespace HolyQuran.Vms
{
    public class SurahNamesVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public PlaceOfLanding Landing { get; set; }
        public string Description { get; set; }
        public int AyaCount { get; set; }
    }

    public class RawySurahVm
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public string Landing { get; set; }
        public string Description { get; set; }
        public Dictionary<Rawy, List<AyahVm>> QuranRewat { get; set; }
    }

    public class SurahVm
    {
        public int Id { get; set; }
        public string HolySurahNameAr { get; set; }
        public string HolySurahNameEn { get; set; }
        public int Order { get; set; }
        public string Landing { get; set; }
        public string Description { get; set; }
        public int AyaCount => Ayah.Count;
        public List<AyahVm> Ayah { get; set; }
    }


    public class AyahVm
    {
        public int Id { get; set; }
        public int SorahId { get; set; }
        public int AyaNumber { get; set; }
        public HolyText RawyText { get; set; }
        public List<ReadingVm> Readings { get; set; }
    }

    public class ReadingVm
    {
        public int Id { get; set; }
        public int AyahId { get; set; }
        public int AyaNumber { get; set; }
        public string Readers { get; set; }
        public string ReadView { get; set; }
        public string HolyRead { get; set; }
        public string ReadInfo { get; set; } = string.Empty;
        public bool AgreedOn { get; set; }
    }

    public class ReadersVm
    {
        public Readers Reader { get; set; }
        public string Name { get; set; }
    }

    public class ReyawaVm
    {
        public Read Read { get; set; }
        public string NameView { get; set; }
        public List<ReadersVm> Readers { get; set; }
    }
}
