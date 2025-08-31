namespace SmartEducation.Services
{
    public interface IGradeStandardService
    {
        Task SeedOrUpdateStandardsAsync(string jsonFilePath);
    }
}
