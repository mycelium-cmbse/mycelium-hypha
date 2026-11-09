---
name: VisibilityKind
package: Namespaces
fully qualified name: KerML::Root::Namespaces::VisibilityKind
visibility: public
kind: enumeration
---

# VisibilityKind

`Namespaces` package · enumeration

VisibilityKind is an enumeration whose literals specify the visibility of a Membership of an Element in a Namespace outside of that Namespace. Note that &quot;visibility&quot; specifically restricts whether an Element in a Namespace may be referenced by name from outside the Namespace and only otherwise restricts access to an Element as provided by specific constraints in the abstract syntax (e.g., preventing the import or inheritance of private Elements).

## Literals

- `private` — Indicates a Membership is not visible outside its owning Namespace.
- `protected` — An intermediate level of visibility between public and private. By default, it is equivalent to private for the purposes of normal access to and import of Elements from a Namespace. However, other Relationships may be specified to include Memberships with protected visibility in the list of memberships for a Namespace (e.g., Specialization).
- `public` — Indicates that a Membership is publicly visible outside its owning Namespace.
