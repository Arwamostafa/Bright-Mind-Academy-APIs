using Domain.Common;
using Domain.Models;
using E_LearningPlatform.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contract;

namespace E_LearningPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class TrackController : ControllerBase
    {
        private readonly ITrackService newTrack;
        public TrackController(ITrackService _track)
        {
            newTrack = _track;
        }

        [HttpGet]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> GetAllTracks(CancellationToken cancellationToken)
        {
            var response = await newTrack.GetAllTracksAsync(cancellationToken);
            return Ok(response);
        }

        [HttpGet("page")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> GetPageOfTracks([FromQuery] RequestFilters requestFilters, CancellationToken cancellationToken)
        {
            var response = await newTrack.GetPageOfTracksAsync(requestFilters, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> GetTrackById(int id, CancellationToken cancellationToken)
        {
            var result = await newTrack.GetTrackByIdAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("{name:alpha}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> GetTrackByName(string name, CancellationToken cancellationToken)
        {
            var result = await newTrack.GetTrackByNameAsync(name, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost]
        //[Authorize(Roles = ("Admin"))]
        public async Task<IActionResult> PostTrack([FromBody] Track addedTrack, CancellationToken cancellationToken)
        {
            var result = await newTrack.AddTrackAsync(addedTrack, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = ("Admin"))]
        public async Task<IActionResult> UpdateTrack(int id, [FromBody] Track upTrack, CancellationToken cancellationToken)
        {
            var result = await newTrack.UpdateTrackByIdAsync(id, upTrack, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = ("Admin"))]
        public async Task<IActionResult> DeleteTrackById(int id, CancellationToken cancellationToken)
        {
            var result = await newTrack.RemoveTrackByIdAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
