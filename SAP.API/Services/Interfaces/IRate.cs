﻿using SAP.API.Entities.Rates;

namespace SAP.API.Services.Interfaces
{
    public interface IRate
    {
        Task<List<Rate>> GetAll(int Cur_ID, string startDate, string endDate);
    }
}
