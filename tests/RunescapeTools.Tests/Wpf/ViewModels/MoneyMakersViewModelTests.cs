namespace RunescapeTools.Tests.Wpf.ViewModels;

[TestFixture]
[Category("Unit")]
public sealed class MoneyMakersViewModelTests
{
    [Test]
    [Property("LegacyScenario", "MoneyMakerViewModelFlow")]
    [Description("money-maker view-model shares and resets the priced selection")]
    public async Task MoneyMakerViewModelFlow()
    {
        var method = new VyrewatchMethod();
        var secondMethod = new ZulrahMethod();
        var thirdMethod = new RuneDragonMethod();
        var selection = new MoneyMakerSelectionContext();
        var preferences = new MemoryMoneyMakingPreferenceStore();
        var market = new FakeMarketDataService
        {
            Latest = method.Definition.RequiredItemIds
                .Append(30125)
                .Concat(secondMethod.Definition.RequiredItemIds)
                .Concat(thirdMethod.Definition.RequiredItemIds)
                .Distinct()
                .ToDictionary(id => id, id => Quote(id, 1_000))
        };
        var viewModel = new MoneyMakersViewModel(
            [thirdMethod, secondMethod, method],
            new MoneyMakingCalculator(),
            market,
            preferences,
            selection);

        await viewModel.LoadAsync();
        Assert.That(viewModel.Methods[0].Method.Definition.Slug, Is.EqualTo(method.Definition.Slug), "Vyrewatch display priority");
        Assert.That(viewModel.Methods[1].Method.Definition.Slug, Is.EqualTo(secondMethod.Definition.Slug), "Zulrah display priority");
        Assert.That(viewModel.Methods[2].Method.Definition.Slug, Is.EqualTo(thirdMethod.Definition.Slug), "remaining methods follow priorities");
        Assert.That(viewModel.SelectedMethod is null, Is.True, "money maker should require an explicit selection");
        var primaryRow = viewModel.Methods.Single(row => row.Method.Definition.Slug == method.Definition.Slug);
        var secondaryRow = viewModel.Methods.Single(row => row.Method.Definition.Slug == secondMethod.Definition.Slug);
        viewModel.SelectedMethod = primaryRow;

        Assert.That(viewModel.FlowRows.Count, Is.EqualTo(method.Definition.Items.Count), "money-making ledger rows");
        Assert.That(viewModel.ProfitAllAccounts.EndsWith(" gp", StringComparison.Ordinal), Is.True, "formatted total profit");
        Assert.That(!viewModel.HasMissingPrices, Is.True, "complete pricing state");
        Assert.That(selection.Current?.Slug ?? string.Empty, Is.EqualTo(method.Definition.Slug), "shared money-maker selection");
        Assert.That(viewModel.AccountCount, Is.EqualTo(method.Definition.Accounts), "method default account quantity");
        Assert.That(selection.Current?.AccountCount ?? 0, Is.EqualTo(method.Definition.Accounts), "shared default account quantity");
        Assert.That(viewModel.ShowRegenPotionOption, Is.True, "Vyrewatch exposes the regen-potion option");
        Assert.That(!viewModel.UsingRegenPotions, Is.True, "Vyrewatch defaults to no regeneration potions");
        Assert.That(viewModel.ActionsPerHour, Is.EqualTo(88m).Within(0m), "Vyrewatch no-regen default action rate");
        Assert.That(!viewModel.IsActionsPerHourOverridden, Is.True, "default action rate is not an override");

        viewModel.ActionsPerHour = 95m;
        Assert.That(viewModel.ActionsPerHour, Is.EqualTo(95m).Within(0m), "custom action rate");
        Assert.That(preferences.Overrides[method.Definition.Slug], Is.EqualTo(95m).Within(0m), "custom action rate persisted by method slug");
        var customResult = new MoneyMakingCalculator().Calculate(
            method.Definition with { ActionsPerHour = 95m },
            market.Latest,
            method.Definition.Accounts);
        Assert.That(selection.Current?.ProfitPerAccountPerHour ?? 0m, Is.EqualTo(customResult.ProfitPerAccount).Within(0m), "custom action rate updates the shared XP Planner profit rate");
        Assert.That(viewModel.IsActionsPerHourOverridden, Is.True, "custom action rate indicator");
        Assert.That(viewModel.MethodKicker.StartsWith("95 actions / hour", StringComparison.Ordinal), Is.True, "custom action rate reprices the method");

        viewModel.UsingRegenPotions = true;
        Assert.That(viewModel.FlowRows.Count, Is.EqualTo(10), "regen ledger adds the prayer regeneration potion");
        Assert.That(viewModel.FlowRows.Any(row => row.Name == "Prayer regeneration potion(4)"), Is.True, "regen ledger contains the prayer regeneration potion row");
        Assert.That(viewModel.FlowRows.Single(row => row.Name == "Prayer regeneration potion(4)").UnitPrice != "Unavailable", Is.True, "enabling regen potions fetches the newly required live price");
        viewModel.UsingRegenPotions = false;
        Assert.That(viewModel.FlowRows.Count, Is.EqualTo(9), "no-regen ledger removes the prayer regeneration potion");
        Assert.That(viewModel.FlowRows.All(row => row.Name != "Prayer regeneration potion(4)"), Is.True, "no-regen ledger contains no prayer regeneration potion row");
        Assert.That(viewModel.MethodKicker.StartsWith("95 actions / hour", StringComparison.Ordinal), Is.True, "custom action rate survives a Vyrewatch configuration change");
        viewModel.ResetActionsPerHourCommand.Execute(null);
        Assert.That(viewModel.ActionsPerHour, Is.EqualTo(88m).Within(0m), "no-regen reset action rate");
        Assert.That(!preferences.Overrides.ContainsKey(method.Definition.Slug), Is.True, "reset removes the persisted override");
        Assert.That(!viewModel.IsActionsPerHourOverridden, Is.True, "reset clears custom action indicator");

        var profitPerAccount = selection.Current?.ProfitPerAccountPerHour ?? 0m;
        viewModel.IncreaseAccountCountCommand.Execute(null);
        Assert.That(viewModel.AccountCount, Is.EqualTo(method.Definition.Accounts + 1), "account quantity increments");
        Assert.That(selection.Current?.AccountCount ?? 0, Is.EqualTo(method.Definition.Accounts + 1), "shared account quantity increments");
        Assert.That(viewModel.MethodKicker.StartsWith("88 actions / hour", StringComparison.Ordinal), Is.True, "account changes preserve the no-regen configuration");
        Assert.That(selection.Current?.TotalProfitPerHour ?? 0m, Is.EqualTo(profitPerAccount * (method.Definition.Accounts + 1)).Within(0m), "shared all-account profit");
        viewModel.SelectedMethod = secondaryRow;
        Assert.That(viewModel.AccountCount, Is.EqualTo(secondMethod.Definition.Accounts), "second method uses its own account quantity");
        Assert.That(viewModel.ActionsPerHour, Is.EqualTo(31m).Within(0m), "second method uses its own default action rate");
        viewModel.ActionsPerHour = 25m;
        Assert.That(preferences.Overrides[secondMethod.Definition.Slug], Is.EqualTo(25m).Within(0m), "generic method action rate persisted");
        viewModel.SelectedMethod = primaryRow;
        Assert.That(viewModel.AccountCount, Is.EqualTo(method.Definition.Accounts + 1), "account quantity is retained per method");
        Assert.That(viewModel.ActionsPerHour, Is.EqualTo(88m).Within(0m), "Vyrewatch retains its current default independently");
        viewModel.SelectedMethod = secondaryRow;
        Assert.That(viewModel.ActionsPerHour, Is.EqualTo(25m).Within(0m), "generic method override retained across selection changes");
        viewModel.SelectedMethod = primaryRow;
        viewModel.DecreaseAccountCountCommand.Execute(null);
        Assert.That(viewModel.AccountCount, Is.EqualTo(method.Definition.Accounts), "account quantity decrements");

        selection.Clear();
        Assert.That(viewModel.SelectedMethod is null, Is.True, "external reset clears Money Makers selection");
        Assert.That(viewModel.FlowRows.Count, Is.EqualTo(0), "external reset clears displayed ledger");

        var restoredViewModel = new MoneyMakersViewModel(
            [method, secondMethod],
            new MoneyMakingCalculator(),
            market,
            preferences,
            new MoneyMakerSelectionContext());
        await restoredViewModel.LoadAsync();
        restoredViewModel.SelectedMethod = restoredViewModel.Methods.Single(
            row => row.Method.Definition.Slug == secondMethod.Definition.Slug);
        Assert.That(restoredViewModel.ActionsPerHour, Is.EqualTo(25m).Within(0m), "saved generic override restores after restart");
    }

