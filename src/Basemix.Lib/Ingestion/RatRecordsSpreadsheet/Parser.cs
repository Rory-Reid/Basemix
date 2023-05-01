using System.Text;
using ExcelDataReader;

namespace Basemix.Lib.Ingestion.RatRecordsSpreadsheet;

public class Parser
{
    public static void Setup()
    {
        // https://github.com/ExcelDataReader/ExcelDataReader#important-note-on-net-core
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    
    /// <summary>
    /// File layout is:
    ///   Sheet 1: Getting Started
    ///   Sheet 2: Rat Summary
    ///     Row 4: Header row
    ///     | A                 | B        | C        | D       | E   | F   | G   | H    | I       | J       | K      | L         | M     | N      | O      | P          | Q          | R              |
    ///     | Litter Identifier | Rat name | Pet Name | Variety | Sex | Ear | Eye | Coat | Marking | Shading | Colour | Tail Kink | Owner | D.O.B. | D.O.D. | Litter Mum | Litter Dad | Known Genetics |
    ///   Sheet 3: Litter Summary
    ///   Sheet 4: Family Tree Generator
    ///   Sheet 5: Family Tree Data
    ///   Sheet 6: Lists
    ///   Sheet 7: Family Tree Guide
    /// </summary>
    public RatRecords ParseFile(Stream stream)
    {
        var reader = ExcelReaderFactory.CreateReader(stream);
        if (reader.ResultsCount != 7)
        {
            throw new Exception($"Expected 7 sheets, but found {reader.ResultsCount}");
        }

        reader.NextResult(); // Move to Rat Summary sheet
        var row = 0;
        while (row < 5) // Rat Summary header row is row 4-5. 6 is start of the data.
        {
            reader.Read();
            row += 1;
        }

        var ratDetailsRows = new List<RatRow>();
        while (row < 5000)
        {
            reader.Read();
            var rat = reader.ReadRatDetailsRow();
            if (rat.IsNullRat)
            {
                break; // End of data
            }
            
            ratDetailsRows.Add(rat);
            row += 1;
        }

        reader.NextResult(); // Move to Litter Summary sheet
        row = 0;
        var litterDetailsRows = new List<LitterRow>();
        while(row < 6) // Litter Summary header row is row 4-6. 7 is start of the data.
        {
            reader.Read();
            row += 1;
        }

        while (row < 5000)
        {
            reader.Read();
            var litter = reader.ReadLitterDetailsRow();
            if (litter.IsNullLitter)
            {
                break; // End of data
            }
            
            litterDetailsRows.Add(litter);
            
            // Litters span across two rows, for now we'll just skip the second row.
            reader.Read();
            row += 2;
        }
        
        return new RatRecords(ratDetailsRows, litterDetailsRows);
    }
}