

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;

namespace AppPages;

public class IndexPage : PageModel
{
    public int visitCount = 0;
    public List<Note> notes = new();

    private readonly AppDb db;
    
    public IndexPage(AppDb db)
    {
        this.db = db;
    }
    
    public void OnGet()
    {
        notes = db.Notes.ToList();
        visitCount++;
    }
}