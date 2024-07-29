using Basemix.Lib.Litters;
using Basemix.Lib.Owners;
using Basemix.Lib.Pedigrees;
using Basemix.Lib.Rats;
using Basemix.Lib.Settings.Persistence;
using Bogus;
using Bogus.DataSets;

namespace Basemix.Tests.sdk;

/// <summary>
/// Until we get too many, they all go here
/// </summary>
public static class FakerExtensions
{
    public static long Id(this Faker faker) => faker.Random.Long(1);

    public static T PickNonDefault<T>(this Faker faker, params T[] except) where T : struct, Enum =>
        faker.PickRandom(Enum.GetValues<T>().Except(new[] {default(T)}.Concat(except)));

    private static string[] Varieties =
    {
        // TODO build a better "Variety builder" - these are more categories than exhaustive varieties
        // Self
        "Pink eyed white", "Champagne", "Buff", "Platinum", "Quicksilver", "British Blue", "Black", "Chocolate", "Mink",
        "Ivory",
        // Marked
        "Berkshire", "Badger", "Irish", "Hooded", "Variegated", "Capped", "Essex", "Blazed Essex", "Chinchilla",
        "Squirrel", "Roan", "Striped Roan",
        // Russian
        "Russian Blue", "Russian Dove", "Russian Blue Agouti", "Russian Topaz",
        // Shaded
        "Argente Crème", "Himalayan", "Siamese", "Blue Point Siamese", "Burmese", "Wheaten Burmese", "Golden Himalayan",
        "Marten", "Silver Agouti",
        // AOV
        "Cream", "Topaz", "Silver Fawn", "Silver", "Agouti", "Cinnamon", "British Blue Agouti", "Lilac Agouti", "Pearl",
        "Cinnamon Pearl", "Platinum Agouti",
        // Rex/Dumbo
        "Rex", "Dumbo",
        // Guide standard
        "Cream Agouti", "Golden Siamese", "Lilac", "Russian Dove", "Russian Silver", "Russian Silver Agouti",
        "Sable Burmese", "Spotted Downunder",
        // Provisional
        "Bareback", "Blue Point Himalayan", "Cinnamon Chinchilla", "Copper", "Dwarf", "Havana", "Havana Agouti",
        "Merle", "Powder Blue", "Pink Eyed Ivory", "Russian Buff", "Russian Burmese", "Russian Pearl", "Satin",
        "Silken", "Variegated Downunder",
    };

    public static string Variety(this Faker faker) =>
        faker.PickRandom(Varieties);

    public static Rat Rat(this Faker faker, RatIdentity? id = null, string? name = null, Sex? sex = null,
        DateOnly? dateOfBirth = null, bool? dead = null, DateOnly? dateOfDeath = null, bool? owned = null,
        Owner? owner = null, float sexProbability = 1.0f)
    {
        var ratSex = sex ?? (faker.Random.Bool(sexProbability) ? faker.PickNonDefault<Sex>() : null);
        var ratName = name ?? ratSex switch
        {
            Sex.Buck => faker.Name.FirstName(Name.Gender.Male),
            Sex.Doe => faker.Name.FirstName(Name.Gender.Female),
            _ => faker.Name.FirstName()
        };

        var ownedByUser = owner == null && (owned ?? faker.Random.Bool());
        return new Rat(id: id, name: ratName, sex: ratSex, variety: faker.Variety(),
            dateOfBirth: dateOfBirth ?? faker.Date.PastDateOnly(1), new List<RatLitter>(),
            owner?.Id, owner?.Name)
        {
            Notes = faker.PickRandom(null, faker.Lorem.Paragraphs()),
            Dead = dead ?? dateOfDeath != null,
            DateOfDeath = dateOfDeath,
            Owned = ownedByUser
        };
    }
    
    public static RatLitter RatLitter(this Faker faker) =>
        new(faker.Id(), faker.Date.RecentDateOnly(), faker.Person.FirstName, faker.Random.Int(1, 15));

    public static Litter BlankLitter(this Faker faker, LitterIdentity? id = null) =>
        faker.Litter(id: id, null, null, null, true, null, 0, 0, 0, 0);

