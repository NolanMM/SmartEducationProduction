namespace SmartEducation.ViewModels
{
    public class DashboardViewModel
    {
        // Key Metrics
        public int TotalUsers { get; set; }
        public int TotalOrganizations { get; set; }
        public int? TotalApiTokensUsed { get; set; }
        public int TotalNgssStandards { get; set; }
        public int TotalGradeStandards { get; set; }
        public int TotalKidsRegistered { get; set; }
        public long TotalRecommendations { get; set; }

        // Chart Data
        public ChartData UserRoleDistribution { get; set; }
        public ChartData RecommendationsLast7Days { get; set; }

        public ChartData TokensUsedLast7Days { get; set; }

        // Table Data
        public List<OrganizationUserCount> OrganizationUserCounts { get; set; }
    }

    public class ChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Data { get; set; } = new List<int>();
    }

    public class OrganizationUserCount
    {
        public string OrganizationName { get; set; }
        public int UserCount { get; set; }
    }
}
