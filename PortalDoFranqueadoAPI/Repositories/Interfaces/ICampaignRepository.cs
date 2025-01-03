﻿using PortalDoFranqueadoAPI.Models.Entities;
using PortalDoFranqueadoAPI.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface ICampaignRepository
    {
        Task<IEnumerable<Campaign>> GetList(bool onlyActives = false, bool loadFiles = false);

        Task<int> Insert(Campaign campaign);

        Task<bool> Delete(int id);

        Task ChangeStatus(int id, CampaignStatus status);
    }
}