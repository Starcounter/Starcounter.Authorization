using Starcounter.Authorization.DatabaseAccess;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Authentication
{
    public class SignOutService<TAuthenticationTicket> : ISignOutService
        where TAuthenticationTicket : class, IScAuthenticationTicket
    {
        private readonly ITransactionFactory _transactionFactory;
        private readonly IAuthenticationTicketProvider<TAuthenticationTicket> _authenticationTicketProvider;
        private readonly IScAuthenticationTicketRepository<TAuthenticationTicket> _authenticationTicketRepository;

        public SignOutService(
            ITransactionFactory transactionFactory,
            IAuthenticationTicketProvider<TAuthenticationTicket> authenticationTicketProvider,
            IScAuthenticationTicketRepository<TAuthenticationTicket> authenticationTicketRepository)
        {
            _transactionFactory = transactionFactory;
            _authenticationTicketProvider = authenticationTicketProvider;
            _authenticationTicketRepository = authenticationTicketRepository;
        }

        public void SignOut()
        {
            var ticket = _authenticationTicketProvider.GetCurrentAuthenticationTicket();
            if (ticket != null)
            {
                _transactionFactory.ExecuteTransaction(() => _authenticationTicketRepository.Delete(ticket));
            }
        }
            
    }
}