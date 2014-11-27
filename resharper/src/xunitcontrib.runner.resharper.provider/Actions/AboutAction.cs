using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.UI.ActionsRevised;
using JetBrains.UI.MenuGroups;
using JetBrains.UI.StdApplicationUI.About;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Actions
{
  [Action("About xunit", Id = 90001)]
  public class AboutAction : IExecutableAction, IInsertAfter<HelpMenu, AboutBoxAction>
  {
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      return true;
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
      MessageBox.ShowInfo("Alloha!");
    }
  }
}