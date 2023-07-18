using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IdentityApp.Data;
using IdentityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IdentityApp.Authorization;

namespace IdentityApp.Pages.Invoices
{
    [AllowAnonymous]
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IList<Invoice> Invoice { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public List<Invoice> PaginatedInvoiceList { get; set; }

        public async Task<IActionResult> OnGetAsync(int pageIndex = 1)
        {
            const int pageSize = 7;

            var invoices = from i in Context.Invoice
                           select i;

            var isManager = User.IsInRole(Constants.InvoiceManagersRole);
            var isAdmin = User.IsInRole(Constants.InvoiceAdminRole);

            var currentUserId = UserManager.GetUserId(User);

            if (isManager == false && isAdmin == false)
            {
                invoices = invoices.Where(i => i.CreatorId == currentUserId);
            }

            var totalItems = await invoices.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            PageIndex = pageIndex > 0 ? pageIndex : 1;
            PageIndex = pageIndex > TotalPages ? TotalPages : pageIndex;

            var skipAmount = (PageIndex - 1) * pageSize;

            PaginatedInvoiceList = await invoices
                .OrderByDescending(invoice => invoice.InvoiceId)
                .Skip(skipAmount)
                .Take(pageSize)
                .ToListAsync();

            return Page();
        }
    }
}
