using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models;

public class Memorization : BaseEntity
{
    public int CurrentSurah { get; set; }
    public int SurahMemorized { get; set; }
    public double TotalGrade { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    public List<MemorizationRecord> MemorizationRecords { get; set; }
}

public class MemorizationRecord : BaseEntity
{
    public string From { get; set; }
    public string To { get; set; }
    public string SurahId { get; set; }
    public Grade Grade { get; set; }
    public Guid MemorizationId { get; set; }
    public Memorization Memorization { get; set; }
}

public enum Grade
{
    star, twoStars, threeStars, fourStars, fiveStars,
}