using SmartEducation.Entities;

namespace SmartEducation.ViewModels
{
    public class StandardViewModel
    {
        public IEnumerable<Grade_Standards> GradeStandards { get; set; }
        public IEnumerable<NGSS_Detailed_Standard> NgssDetailedStandards { get; set; }
    }
}
