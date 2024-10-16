using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NsfwSpyNS;
using PLM.api.Data;
using PLM.api.Models.Domain;
using PLM.api.Models.DTO;
using System.Security.Claims;

namespace PLM.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly PLMDbContext pLMDbContext;
        private readonly INsfwSpy nsfwSpy;
        private readonly UserManager<IdentityUser> userManager;

        public MediaController(PLMDbContext pLMDbContext, INsfwSpy nsfwSpy, UserManager<IdentityUser> userManager)
        {
            this.pLMDbContext = pLMDbContext;
            this.nsfwSpy = nsfwSpy;
            this.userManager = userManager;
        }
        [HttpPost]
        [Route("AddMedia")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddMedia([FromForm] MediaDTO model)
        {
            try
            {
                IFormFile file = model.fildata;
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Invalid file");
                }

                byte[] fileData;
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    fileData = ms.ToArray();
                }

                // Get the authenticated user using UserManager
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("Could not determine the uploader's identity.");
                }

                // Find the user in the regular database by email
                var regularUser = await pLMDbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (regularUser == null)
                {
                    return NotFound("User not found in the regular database.");
                }

                // Check if the file is an image
                if (IsImage(file.ContentType))
                {
                    var result = nsfwSpy.ClassifyImage(fileData);

                    // Check if the image is NSFW
                    if (result.IsNsfw)
                    {
                        return BadRequest("The uploaded image contains nude content and cannot be processed.");
                    }
                }

                // Upload the media
                var media = new Media
                {
                    Title = model.Title,
                    Description = model.Description,
                    FileData = fileData,
                    UploaderId = regularUser.Id, // Use the regular database user ID
                    Status = MediaStatus.Pending, // Set status to pending
                    ApprovedById = null // Initially, the media is not approved
                };

                await pLMDbContext.Media.AddAsync(media);
                await pLMDbContext.SaveChangesAsync();

                return Ok("Media added successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        private bool IsImage(string contentType)
        {

            var acceptedImageTypes = new string[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/jpg", "image/webp" };


            return acceptedImageTypes.Contains(contentType); 
        }

        [HttpGet]
        [Route("GetAllMedia")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAllMedia()
        {
            try
            {
                // Retrieve all media from the database
                var mediaList = await pLMDbContext.Media
                    .Include(m => m.Uploader) // Include uploader info
                    .Include(m => m.ApprovedBy) // Include approver info
                    .ToListAsync();

                // Optionally, map to DTOs to avoid returning entire domain models
                var mediaDTOList = mediaList.Select(media => new GetMediaDTO
                {
                    Title = media.Title,
                    Description = media.Description,
                    Status = media.Status.ToString(), // Enum to string conversion
                    UploaderId = media.UploaderId,
                    ApprovedById = media.ApprovedById,
                    // Add other fields as needed
                }).ToList();

                return Ok(mediaDTOList); // Return the list of media
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        [Route("GetMediaById")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetMediaById(int id)
        {
            try
            {
                // Find the media by its ID
                var media = await pLMDbContext.Media
                    .Include(m => m.Uploader) // Optionally include uploader information
                    .Include(m => m.ApprovedBy) // Optionally include approver information
                    .FirstOrDefaultAsync(m => m.MediaId == id);

                if (media == null)
                {
                    return NotFound("Media not found");
                }

                // Optionally map the media to a DTO
                var mediaDTO = new GetMediaDTO
                {
                    Title = media.Title,
                    Description = media.Description,
                    Status = media.Status.ToString(), // Enum to string
                    UploaderId = media.UploaderId,
                    ApprovedById = media.ApprovedById,
                    // Add other fields as necessary
                };

                return Ok(mediaDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        [Route("GetPendingMedia")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingMedia()
        {
            try
            {
                // Retrieve all media items with the "Pending" status
                var pendingMediaList = await pLMDbContext.Media
                    .Where(m => m.Status == MediaStatus.Pending)
                    .Include(m => m.Uploader) // Optionally include uploader information
                    .Include(m => m.ApprovedBy) // Optionally include approver information (null in this case)
                    .ToListAsync();

                // Map to DTO
                var pendingMediaDTOList = pendingMediaList.Select(media => new GetMediaDTO
                {
                    Title = media.Title,
                    Description = media.Description,
                    Status = media.Status.ToString(), // Enum to string conversion
                    UploaderId = media.UploaderId,
                    ApprovedById = media.ApprovedById, // Should be null for pending items
                                                       // Add other fields as needed
                }).ToList();

                return Ok(pendingMediaDTOList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        [Route("GetRejectedMedia")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRejectedMedia()
        {
            try
            {
                // Retrieve all media items with the "Rejected" status
                var rejectedMediaList = await pLMDbContext.Media
                    .Where(m => m.Status == MediaStatus.Rejected)
                    .Include(m => m.Uploader) // Optionally include uploader information
                    .Include(m => m.ApprovedBy) // Optionally include approver information (can be null if rejected)
                    .ToListAsync();

                // Map to DTO
                var rejectedMediaDTOList = rejectedMediaList.Select(media => new GetMediaDTO
                {
                    Title = media.Title,
                    Description = media.Description,
                    Status = media.Status.ToString(), // Enum to string conversion
                    UploaderId = media.UploaderId,
                    ApprovedById = media.ApprovedById, // Might be null in case of rejection
                                                       // Add other fields as needed
                }).ToList();

                return Ok(rejectedMediaDTOList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        [Route("GetAcceptedMedia")]
        [Authorize]
        public async Task<IActionResult> GetAcceptedMedia()
        {
            try
            {
                // Retrieve all media items with the "Accepted" status
                var acceptedMediaList = await pLMDbContext.Media
                    .Where(m => m.Status == MediaStatus.Accepted)
                    .Include(m => m.Uploader) // Optionally include uploader information
                    .Include(m => m.ApprovedBy) // Optionally include approver information
                    .ToListAsync();

                // Map to DTO
                var acceptedMediaDTOList = acceptedMediaList.Select(media => new GetMediaDTO
                {
                    Title = media.Title,
                    Description = media.Description,
                    Status = media.Status.ToString(), // Enum to string conversion
                    UploaderId = media.UploaderId,
                    ApprovedById = media.ApprovedById, // Should have an approver if accepted
                                                       // Add other fields as needed
                }).ToList();

                return Ok(acceptedMediaDTOList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPatch]
        [Route("ApproveOrRejectMedia")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveOrRejectMedia(int mediaId, [FromBody] ApproveOrRejectDTO model)
        {
            try
            {
                // Find the media by its ID
                var media = await pLMDbContext.Media.FirstOrDefaultAsync(m => m.MediaId == mediaId);
                if (media == null)
                {
                    return NotFound("Media not found.");
                }

                // Update the media's status based on the model input
                if (model.IsApproved)
                {
                    media.Status = MediaStatus.Accepted;
                }
                else
                {
                    media.Status = MediaStatus.Rejected;
                }


                // Set the ApprovedById to the current admin's ID (authenticated user)
                // Get the email of the authenticated admin
                var adminEmail = User.FindFirstValue(ClaimTypes.Email);

                // Find the admin in the regular database by email
                var adminUser = await pLMDbContext.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
                if (adminUser == null)
                {
                    return NotFound("Admin user not found in the regular database.");
                }

                // Set the admin ID as the ApprovedById
                media.ApprovedById = adminUser.Id; // Assuming adminUser.Id is an integer


                // Save changes to the database
                pLMDbContext.Media.Update(media);
                await pLMDbContext.SaveChangesAsync();

                return Ok("Media status updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} - {ex.StackTrace}");
            }
        }
        [HttpDelete]
        [Route("DeleteMedia")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMedia(int mediaId)
        {
            try
            {
                // Find the media by ID
                var media = await pLMDbContext.Media.FindAsync(mediaId);

                if (media == null)
                {
                    return NotFound("Media not found.");
                }

                // Remove the media from the database
                pLMDbContext.Media.Remove(media);
                await pLMDbContext.SaveChangesAsync();

                return Ok("Media deleted successfully.");
            }
            catch (Exception ex)
            {
                // Log the error (you can add logging here if needed)
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


    }
}
