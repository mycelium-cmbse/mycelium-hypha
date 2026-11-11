// ------------------------------------------------------------------------------------------------
// <copyright file="QueryCommand.cs" company="Starion Group S.A.">
//
//   Copyright 2026 Starion Group S.A.
//   SPDX-License-Identifier: Apache-2.0
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Hypha.Tools.Commands
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization.Metadata;
    using System.Threading;
    using System.Threading.Tasks;

    using Hypha.Knowledge.Lookup;
    using Hypha.Knowledge.Query;

    using Spectre.Console;

    /// <summary>
    /// Answers the structural questions of <see cref="IKnowledgeQuery"/> and <see cref="IApiQuery"/>, one
    /// verb per question, such as <c>hypha query subclasses --of Usage</c> or
    /// <c>hypha query api type --name Commit</c>. Read-only: never fetches or generates.
    /// </summary>
    /// <remarks>
    /// With <c>--json</c>, a verb prints the whole <see cref="QueryResult{T}"/> as one line, written
    /// through <see cref="QueryJsonContext"/>: the answer, where it comes from (tier, release, source
    /// file) and, when an optional artifact was never generated, why. Without it, the grounding on one
    /// line and the answer as indented JSON.
    /// </remarks>
    public sealed class QueryCommand : Command
    {
        /// <summary>The <c>axis</c> of a verb whose <c>--tag</c> names a release.</summary>
        private const string Release = "release";

        /// <summary>The <c>axis</c> of a verb whose <c>--tag</c> names an API version.</summary>
        private const string ApiVersion = "api version";

        /// <summary>The answer of each verb, by the verb's command.</summary>
        private readonly Dictionary<Command, Func<Question, JsonNode>> answers = [];

        /// <summary>Every verb, in the order <c>--help</c> lists them.</summary>
        private readonly List<Command> verbs = [];

        /// <summary>Initializes a new instance of the <see cref="QueryCommand"/> class.</summary>
        public QueryCommand()
            : base("query", "Answer a structural question about a release or an API version from its generated knowledge")
        {
            this.AddReleaseVerbs();
            this.AddMetaclassVerbs();
            this.AddFeatureVerbs();
            this.AddCrossReferenceVerbs();
            this.AddTextVerbs();
            this.Add(this.ApiVerbs());
        }

        /// <summary>Gets every verb, each of which needs an action that runs a <see cref="Handler"/>.</summary>
        public IReadOnlyList<Command> Verbs => this.verbs;

        /// <summary>A required text option.</summary>
        /// <param name="name">The option, such as <c>--of</c>.</param>
        /// <param name="description">What it names.</param>
        /// <returns>The option.</returns>
        private static Option<string> Required(string name, string description) =>
            new(name) { Description = description, Required = true };

        /// <summary>An optional text option.</summary>
        /// <param name="name">The option.</param>
        /// <param name="description">What it names.</param>
        /// <returns>The option.</returns>
        private static Option<string?> Optional(string name, string description) =>
            new(name) { Description = description };

        /// <summary>A switch, off unless given.</summary>
        /// <param name="name">The option.</param>
        /// <param name="description">What it turns on.</param>
        /// <returns>The option.</returns>
        private static Option<bool> Flag(string name, string description) =>
            new(name) { Description = description, DefaultValueFactory = _ => false };

        /// <summary>Serializes a result through <see cref="QueryJsonContext.Readable"/>.</summary>
        /// <typeparam name="T">The result's type, which the context must hold.</typeparam>
        /// <param name="result">The result.</param>
        /// <returns>The result as a JSON tree.</returns>
        /// <exception cref="InvalidOperationException">The context does not hold <typeparamref name="T"/>.</exception>
        private static JsonNode Node<T>(T result) =>
            JsonSerializer.SerializeToNode(
                result,
                QueryJsonContext.Readable.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
                ?? throw new InvalidOperationException($"QueryJsonContext does not hold {typeof(T).Name}."))!;

        /// <summary>Adds a verb.</summary>
        /// <param name="parent">The command the verb belongs to.</param>
        /// <param name="name">The verb.</param>
        /// <param name="description">What it answers.</param>
        /// <param name="axis">
        /// What the verb's <c>--tag</c> names: a release (<see cref="Release"/>) or an API version
        /// (<see cref="ApiVersion"/>); <c>null</c> when the verb names its releases itself.
        /// </param>
        /// <param name="answer">Asks the question and serializes the result.</param>
        /// <param name="options">The verb's own options.</param>
        private void Verb(Command parent, string name, string description, string? axis, Func<Question, JsonNode> answer, params Option[] options)
        {
            var verb = new Command(name, description);

            foreach (var option in options)
            {
                verb.Options.Add(option);
            }

            Option<string?>? tag = null;

            if (axis is not null)
            {
                // The same option as every other verb's, lookup and the api verbs included.
                tag = new Option<string?>("--tag", "-t")
                {
                    Description = axis == ApiVersion
                        ? "The installed API version to answer from. The default API version when omitted."
                        : "The installed release to answer from. The default release when omitted.",
                };
                verb.Options.Add(tag);
            }

            verb.Options.Add(JsonOutput.CreateOption("Print the result, with its grounding, as one line of JSON."));

            this.answers[verb] = question => answer(question with { TagOption = tag });
            this.verbs.Add(verb);
            parent.Add(verb);
        }

        /// <summary>Adds the verbs about the installed releases and what changed between two.</summary>
        private void AddReleaseVerbs()
        {
            this.Verb(this, "releases", "The installed releases and API versions, the defaults, and what each has generated", null,
                question => Node(question.Knowledge.Releases()));

            var from = Required("--from", "The older release, such as 2026-02.");
            var to = Required("--to", "The newer release, such as 2026-05.");
            var element = Optional("--element", "Only this element, by its name in either release.");
            this.Verb(this, "diff", "What changed in the metamodel and the cross-references between two installed releases", null,
                question => Node(question.Knowledge.Diff(question.Get(from)!, question.Get(to)!, question.Get(element))),
                from, to, element);
        }

        /// <summary>Adds the verbs about one metaclass and its place in the hierarchy.</summary>
        private void AddMetaclassVerbs()
        {
            var of = Required("--of", "The metaclass, such as Usage.");
            var concrete = Flag("--concrete-only", "Only the metaclasses that are not abstract.");
            this.Verb(this, "subclasses", "The metaclasses that specialize a metaclass, directly or not", Release,
                question => Node(question.Knowledge.Subclasses(question.Get(of)!, question.Get(concrete), question.Tag)),
                of, concrete);

            var className = Required("--class", "The metaclass, such as PartUsage.");
            this.Verb(this, "ancestors", "Every supertype of a metaclass", Release,
                question => Node(question.Knowledge.Ancestors(question.Get(className)!, question.Tag)),
                className);

            var specific = Required("--specific", "The metaclass that may specialize the other, such as PartUsage.");
            var general = Required("--general", "The supertype, such as Feature.");
            this.Verb(this, "specializes", "Whether one metaclass specializes another, and through which supertypes", Release,
                question => Node(question.Knowledge.Specializes(question.Get(specific)!, question.Get(general)!, question.Tag)),
                specific, general);

            var constraintsOf = Required("--class", "The metaclass, such as PartUsage.");
            var ownConstraints = Flag("--own-only", "Only the constraints the metaclass declares itself.");
            this.Verb(this, "constraints", "The OCL constraints that apply to a metaclass, each naming where it is declared", Release,
                question => Node(question.Knowledge.Constraints(question.Get(constraintsOf)!, !question.Get(ownConstraints), question.Tag)),
                constraintsOf, ownConstraints);

            var operationsOf = Required("--class", "The metaclass, such as PartUsage.");
            var ownOperations = Flag("--own-only", "Only the operations the metaclass declares itself.");
            this.Verb(this, "operations", "The operations that apply to a metaclass, each naming where it is declared", Release,
                question => Node(question.Knowledge.Operations(question.Get(operationsOf)!, !question.Get(ownOperations), question.Tag)),
                operationsOf, ownOperations);

            var enumeration = Required("--enumeration", "The enumeration, such as VisibilityKind.");
            this.Verb(this, "literals", "The literals of an enumeration", Release,
                question => Node(question.Knowledge.Literals(question.Get(enumeration)!, question.Tag)),
                enumeration);

            var neighboursOf = Required("--class", "The metaclass, such as PartUsage.");
            this.Verb(this, "neighbours", "The metaclasses a metaclass's features are typed by, and the features typed by it", Release,
                question => Node(question.Knowledge.Neighbours(question.Get(neighboursOf)!, question.Tag)),
                neighboursOf);

            var package = Required("--name", "The package, by qualified name or name, such as Parts.");
            this.Verb(this, "package", "A package's metaclasses, its nested packages and every metaclass below it", Release,
                question => Node(question.Knowledge.Package(question.Get(package)!, question.Tag)),
                package);

            var elementName = Required("--name", "The element, such as PartUsage.");
            this.Verb(this, "element", "An element's entry in the metamodel index: its kind, package, supertypes and page", Release,
                question => Node(question.Knowledge.Element(question.Get(elementName)!, question.Tag)),
                elementName);
        }

        /// <summary>Adds the verbs about features: their types, redefinitions, subsettings and ends.</summary>
        private void AddFeatureVerbs()
        {
            var type = Required("--type", "The type, such as Expression.");
            var composite = Flag("--composite-only", "Only composite features.");
            this.Verb(this, "classes-with-feature-typed", "The metaclasses with a feature typed by a type", Release,
                question => Node(question.Knowledge.ClassesWithFeatureTyped(question.Get(type)!, question.Get(composite), question.Tag)),
                type, composite);

            var featuresOf = Required("--class", "The metaclass, such as PartUsage.");
            var featureName = Optional("--name", "Only the feature of this name.");
            this.Verb(this, "features", "A metaclass's owned and inherited features, each naming where it comes from", Release,
                question => Node(question.Knowledge.Features(question.Get(featuresOf)!, question.Get(featureName), question.Tag)),
                featuresOf, featureName);

            var effectiveOf = Required("--class", "The metaclass, such as PartUsage.");
            this.Verb(this, "effective-features", "A metaclass's features without the inherited ones it redefines", Release,
                question => Node(question.Knowledge.EffectiveFeatures(question.Get(effectiveOf)!, question.Tag)),
                effectiveOf);

            var redefined = Required("--feature", "The feature, such as definition.");
            var redefinedOwner = Optional("--owner", "Only the feature of this metaclass.");
            this.Verb(this, "redefinitions", "The features that redefine a feature", Release,
                question => Node(question.Knowledge.Redefinitions(question.Get(redefined)!, question.Tag, question.Get(redefinedOwner))),
                redefined, redefinedOwner);

            var subsetted = Required("--feature", "The feature, such as usage.");
            var subsettedOwner = Optional("--owner", "Only the feature of this metaclass.");
            this.Verb(this, "subsettings", "The features that subset a feature", Release,
                question => Node(question.Knowledge.Subsettings(question.Get(subsetted)!, question.Tag, question.Get(subsettedOwner))),
                subsetted, subsettedOwner);

            var contained = Required("--element", "The element, such as PartUsage.");
            var containerClass = Optional("--class", "Only the effective features of this metaclass.");
            this.Verb(this, "containers", "The composite features through which an element can be owned", Release,
                question => Node(question.Knowledge.Containers(question.Get(contained)!, question.Get(containerClass), question.Tag)),
                contained, containerClass);

            var accepted = Required("--type", "The type, such as Expression.");
            var acceptingClass = Optional("--class", "Only the effective features of this metaclass.");
            this.Verb(this, "features-accepting", "Every feature that can hold a value of a type", Release,
                question => Node(question.Knowledge.FeaturesAccepting(question.Get(accepted)!, question.Get(acceptingClass), question.Tag)),
                accepted, acceptingClass);

            var oppositeClass = Required("--class", "The metaclass, such as Membership.");
            var oppositeFeature = Required("--feature", "The feature, such as memberElement.");
            this.Verb(this, "opposite", "The other end of a feature's association", Release,
                question => Node(question.Knowledge.Opposite(question.Get(oppositeClass)!, question.Get(oppositeFeature)!, question.Tag)),
                oppositeClass, oppositeFeature);

            var constrainedClass = Required("--class", "The metaclass, such as PartUsage.");
            var constrainedFeature = Required("--feature", "The feature, such as partDefinition.");
            this.Verb(this, "feature-constraints", "The constraints that name a feature among what they constrain", Release,
                question => Node(question.Knowledge.FeatureConstraints(question.Get(constrainedClass)!, question.Get(constrainedFeature)!, question.Tag)),
                constrainedClass, constrainedFeature);

            var contributedClass = Required("--class", "The metaclass, such as Namespace.");
            var contributedFeature = Required("--feature", "The feature, such as ownedMember.");
            this.Verb(this, "contributors", "The features that subset or redefine a feature, directly or through another contributor", Release,
                question => Node(question.Knowledge.Contributors(question.Get(contributedClass)!, question.Get(contributedFeature)!, question.Tag)),
                contributedClass, contributedFeature);
        }

        /// <summary>Adds the verbs that read the cross-references, the keywords and the model library.</summary>
        private void AddCrossReferenceVerbs()
        {
            var element = Required("--element", "The element, such as PartUsage.");
            this.Verb(this, "cross-reference", "An element's clauses, grammar productions, features its syntax fills in and examples", Release,
                question => Node(question.Knowledge.CrossReference(question.Get(element)!, question.Tag)),
                element);

            var feature = Optional("--feature", "Only this feature.");
            var assignment = Optional("--operator", "Only this assignment operator: =, += or ?=.");
            this.Verb(this, "feature-assignments", "Which elements' syntax fills in a feature, through which operator and productions", Release,
                question => Node(question.Knowledge.FeatureAssignments(question.Get(feature), question.Get(assignment), question.Tag)),
                feature, assignment);

            var production = Required("--production", "The grammar production, such as PartUsage.");
            var builtGrammar = Optional("--grammar", "Only this grammar: kerml or sysml.");
            this.Verb(this, "built-by", "The elements a grammar production builds", Release,
                question => Node(question.Knowledge.BuiltBy(question.Get(production)!, question.Get(builtGrammar), question.Tag)),
                production, builtGrammar);

            var word = Required("--word", "The keyword or operator, such as ref or :>>.");
            var keywordGrammar = Optional("--grammar", "Only this grammar: kerml or sysml.");
            this.Verb(this, "keyword", "The productions that write a keyword or operator, and the metaclasses whose syntax includes it", Release,
                question => Node(question.Knowledge.Keyword(question.Get(word)!, question.Get(keywordGrammar), question.Tag)),
                word, keywordGrammar);

            var document = Required("--document", "The specification: kerml or sysml2.");
            var clause = Required("--clause", "The clause number, such as 8.3.6.");
            var subclauses = Flag("--include-subclauses", "Include the clauses below it.");
            this.Verb(this, "elements-in-clause", "The elements a specification clause treats", Release,
                question => Node(question.Knowledge.ElementsInClause(question.Get(document)!, question.Get(clause)!, question.Get(subclauses), question.Tag)),
                document, clause, subclauses);

            var qualifiedName = Required("--name", "The qualified name, such as ISQBase::mass.");
            this.Verb(this, "declaration", "A standard-library declaration: its kind, page and source file", Release,
                question => Node(question.Knowledge.Declaration(question.Get(qualifiedName)!, question.Tag)),
                qualifiedName);
        }

        /// <summary>Adds the verbs that read the specification text, search and resolve addresses.</summary>
        private void AddTextVerbs()
        {
            var document = Required("--document", "The specification: kerml or sysml2.");
            var clause = Required("--clause", "The clause number, such as 7.4.2.");
            this.Verb(this, "clause", "One clause of the KerML or SysML specification, with its verbatim text", Release,
                question => Node(question.Knowledge.Clause(question.Get(document)!, question.Get(clause)!, question.Tag)),
                document, clause);

            var text = Required("--text", "The text to look for: a name, qualified name, clause number or title.");
            var kind = Optional("--kind", $"Only hits of this kind: {string.Join(", ", HyphaAddress.Kinds)}.");
            var limit = new Option<int>("--limit")
            {
                Description = $"The most hits returned, from 1 to {KnowledgeQuery.MaxSearchHits}.",
                DefaultValueFactory = _ => 20,
            };
            this.Verb(this, "search", "Search the metaclasses, library declarations and specification clauses", Release,
                question => Node(question.Knowledge.Search(question.Get(text)!, question.Get(kind), question.Get(limit), question.Tag)),
                text, kind, limit);

            var address = Required("--address", "The address, such as hypha://2026-05/metamodel/PartUsage.");
            this.Verb(this, "resolve", "The generated page a hypha:// address names", null,
                question => Node(question.Knowledge.Resolve(HyphaAddress.Parse(question.Get(address)!))),
                address);
        }

        /// <summary>Builds the <c>api</c> verbs, which answer from a Systems Modeling API version.</summary>
        /// <returns>The <c>api</c> command.</returns>
        private Command ApiVerbs()
        {
            var api = new Command("api", "Answer a question about a Systems Modeling API version");

            var className = Required("--name", "The PIM class, such as Project.");
            this.Verb(api, "pim-class", "A class of the API's platform-independent model", ApiVersion,
                question => Node(question.Api.PimClass(question.Get(className)!, question.Tag)),
                className);

            var type = Required("--name", "The type, such as Commit.");
            this.Verb(api, "type", "An API type's cross-references: its PIM class, schema and operations", ApiVersion,
                question => Node(question.Api.Type(question.Get(type)!, question.Tag)),
                type);

            var operation = Required("--name", "The operation, qualified by its service, such as ProjectService.createProject.");
            this.Verb(api, "operation", "An API operation's cross-references", ApiVersion,
                question => Node(question.Api.Operation(question.Get(operation)!, question.Tag)),
                operation);

            var clause = Required("--clause", "The clause number, such as 7.1.4.");
            this.Verb(api, "clause", "One clause of the API specification, with its verbatim text", ApiVersion,
                question => Node(question.Api.Clause(question.Get(clause)!, question.Tag)),
                clause);

            return api;
        }

        /// <summary>Runs one <see cref="QueryCommand"/> verb.</summary>
        public sealed class Handler
        {
            /// <summary>How the answer is printed without <c>--json</c>.</summary>
            private static readonly JsonSerializerOptions Indented = new()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            /// <summary>The command whose verbs this handler runs.</summary>
            private readonly QueryCommand command;

            /// <summary>Answers the questions about a release.</summary>
            private readonly IKnowledgeQuery knowledge;

            /// <summary>Answers the questions about an API version.</summary>
            private readonly IApiQuery api;

            /// <summary>Initializes a new instance of the <see cref="Handler"/> class.</summary>
            /// <param name="command">The command whose verbs this handler runs.</param>
            /// <param name="knowledge">Answers the questions about a release.</param>
            /// <param name="api">Answers the questions about an API version.</param>
            /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
            public Handler(QueryCommand command, IKnowledgeQuery knowledge, IApiQuery api)
            {
                ArgumentNullException.ThrowIfNull(command);
                ArgumentNullException.ThrowIfNull(knowledge);
                ArgumentNullException.ThrowIfNull(api);

                this.command = command;
                this.knowledge = knowledge;
                this.api = api;
            }

            /// <summary>Asks the verb's question and prints the result.</summary>
            /// <param name="parseResult">The parsed command line, whose command is one of the verbs.</param>
            /// <param name="cancellationToken">Cancels the run before it starts.</param>
            /// <returns>
            /// 0 when the question was answered, whether or not anything was found; the result says that.
            /// 1 when it could not be: the release is not installed or generated, or an argument is invalid.
            /// </returns>
            /// <exception cref="ArgumentNullException"><paramref name="parseResult"/> is <c>null</c>.</exception>
            /// <exception cref="ArgumentException">The parsed command is not one of this command's verbs.</exception>
            /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
            public async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(parseResult);
                cancellationToken.ThrowIfCancellationRequested();

                if (!this.command.answers.TryGetValue(parseResult.CommandResult.Command, out var answer))
                {
                    throw new ArgumentException($"'{parseResult.CommandResult.Command.Name}' is not a query verb.", nameof(parseResult));
                }

                var json = JsonOutput.IsRequested(parseResult);
                JsonNode result;

                try
                {
                    result = answer(new Question(this.knowledge, this.api, parseResult, null));
                }
                catch (Exception exception) when (exception is KnowledgeLookupException or ArgumentException or FormatException)
                {
                    if (json)
                    {
                        await JsonOutput.WriteErrorAsync(exception.Message);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]{Markup.Escape(exception.Message)}[/]");
                    }

                    return 1;
                }

                if (json)
                {
                    await AnsiConsole.Console.Profile.Out.Writer.WriteLineAsync(result.ToJsonString(JsonOutput.Format));
                    await AnsiConsole.Console.Profile.Out.Writer.FlushAsync(cancellationToken);
                }
                else
                {
                    await Report(result);
                }

                return 0;
            }

            /// <summary>Prints a result for a person: where it comes from, then the answer or why there is none.</summary>
            /// <param name="result">The serialized result.</param>
            /// <returns>A task that completes once the result has been printed.</returns>
            private static async Task Report(JsonNode result)
            {
                if (result["grounding"] is not JsonObject grounding)
                {
                    // The release inventory, which is not read from one document.
                    await AnsiConsole.Console.Profile.Out.Writer.WriteLineAsync(result.ToJsonString(Indented));

                    return;
                }

                var release = (string?)grounding["release"];

                AnsiConsole.MarkupLine(
                    $"[grey]({Markup.Escape((string?)grounding["tier"] ?? string.Empty)}, {Markup.Escape(release ?? string.Empty)}, "
                    + $"{Markup.Escape((string?)grounding["source"] ?? string.Empty)})[/]");

                if ((string?)result["unavailable"] is { } unavailable)
                {
                    AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(unavailable)}[/]");
                }
                else if (result["value"] is not { } value)
                {
                    AnsiConsole.MarkupLine($"[yellow]Nothing in release {Markup.Escape(release ?? string.Empty)} answers that.[/]");
                }
                else
                {
                    // The answer is printed as is: it is JSON, not markup.
                    await AnsiConsole.Console.Profile.Out.Writer.WriteLineAsync(value.ToJsonString(Indented));
                }
            }
        }

        /// <summary>One question being asked: the queries, the command line and the verb's release option.</summary>
        /// <param name="Knowledge">Answers the questions about a release.</param>
        /// <param name="Api">Answers the questions about an API version.</param>
        /// <param name="Parse">The parsed command line.</param>
        /// <param name="TagOption">The verb's <c>--tag</c> option, or <c>null</c> when it has none.</param>
        private sealed record Question(IKnowledgeQuery Knowledge, IApiQuery Api, ParseResult Parse, Option<string?>? TagOption)
        {
            /// <summary>Gets the release or API version asked for, or <c>null</c> for the default.</summary>
            public string? Tag => this.TagOption is null ? null : this.Parse.GetValue(this.TagOption);

            /// <summary>Gets an option's value.</summary>
            /// <typeparam name="T">The option's type.</typeparam>
            /// <param name="option">The option.</param>
            /// <returns>Its value.</returns>
            public T? Get<T>(Option<T> option) => this.Parse.GetValue(option);
        }
    }
}
