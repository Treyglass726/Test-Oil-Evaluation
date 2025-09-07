namespace Backend.Models;

public class DailyForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string Summary { get; set; } = string.Empty;
    public bool IsDaytime { get; set; }
}