using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CateringManagement.Data;
using CateringManagement.Models;
using Microsoft.AspNetCore.Authorization;
using CateringManagement.CustomControllers;
using CateringManagement.Utilities;
using String = System.String;

namespace CateringManagement.Controllers
{
    [Authorize(Roles = "Admin,Supervisor,User,Staff")]
    public class CustomerFunctionController : ElephantController
    {
        private readonly CateringContext _context;

        public CustomerFunctionController(CateringContext context)
        {
            _context = context;
        }

        // GET: CustomerFunction
        public async Task<IActionResult> Index(int? CustomerID, int? page, int? pageSizeID, int? FunctionTypeID, string actionButton,
            string SearchString, string sortDirection = "desc", string sortField = "Function")
        {
            //Get the URL with the last filter, sort and page parameters from THE Customer Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Customer");

            if (!CustomerID.HasValue)
            {
                //Go back to the proper return URL for the Customer controller
                return Redirect(ViewData["returnURL"].ToString());
            }
            PopulateDropDownLists();

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Function", "Guar. No."};

            
            var funcs = from f in _context.Functions
                        .Include(f => f.Customer)
                        .Include(f => f.FunctionType)
                        .Include(f => f.MealType)
                        .Include(f => f.FunctionRooms).ThenInclude(fr => fr.Room)
                        .Include(f => f.FunctionDocuments)
                        where f.CustomerID == CustomerID.GetValueOrDefault()
                        select f;

            if (FunctionTypeID.HasValue)
            {
                funcs = funcs.Where(f => f.FunctionTypeID == FunctionTypeID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                funcs = funcs.Where(p => p.Name.ToUpper().Contains(SearchString.ToUpper())
                                       || p.LobbySign.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            //Give feedback about the state of the filters
            if (numberFilters != 0)
            {
                //Toggle the Open/Closed state of the collapse depending on if we are filtering
                ViewData["Filtering"] = " btn-danger";
                //Show how many filters have been applied
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //Keep the Bootstrap collapse open
                //@ViewData["ShowFilter"] = " show";
            }

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted so lets sort!
            {
                page = 1;//Reset back to first page when sorting or filtering

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by.
            if (sortField == "Function")
            {
                if (sortDirection == "asc")
                {
                    funcs = funcs
                        .OrderBy(p => p.Name);
                }
                else
                {
                    funcs = funcs
                        .OrderByDescending(p => p.Name);
                }
            }
            else if (sortField == "Guar. No.")
            {
                if (sortDirection == "asc")
                {
                    funcs = funcs
                        .OrderBy(p => p.GuaranteedNumber);
                }
                else
                {
                    funcs = funcs
                        .OrderByDescending(p => p.GuaranteedNumber);
                }
            }
            else // Date Range
            {
                if (sortDirection == "asc")
                {
                    funcs = funcs
                        .OrderByDescending(p => p.StartTime);
                }
                else
                {
                    funcs = funcs
                        .OrderBy(p => p.StartTime);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Now get the MASTER record, the customer, so it can be displayed at the top of the screen
            Customer customer = await _context.Customers
                .Include(p => p.CustomerThumbnail)
                .Where(p => p.ID == CustomerID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.Customer = customer;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Function>.CreateAsync(funcs.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

         

        // GET: CustomerFunction/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.Functions == null)
        //    {
        //        return NotFound();
        //    }

        //    var function = await _context.Functions
        //        .Include(f => f.Customer)
        //        .Include(f => f.FunctionType)
        //        .Include(f => f.MealType)
        //        .FirstOrDefaultAsync(m => m.ID == id);
        //    if (function == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(function);
        //}

        // GET: CustomerFunction/Add
        public IActionResult Add(int? CustomerID, string CustomerName)
        {
            if (!CustomerID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }
            ViewData["CustomerName"] = CustomerName;

            Function f = new Function()
            {
                CustomerID = CustomerID.GetValueOrDefault()
            };

            PopulateDropDownLists();
            return View(f);
        }

        // POST: CustomerFunction/Add
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Name,LobbySign,StartTime,EndTime,SetupNotes,BaseCharge,PerPersonCharge,GuaranteedNumber,SOCAN,Deposit,Alcohol," +
            "DepositPaid,NoHST,NoGratuity,CustomerID,FunctionTypeID,MealTypeID")] Function function, string CustomerName)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(function);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            PopulateDropDownLists(function);
            ViewData["CustomerName"] = CustomerName;
            return View(function);
        }

        // GET: CustomerFunction/Update/5
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || _context.Functions == null)
            {
                return NotFound();
            }

            var function = await _context.Functions
               .Include(f => f.MealType)
               .Include(f => f.Customer)
               .AsNoTracking()
               .FirstOrDefaultAsync(f => f.ID == id);
            if (function == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(function);
            return View(function);
        }

        // POST: CustomerFunction/Update/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id)
        {
            var functionToUpdate = await _context.Functions
                .Include(a => a.MealType)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (functionToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Function>(functionToUpdate, "",
                f => f.Name, f => f.LobbySign, f => f.StartTime, f => f.EndTime, f => f.SetupNotes,
                f => f.BaseCharge, f => f.PerPersonCharge, f => f.GuaranteedNumber,
                f => f.SOCAN, f => f.Deposit, f => f.DepositPaid, f => f.NoHST,
                f => f.NoGratuity, f => f.Alcohol, f => f.MealTypeID, f => f.FunctionTypeID))
            {
                try
                {
                    _context.Update(functionToUpdate);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!FunctionExists(functionToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                        "persists see your system administrator.");
                }
            }
            PopulateDropDownLists(functionToUpdate);
            return View(functionToUpdate);
        }

        // GET: CustomerFunction/Remove/5
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null || _context.Functions == null)
            {
                return NotFound();
            }

            var function = await _context.Functions
                .Include(f => f.Customer)
                .Include(f => f.FunctionType)
                .Include(f => f.MealType)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (function == null)
            {
                return NotFound();
            }

            return View(function);
        }

        // POST: CustomerFunction/Remove/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            if (_context.Functions == null)
            {
                return Problem("There are no Functions to delete.");
            }
            var function = await _context.Functions
                .Include(f => f.Customer)
                .Include(f => f.FunctionType)
                .Include(f => f.MealType)
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                _context.Functions.Remove(function);
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            return View(function);
        }
        private SelectList FunctionSelectList(int? id)
        {
            var dQuery = from f in _context.FunctionTypes
                         orderby f.Name
                         select f;
            return new SelectList(dQuery, "ID", "Name", id);
        }
        private SelectList MealTypeSelectList(int? id)
        {
            var dQuery = from d in _context.MealTypes
                         orderby d.Name
                         select d;
            return new SelectList(dQuery, "ID", "Name", id);
        }
        //private SelectList CustomerSelectList(int? id)
        //{
        //    var dQuery = from d in _context.Customers
        //                 orderby d.FullName
        //                 select d;
        //    return new SelectList(dQuery, "ID", "FullName", id);
        //}
        private void PopulateDropDownLists(Function function = null)
        {
            ViewData["FunctionTypeID"] = FunctionSelectList(function?.FunctionTypeID);
            ViewData["MealTypeID"] = MealTypeSelectList(function?.MealTypeID);
            //ViewData["CustomerName"] = CustomerSelectList(function?.CustomerID);
        }
        private bool FunctionExists(int id)
        {
          return _context.Functions.Any(e => e.ID == id);
        }
    }
}
