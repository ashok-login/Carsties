using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
                    .Include(x => x.Item)
                    .FirstOrDefaultAsync(x => x.Id == id);
        if(auction == null) return NotFound();
        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuctionDto(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        // TODO: Add current user as seller
        auction.Seller = "test";
        _context.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if(!result)
        {
            return BadRequest(error: "Could not save changes to the DB");
        }
        return CreatedAtAction(
            nameof(GetAuctionById),
            new { auction.Id },
            _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionToUpdate)
    {
        var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);
        if(auction == null)
        {
            return NotFound();
        }
        // TODO: Check seller == username
        auction.Item.Make = auctionToUpdate.Make ?? auction.Item.Make;
        auction.Item.Model = auctionToUpdate.Model ?? auction.Item.Model;
        auction.Item.Color = auctionToUpdate.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionToUpdate.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = auctionToUpdate.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;

        if(result) return Ok();

        return BadRequest("Problem saving changes");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions
                .FindAsync(id);
        if(auction == null)
        {
            return NotFound();
        }
        // TODO: Check seller == username
        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if(!result)
        {
            return BadRequest("Could not update DB");
        }
        return Ok()`;
    }
}
