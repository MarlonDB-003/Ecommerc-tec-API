using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWorld.Application.Addresses.Commands.CreateAddress;
using TechWorld.Application.Addresses.Commands.DeleteAddress;
using TechWorld.Application.Addresses.Commands.SetDefaultAddress;
using TechWorld.Application.Addresses.Queries.GetMyAddresses;

namespace TechWorld.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyAddresses(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyAddressesQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetMyAddresses), result);
    }

    [HttpPatch("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken ct)
    {
        await mediator.Send(new SetDefaultAddressCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteAddressCommand(id), ct);
        return NoContent();
    }
}
