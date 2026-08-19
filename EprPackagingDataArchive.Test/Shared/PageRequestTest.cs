using EprPackagingDataArchive.Shared;

namespace EprPackagingDataArchive.Test.Shared;

public class PageRequestTest
{
    [Fact]
    public void Defaults_to_the_first_page_when_nothing_is_supplied()
    {
        var request = PageRequest.From(null, null);

        Assert.Equal(1, request.Number);
        Assert.Equal(PageRequest.DefaultSize, request.Size);
        Assert.Equal(0, request.Skip);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public void Page_numbers_below_one_are_lifted_to_one(int requested, int expected) =>
        Assert.Equal(expected, PageRequest.From(requested, null).Number);

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(10, 10)]
    [InlineData(99999, PageRequest.MaxSize)]
    public void Page_size_is_clamped_rather_than_rejected(int requested, int expected) =>
        // A caller asking for too much gets the maximum, not a 400. Being strict here would break
        // integrations for no benefit.
        Assert.Equal(expected, PageRequest.From(null, requested).Size);

    [Fact]
    public void Skip_is_derived_from_the_page_number_and_size()
    {
        var request = PageRequest.From(3, 20);

        Assert.Equal(40, request.Skip);
    }

    [Fact]
    public void Applies_the_window_to_a_collection()
    {
        var items = Enumerable.Range(1, 100).ToList();

        var page = PageRequest.From(2, 10).Apply(items);

        Assert.Equal(10, page.Count);
        Assert.Equal(11, page.First());
        Assert.Equal(20, page.Last());
    }

    [Fact]
    public void A_page_beyond_the_end_is_empty_rather_than_an_error()
    {
        var page = PageRequest.From(50, 10).Apply(Enumerable.Range(1, 5).ToList());

        Assert.Empty(page);
    }
}
