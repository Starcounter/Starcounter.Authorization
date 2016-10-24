using Simplified.Ring2;

namespace Starcounter.Authorization.Core
{
    public class PersonClaim : Claim
    {
        public Person Person { get; private set; }

        public PersonClaim(Person person)
        {
            Person = person;
        }
    }
}