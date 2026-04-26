using Dictator.Domain.Shared.LegalTerms;
using System;

namespace Dictator.Domain.Actors.Types
{
    public class Actor
    {
        public Guid Id { get; }
        public SubjectType Subject { get; }

        public Actor(Guid id, SubjectType subject)
        {
            Id = id;
            Subject = subject;
        }
    }
}