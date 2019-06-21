using System.Linq;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using System;

namespace Microsoft.eShopWeb.Web.Controllers.Api
{
    public class CatalogController : BaseApiController
    {
        private readonly ICatalogViewModelService _catalogViewModelService;
        private readonly IAsyncRepository<CatalogItem> _itemRepository;

        public CatalogController(ICatalogViewModelService catalogViewModelService, IAsyncRepository<CatalogItem> itemRepository)
        {
            _catalogViewModelService = catalogViewModelService;
            _itemRepository = itemRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List(int? brandFilterApplied, int? typesFilterApplied, int? page)
        {
            var itemsPage = 10;           
            var catalogModel = await _catalogViewModelService.GetCatalogItems(page ?? 0, itemsPage, brandFilterApplied, typesFilterApplied);
            return Ok(catalogModel);
        }

        [HttpGet]
        public async Task<IActionResult> Expand()
        {
            var items = await _itemRepository.ListAllAsync();
            items = items.Take(100).ToList();            
            var brandsIds = items.Select(i=>i.CatalogBrandId).Distinct().ToList();
            var catalogsTypesIds = items.Select(i=>i.CatalogTypeId).Distinct().ToList();

            var rnd = new Random(DateTime.UtcNow.Second);
            foreach(var item in items)
            {
                foreach(var brandId in brandsIds)
                {
                    foreach(var catalogTypeId in catalogsTypesIds)
                    {
                        await _itemRepository.AddAsync(new CatalogItem
                        {
                            CatalogBrandId = brandId,
                            CatalogTypeId = catalogTypeId,
                            Description = item.Description,
                            Name = $"item.Name {Guid.NewGuid().ToString()}",
                            Price = (decimal) rnd.NextDouble()
                        });
                    }
                }
            }
            return Redirect("List");        
        }
    }
}
