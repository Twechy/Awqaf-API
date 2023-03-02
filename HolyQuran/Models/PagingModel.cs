using HolyQuran.Vms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HolyQuran.Models
{
    public class PagingModel
    {
        public int Id { get; set; }
        public int PageNumber { get; set; }
        public PageSide PageSide => PageNumber % 2 == 0 ? PageSide.Left : PageSide.Right;
        public List<SurahChunk> SurahChunks { get; set; } = new List<SurahChunk>();
        public List<Waqaf> Waqafs { get; set; } = new List<Waqaf>();

        public static PagingModel Create(int pageNumber, List<SurahChunk> chunks = null, List<Waqaf> waqafs = null) => new()
        {
            PageNumber = pageNumber,
            SurahChunks = chunks,
            Waqafs = waqafs
        };
    }

    public class SurahChunk
    {
        public int SurahOrder { get; set; }
        public int StartedAt { get; set; }
        public int EndedAt { get; set; }
        public List<AyahVm> Ayah { get; set; } = new List<AyahVm>();
    }

    public enum PageSide
    {
        Right, Left
    }

    public class Waqaf
    {
        public int Id { get; set; }
        public WaqafType Type { get; set; }
        public int AtAya { get; set; }
    }

    public enum WaqafType
    { }
}