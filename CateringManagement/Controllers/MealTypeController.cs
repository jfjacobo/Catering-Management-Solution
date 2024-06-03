using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CateringManagement.Data;
using CateringManagement.Models;
using CateringManagement.CustomControllers;
using Microsoft.AspNetCore.Authorization;
using CateringManagement.Utilities;
using CateringManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CateringManagement.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]
    public class MealTypeController : LookupsController
    {
        //for sending email
        //private readonly IMyEmailSender _emailSender;
        private readonly CateringContext _context;

        public MealTypeController(CateringContext context)
        {
            _context = context;
            //_emailSender = emailSender;
        }

        // GET: MealType
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: MealType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MealType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] MealType mealType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(mealType);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("Name", "Unable to save changes. " +
                        "Remember, you cannot have duplicate Meal Types Names.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, " +
                        "and if the problem persists see your system administrator.");
                }
            }
            //Decide if we need to send the Validaiton Errors directly to the client
            if (!ModelState.IsValid && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                //Was an AJAX request so build a message with all validation errors
                string errorMessage = "";
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMessage += error.ErrorMessage + "|";
                    }
                }
                //Note: returning a BadRequest results in HTTP Status code 400
                return BadRequest(errorMessage);
            }

            return View(mealType);
        }

        // GET: MealType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MealTypes == null)
            {
                return NotFound();
            }

            var mealType = await _context.MealTypes.FindAsync(id);
            if (mealType == null)
            {
                return NotFound();
            }
            return View(mealType);
        }

        // POST: MealType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the MealType to update
            var mealTypeToUpdate = await _context.MealTypes.FirstOrDefaultAsync(p => p.ID == id);

            //Check that you got it or exit with a not found error
            if (mealTypeToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<MealType>(mealTypeToUpdate, "",
                d => d.Name))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MealTypeExists(mealTypeToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Name", "Unable to save changes. " +
                            "Remember, you cannot have duplicate Meal Types Names.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, " +
                            "and if the problem persists see your system administrator.");
                    }
                }

                //Decide if we need to send the Validaiton Errors directly to the client
                if (!ModelState.IsValid && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    //Was an AJAX request so build a message with all validation errors
                    string errorMessage = "";
                    foreach (var modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                            errorMessage += error.ErrorMessage + "|";
                        }
                    }
                    //Note: returning a BadRequest results in HTTP Status code 400
                    return BadRequest(errorMessage);
                }
            }
            return View(mealTypeToUpdate);
        }

        // GET: MealType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MealTypes == null)
            {
                return NotFound();
            }

            var mealType = await _context.MealTypes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (mealType == null)
            {
                return NotFound();
            }

            return View(mealType);
        }

        // POST: MealType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MealTypes == null)
            {
                return Problem("There are no Function Types to delete.");
            }
            var mealType = await _context.MealTypes.FindAsync(id);
            try
            {
                if (mealType != null)
                {
                    _context.MealTypes.Remove(mealType);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Meal Type. Remember, you cannot delete a Meal Type that is used in the system.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(mealType);
        }
        /*public async Task<IActionResult> Notification(int? id, string Subject, string emailContent)
        {
            if (id == null)
            {
                return NotFound();
            }
            MealType m = await _context.MealTypes.FindAsync(id);

            ViewData["id"] = id;
            ViewData["MealName"] = m.Name;

            if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(emailContent))
            {
                ViewData["Message"] = "You must enter both a Subject and some message Content before sending the message.";
            }
            else
            {
                int folksCount = 0;
                try
                {
                    //Send a Notice.
                    List<EmailAddress> folks = (from c in _context.Functions
                                                where c.MealTypeID == id
                                                select new EmailAddress
                                                {
                                                    Name = c.Customer.FullName,
                                                    Address = c.Customer.Email
                                                }).ToList();
                    folksCount = folks.Count;
                    if (folksCount > 0)
                    {
                        var msg = new EmailMessage()
                        {
                            ToAddresses = folks,
                            Subject = Subject,
                            Content = "<p>" + emailContent + "</p><p>Please access the <strong>Niagara College</strong> web site to review.</p>"

                        };
                        await _emailSender.SendToManyAsync(msg);
                        ViewData["Message"] = "Message sent to " + folksCount + " Customer"
                            + ((folksCount == 1) ? "." : "s.");
                    }
                    else
                    {
                        ViewData["Message"] = "Message NOT sent!  No Patients in medical trial.";
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.GetBaseException().Message;
                    ViewData["Message"] = "Error: Could not send email message to the " + folksCount + " Customer"
                        + ((folksCount == 1) ? "" : "s") + " in the trial.";
                }
            }
            return View();
        }*/
        private bool MealTypeExists(int id)
        {
          return _context.MealTypes.Any(e => e.ID == id);
        }
    }
}
