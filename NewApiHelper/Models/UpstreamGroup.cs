using System.ComponentModel.DataAnnotations;

namespace NewApiHelper.Models;

public class UpstreamGroup
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int UpstreamId { get; set; }

    public double GroupRatio { get; set; } = 1.0;

    [Required]
    public string Key { get; set; } = string.Empty;

    // 导航属性
    public Upstream? Upstream { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}