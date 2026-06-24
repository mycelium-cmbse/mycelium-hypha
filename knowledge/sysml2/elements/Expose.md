# Expose

`Views` package · abstract metaclass

An Expose is an Import of Memberships into a ViewUsage that provide the Elements to be included in a view. Visibility is always ignored for an Expose (i.e., isImportAll = true).

## Owned features

### isImportAll : Boolean [1..1]

An Expose always imports all Elements, regardless of visibility (isImportAll = true).

### visibility : &#171;untyped&#187; [1..1]

An Expose always has protected visibility.


## Constraints

- **validateExposeIsImportAll**
- **validateExposeOwningNamespace**
- **validateExposeVisibility**
