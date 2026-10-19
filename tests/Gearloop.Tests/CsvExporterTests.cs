using Gearloop.Data;

namespace Gearloop.Tests;

public class CsvExporterTests
{
    [Fact]
    public void Writes_a_header_and_one_row_per_item_with_profit()
    {
        var item = TestData.SoldBass();
        item.Name = "Fender \"P\" Bass, 1978";

        var writer = new StringWriter();
        CsvExporter.WriteItems([item], writer);
        var lines = writer.ToString().TrimEnd().Split(Environment.NewLine);

        Assert.Equal(2, lines.Length);
        Assert.StartsWith("Id,Name,", lines[0]);
        Assert.Contains("\"Fender \"\"P\"\" Bass, 1978\"", lines[1]);
        Assert.EndsWith(",1632.18,237.18,45", lines[1]);
    }
}
