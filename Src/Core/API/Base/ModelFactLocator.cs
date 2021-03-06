namespace Microsoft.Formula.API
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;
    using System.Threading;

    using Common;
    using Common.Terms;
    using Compiler;
    using Nodes;

    /// <summary>
    /// A ModelFactLocator locates a (sub-) term of a model fact. Sub-locators are expanded on demand and
    /// expansion through symbolic costants is automatic.
    /// </summary>
    internal class ModelFactLocator : Locator
    {
        /// <summary>
        /// The location of this model fact subterm. If this points to a symbolic constant, then holds the
        /// actual span of that symbolic constant. Source node holds the definition of this symbolic constant.
        /// </summary>
        private Span actualSpan;
        
        /// <summary>
        /// A source node that locates the children of this locator.
        /// </summary>
        private Node sourceNode;
        
        /// <summary>
        /// The FactSet where this model fact is held.
        /// </summary>
        private FactSet source;

        /// <summary>
        /// Spin lock for args
        /// </summary>
        private SpinLock argsLock = new SpinLock();

        /// <summary>
        /// Becomes non-null after a child of this locator has been accessed.
        /// </summary>
        private ModelFactLocator[] args;

        /// <summary>
        /// A span that is related to this term.
        /// </summary>
        public override Span Span
        {
            get { return actualSpan; }
        }

        public override int Arity
        {
            get
            {
                if (sourceNode.NodeKind == NodeKind.FuncTerm)
                {
                    return ((FuncTerm)sourceNode).Args.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets the locators of this locators subterms
        /// </summary>
        public override Locator this[int index]
        {
            get
            {
                Contract.Assert(index >= 0 && index < Arity);
                bool gotLock = false;
                try
                {
                    argsLock.Enter(ref gotLock);
                    if (args != null)
                    {
                        return args[index];
                    }

                    var ft = (FuncTerm)sourceNode;
                    args = new ModelFactLocator[ft.Args.Count];
                    int i = 0;
                    foreach (var a in ft.Args)
                    {
                        args[i] = ExpandLocation(a, source);
                        ++i;
                    }

                    return args[index];
                }
                finally
                {
                    if (gotLock)
                    {
                        argsLock.Exit();
                    }
                }
            }
        }

        /// <summary>
        /// Span is the locator for this model fact, and (sourceNode) are used to locate the subterms of this fact.
        /// They can differ is this instance locates a symbolic constant, and (sourceNode) locate the definition of this 
        /// symbolic constant.
        /// </summary>
        internal ModelFactLocator(Span actualSpan, Node sourceNode, FactSet source)
        {
            Contract.Requires(sourceNode != null && source != null);

            if (sourceNode.NodeKind == NodeKind.ModelFact)
            {
                sourceNode = ((ModelFact)sourceNode).Match;
            }

            this.actualSpan = actualSpan;
            this.source = source;
            this.sourceNode = sourceNode;
            args = null;
        }

        private static ModelFactLocator ExpandLocation(Node node, FactSet source)
        {
            if (node.NodeKind == NodeKind.Id && node.CompilerData is UserCnstSymb)
            {
                var cnst = (UserCnstSymb)node.CompilerData;
                if (cnst.IsSymbolicConstant)
                {
                    ModelFactLocator loc;
                    var result = source.TryGetLocator(node.Span, cnst, out loc);
                    Contract.Assert(result);
                    return loc;
                }
            }

            return new ModelFactLocator(node.Span, node, source);
        }
    }
}
