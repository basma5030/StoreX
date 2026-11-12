
using StoreX.Api.Modules.Organization.Entities;

namespace StoreX.Tests;

public class BranchTests
{
    [Fact]
    public void Constructor_ShouldNormalizeCodeToUppercase()
    {
        var branch = new Branch("br001", "Main Branch", "Cairo");

        Assert.Equal("BR001", branch.Code);
    }

    [Fact]
    public void Constructor_ShouldTrimCodeBeforeNormalizing()
    {
        var branch = new Branch(" br001 ", "Main Branch", "Cairo");

        Assert.Equal("BR001", branch.Code);
    }

    [Fact]
    public void Constructor_ShouldKeepCodeUppercaseWhenAlreadyNormalized()
    {
        var branch = new Branch("BR001", "Main Branch", "Cairo");

        Assert.Equal("BR001", branch.Code);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyCode()
    {
        Assert.Throws<ArgumentException>(
            () => new Branch("", "Main Branch", "Cairo"));
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyName()
    {
        Assert.Throws<ArgumentException>(
            () => new Branch("BR001", "", "Cairo"));
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyAddress()
    {
        Assert.Throws<ArgumentException>(
            () => new Branch("BR001", "Main Branch", ""));
    }

    [Fact]
    public void Constructor_ShouldNormalizeBlankPhoneNumberToNull()
    {
        var branch = new Branch(
            "BR001", "Main Branch", "Cairo", "   ");

        Assert.Null(branch.PhoneNumber);
    }

    [Fact]
    public void Constructor_ShouldStartAsActiveAndNotDeleted()
    {
        var branch = new Branch("BR001", "Main Branch", "Cairo");

        Assert.True(branch.IsActive);
        Assert.False(branch.IsDeleted);
    }

    [Fact]
    public void Constructor_ShouldRejectCodeExceedingMaximumLength()
    {
        var code = new string('A', 21);

        Assert.Throws<ArgumentException>(
            () => new Branch(code, "Main Branch", "Cairo"));
    }

    [Fact]
    public void Constructor_ShouldRejectNameExceedingMaximumLength()
    {
        var name = new string('A', 151);

        Assert.Throws<ArgumentException>(
            () => new Branch("BR001", name, "Cairo"));
    }

    [Fact]
    public void Constructor_ShouldRejectAddressExceedingMaximumLength()
    {
        var address = new string('A', 301);

        Assert.Throws<ArgumentException>(
            () => new Branch("BR001", "Main Branch", address));
    }

    [Fact]
    public void Constructor_ShouldRejectPhoneNumberExceedingMaximumLength()
    {
        var phoneNumber = new string('1', 31);

        Assert.Throws<ArgumentException>(
            () => new Branch("BR001", "Main Branch", "Cairo", phoneNumber));
    }
}
