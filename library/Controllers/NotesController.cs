using library.Data;
using library.Models;
using Microsoft.AspNetCore.Mvc;
namespace library.Controllers;

public class NotesController : Controller
    {
    private readonly Noterepository _notes;

    // ASP.NET Core hands in the repository, because Program.cs registered it.
    public NotesController(Noterepository notes)
    {
        _notes = notes;
    }

    // GET /Notes : the list of notes.
    public async Task<IActionResult> Index()
    {
        var notes = await _notes.GetAllAsync();
        return View(notes);                             // Views/notes/Index.cshtml
    }

    // GET /Notes/Create : an empty form.
    public IActionResult Create()
    {
        return View(new Notes());
    }

    // POST /Notes/Create : the form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Notes notes)
    {
        if (!ModelState.IsValid)                        // a required field was left empty
        {
            return View(notes);
        }

        await _notes.AddAsync(notes);
        return RedirectToAction(nameof(Index));
    }


}

