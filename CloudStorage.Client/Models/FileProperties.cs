namespace CloudStorage.Client.Models;

public record FileProperties(string Path, DateTime CreatedDate, DateTime ModifiedDate, DateTime UploadedDate, decimal Size);