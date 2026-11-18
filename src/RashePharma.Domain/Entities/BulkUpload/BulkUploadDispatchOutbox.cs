namespace RashePharma.Domain.Entities.BulkUpload;
public sealed class BulkUploadDispatchOutbox
{
 public Guid Id {get;set;}=Guid.NewGuid(); public int BulkUploadJobId {get;set;}
 public string EventType {get;set;}="BulkUploadRequested"; public int SchemaVersion {get;set;}=1;
 public string PayloadJson {get;set;}="{}"; public string Status {get;set;}=BulkUploadDispatchStatus.Pending;
 public DateTime CreatedAtUtc {get;set;}=DateTime.UtcNow; public DateTime? PublishedAtUtc {get;set;}
 public int AttemptCount {get;set;} public DateTime? NextAttemptAtUtc {get;set;} public DateTime? LeaseUntilUtc {get;set;}
 public string? LastError {get;set;} public byte[] RowVersion {get;set;}=[];
 public RashePharma.Domain.Entities.BulkUploadJob BulkUploadJob {get;set;}=null!;
}