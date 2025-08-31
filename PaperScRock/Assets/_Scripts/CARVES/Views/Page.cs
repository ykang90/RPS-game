namespace CARVES.Views
{
    public class Page : View
    {
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}