﻿using System.Collections.Generic;
using System.Linq;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Inspections.Resources;
using Rubberduck.Inspections.Results;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.VBA;

namespace Rubberduck.Inspections
{
    public sealed class ObsoleteCommentSyntaxInspection : InspectionBase
    {
        /// <summary>
        /// Parameterless constructor required for discovery of implemented code inspections.
        /// </summary>
        public ObsoleteCommentSyntaxInspection(RubberduckParserState state)
            : base(state, CodeInspectionSeverity.Suggestion)
        {
        }

        public override string Meta { get { return InspectionsUI.ObsoleteCommentSyntaxInspectionMeta; } }
        public override string Description { get { return InspectionsUI.ObsoleteCommentSyntaxInspectionName; } }
        public override CodeInspectionType InspectionType { get {return CodeInspectionType.LanguageOpportunities; } }

        public override IEnumerable<InspectionResultBase> GetInspectionResults()
        {
            return State.AllComments.Where(comment => comment.Marker == Tokens.Rem &&
                                            !IsInspectionDisabled(comment.QualifiedSelection.QualifiedName.Component, comment.QualifiedSelection.Selection.StartLine))
                .Select(comment => new ObsoleteCommentSyntaxInspectionResult(this, comment));
        }
    }
}