    [Test]
    [Property("LegacyScenario", "FrostDragonViewModelConfiguration")]
    [Description("Frost Dragon bone collection persists through the money-maker view-model")]
    public async Task FrostDragonViewModelConfiguration()
    {
        var method = new FrostDragonMethod();
        var preferences = new MemoryMoneyMakingPreferenceStore();
        var market = new FakeMarketDataService
        {
            Latest = method.Definition.RequiredItemIds
                .ToDictionary(id => id, id => Quote(id, 1_000))
        };
        var viewModel = new MoneyMakersViewModel(
            [method],
            new MoneyMakingCalculator(),
            market,
            preferences,
            new MoneyMakerSelectionContext());

        await viewModel.LoadAsync();
        viewModel.SelectedMethod = viewModel.Methods.Single();

        Assert.That(viewModel.ShowFrostDragonBonesOption, Is.True, "Frost Dragons expose the bone configurator");
        Assert.That(viewModel.PickingUpFrostDragonBones, Is.True, "Frost Dragon bones default to collected");
        Assert.That(viewModel.FlowRows.Any(row => row.Name == "Frost dragon bones"), Is.True, "default Frost Dragon ledger includes bones");
        Assert.That(viewModel.ActionsPerHour, Is.EqualTo(120m).Within(0m), "Frost Dragons default to 120 kills per hour");

        viewModel.PickingUpFrostDragonBones = false;
        Assert.That(viewModel.FlowRows.All(row => row.Name != "Frost dragon bones"), Is.True, "disabled Frost Dragon ledger excludes bones");
        Assert.That(!preferences.BooleanOptions[FrostDragonMethod.Slug][FrostDragonMethod.PickUpBonesOptionKey], Is.True, "disabled bone collection is persisted");

        var restored = new MoneyMakersViewModel(
            [method],
            new MoneyMakingCalculator(),
            market,
            preferences,
            new MoneyMakerSelectionContext());
        await restored.LoadAsync();
        restored.SelectedMethod = restored.Methods.Single();
        Assert.That(!restored.PickingUpFrostDragonBones, Is.True, "saved bone configuration restores after restart");
        Assert.That(restored.FlowRows.All(row => row.Name != "Frost dragon bones"), Is.True, "restored Frost Dragon ledger respects saved bone configuration");
    }
}
