# ViewRenderingMembership

`Views` package · concrete metaclass

A ViewRenderingMembership is a <coed>FeatureMembership that identifies the viewRendering of a ViewDefinition or ViewUsage.</coed>

## Owned features

### ownedRendering : RenderingUsage [1..1]

The owned RenderingUsage that is either itself the referencedRendering or subsets the referencedRendering.

### referencedRendering : RenderingUsage [1..1]

The RenderingUsage that is referenced through this ViewRenderingMembership. It is the referencedFeature of the ownedReferenceSubsetting for the ownedRendering, if there is one, and, otherwise, the ownedRendering itself.


## Constraints

- **deriveVewRenderingMembershipReferencedRendering**
- **validateViewRenderingMembershipOwningType**
