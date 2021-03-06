namespace Microsoft.Formula.API
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using Nodes;
    using Common;

    public sealed class ParseResult
    {
        private List<Flag> flags = new List<Flag>();

        public AST<Program> Program
        {
            get;
            private set;
        }

        public ImmutableCollection<Flag> Flags
        {
            get;
            private set;
        }

        public bool Succeeded
        {
            get;
            private set;
        }

        public ProgramName Name
        {
            get;
            private set;
        }

        internal ParseResult(Program program)
        {
            Name = program.Name;
            Program = new ASTConcr<Program>(program, false);
            Flags = new ImmutableCollection<Flag>(flags);
            Succeeded = true;
        }

        /// <summary>
        /// This constructor is for parsing data terms and rules.
        /// </summary>
        internal ParseResult()
        {
            Name = new API.ProgramName("dummy.4ml");
            Program = new ASTConcr<Program>(new Program(Name), false);
            Flags = new ImmutableCollection<Flag>(flags);
            Succeeded = true;
        }

        internal void AddFlag(Flag f)
        {
            Succeeded = Succeeded && f.Severity != SeverityKind.Error;
            flags.Add(f);
        }

        internal void ClearFlags()
        {
            flags.Clear();
            Succeeded = true;
        }
    }
}
