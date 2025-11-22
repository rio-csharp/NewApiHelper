using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewApiHelper.Models;

public enum TestResultStatus
{
    Untested = 0,
    Success = 1,
    Failed = 2,
    Skipped = 3
}

public class ModelTestResult
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ModelSyncId { get; set; }

    public DateTime TestTime { get; set; } = DateTime.Now;

    public TestResultStatus Status { get; set; } = TestResultStatus.Untested;

    public string? ErrorMessage { get; set; }

    public string TestType { get; set; } = string.Empty; // e.g., "Test" or "TestFailed"

    // 导航属性
    public ModelSync? ModelSync { get; set; }
}