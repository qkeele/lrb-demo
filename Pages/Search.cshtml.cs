using LrbDemo.Data;
using LrbDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LrbDemo.Pages;

public sealed class SearchModel : PageModel
{
    private readonly LrbDbContext _db;
    private readonly OpenAiQueryParser _parser;

    public SearchModel(LrbDbContext db, OpenAiQueryParser parser)
    {
        _db = db;
        _parser = parser;
    }

    [BindProperty]
    public string UserQuery { get; set; } = "";

    public List<Measure>? Results { get; private set; }
    public string? OpenAiRawJson { get; private set; }
    public QueryFilters? OpenAiFilters { get; private set; }

    public void OnGet() { }

    public async Task OnPostAsync()
    {
        var parse = await _parser.ParseAsync(UserQuery);

        OpenAiRawJson = parse.RawJson;
        OpenAiFilters = parse.Filters;

        var f = parse.Filters;
        IQueryable<Measure> q = _db.Measures;

        if (f.Introducers.Any())
            q = q.Where(m => f.Introducers.Any(i => m.Introducer.Contains(i)));

        if (f.Statuses.Any())
            q = q.Where(m => f.Statuses.Any(s => m.CurrentStatus.Contains(s)));

        if (f.Committees.Any())
            q = q.Where(m => f.Committees.Any(c => (m.CommitteeReferrals ?? "").Contains(c)));

        if (f.CommitteeReports.Any())
            q = q.Where(m => f.CommitteeReports.Any(r => (m.CommitteeReportNumbers ?? "").Contains(r)));

        if (f.HrsSections.Any())
            q = q.Where(m => f.HrsSections.Any(h => (m.HrsSectionsAffected ?? "").Contains(h)));

        if (f.Keywords.Any())
        {
            foreach (var kw in f.Keywords)
                q = q.Where(m => m.Description.Contains(kw));
        }

        Results = await q.Take(200).ToListAsync();
    }
}

