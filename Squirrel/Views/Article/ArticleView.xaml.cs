using System;
using System.Windows.Navigation;
using Coding4Fun.Toolkit.Controls;
using Squirrel.Extensions;
using Squirrel.Model;
using Squirrel.Resources;
using Squirrel.ViewModel;

namespace Squirrel.Views.Article
{
    public partial class ArticleView
    {
        public ArticleView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ApplicationBar.Reset();
        }

        private void FontSizeButton_OnClick(object sender, EventArgs e)
        {
            new AppBarPrompt(
                new AppBarPromptAction(AppResources.LabelLarge, () => ChangeFontSize(Model.FontSize.Large)),
                new AppBarPromptAction(AppResources.LabelMedium, () => ChangeFontSize(Model.FontSize.Medium)),
                new AppBarPromptAction(AppResources.LabelSmall, () => ChangeFontSize(Model.FontSize.Small))
                ).Show();
        }

        private void ChangeFontSize(FontSize fontSize)
        {
            var vm = (ArticleViewModel)DataContext;
            vm.ArticleFontSize = fontSize;
        }

        private void ThemeButton_OnClick(object sender, EventArgs e)
        {
            new AppBarPrompt(
                new AppBarPromptAction("default", () => ChangeTheme(SquirrelTheme.Default)),
                new AppBarPromptAction("acorn", () => ChangeTheme(SquirrelTheme.Acorn)),
                new AppBarPromptAction("light", () => ChangeTheme(SquirrelTheme.Light))
                ).Show();
        }

        private void ChangeTheme(SquirrelTheme theme)
        {
            var vm = (ArticleViewModel) DataContext;
            vm.SquirrelTheme = theme;
        }
    }
}