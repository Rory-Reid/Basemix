namespace Basemix.Pedigrees.Persistence;

public class PersistedPedigree
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Variety { get; set; }
    
    public long? DamId { get; set; }
    public string? DamName { get; set; }
    public string? DamVariety { get; set; }
    
    public long? DamDamId { get; set; }
    public string? DamDamName { get; set; }
    public string? DamDamVariety { get; set; }
    
    public long? DamDamDamId { get; set; }
    public string? DamDamDamName { get; set; }
    public string? DamDamDamVariety { get; set; }

    public long? DamDamDamDamId { get; set; }
    public string? DamDamDamDamName { get; set; }
    public string? DamDamDamDamVariety { get; set; }
    
    public long? DamDamDamSireId { get; set; }
    public string? DamDamDamSireName { get; set; }
    public string? DamDamDamSireVariety { get; set; }
    
    public long? DamDamSireId { get; set; }
    public string? DamDamSireName { get; set; }
    public string? DamDamSireVariety { get; set; }
    
    public long? DamDamSireDamId { get; set; }
    public string? DamDamSireDamName { get; set; }
    public string? DamDamSireDamVariety { get; set; }
    
    public long? DamDamSireSireId { get; set; }
    public string? DamDamSireSireName { get; set; }
    public string? DamDamSireSireVariety { get; set; }
    
    public long? DamSireId { get; set; }
    public string? DamSireName { get; set; }
    public string? DamSireVariety { get; set; }
    
    public long? DamSireDamId { get; set; }
    public string? DamSireDamName { get; set; }
    public string? DamSireDamVariety { get; set; }
    
    public long? DamSireDamDamId { get; set; }
    public string? DamSireDamDamName { get; set; }
    public string? DamSireDamDamVariety { get; set; }
    
    public long? DamSireDamSireId { get; set; }
    public string? DamSireDamSireName { get; set; }
    public string? DamSireDamSireVariety { get; set; }

    public long? DamSireSireId { get; set; }
    public string? DamSireSireName { get; set; }
    public string? DamSireSireVariety { get; set; }
    
    public long? DamSireSireDamId { get; set; }
    public string? DamSireSireDamName { get; set; }
    public string? DamSireSireDamVariety { get; set; }
    
    public long? DamSireSireSireId { get; set; }
    public string? DamSireSireSireName { get; set; }
    public string? DamSireSireSireVariety { get; set; }
    
    public long? SireId { get; set; }
    public string? SireName { get; set; }
    public string? SireVariety { get; set; }
    
    public long? SireDamId { get; set; }
    public string? SireDamName { get; set; }
    public string? SireDamVariety { get; set; }
    
    public long? SireDamDamId { get; set; }
    public string? SireDamDamName { get; set; }
    public string? SireDamDamVariety { get; set; }

    public long? SireDamDamDamId { get; set; }
    public string? SireDamDamDamName { get; set; }
    public string? SireDamDamDamVariety { get; set; }
    
    public long? SireDamDamSireId { get; set; }
    public string? SireDamDamSireName { get; set; }
    public string? SireDamDamSireVariety { get; set; }
    
    public long? SireDamSireId { get; set; }
    public string? SireDamSireName { get; set; }
    public string? SireDamSireVariety { get; set; }
    
    public long? SireDamSireDamId { get; set; }
    public string? SireDamSireDamName { get; set; }
    public string? SireDamSireDamVariety { get; set; }
    
    public long? SireDamSireSireId { get; set; }
    public string? SireDamSireSireName { get; set; }
    public string? SireDamSireSireVariety { get; set; }
    
    public long? SireSireId { get; set; }
    public string? SireSireName { get; set; }
    public string? SireSireVariety { get; set; }
    
    public long? SireSireDamId { get; set; }
    public string? SireSireDamName { get; set; }
    public string? SireSireDamVariety { get; set; }
    
    public long? SireSireDamDamId { get; set; }
    public string? SireSireDamDamName { get; set; }
    public string? SireSireDamDamVariety { get; set; }
    
    public long? SireSireDamSireId { get; set; }
    public string? SireSireDamSireName { get; set; }
    public string? SireSireDamSireVariety { get; set; }

    public long? SireSireSireId { get; set; }
    public string? SireSireSireName { get; set; }
    public string? SireSireSireVariety { get; set; }
    
    public long? SireSireSireDamId { get; set; }
    public string? SireSireSireDamName { get; set; }
    public string? SireSireSireDamVariety { get; set; }
    
    public long? SireSireSireSireId { get; set; }
    public string? SireSireSireSireName { get; set; }
    public string? SireSireSireSireVariety { get; set; }

    public Node ToPedigreeNodes()
    {
        var pedigree = new Node
        {
            Name = this.Name,
            Variety = this.Variety
        };

        if (this.DamId != null)
        {
            pedigree.Dam = new Node
            {
                Name = this.DamName,
                Variety = this.DamVariety
            };

            if (this.DamDamId != null)
            {
                pedigree.Dam.Dam = new Node
                {
                    Name = this.DamDamName,
                    Variety = this.DamDamVariety
                };

                if (this.DamDamDamId != null)
                {
                    pedigree.Dam.Dam.Dam = new Node
                    {
                        Name = this.DamDamDamName,
                        Variety = this.DamDamDamVariety
                    };

                    if (this.DamDamDamDamId != null)
                    {
                        pedigree.Dam.Dam.Dam.Dam = new Node
                        {
                            Name = this.DamDamDamDamName,
                            Variety = this.DamDamDamDamVariety
                        };
                    }

                    if (this.DamDamDamSireId != null)
                    {
                        pedigree.Dam.Dam.Dam.Sire = new Node
                        {
                            Name = this.DamDamDamSireName,
                            Variety = this.DamDamDamSireVariety
                        };
                    }
                }

                if (this.DamDamSireId != null)
                {
                    pedigree.Dam.Dam.Sire = new Node
                    {
                        Name = this.DamDamSireName,
                        Variety = this.DamDamSireVariety
                    };

                    if (this.DamDamSireDamId != null)
                    {
                        pedigree.Dam.Dam.Sire.Dam = new Node
                        {
                            Name = this.DamDamSireDamName,
                            Variety = this.DamDamSireDamVariety
                        };
                    }

                    if (this.DamDamSireSireId != null)
                    {
                        pedigree.Dam.Dam.Sire.Sire = new Node
                        {
                            Name = this.DamDamSireSireName,
                            Variety = this.DamDamSireSireVariety
                        };
                    }
                }
            }

            if (this.DamSireId != null)
            {
                pedigree.Dam.Sire = new Node
                {
                    Name = this.DamSireName,
                    Variety = this.DamSireVariety
                };
                
                if (this.DamSireDamId != null)
                {
                    pedigree.Dam.Sire.Dam = new Node
                    {
                        Name = this.DamSireDamName,
                        Variety = this.DamSireDamVariety
                    };
                    
                    if (this.DamSireDamDamId != null)
                    {
                        pedigree.Dam.Sire.Dam.Dam = new Node
                        {
                            Name = this.DamSireDamDamName,
                            Variety = this.DamSireDamDamVariety
                        };
                    }

                    if (this.DamSireDamSireId != null)
                    {
                        pedigree.Dam.Sire.Dam.Sire = new Node
                        {
                            Name = this.DamSireDamSireName,
                            Variety = this.DamSireDamSireVariety
                        };
                    }
                }

                if (this.DamSireSireId != null)
                {
                    pedigree.Dam.Sire.Sire = new Node
                    {
                        Name = this.DamSireSireName,
                        Variety = this.DamSireSireVariety
                    };
                    
                    if (this.DamSireSireDamId != null)
                    {
                        pedigree.Dam.Sire.Sire.Dam = new Node
                        {
                            Name = this.DamSireSireDamName,
                            Variety = this.DamSireSireDamVariety
                        };
                    }

                    if (this.DamSireSireSireId != null)
                    {
                        pedigree.Dam.Sire.Sire.Sire = new Node
                        {
                            Name = this.DamSireSireSireName,
                            Variety = this.DamSireSireSireVariety
                        };
                    }
                }
            }
        }

        if (this.SireId != null)
        {
            pedigree.Sire = new Node
            {
                Name = this.SireName,
                Variety = this.SireVariety
            };

            if (this.SireDamId != null)
            {
                pedigree.Sire.Dam = new Node
                {
                    Name = this.SireDamName,
                    Variety = this.SireDamVariety
                };
                
                if (this.SireDamDamId != null)
                {
                    pedigree.Sire.Dam.Dam = new Node
                    {
                        Name = this.SireDamDamName,
                        Variety = this.SireDamDamVariety
                    };
                    
                    if (this.SireDamDamDamId != null)
                    {
                        pedigree.Sire.Dam.Dam.Dam = new Node
                        {
                            Name = this.SireDamDamDamName,
                            Variety = this.SireDamDamDamVariety
                        };
                    }

                    if (this.SireDamDamSireId != null)
                    {
                        pedigree.Sire.Dam.Dam.Sire = new Node
                        {
                            Name = this.SireDamDamSireName,
                            Variety = this.SireDamDamSireVariety
                        };
                    }
                }

                if (this.SireDamSireId != null)
                {
                    pedigree.Sire.Dam.Sire = new Node
                    {
                        Name = this.SireDamSireName,
                        Variety = this.SireDamSireVariety
                    };
                    
                    if (this.SireDamSireDamId != null)
                    {
                        pedigree.Sire.Dam.Sire.Dam = new Node
                        {
                            Name = this.SireDamSireDamName,
                            Variety = this.SireDamSireDamVariety
                        };
                    }

                    if (this.SireDamSireSireId != null)
                    {
                        pedigree.Sire.Dam.Sire.Sire = new Node
                        {
                            Name = this.SireDamSireSireName,
                            Variety = this.SireDamSireSireVariety
                        };
                    }
                }
            }

            if (this.SireSireId != null)
            {
                pedigree.Sire.Sire = new Node
                {
                    Name = this.SireSireName,
                    Variety = this.SireSireVariety
                };
                
                if (this.SireSireDamId != null)
                {
                    pedigree.Sire.Sire.Dam = new Node
                    {
                        Name = this.SireSireDamName,
                        Variety = this.SireSireDamVariety
                    };
                    
                    if (this.SireSireDamDamId != null)
                    {
                        pedigree.Sire.Sire.Dam.Dam = new Node
                        {
                            Name = this.SireSireDamDamName,
                            Variety = this.SireSireDamDamVariety
                        };
                    }

                    if (this.SireSireDamSireId != null)
                    {
                        pedigree.Sire.Sire.Dam.Sire = new Node
                        {
                            Name = this.SireSireDamSireName,
                            Variety = this.SireSireDamSireVariety
                        };
                    }
                }

                if (this.SireSireSireId != null)
                {
                    pedigree.Sire.Sire.Sire = new Node
                    {
                        Name = this.SireSireSireName,
                        Variety = this.SireSireSireVariety
                    };
                    
                    if (this.SireSireSireDamId != null)
                    {
                        pedigree.Sire.Sire.Sire.Dam = new Node
                        {
                            Name = this.SireSireSireDamName,
                            Variety = this.SireSireSireDamVariety
                        };
                    }

                    if (this.SireSireSireSireId != null)
                    {
                        pedigree.Sire.Sire.Sire.Sire = new Node
                        {
                            Name = this.SireSireSireSireName,
                            Variety = this.SireSireSireSireVariety
                        };
                    }
                }
            }
        }

        return pedigree;
    }
}