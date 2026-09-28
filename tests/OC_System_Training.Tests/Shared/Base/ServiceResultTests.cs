using FluentAssertions;
using OC_System_Training.Shared.Base.dto;
using OC_System_Training.Shared.Constants;
using Xunit;

namespace OC_System_Training.Tests.Shared.Base;

public class ServiceResultTests
{
    [Fact]
    public void Ok_CreatesSuccessfulResultWithData()
    {
        var result = ServiceResult<string>.Ok("SuccessData");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be("SuccessData");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_CreatesFailedResultWithError()
    {
        var result = ServiceResult<string>.Failure(Messages.DepartmentNotFound);

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().Be(Messages.DepartmentNotFound);
    }

    [Fact]
    public void PagedOk_SetsTotalCountAndData()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = ServiceResult<List<int>>.PagedOk(list, 50);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(list);
        result.TotalCount.Should().Be(50);
    }
}
