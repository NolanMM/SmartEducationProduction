using Microsoft.EntityFrameworkCore;
using SmartEducation.dbContext;
using SmartEducation.Entities;
using System.Text.Json;

namespace SmartEducation.Services
{
    public class GradeStandardService : IGradeStandardService
    {
        private readonly SmartEduDbContext _context;

        public GradeStandardService(SmartEduDbContext context)
        {
            _context = context;
        }

        public async Task SeedOrUpdateStandardsAsync(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException("The standards JSON file was not found.", jsonFilePath);
            }

            var jsonText = await File.ReadAllTextAsync(jsonFilePath);
            var standardsFromFile = JsonSerializer.Deserialize<List<Grade_Standards>>(jsonText);

            if (standardsFromFile == null || !standardsFromFile.Any())
            {
                return; // Nothing to process
            }

            foreach (var standardFromFile in standardsFromFile)
            {
                var existingStandard = await _context.NGSS_Standard
                    .FirstOrDefaultAsync(s => s.Title_Grade_Standard == standardFromFile.Title_Grade_Standard);

                if (existingStandard == null)
                {
                    // If it doesn't exist, add it
                    _context.NGSS_Standard.Add(standardFromFile);
                }
                else
                {
                    // If it exists, update its description
                    existingStandard.Description = standardFromFile.Description;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
