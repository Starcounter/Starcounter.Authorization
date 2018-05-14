using System;
using Starcounter.Authorization.DatabaseAccess;

namespace Starcounter.Authorization.Tests
{
    public class FakeTransactionFactory : ITransactionFactory
    {
        public T ExecuteTransaction<T>(Func<T> action) => action();
    }
}