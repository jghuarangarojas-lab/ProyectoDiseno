using MsServices.Domain.Entities;
using MsServices.Domain.Enums;
using MsServices.Domain.Exceptions;
using Xunit;

namespace MsServices.Tests;

public class ServiceProjectStageTests
{
    private static ServiceProject CreateProject(ProjectStage stage = ProjectStage.REQUESTED) => new()
    {
        ProjectId = Guid.NewGuid(),
        ClientId = Guid.NewGuid(),
        Title = "Jardín frontal",
        Stage = stage,
        TotalBudget = 3500m,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    [Fact]
    public void Lifecycle_MovesThroughAllStages()
    {
        var project = CreateProject();
        var now = DateTime.UtcNow;

        foreach (var stage in new[]
        {
            ProjectStage.TECH_VISIT,
            ProjectStage.PROPOSAL,
            ProjectStage.APPROVED,
            ProjectStage.CONTRACTED,
            ProjectStage.IN_PROGRESS,
            ProjectStage.COMPLETED
        })
        {
            project.ChangeStage(stage, now);
            Assert.Equal(stage, project.Stage);
        }
    }

    [Fact]
    public void ChangeStage_FromCompleted_IsRejected()
    {
        var project = CreateProject(ProjectStage.COMPLETED);

        Assert.Throws<DomainException>(() => project.ChangeStage(ProjectStage.IN_PROGRESS, DateTime.UtcNow));
        Assert.Equal(ProjectStage.COMPLETED, project.Stage);
    }

    [Fact]
    public void ChangeStage_SkippingStages_IsRejected()
    {
        var project = CreateProject(ProjectStage.REQUESTED);

        Assert.Throws<DomainException>(
            () => project.ChangeStage(ProjectStage.APPROVED, DateTime.UtcNow));

        Assert.Equal(ProjectStage.REQUESTED, project.Stage);
    }

    [Fact]
    public void ChangeStage_ToSameStage_IsRejected()
    {
        var project = CreateProject(ProjectStage.PROPOSAL);

        Assert.Throws<DomainException>(
            () => project.ChangeStage(ProjectStage.PROPOSAL, DateTime.UtcNow));
    }

    [Fact]
    public void EnsureCanAdvanceFrom_RequiresExpectedStage()
    {
        var project = CreateProject(ProjectStage.PROPOSAL);

        var exception = Assert.Throws<DomainException>(
            () => project.EnsureCanAdvanceFrom(ProjectStage.APPROVED, ProjectStage.CONTRACTED));

        Assert.Contains("PROPOSAL", exception.Message);

        project.Stage = ProjectStage.APPROVED;
        project.EnsureCanAdvanceFrom(ProjectStage.APPROVED, ProjectStage.CONTRACTED);
    }

    [Fact]
    public void AddInstallment_ValidatesAmountAndDate()
    {
        var project = CreateProject();

        Assert.Throws<DomainException>(() => project.AddInstallment(0m, DateTime.UtcNow.AddDays(30), DateTime.UtcNow));
        Assert.Throws<DomainException>(() => project.AddInstallment(100m, default, DateTime.UtcNow));

        var installment = project.AddInstallment(100m, DateTime.UtcNow.AddDays(30), DateTime.UtcNow);

        Assert.Equal(InstallmentStatus.PENDING, installment.Status);
        Assert.Single(project.Installments);
    }
}
