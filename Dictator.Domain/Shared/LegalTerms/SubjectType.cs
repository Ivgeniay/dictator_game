using Dictator.Domain.Utils;

namespace Dictator.Domain.Shared.LegalTerms
{
    /// <summary>
    /// Тип субъекта
    /// </summary>
    public class SubjectType : StringType
    {
        public SubjectType() : base() { }
        public SubjectType(string value) : base (value) {}

        public static readonly SubjectType People     = new SubjectType(Constants.Subjects.People);
        public static readonly SubjectType Officials  = new SubjectType(Constants.Subjects.Officials);
        public static readonly SubjectType Media      = new SubjectType(Constants.Subjects.Media);
        public static readonly SubjectType Oligarchs  = new SubjectType(Constants.Subjects.Oligarchs);
        public static readonly SubjectType Military   = new SubjectType(Constants.Subjects.Military);
        public static readonly SubjectType Police     = new SubjectType(Constants.Subjects.Police);
        public static readonly SubjectType Citizens   = new SubjectType(Constants.Subjects.Citizens);
        public static readonly SubjectType Business   = new SubjectType(Constants.Subjects.Business);
    }
}
