using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Common;
using System.Threading.Tasks;

namespace SV22T1020375.Shop.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var searchInput = new PaginationSearchInput { Page = 1, PageSize = 100, SearchValue = "" };
            var data = await CatalogDataService.ListCategoriesAsync(searchInput);

            return View(data.DataItems);
        }
    }
}
