namespace SAP.WPF
{
    public interface IFileWorker
    {
        public void FileToCache();
        public Task CacheToFile();
    }
}
