using System;

namespace Starcounter.Authorization.DatabaseAccess
{
    public class StarcounterTransactionFactory : ITransactionFactory
    {
        public T ExecuteTransaction<T>(Func<T> action)
        {
            return Db.Transact(action);
        }
    }
}