    public static Litter Litter(this Faker faker, LitterIdentity? id = null,
        (RatIdentity, string?)? dam = null, (RatIdentity, string?)? sire = null,
        List<Rat>? offspring = null, bool? bredByMe = true, string? name = null,
        float damProbability = 0.5f, float sireProbability = 0.5f,
        int minimumOffspring = 0, int maximumOffspring = 12)
    {
        var hasDam = faker.Random.Bool(damProbability);
        var hasSire = faker.Random.Bool(sireProbability);
        return new Litter(
            identity: id,
            dam: dam ?? (hasDam ? new(faker.Id(), faker.Name.FirstName(Name.Gender.Female)) : null),
            sire: sire ?? (hasSire ? new(faker.Id(), faker.Name.FirstName(Name.Gender.Male)) : null),
            dateOfBirth: faker.Date.PastDateOnly(),
            offspring: offspring?.Select(x => new Offspring(x.Id, x.Name)).ToList() ?? faker.Make(
                faker.Random.Int(minimumOffspring, maximumOffspring),
                _ => new Offspring(faker.Id(), faker.Name.FirstName(), faker.Name.FullName())).ToList())
        {
            BredByMe = bredByMe ?? true,
            Name = name,
            Notes = faker.PickRandom(null, faker.Lorem.Paragraphs())
        };
    }

    public static Node CodedPedigree(this Faker faker) =>
        new() { Name = "root", Variety = "root",
            Sire = new Node { Name = "s", Variety = "s",
                Sire = new Node { Name = "ss", Variety = "ss",
                    Sire = new Node { Name = "sss", Variety = "sss",
                        Sire = new Node { Name = "ssss", Variety = "ssss" },
                        Dam = new Node { Name = "sssd", Variety = "sssd" }
                    },
                    Dam = new Node { Name = "ssd", Variety = "ssd",
                        Sire = new Node { Name = "ssds", Variety = "ssds" },
                        Dam = new Node { Name = "ssdd", Variety = "ssdd" }
                    }
                },
                Dam = new Node { Name = "sd", Variety = "sd",
                    Sire = new Node { Name = "sds", Variety = "sds",
                        Sire = new Node { Name = "sdss", Variety = "sdss" },
                        Dam = new Node { Name = "sdsd", Variety = "sdsd" }
                    },
                    Dam = new Node { Name = "sdd", Variety = "sdd",
                        Sire = new Node { Name = "sdds", Variety = "sdds" },
                        Dam = new Node { Name = "sddd", Variety = "sddd" }
                    }
                }
            },
            Dam = new Node { Name = "d", Variety = "d",
                Sire = new Node { Name = "ds", Variety = "ds",
                    Sire = new Node { Name = "dss", Variety = "dss",
                        Sire = new Node { Name = "dsss", Variety = "dsss" },
                        Dam = new Node { Name = "dssd", Variety = "dssd" }
                    },
                    Dam = new Node { Name = "dsd", Variety = "dsd",
                        Sire = new Node { Name = "dsds", Variety = "dsds" },
                        Dam = new Node { Name = "dsdd", Variety = "dsdd" }
                    }
                },
                Dam = new Node { Name = "dd", Variety = "dd",
                    Sire = new Node { Name = "dds", Variety = "dds",
                        Sire = new Node { Name = "ddss", Variety = "ddss" },
                        Dam = new Node { Name = "ddsd", Variety = "ddsd" }
                    },
                    Dam = new Node { Name = "ddd", Variety = "ddd",
                        Sire = new Node { Name = "ddds", Variety = "ddds" },
                        Dam = new Node { Name = "dddd", Variety = "dddd" }
                    }
                }
            }
        };

    public static Owner Owner(this Faker faker, OwnerIdentity? id = null)
    {
        var person = new Person();
        return new(id)
        {
            Name = person.FullName,
            Email = person.Email,
            Phone = person.Phone,
            Notes = faker.Lorem.Paragraphs()
        };
    }

    public static EstimationParameters EstimationParameters(this Faker faker)
    {
        var minBirthAfterPairing = faker.Random.Int(1, 30);
        var maxBirthAfterPairing = minBirthAfterPairing + faker.Random.Int(1,4);
        var minWeaning = faker.Random.Int(1, 30);
        var minSeparation = minWeaning + faker.Random.Int(1, 14);
        var minRehome = minSeparation + faker.Random.Int(7, 14);
        
        return new EstimationParameters(
            minBirthAfterPairing,
            maxBirthAfterPairing,
            minWeaning,
            minSeparation,
            minRehome);
    }
    
    public static DeathReason DeathReason(this Faker faker, string? name = null) =>
        new(faker.Random.Long(1), name ?? faker.Lorem.Word());
}