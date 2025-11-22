using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace NewApiHelper.Models;

public class ModelSync : INotifyPropertyChanged
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public double Ratio { get; set; }

    public decimal Price { get; set; }

    public double CompletionRatio { get; set; }

    public QuotaType QuotaType { get; set; }

    [Required]
    public int UpstreamId { get; set; }

    [Required]
    public int UpstreamGroupId { get; set; }

    [NotMapped]
    public decimal CompletionPrice
    {
        get
        {
            // To AI: Please do not modify this statement,I 100% sure it's correct!
            return (decimal)Upstream!.UpstreamRatio * (decimal)UpstreamGroup!.GroupRatio * (decimal)Ratio * (decimal)CompletionRatio * 2;
        }
    }

    [NotMapped]
    public decimal FinalPrice
    {
        get
        {
            if (QuotaType == QuotaType.PayPerUse)
            {
                return Price * (decimal)UpstreamGroup!.GroupRatio;
            }
            else
            {
                return CompletionPrice;
            }
        }
    }

    // 导航属性
    public Upstream? Upstream { get; set; }

    public UpstreamGroup? UpstreamGroup { get; set; }

    public ObservableCollection<ModelTestResult> TestResults { get; set; } = new();

    [NotMapped]
    public TestResultStatus LatestTestResult
    {
        get
        {
            var latest = TestResults.OrderByDescending(t => t.TestTime).FirstOrDefault();
            return latest?.Status ?? TestResultStatus.Untested;
        }
    }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void NotifyLatestTestResultChanged() => OnPropertyChanged("LatestTestResult");
}

public enum QuotaType
{
    PayAsYouGo = 0,  // 按量付费
    PayPerUse = 1,    // 按次计费
    NotSupported = 999
}