# Support for litters with multiple sires

## Status

Accepted

## Context

It is possible for a doe to give birth to a litter that has been fathered by more than one buck. As this application intends to handle litters as a distinct model, this represents a concern for that model to address.

For the avoidance of doubt, each rat in the litter has precisely one father, but there are many rats to a litter and it is possible that not all have the same father if multiple bucks have been paired with the dam. This can be difficult to detect unless the fathers are of distinctly different varieties.

It is understood to be a rare and undesirable case - UK rat breeders favour predictability in breeding and reliability in their records, and as such will not typically pair a doe with more than one buck when she is in heat so that the sire of the resulting litter is obvious. This makes tracking genetics and related information (health, for example) more reliable.

In spite of that, it does happen.

## Decision

We will only model litters in such a way that they have a single Sire.

We will not prevent multiple "litters" being recorded for the same Dam, different Sire on the same day, effectively keeping the door open to a workaround for if the user must log this and is certain which offspring belong to which Sire.

We will allow for notes to be added to litters so that if a user really feels inclined, they can manually note such things.

We will not ask for community feedback on this at this moment in time - it is an edge case.

## Consequences

- The model of the litter becomes uncomplicated for what is believed to be a rare case.
- Future rendering of family trees remains uncomplicated by this scenario
- Users cannot truly represent a multiple-sire litter in the system without putting in a bit more legwork.
- Workarounds are still possible
- Workarounds involve falsely representing the one litter as two
- If the users do not know which offspring belong to which father, there is no workaround