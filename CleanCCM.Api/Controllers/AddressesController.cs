using AutoMapper;
using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Addresses.Commands;
using CleanCCM.Shared.Addresses.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

public class AddressesController : BaseApiController
{
    private readonly IMapper _mapper;

    public AddressesController(IMapper mapper)
    {
        _mapper = mapper;
    }

    // GET: api/addresses/by-user?userId=...
    [HttpGet("by-user")]
    public async Task<IActionResult> GetByUser([FromQuery] string userId)
    {
        var result = await Mediator.Send(new GetAddressByUserIdQuery(userId));
        return HandleResult(result);
    }

    // GET: api/addresses/by-product?productId=...
    [HttpGet("by-product")]
    public async Task<IActionResult> GetByProduct([FromQuery] Guid productId)
    {
        var result = await Mediator.Send(new GetAddressByProductIdQuery(productId));
        return HandleResult(result);
    }

    // POST: api/addresses/create
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateAddressRequest request)
    {
        var command = _mapper.Map<CreateAddressCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    // POST: api/addresses/update
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateAddressRequest request)
    {
        var command = _mapper.Map<UpdateAddressCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new DeleteAddressCommand(id));
        return HandleResult(result);
    }
}
