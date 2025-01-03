﻿using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.Model.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Api
{
    public static class ApiCampaign
    {
        public static async Task<IEnumerable<Campaign>> GetCampaigns()
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<Campaign>>("campaign/all")
                            .Get();

        public static async Task<int> Insert(Campaign campaign)
            => await BaseApi.GetSimpleHttpClientRequest<int>("campaign")
                            .Post(campaign);

        public static async Task<bool> Delete(int id)
            => await BaseApi.GetSimpleHttpClientRequest<bool>($"campaign/{id}")
                            .Delete();

        public static async Task ChangeStatus(int id, CampaignStatus status)
            => await BaseApi.GetSimpleHttpClientRequest($"campaign/changestatus/{id}")
                            .Put((int)status);
    }
}
