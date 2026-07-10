using System;
using MediatR;

namespace ThriveERP.Application.Features.Products;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;
