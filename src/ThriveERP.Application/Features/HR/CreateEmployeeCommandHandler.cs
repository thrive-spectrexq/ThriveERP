namespace ThriveERP.Application.Features.HR;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThriveERP.Application.Common.Interfaces;
using ThriveERP.Domain.Entities;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Guid>
{
    private readonly IEmployeeRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandHandler(
        IEmployeeRepository repository, 
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        Guid? userId = null;

        if (request.CreateUserAccount && !string.IsNullOrWhiteSpace(request.Username) && !string.IsNullOrWhiteSpace(request.Password) && request.RoleId.HasValue)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = _passwordHasher.Hash(request.Password),
                FullName = request.FullName,
                Email = request.Email,
                RoleId = request.RoleId.Value,
                IsActive = true
            };
            await _userRepository.AddAsync(user, cancellationToken);
            userId = user.Id;
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = request.FullName,
            Position = request.Position,
            Phone = request.Phone,
            Email = request.Email,
            HireDate = request.HireDate,
            IsActive = true
        };

        await _repository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}
