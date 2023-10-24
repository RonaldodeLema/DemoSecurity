namespace Infrastructure;

public class DataResponse
{
    public int Code { get; set; }
    public string Message { get; set; }
    public List<Cipher>? Data { get; set; }
}