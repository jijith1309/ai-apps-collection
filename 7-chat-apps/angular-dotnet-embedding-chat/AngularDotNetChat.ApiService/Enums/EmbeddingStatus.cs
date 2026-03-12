namespace AngularDotNetChat.ApiService.Enums;

/// <summary>Represents the processing state of a document's embedding.</summary>
public enum EmbeddingStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
