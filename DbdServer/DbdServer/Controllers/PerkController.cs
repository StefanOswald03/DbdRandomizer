using Microsoft.AspNetCore.Mvc;
using Core.Contracts;
using System.ComponentModel.DataAnnotations;
using Core.Entities;

namespace UserManager.Api.Controllers
{
    #region DTOs
    public record PerkGetDto(string Name, string Description, string ImageURL, int page);
    #endregion

    /// <summary>
    /// API-Controller to get Random Perks
    /// </summary>
    [Route("api/perks")]
    [ApiController]
    public class PerkController : ControllerBase
    {
        private IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public PerkController(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get 4 random perks for a specific role
        /// </summary>
        /// <param name="role">The role the perks should fit to (survivor/killer)</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("{role}", Name = nameof(GetRandom))]
        public async Task<ActionResult<PerkGetDto[]>> GetRandom(string role)
        {
            if (role != "survivor" && role != "killer")
            {
                return BadRequest("Invalid Role!");
            }

            var randomPerks = await UnitOfWork.Perk.GetFourRandomAsync(role);
            if (randomPerks.Length != 4)
            {
                return BadRequest("No 4 perks found!");
            }
            //BUG: Image URL not Working
            var perkGetDtos = randomPerks.Select(p => new PerkGetDto(p.Name, p.Description, p.ImageUrl, p.Page));


            return Ok(perkGetDtos);
        }

    }
}
