using Npgsql;
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
    public async Task<IActionResult> Create(Members member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }
        try
        {
            await _members.AddAsync(member);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // 5. Put a message under the Email box.
            ModelState.AddModelError("Email", "Another member already uses this email.");
            // 6. Show the form again, with what was typed.
            return View(member);
        }
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

    // POST /Members/Edit/5 : the changed form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Members member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }
        try
        {
            await _members.UpdateAsync(member);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // 5. Put a message under the Email box.
            ModelState.AddModelError("Email", "Another member already uses this email.");
            // 6. Show the form again, with what was typed.
            return View(member);
        }
        // 7. The changes were saved: go back to the list.
        
        return RedirectToAction(nameof(Index));
    }



    // POST /Members/Delete/5 : the delete was confirmed.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            // 1. Delete the member.
            await _members.DeleteAsync(id);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            // 3. Load the member again, to show them on the page.
            var member = await _members.GetByIdAsync(id);
            if (member == null)
            {
            // 5. Answer "not found".
            return NotFound();
            }
            // 6. Set the message the page will show.
            ViewBag.Error = "This member has loans on record, so the database will not delete them.";
            // 7. Show the Delete page again, with the member.
            return View("Delete", member);
        }
        // 8. The member was deleted: go back to the list.
            return RedirectToAction(nameof(Index));
    }
    }
