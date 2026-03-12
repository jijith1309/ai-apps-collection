using System.Security.Claims;
using AngularDotNetChat.ApiService.Common;
using AngularDotNetChat.ApiService.DTOs;
using AngularDotNetChat.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularDotNetChat.ApiService.Controllers;

/// <summary>Manages document uploads and retrieval. Requires authentication.</summary>
[ApiController]
[Route("api/documents")]
[Authorize]
[Produces("application/json")]
public class DocumentController(IDocumentService documentService) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Uploads a PDF, DOC, or DOCX file and triggers background embedding generation.</summary>
    /// <param name="file">The document file to upload (PDF, DOC, DOCX — max 20 MB).</param>
    /// <returns>Metadata of the created document including its embedding status.</returns>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ServiceResponse<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<DocumentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<DocumentDto>>> Upload(IFormFile file)
    {
        var result = await documentService.UploadAsync(file, CurrentUserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Retries embedding generation for a document that previously failed.</summary>
    /// <param name="id">The document ID to retry.</param>
    /// <returns>Updated document metadata with reset embedding status.</returns>
    [HttpPost("{id:int}/retry-embedding")]
    [ProducesResponseType(typeof(ServiceResponse<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<DocumentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<DocumentDto>>> RetryEmbedding(int id)
    {
        var result = await documentService.RetryEmbeddingAsync(id, CurrentUserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Returns all documents uploaded by the authenticated user.</summary>
    /// <returns>List of documents with their embedding status.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ServiceResponse<List<DocumentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<List<DocumentDto>>>> GetAll()
    {
        var result = await documentService.GetAllAsync(CurrentUserId);
        return Ok(result);
    }
}
