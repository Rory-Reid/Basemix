using Basemix.Lib.Rats;
using Dapper;

namespace Basemix.Lib.Pedigrees.Persistence;

public class SqlitePedigreeRepository : IPedigreeRepository
{
    private readonly GetDatabase getDatabase;

    public SqlitePedigreeRepository(GetDatabase getDatabase)
    {
        this.getDatabase = getDatabase;
    }
    
    public async Task<Node?> GetPedigree(RatIdentity id)
    {
        using var db = this.getDatabase();

        var pedigree = await db.QuerySingleOrDefaultAsync<PersistedPedigree>(
            @"SELECT 
                rat.id, rat.name, rat.variety,
                d.id AS dam_id, d.name AS dam_name, d.variety AS dam_variety,
                    dd.id AS dam_dam_id, dd.name AS dam_dam_name, dd.variety AS dam_dam_variety,
                        ddd.id AS dam_dam_dam_id, ddd.name AS dam_dam_dam_name, ddd.variety AS dam_dam_dam_variety,
                            dddd.id AS dam_dam_dam_dam_id, dddd.name AS dam_dam_dam_dam_name, dddd.variety AS dam_dam_dam_dam_variety,
                            ddds.id AS dam_dam_dam_sire_id, ddds.name AS dam_dam_dam_sire_name, ddds.variety AS dam_dam_dam_sire_variety,
                        dds.id AS dam_dam_sire_id, dds.name AS dam_dam_sire_name, dds.variety AS dam_dam_sire_variety,
                            ddsd.id AS dam_dam_sire_dam_id, ddsd.name AS dam_dam_sire_dam_name, ddsd.variety AS dam_dam_sire_dam_variety,
                            ddss.id AS dam_dam_sire_sire_id, ddss.name AS dam_dam_sire_sire_name, ddss.variety AS dam_dam_sire_sire_variety,
                    ds.id AS dam_sire_id, ds.name AS dam_sire_name, ds.variety AS dam_sire_variety,
                        dsd.id AS dam_sire_dam_id, dsd.name AS dam_sire_dam_name, dsd.variety AS dam_sire_dam_variety,
                            dsdd.id AS dam_sire_dam_dam_id, dsdd.name AS dam_sire_dam_dam_name, dsdd.variety AS dam_sire_dam_dam_variety,
                            dsds.id AS dam_sire_dam_sire_id, dsds.name AS dam_sire_dam_sire_name, dsds.variety AS dam_sire_dam_sire_variety,
                        dss.id AS dam_sire_sire_id, dss.name AS dam_sire_sire_name, dss.variety AS dam_sire_sire_variety,
                            dssd.id AS dam_sire_sire_dam_id, dssd.name AS dam_sire_sire_dam_name, dssd.variety AS dam_sire_sire_dam_variety,
                            dsss.id AS dam_sire_sire_sire_id, dsss.name AS dam_sire_sire_sire_name, dsss.variety AS dam_sire_sire_sire_variety,
                s.id AS sire_id, s.name AS sire_name, s.variety AS sire_variety,
                    sd.id AS sire_dam_id, sd.name AS sire_dam_name, sd.variety AS sire_dam_variety,
                        sdd.id AS sire_dam_dam_id, sdd.name AS sire_dam_dam_name, sdd.variety AS sire_dam_dam_variety,
                            sddd.id AS sire_dam_dam_dam_id, sddd.name AS sire_dam_dam_dam_name, sddd.variety AS sire_dam_dam_dam_variety,
                            sdds.id AS sire_dam_dam_sire_id, sdds.name AS sire_dam_dam_sire_name, sdds.variety AS sire_dam_dam_sire_variety,
                        sds.id AS sire_dam_sire_id, sds.name AS sire_dam_sire_name, sds.variety AS sire_dam_sire_variety,
                            sdsd.id AS sire_dam_sire_dam_id, sdsd.name AS sire_dam_sire_dam_name, sdsd.variety AS sire_dam_sire_dam_variety,
                            sdss.id AS sire_dam_sire_sire_id, sdss.name AS sire_dam_sire_sire_name, sdss.variety AS sire_dam_sire_sire_variety,
                    ss.id AS sire_sire_id, ss.name AS sire_sire_name, ss.variety AS sire_sire_variety,
                        ssd.id AS sire_sire_dam_id, ssd.name AS sire_sire_dam_name, ssd.variety AS sire_sire_dam_variety,
                            ssdd.id AS sire_sire_dam_dam_id, ssdd.name AS sire_sire_dam_dam_name, ssdd.variety AS sire_sire_dam_dam_variety,
                            ssds.id AS sire_sire_dam_sire_id, ssds.name AS sire_sire_dam_sire_name, ssds.variety AS sire_sire_dam_sire_variety,
                        sss.id AS sire_sire_sire_id, sss.name AS sire_sire_sire_name, sss.variety AS sire_sire_sire_variety,
                            sssd.id AS sire_sire_sire_dam_id, sssd.name AS sire_sire_sire_dam_name, sssd.variety AS sire_sire_sire_dam_variety,
                            ssss.id AS sire_sire_sire_sire_id, ssss.name AS sire_sire_sire_sire_name, ssss.variety AS sire_sire_sire_sire_variety

            FROM rat
            LEFT JOIN litter immediate_family ON immediate_family.id=rat.litter_id
            LEFT JOIN rat d ON immediate_family.dam_id=d.id
                LEFT JOIN litter d_family ON d_family.id=d.litter_id
                LEFT JOIN rat dd ON d_family.dam_id=dd.id
                    LEFT JOIN litter dd_family ON dd_family.id=dd.litter_id
                    LEFT JOIN rat ddd ON dd_family.dam_id=ddd.id
                        LEFT JOIN litter ddd_family ON ddd_family.id=ddd.litter_id
                        LEFT JOIN rat dddd ON ddd_family.dam_id=dddd.id
                        LEFT JOIN rat ddds ON ddd_family.sire_id=ddds.id
                    LEFT JOIN rat dds ON dd_family.sire_id=dds.id
                        LEFT JOIN litter dds_family ON dds_family.id=dds.litter_id
                        LEFT JOIN rat ddsd ON dds_family.dam_id=ddsd.id
                        LEFT JOIN rat ddss ON dds_family.sire_id=ddss.id
                LEFT JOIN rat ds ON d_family.sire_id=ds.id
                    LEFT JOIN litter ds_family ON ds_family.id=ds.litter_id
                    LEFT JOIN rat dsd ON ds_family.dam_id=dsd.id
                        LEFT JOIN litter dsd_family ON dsd_family.id=dsd.litter_id
                        LEFT JOIN rat dsdd ON dsd_family.dam_id=dsdd.id
                        LEFT JOIN rat dsds ON dsd_family.sire_id=dsds.id
                    LEFT JOIN rat dss ON ds_family.sire_id=dss.id
                        LEFT JOIN litter dss_family ON dss_family.id=dss.litter_id
                        LEFT JOIN rat dssd ON dss_family.dam_id=dssd.id
                        LEFT JOIN rat dsss ON dss_family.sire_id=dsss.id
            LEFT JOIN rat s ON immediate_family.sire_id=s.id
                LEFT JOIN litter s_family ON s_family.id=s.litter_id
                LEFT JOIN rat sd ON s_family.dam_id=sd.id
                    LEFT JOIN litter sd_family ON sd_family.id=sd.litter_id
                    LEFT JOIN rat sdd ON sd_family.dam_id=sdd.id
                        LEFT JOIN litter sdd_family ON sdd_family.id=sdd.litter_id
                        LEFT JOIN rat sddd ON sdd_family.dam_id=sddd.id
                        LEFT JOIN rat sdds ON sdd_family.sire_id=sdds.id
                    LEFT JOIN rat sds ON sd_family.sire_id=sds.id
                        LEFT JOIN litter sds_family ON sds_family.id=sds.litter_id
                        LEFT JOIN rat sdsd ON sds_family.dam_id=sdsd.id
                        LEFT JOIN rat sdss ON sds_family.sire_id=sdss.id
                LEFT JOIN rat ss ON s_family.sire_id=ss.id
                    LEFT JOIN litter ss_family ON ss_family.id=ss.litter_id
                    LEFT JOIN rat ssd ON ss_family.dam_id=ssd.id
                        LEFT JOIN litter ssd_family ON ssd_family.id=ssd.litter_id
                        LEFT JOIN rat ssdd ON ssd_family.dam_id=ssdd.id
                        LEFT JOIN rat ssds ON ssd_family.sire_id=ssds.id
                    LEFT JOIN rat sss ON ss_family.sire_id=sss.id
                        LEFT JOIN litter sss_family ON sss_family.id=sss.litter_id
                        LEFT JOIN rat sssd ON sss_family.dam_id=sssd.id
                        LEFT JOIN rat ssss ON sss_family.sire_id=ssss.id
            WHERE rat.id=@Id",
            new { Id = id.Value });

        return pedigree?.ToPedigreeNodes();
    }
}