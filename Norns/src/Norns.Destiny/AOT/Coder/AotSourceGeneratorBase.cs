﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Norns.Destiny.Abstraction.Coder;
using Norns.Destiny.Notations;
using Norns.Destiny.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Norns.Destiny.AOT.Coder
{
    public abstract class AotSourceGeneratorBase : ISourceGenerator
    {
        protected abstract INotationGenerator CreateNotationGenerator();

        protected virtual bool FilterSyntaxNode(SyntaxNode syntaxNode)
        {
            return syntaxNode is TypeDeclarationSyntax;
        }

        protected virtual ISymbolSource CreateGenerateSymbolSource(IEnumerable<SyntaxNode> syntaxNodes, SourceGeneratorContext context)
        {
            return new AotSyntaxNodeSymbolSource(syntaxNodes, context);
        }

        protected virtual SourceText CreateSourceText(INotation notation)
        {
            var sb = new StringBuilder();
            notation.Record(sb);
            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }

        public void Execute(SourceGeneratorContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                return;
            var source = CreateGenerateSymbolSource(receiver.SyntaxNodes, context);
            var notations = CreateNotationGenerator().GenerateNotations(source);
            context.AddSource(RandomUtils.NewCSFileName(), CreateSourceText(notations));
        }

        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver(FilterSyntaxNode));
        }

        internal class SyntaxReceiver : ISyntaxReceiver
        {
            private readonly Func<SyntaxNode, bool> filter;

            internal List<SyntaxNode> SyntaxNodes { get; } = new List<SyntaxNode>();

            public SyntaxReceiver(Func<SyntaxNode, bool> filter)
            {
                this.filter = filter;
            }

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (filter(syntaxNode))
                {
                    SyntaxNodes.Add(syntaxNode);
                }
            }
        }
    }
}