namespace CARVES.Views
{
    /// <summary>
    /// 窗口Ui
    /// </summary>
    public abstract class WinUiBase : PageUiBase
    {
        protected WinUiBase(IView v, bool display = false) : base(v, display)
        {
        }
    }
    /// <summary>
    /// 页面类
    /// </summary>
    public abstract class PageUiBase : UiBase
    {
        protected PageUiBase(IView v, bool display = false) : base(v, display)
        {

        }
    }
}