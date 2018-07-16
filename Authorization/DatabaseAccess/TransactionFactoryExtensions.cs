using System;

namespace Starcounter.Authorization.DatabaseAccess
{
    internal static class TransactionFactoryExtensions
    {
        /// <summary>
        /// Executes the supplied delegate in a transaction. The delegate can be executed multiple times if any transaction conflicts occur.
        /// </summary>
        /// <param name="transactionFactory"></param>
        /// <param name="action">The delegate to execute. It will be executed at least once, but possibly multiple times</param>
        public static void ExecuteTransaction(this ITransactionFactory transactionFactory, Action action)
        {
            transactionFactory.ExecuteTransaction(() => {
                action();
                return 0;
            });
        }
    }
}