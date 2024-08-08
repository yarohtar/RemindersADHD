using RemindersADHD.CustomControls;
using RemindersADHD.MVVM.Views;

namespace RemindersADHD
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            TestFlyoutItem.ContentTemplate = new DataTemplate(() => new TestControlsView());
            HabitsFlyoutItem.ContentTemplate = new DataTemplate(() => new HabitsView());
            ShoppingListFlyoutItem.ContentTemplate = new DataTemplate(()=>new ShoppingListView());
            PomodoroFlyoutItem.ContentTemplate = new DataTemplate(() => new Pomodoro());
            Routing.RegisterRoute("shoppingitemedit", typeof(ShoppingItemEditView));
            Routing.RegisterRoute("habitedit", typeof(HabitEditView));
        }
    }
}
