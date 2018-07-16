using System;

namespace Starcounter.Authorization.DatabaseAccess
{
    internal interface ITransactionFactory
    {
        /// <summary>
        /// Executes the supplied delegate in a transaction. The delegate can be executed multiple times if any transaction conflicts occur.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The delegate to execute. It will be executed at least once, but possibly multiple times</param>
        /// <returns>The value returned from <paramref name="action"/> on its last execution</returns>
        T ExecuteTransaction<T>(Func<T> action);
    }
}