using MudBlazor;

namespace Basemix.Shared;

public class BasemixTheme : MudTheme
{
    public BasemixTheme()
    {
        // https://coolors.co/09caec-20285a-183463-065275-db504a
        this.PaletteLight.Primary = "#08B1CF";
        this.PaletteLight.PrimaryDarken = "#07A8C5";
        this.PaletteLight.PrimaryLighten = "#09CAEC";

        this.PaletteLight.Dark = "#21295C";
        this.PaletteLight.DarkLighten = "#2B3578";
        this.PaletteLight.DarkDarken = "#20285A";
        
        this.PaletteLight.Secondary = "#1B3B6F";
        this.PaletteLight.SecondaryLighten = "#204683";
        this.PaletteLight.SecondaryDarken = "#1A3A6D";

        this.PaletteLight.Tertiary = "#065A82";
        this.PaletteLight.TertiaryLighten = "#076D9C";
        this.PaletteLight.TertiaryDarken = "#065275";

        this.Typography.Default.FontSize = "1.2rem";
    }
    public static BasemixTheme Instance => new();
}