using System.ComponentModel.DataAnnotations;

namespace NewApiHelper.Models;

public class UpStreamChannel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Url { get; set; } = string.Empty;

    public double Multiplier { get; set; } = 1.0;

    // 可以添加其他字段，如创建时间等
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}