namespace ThriveERP.Application.Features.Auth;
using MediatR;
public record LoginCommand(string Username, string Password) : IRequest<LoginResultDto>;
