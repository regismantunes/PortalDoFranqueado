using PortalDoFranqueado.Model;
using System.Threading.Tasks;
using System.Linq;

namespace PortalDoFranqueado.Api
{
    public static class ApiPurchaseSuggestion
    {
        public static async Task<int> Save(PurchaseSuggestion purchaseSuggestion)
        {
            var cleanPurchaseSuggestion = new PurchaseSuggestion
            {
                Id = purchaseSuggestion.Id,
                AverageTicket = purchaseSuggestion.AverageTicket,
                Coverage = purchaseSuggestion.Coverage,
                PartsPerService = purchaseSuggestion.PartsPerService,
                PurchaseId = purchaseSuggestion.PurchaseId,
                Target = purchaseSuggestion.Target,
                TotalSuggestedItems = purchaseSuggestion.TotalSuggestedItems,
                Families = (from f in purchaseSuggestion.Families
                            select new PurchaseSuggestionFamily()
                            {
                                Id = f.Id,
                                FamilyId = f.FamilyId,
                                FamilySuggestedItems = f.FamilySuggestedItems,
                                Percentage = f.Percentage,
                                PurchaseSuggestionId = f.PurchaseSuggestionId,
                                Sizes = (from s in f.Sizes
                                         select new PurchaseSuggestionFamilySize()
                                         {
                                             Id = s.Id,
                                             Percentage = s.Percentage,
                                             PurchaseSuggestionFamilyId = s.PurchaseSuggestionFamilyId,
                                             Size = s.Size,
                                             SizeSuggestedItems = s.SizeSuggestedItems,
                                         })
                                         .ToArray()
                            })
                            .ToArray()
            };

            return await BaseApi.GetSimpleHttpClientRequest<int>("purchasesuggestion")
                            .Put(cleanPurchaseSuggestion);
        }

        public static async Task<PurchaseSuggestion?> GetByPurchaseId(int purchaseId) 
            => await BaseApi.GetSimpleHttpClientRequest<PurchaseSuggestion?>($"purchasesuggestion/purchaseid/{purchaseId}")
                .Get();
    }
}
