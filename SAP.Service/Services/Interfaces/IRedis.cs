namespace SAP.API.Services.Interfaces
{
    public interface IRedis
    {
        Task<string?> GetString(string key);
        Task SetAsync(string key, string value);
        Task DeleteAsync(string key);
    }
}
