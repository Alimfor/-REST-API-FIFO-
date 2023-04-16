namespace ProjectHack.Model.RAMinformationService
{
    public interface IMemoryInfoService
    {
        long GetUsedMemory();
    }
    public class MemoryInfoService : IMemoryInfoService
    {
        private readonly ILogger<MemoryInfoService> _logger;
        public MemoryInfoService(ILogger<MemoryInfoService> logger) => _logger = logger;

        public long GetUsedMemory()
        {
            long usedMemory = GC.GetTotalMemory(false);
            _logger.LogInformation($"Used memory {usedMemory} bytes");
            return usedMemory;
        }
    }
}
