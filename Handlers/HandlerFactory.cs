using VendingMachine.Models;

namespace VendingMachine.Handlers;

public static class HandlerFactory
{
    public static IRoleHandler Create(UserRoles role, VendingMachine machine)
    {
        switch (role)
        {
            case UserRoles.Admin:
                return new AdminHandler(machine);
            case UserRoles.Customer:
                return new CustomerHandler(machine);
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }
    }
}