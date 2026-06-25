---
name: ViewRenderingMembership
package: Views
isAbstract: false
generalizes: [FeatureMembership]
specializedBy: []
---

# ViewRenderingMembership

`Views` package · concrete metaclass

A ViewRenderingMembership is a <coed>FeatureMembership that identifies the viewRendering of a ViewDefinition or ViewUsage.</coed>

## Generalizations

- [FeatureMembership](FeatureMembership.md)

## Owned features

### ownedRendering : RenderingUsage [1..1] {derived, composite}

The owned RenderingUsage that is either itself the referencedRendering or subsets the referencedRendering.

Redefines: `ownedMemberFeature`

### referencedRendering : RenderingUsage [1..1] {derived}

The RenderingUsage that is referenced through this ViewRenderingMembership. It is the referencedFeature of the ownedReferenceSubsetting for the ownedRendering, if there is one, and, otherwise, the ownedRendering itself.


## Inherited features

| Feature | Type | Multiplicity | Owner | Modifiers |
| --- | --- | --- | --- | --- |
| aliasIds | String | [0..*] | [Element](Element.md) | ordered |
| declaredName | String | [0..1] | [Element](Element.md) |  |
| declaredShortName | String | [0..1] | [Element](Element.md) |  |
| documentation | Documentation | [0..*] | [Element](Element.md) | derived, ordered |
| elementId | String | [1..1] | [Element](Element.md) |  |
| isImplied | Boolean | [1..1] | [Relationship](Relationship.md) |  |
| isImpliedIncluded | Boolean | [1..1] | [Element](Element.md) |  |
| isLibraryElement | Boolean | [1..1] | [Element](Element.md) | derived |
| memberElement | Element | [1..1] | [Membership](Membership.md) |  |
| memberElementId | String | [1..1] | [Membership](Membership.md) | derived |
| memberName | String | [0..1] | [Membership](Membership.md) |  |
| memberShortName | String | [0..1] | [Membership](Membership.md) |  |
| membershipOwningNamespace | Namespace | [1..1] | [Membership](Membership.md) | derived |
| name | String | [0..1] | [Element](Element.md) | derived |
| ownedAnnotation | Annotation | [0..*] | [Element](Element.md) | derived, composite, ordered |
| ownedElement | Element | [0..*] | [Element](Element.md) | derived, ordered |
| ownedMemberElement | Element | [1..1] | [OwningMembership](OwningMembership.md) | derived, composite |
| ownedMemberElementId | String | [1..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberFeature | Feature | [1..1] | [FeatureMembership](FeatureMembership.md) | derived, composite |
| ownedMemberName | String | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedMemberShortName | String | [0..1] | [OwningMembership](OwningMembership.md) | derived |
| ownedRelatedElement | Element | [0..*] | [Relationship](Relationship.md) | composite, ordered |
| ownedRelationship | Relationship | [0..*] | [Element](Element.md) | composite, ordered |
| owner | Element | [0..1] | [Element](Element.md) | derived |
| owningMembership | OwningMembership | [0..1] | [Element](Element.md) | derived |
| owningNamespace | Namespace | [0..1] | [Element](Element.md) | derived |
| owningRelatedElement | Element | [0..1] | [Relationship](Relationship.md) |  |
| owningRelationship | Relationship | [0..1] | [Element](Element.md) |  |
| owningType | Type | [1..1] | [FeatureMembership](FeatureMembership.md) | derived |
| qualifiedName | String | [0..1] | [Element](Element.md) | derived |
| relatedElement | Element | [0..*] | [Relationship](Relationship.md) | derived, ordered |
| shortName | String | [0..1] | [Element](Element.md) | derived |
| source | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| target | Element | [0..*] | [Relationship](Relationship.md) | ordered |
| textualRepresentation | TextualRepresentation | [0..*] | [Element](Element.md) | derived, ordered |
| visibility | VisibilityKind | [1..1] | [Membership](Membership.md) |  |

## Constraints

### deriveVewRenderingMembershipReferencedRendering

The referencedRendering of a ViewRenderingMembership is the the featureTarget of the referencedFeature of the ownedReferenceSubsetting (which must be a RenderingUsage) of the ownedRendering, if there is one, and, otherwise, the ownedRendering itself.

```ocl
referencedRendering =
    let referencedFeature : Feature = 
        ownedRendering.referencedFeatureTarget() in
    if referencedFeature = null then ownedRendering
    else if referencedFeature.oclIsKindOf(RenderingUsage) then
        refrencedFeature.oclAsType(RenderingUsage)
    else null
    endif endif
```

### validateViewRenderingMembershipOwningType

The owningType of a ViewRenderingMembership must be a ViewDefinition or a ViewUsage.

```ocl
owningType.oclIsKindOf(ViewDefinition) or
owningType.oclIsKindOf(ViewUsage)
```

