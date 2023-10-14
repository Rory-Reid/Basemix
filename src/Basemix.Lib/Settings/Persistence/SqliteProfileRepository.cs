using Dapper;

namespace Basemix.Lib.Settings.Persistence;

public class SqliteProfileRepository : IProfileRepository
{
    private readonly GetDatabase getDatabase;
    
    public SqliteProfileRepository(GetDatabase getDatabase)
    {
        this.getDatabase = getDatabase;
    }

    public async Task<Profile> GetDefaultProfile()
    {
        using var db = this.getDatabase();
        
        var profile = await db.QuerySingleAsync<PersistedProfile>(
            """
            SELECT
                id, name, rattery_name,
                pedigree_footer, pedigree_show_sex,
                pedigree_pdf_page_margin, pedigree_pdf_font, pedigree_pdf_header_font_size,
                pedigree_pdf_subheader_font_size, pedigree_pdf_font_size, pedigree_pdf_footer_font_size
            FROM settings_profile
            WHERE id = 1
            """);

        return profile.ToModelledProfile();
    }

    public async Task ResetProfileDefaults(long profileId)
    {
        var db = this.getDatabase();

        // TODO - don't duplicate "default" knowledge somehow. Maybe consider a hidden "Default" row that we copy from?
        await db.ExecuteAsync(
            """
            UPDATE settings_profile
            SET
                rattery_name = NULL,
                pedigree_footer = NULL,
                pedigree_show_sex = TRUE,
                pedigree_pdf_page_margin = 25,
                pedigree_pdf_font = 'Carlito',
                pedigree_pdf_header_font_size = 36,
                pedigree_pdf_subheader_font_size = 26,
                pedigree_pdf_font_size = 10,
                pedigree_pdf_footer_font_size = 10
            WHERE id = @Id
            """, new {Id = profileId});
    }

    public async Task UpdateProfileSettings(Profile profile)
    {
        var db = this.getDatabase();

        await db.ExecuteAsync(
            """
            UPDATE settings_profile
            SET
                name = @Name,
                rattery_name = @RatteryName,
                pedigree_footer = @PedigreeFooter,
                pedigree_show_sex = @PedigreeShowSex,
                pedigree_pdf_page_margin = @PedigreePdfPageMargin,
                pedigree_pdf_font = @PedigreePdfFont,
                pedigree_pdf_header_font_size = @PedigreePdfHeaderFontSize,
                pedigree_pdf_subheader_font_size = @PedigreePdfSubheaderFontSize,
                pedigree_pdf_font_size = @PedigreePdfFontSize,
                pedigree_pdf_footer_font_size = @PedigreePdfFooterFontSize
            WHERE id = @Id
            """,
            new PersistedProfile(profile));
    }
}