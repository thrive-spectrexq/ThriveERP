namespace ThriveERP.Application.Features.Returns;

using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;
using ThriveERP.Domain.Enums;

public class ProcessReturnCommandHandler : IRequestHandler<ProcessReturnCommand, bool>
{
    private readonly ISalesOrderRepository _salesRepository;
    private readonly IStockLevelRepository _stockRepository;
    private readonly IRepository<Return> _returnRepository;
    private readonly IRepository<Expense> _expenseRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessReturnCommandHandler(
        ISalesOrderRepository salesRepository,
        IStockLevelRepository stockRepository,
        IRepository<Return> returnRepository,
        IRepository<Expense> expenseRepository,
        IRepository<Account> accountRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _salesRepository = salesRepository;
        _stockRepository = stockRepository;
        _returnRepository = returnRepository;
        _expenseRepository = expenseRepository;
        _accountRepository = accountRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ProcessReturnCommand request, CancellationToken cancellationToken)
    {
        var salesOrder = await _salesRepository.GetWithItemsAsync(request.SalesOrderId);
        if (salesOrder == null) return false;

        var saleItem = salesOrder.SaleItems.FirstOrDefault(x => x.ProductId == request.ProductId);
        if (saleItem == null) return false;

        if (request.Quantity <= 0 || request.Quantity > saleItem.Quantity) return false;

        decimal refundAmount = request.Quantity * saleItem.UnitPrice; // Ignoring discount logic for simplicity

        // 1. Log the return
        var returnEntry = new Return
        {
            SalesOrderId = request.SalesOrderId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            RefundAmount = refundAmount,
            Reason = request.Reason,
            ProcessedByUserId = _currentUser.UserId ?? Guid.Empty,
            ProcessedAtUtc = DateTime.UtcNow
        };
        await _returnRepository.AddAsync(returnEntry, cancellationToken);

        // 2. Adjust stock
        var stockLevel = await _stockRepository.GetByProductAndWarehouseAsync(request.ProductId, salesOrder.WarehouseId);
        if (stockLevel != null)
        {
            stockLevel.AdjustQuantity(request.Quantity);
            _stockRepository.Update(stockLevel);
            
            salesOrder.AddDomainEvent(new ThriveERP.Domain.Events.StockLevelBelowThresholdEvent(request.ProductId, salesOrder.WarehouseId, stockLevel.QuantityOnHand, 0)); // Simplified event for trigger
        }

        // 3. Log a refund expense if applicable (optional simplified accounting)
        var accounts = await _accountRepository.GetAllAsync(cancellationToken);
        var expenseAccount = accounts.FirstOrDefault(x => x.Type == AccountType.Expense);
        
        if (expenseAccount != null)
        {
            var expense = new Expense
            {
                AccountId = expenseAccount.Id,
                Category = "Customer Refund",
                Amount = refundAmount,
                Description = $"Refund for Order {salesOrder.OrderNumber}",
                ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow),
                RecordedByUserId = _currentUser.UserId ?? Guid.Empty
            };
            await _expenseRepository.AddAsync(expense, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
