using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    internal class SignOutService<TAuthenticationTicket> : ISignOutService
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        private readonly ITransactionFactory _transactionFactory;
        private readonly IAuthenticationTicketService<TAuthenticationTicket> _authenticationTicketService;
        private readonly IScAuthenticationTicketRepository<TAuthenticationTicket> _authenticationTicketRepository;

        public SignOutService(
            ITransactionFactory transactionFactory,
            IAuthenticationTicketService<TAuthenticationTicket> authenticationTicketService,
            IScAuthenticationTicketRepository<TAuthenticationTicket> authenticationTicketRepository)
        {
            _transactionFactory = transactionFactory;
            _authenticationTicketService = authenticationTicketService;
            _authenticationTicketRepository = authenticationTicketRepository;
        }

        public void SignOut()
        {
            var ticket = _authenticationTicketService.GetCurrentAuthenticationTicket();
            if (ticket != null)
            {
                _transactionFactory.ExecuteTransaction(() =>
                {
                    _authenticationTicketRepository.Delete(ticket);
                });
            }
        }
            
    }
}