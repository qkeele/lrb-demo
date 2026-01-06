using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace LrbDemo.Data;

public sealed class LrbDbContext : DbContext
{
    public LrbDbContext(DbContextOptions<LrbDbContext> options) : base(options) {}

    public DbSet<Measure> Measures => Set<Measure>();
}

[Table("measures")]
public sealed class Measure
{
    [Column("id")]
    public int Id { get; set; }

    [Column("measure_number")]
    public string MeasureNumber { get; set; } = "";

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("introducer")]
    public string Introducer { get; set; } = "";

    [Column("committee_referrals")]
    public string? CommitteeReferrals { get; set; }

    [Column("committee_report_numbers")]
    public string? CommitteeReportNumbers { get; set; }

    [Column("current_status")]
    public string CurrentStatus { get; set; } = "";

    [Column("roll_call_votes")]
    public string? RollCallVotes { get; set; }

    [Column("hrs_sections_affected")]
    public string? HrsSectionsAffected { get; set; }
}
