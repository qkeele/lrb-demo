using LrbDemo.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LrbDemo.Pages;

public sealed class MeasuresModel : PageModel
{
    private readonly LrbDbContext _db;
    public List<Measure> Measures { get; private set; } = [];

    public MeasuresModel(LrbDbContext db) => _db = db;

    public async Task OnGetAsync()
    {
        Measures = await _db.Measures
            .OrderByDescending(m => m.Id)
            .Take(200)
            .ToListAsync();
    }
}
