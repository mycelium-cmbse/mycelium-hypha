# StateSubactionMembership

`States` package · concrete metaclass

A StateSubactionMembership is a FeatureMembership for an entry, do or exit ActionUsage of a StateDefinition or StateUsage.

## Owned features

### action : ActionUsage [1..1]

The ActionUsage that is the ownedMemberFeature of this StateSubactionMembership.

### kind : StateSubactionKind [1..1]

Whether this StateSubactionMembership is for an entry, do or exit ActionUsage.


## Constraints

- **validateStateSubactionMembershipOwningType**
