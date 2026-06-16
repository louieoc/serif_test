namespace SearchCore.Services;

public class SearchRequest
{
	public string BillCode { get; set; } = string.Empty;
	public string? Npi { get; set; }
	public string? Ein { get; set; }
}
