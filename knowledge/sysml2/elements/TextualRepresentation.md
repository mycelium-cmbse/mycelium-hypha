---
name: TextualRepresentation
package: Annotations
fully qualified name: KerML::Root::Annotations::TextualRepresentation
isAbstract: false
visibility: public
generalizes: [AnnotatingElement]
specializedBy: []
---

# TextualRepresentation

`Annotations` package · concrete metaclass

A TextualRepresentation is an AnnotatingElement whose body represents the representedElement in a given language. The representedElement must be the owner of the TextualRepresentation. The named language can be a natural language, in which case the body is an informal representation, or an artificial language, in which case the body is expected to be a formal, machine-parsable representation.If the named language of a TextualRepresentation is machine-parsable, then the body text should be legal input text as defined for that language. The interpretation of the named language string shall be case insensitive. The following language names are defined to correspond to the given standard languages:<table border="1" cellpadding="1" cellspacing="1" width="498"> <thead> </thead> <tbody> <tr> <td style="text-align: center; width: 154px;">kerml</td> <td style="width: 332px;">Kernel Modeling Language</td> </tr> <tr> <td style="text-align: center; width: 154px;">ocl</td> <td style="width: 332px;">Object Constraint Language</td> </tr> <tr> <td style="text-align: center; width: 154px;">alf</td> <td style="width: 332px;">Action Language for fUML</td> </tr> </tbody></table>Other specifications may define specific language strings, other than those shown above, to be used to indicate the use of languages from those specifications in KerML TextualRepresentation.If the language of a TextualRepresentation is &quot;kerml&quot;, then the body text shall be a legal representation of the representedElement in the KerML textual concrete syntax. A conforming tool can use such a TextualRepresentation Annotation to record the original KerML concrete syntax text from which an Element was parsed. In this case, it is a tool responsibility to ensure that the body of the TextualRepresentation remains correct (or the Annotation is removed) if the annotated Element changes other than by re-parsing the body text.An Element with a TextualRepresentation in a language other than KerML is essentially a semantically &quot;opaque&quot; Element specified in the other language. However, a conforming KerML tool may interpret such an element consistently with the specification of the named language.

## Generalizations

- [AnnotatingElement](AnnotatingElement.md)

## Owned features

### body

`+` String · `[1..1]`

The textual representation of the representedElement in the given language.

### language

`+` String · `[1..1]`

The natural or artifical language in which the body text is written.

### representedElement

`+` [Element](Element.md) · `[1..1]` · *derived*

The Element that is represented by this TextualRepresentation.

Redefines [annotatedElement](AnnotatingElement.md#annotatedelement)

Subsets [owner](Element.md#owner)


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| annotatedElement | [Element](Element.md) | [1..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| annotation | [Annotation](Annotation.md) | [0..*] | [AnnotatingElement](AnnotatingElement.md) | derived, ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | [Documentation](Documentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotatingRelationship | [Annotation](Annotation.md) | [0..*] | [AnnotatingElement](AnnotatingElement.md) | derived, composite, ordered |
| ownedAnnotation | [Annotation](Annotation.md) | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | [Element](Element.md) | [0..*] | [Element](Element.md) | derived, ordered |
| ownedRelationship | [Relationship](Relationship.md) | [0..*] | [Element](Element.md) | composite, ordered |
| owner | [Element](Element.md) | [0..1] | [Element](Element.md) | derived |
| owningAnnotatingRelationship | [Annotation](Annotation.md) | [0..1] | [AnnotatingElement](AnnotatingElement.md) | derived |
| owningMembership | [OwningMembership](OwningMembership.md) | [0..1] | [Element](Element.md) | derived |
| owningNamespace | [Namespace](Namespace.md) | [0..1] | [Element](Element.md) | derived |
| owningRelationship | [Relationship](Relationship.md) | [0..1] | [Element](Element.md) |  |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| textualRepresentation | [TextualRepresentation](TextualRepresentation.md) | [0..*] | [Element](Element.md) | derived, ordered |
