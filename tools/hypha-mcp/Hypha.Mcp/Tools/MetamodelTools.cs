// ------------------------------------------------------------------------------------------------
// <copyright file="MetamodelTools.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Mcp.Tools
{
    using System;
    using System.ComponentModel;

    using Hypha.Knowledge.Query;

    using ModelContextProtocol;
    using ModelContextProtocol.Server;

    /// <summary>
    /// The tools that answer structural questions about a release's metamodel: a metaclass, a feature,
    /// what a type can be held by, and a package.
    /// </summary>
    /// <remarks>
    /// Each tool groups the <see cref="IKnowledgeQuery"/> questions about one subject behind a
    /// <c>question</c> argument, which keeps the server's tool count low; every answer is the same JSON
    /// document <c>hypha query --json</c> prints for that question.
    /// </remarks>
    [McpServerToolType]
    public sealed class MetamodelTools
    {
        /// <summary>The description of every tool's <c>tag</c> argument.</summary>
        private const string TagDescription = "The installed release to answer from, such as 2026-05. The default release when omitted.";

        /// <summary>Answers the questions.</summary>
        private readonly IKnowledgeQuery query;

        /// <summary>Initializes a new instance of the <see cref="MetamodelTools"/> class.</summary>
        /// <param name="query">Answers the questions.</param>
        /// <exception cref="ArgumentNullException"><paramref name="query"/> is <c>null</c>.</exception>
        public MetamodelTools(IKnowledgeQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            this.query = query;
        }

        /// <summary>Answers a question about one metaclass.</summary>
        /// <param name="question">What to answer.</param>
        /// <param name="name">The metaclass, or the enumeration for <c>literals</c>.</param>
        /// <param name="general">The supertype, for <c>specializes</c>.</param>
        /// <param name="feature">Only the feature of this name, for <c>features</c>.</param>
        /// <param name="concreteOnly">Only the metaclasses that are not abstract, for <c>subclasses</c>.</param>
        /// <param name="includeInherited">Also what the metaclass inherits, for <c>constraints</c> and <c>operations</c>.</param>
        /// <param name="tag">The release; the default release when <c>null</c>.</param>
        /// <returns>The result, with its grounding, as JSON.</returns>
        /// <exception cref="McpException">The question could not be answered; the message says why.</exception>
        [McpServerTool(Name = "hypha_metaclass", Title = "Metaclass", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Answers a question about one SysML v2 or KerML metaclass: 'element' (its index entry: kind, package, "
            + "supertypes, page), 'ancestors' (every supertype), 'subclasses' (what specializes it), 'specializes' "
            + "(whether it specializes 'general', and through which supertypes), 'neighbours' (the metaclasses its "
            + "features are typed by, and the features typed by it), 'constraints' and 'operations' (each naming where "
            + "it is declared), 'literals' (of an enumeration), 'features' (owned and inherited, each naming where it "
            + "comes from) or 'effective-features' (without the inherited ones it redefines).")]
        public string Metaclass(
            [Description("What to answer.")] MetaclassQuestion question,
            [Description("The metaclass, such as PartUsage; for 'literals', the enumeration, such as VisibilityKind.")] string name,
            [Description("For 'specializes': the supertype, such as Feature.")] string? general = null,
            [Description("For 'features': only the feature of this name.")] string? feature = null,
            [Description("For 'subclasses': only the metaclasses that are not abstract.")] bool concreteOnly = false,
            [Description("For 'constraints' and 'operations': also those the metaclass inherits.")] bool includeInherited = true,
            [Description(TagDescription)] string? tag = null) =>
            question switch
            {
                MetaclassQuestion.Element => Answer.Of(() => this.query.Element(name, tag)),
                MetaclassQuestion.Ancestors => Answer.Of(() => this.query.Ancestors(name, tag)),
                MetaclassQuestion.Subclasses => Answer.Of(() => this.query.Subclasses(name, concreteOnly, tag)),
                MetaclassQuestion.Specializes => Answer.Of(
                    () => this.query.Specializes(name, Answer.Required(general, "general", "specializes"), tag)),
                MetaclassQuestion.Neighbours => Answer.Of(() => this.query.Neighbours(name, tag)),
                MetaclassQuestion.Constraints => Answer.Of(() => this.query.Constraints(name, includeInherited, tag)),
                MetaclassQuestion.Operations => Answer.Of(() => this.query.Operations(name, includeInherited, tag)),
                MetaclassQuestion.Literals => Answer.Of(() => this.query.Literals(name, tag)),
                MetaclassQuestion.Features => Answer.Of(() => this.query.Features(name, feature, tag)),
                MetaclassQuestion.EffectiveFeatures => Answer.Of(() => this.query.EffectiveFeatures(name, tag)),
                _ => throw new McpException($"'{question}' is not a question hypha_metaclass answers."),
            };

        /// <summary>Answers a question about one feature.</summary>
        /// <param name="question">What to answer.</param>
        /// <param name="feature">The feature's name.</param>
        /// <param name="class">
        /// The metaclass the feature belongs to: required for <c>opposite</c>, <c>feature-constraints</c>
        /// and <c>contributors</c>; for <c>redefinitions</c> and <c>subsettings</c>, it narrows the answer
        /// to the feature this metaclass owns.
        /// </param>
        /// <param name="tag">The release; the default release when <c>null</c>.</param>
        /// <returns>The result, with its grounding, as JSON.</returns>
        /// <exception cref="McpException">The question could not be answered; the message says why.</exception>
        [McpServerTool(Name = "hypha_feature", Title = "Feature", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Answers a question about one feature of the SysML v2 or KerML metamodel: 'redefinitions' and "
            + "'subsettings' (the features that redefine or subset it), 'opposite' (the other end of its association), "
            + "'feature-constraints' (the constraints that name it) or 'contributors' (the features that subset or "
            + "redefine it, directly or through another contributor).")]
        public string Feature(
            [Description("What to answer.")] FeatureQuestion question,
            [Description("The feature's name, such as ownedMember.")] string feature,
            [Description(
                "The metaclass the feature belongs to, such as Namespace. Required for 'opposite', 'feature-constraints' "
                + "and 'contributors'; for 'redefinitions' and 'subsettings', only the feature this metaclass owns.")]
            string? @class = null,
            [Description(TagDescription)] string? tag = null) =>
            question switch
            {
                FeatureQuestion.Redefinitions => Answer.Of(() => this.query.Redefinitions(feature, tag, @class)),
                FeatureQuestion.Subsettings => Answer.Of(() => this.query.Subsettings(feature, tag, @class)),
                FeatureQuestion.Opposite => Answer.Of(
                    () => this.query.Opposite(Answer.Required(@class, "class", "opposite"), feature, tag)),
                FeatureQuestion.FeatureConstraints => Answer.Of(
                    () => this.query.FeatureConstraints(Answer.Required(@class, "class", "feature-constraints"), feature, tag)),
                FeatureQuestion.Contributors => Answer.Of(
                    () => this.query.Contributors(Answer.Required(@class, "class", "contributors"), feature, tag)),
                _ => throw new McpException($"'{question}' is not a question hypha_feature answers."),
            };

        /// <summary>Answers a question about the features a type can be held by.</summary>
        /// <param name="question">What to answer.</param>
        /// <param name="type">The type, or for <c>containers</c> the element to be owned.</param>
        /// <param name="class">
        /// Only the effective features of this metaclass, for <c>features-accepting</c> and <c>containers</c>.
        /// </param>
        /// <param name="compositeOnly">Only composite features, for <c>classes-with-feature-typed</c>.</param>
        /// <param name="tag">The release; the default release when <c>null</c>.</param>
        /// <returns>The result, with its grounding, as JSON.</returns>
        /// <exception cref="McpException">The question could not be answered; the message says why.</exception>
        [McpServerTool(Name = "hypha_typed_by", Title = "Typed by", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description(
            "Answers what can hold or own a type in the SysML v2 or KerML metamodel: 'classes-with-feature-typed' (the "
            + "metaclasses with a feature typed by it), 'features-accepting' (every feature that can hold a value of "
            + "it) or 'containers' (the composite features through which an element of it can be owned).")]
        public string TypedBy(
            [Description("What to answer.")] TypedByQuestion question,
            [Description("The type, such as Expression; for 'containers', the element to be owned, such as PartUsage.")] string type,
            [Description("For 'features-accepting' and 'containers': only the effective features of this metaclass.")] string? @class = null,
            [Description("For 'classes-with-feature-typed': only composite features.")] bool compositeOnly = false,
            [Description(TagDescription)] string? tag = null) =>
            question switch
            {
                TypedByQuestion.ClassesWithFeatureTyped => Answer.Of(() => this.query.ClassesWithFeatureTyped(type, compositeOnly, tag)),
                TypedByQuestion.FeaturesAccepting => Answer.Of(() => this.query.FeaturesAccepting(type, @class, tag)),
                TypedByQuestion.Containers => Answer.Of(() => this.query.Containers(type, @class, tag)),
                _ => throw new McpException($"'{question}' is not a question hypha_typed_by answers."),
            };

        /// <summary>Lists a metamodel package's contents.</summary>
        /// <param name="name">The package, by qualified name or name.</param>
        /// <param name="tag">The release; the default release when <c>null</c>.</param>
        /// <returns>The result, with its grounding, as JSON.</returns>
        /// <exception cref="McpException">The question could not be answered; the message says why.</exception>
        [McpServerTool(Name = "hypha_package", Title = "Package", ReadOnly = true, Idempotent = true, OpenWorld = false, Destructive = false)]
        [Description("Lists a SysML v2 or KerML metamodel package's metaclasses, its nested packages and every metaclass below it.")]
        public string Package(
            [Description("The package, by qualified name or name, such as Parts.")] string name,
            [Description(TagDescription)] string? tag = null) =>
            Answer.Of(() => this.query.Package(name, tag));
    }
}
