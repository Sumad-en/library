using library.Data;
using library.Models;
using Microsoft.AspNetCore.Mvc;

namespace library.Controllers;

public class MembersController : Controller
{
    private readonly MemberRepository _members;

    // ASP.NET Core hands in the repository, because Program.cs registered it.
    public MembersController(MemberRepository members)
    {
        _members = members;
    }

    // GET /Members : the list of members.
    public async Task<IActionResult> Index()
    {
        var members = await _members.GetAllAsync();
        return View(members);                             // Views/Members/Index.cshtml
    }

    // GET /Members/Create : an empty form.
    public IActionResult Create()
    {
        return View(new Members());
    }

    // POST /Members/Create : the form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Members members)
    {
        if (!ModelState.IsValid)                        // a required field was left empty
        {
            return View(members);
        }

        await _members.AddAsync(members);
        return RedirectToAction(nameof(Index));
    }

    // GET /Members/Edit/5 : the form, filled with member 5.
    public async Task<IActionResult> Edit(long id)
    {
        var members = await _members.GetByIdAsync(id);
        if (members == null)
        {
            return NotFound();
        }
        return View(members);
    }

    // POST /Books/Edit/5 : the changed form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Members members)
    {
        if (!ModelState.IsValid)
        {
            return View(members);
        }

        await _members.UpdateAsync(members);
        return RedirectToAction(nameof(Index));
    }



}
