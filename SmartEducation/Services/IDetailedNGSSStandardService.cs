namespace SmartEducation.Services
{
    public interface IDetailedNGSSStandardService
    {
        Task SeedOrUpdateStandardsAsync(string jsonFilePath);
    }
}
