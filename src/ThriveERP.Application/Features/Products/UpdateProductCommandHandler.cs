using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Domain.Exceptions;

namespace ThriveERP.Application.Features.Products;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
            throw new EntityNotFoundException(nameof(Product), request.Id);

        product.Sku = request.Sku;
        product.Barcode = request.Barcode;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.CostPrice = request.CostPrice;
        product.SellingPrice = request.SellingPrice;
        product.TrackBatches = request.TrackBatches;
        product.ReorderThreshold = request.ReorderThreshold;
        product.IsActive = request.IsActive;

        _repository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }
}
