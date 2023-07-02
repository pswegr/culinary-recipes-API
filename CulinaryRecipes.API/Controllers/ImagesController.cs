using Amazon.Runtime.Internal;
using AutoMapper;
using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryRecipes.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public ImagesController(IImageService imageService, IMapper mapper)
        {
            _imageService = imageService;
            _mapper = mapper;
        }

        [HttpGet("GetImages")]
        public async Task<ActionResult<IEnumerable<ImageResource>>> GetImages()
        {
            var image = await _imageService.GetImagesAsync();

            var imageResources = _mapper.Map<IEnumerable<ImageResource>>(image);

            return Ok(imageResources);
        }

        [HttpPost("AddImage")]
        public async Task<IActionResult> UploadImageAsync([FromForm] UploadImageResource file)
        {
            if (file is null || file.Image.Length == 0)
            {
                return BadRequest("Image is not provided");
            }

            var uploadImage = _mapper.Map<UploadImage>(file);

            var image = await _imageService.UploadImageAsync(uploadImage);

            var imageResource = _mapper.Map<ImageResource>(image);

            return Created(imageResource.Url, imageResource);
        }
    }
}
