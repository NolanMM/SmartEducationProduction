using Microsoft.EntityFrameworkCore;
using SmartEducation.dbContext;
using SmartEducation.Entities;
using System.Text.Json;

namespace SmartEducation.Services
{
    public class DetailedNGSSStandardService : IDetailedNGSSStandardService
    {
        private readonly SmartEduDbContext _context;

        public DetailedNGSSStandardService(SmartEduDbContext context)
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
            var standardsFromFile = JsonSerializer.Deserialize<List<NGSS_Detailed_Standard>>(jsonText);

            if (standardsFromFile == null || !standardsFromFile.Any())
            {
                return; // Nothing to process
            }

            foreach (var standardFromFile in standardsFromFile)
            {
                var existingStandard = await _context.NGSS_Detailed_Standard
                    .FirstOrDefaultAsync(s => s.Title_NGSS_Standard == standardFromFile.Title_NGSS_Standard);

                if (existingStandard == null)
                {
                    // If it doesn't exist, add it
                    _context.NGSS_Detailed_Standard.Add(standardFromFile);
                }
                else
                {
                    // If it exists, update its information
                    existingStandard.Matter_Interactions = standardFromFile.Matter_Interactions;
                    existingStandard.Science_Engineering_Practices = standardFromFile.Science_Engineering_Practices;
                    existingStandard.Disciplinary_Core_Ideas = standardFromFile.Disciplinary_Core_Ideas;
                    existingStandard.Crosscutting_Concepts = standardFromFile.Crosscutting_Concepts;
                    existingStandard.Connections_To_Other_DCI = standardFromFile.Connections_To_Other_DCI;
                    existingStandard.Common_Core_State_Standards_Connections = standardFromFile.Common_Core_State_Standards_Connections;
                    existingStandard.Articulation_of_DCIs_across_grade_levels = standardFromFile.Articulation_of_DCIs_across_grade_levels;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
