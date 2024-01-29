
namespace SAP.API.Services.Interfaces
{
    public interface IRate
    {
        Task<List<Rate>> GetAll(int Cur_ID, DateTime startDate, DateTime endDate);
    }
}
