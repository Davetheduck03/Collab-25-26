using UnityEngine;

public class BookMarkButton : MonoBehaviour
{
    public BookManager book;

    public void OnStatsBookmark() => book.SwitchToPage(0);
    public void OnRodBookmark() => book.SwitchToPage(1);
    public void OnBoatBookmark() => book.SwitchToPage(2);
    public void OnHooksBookmark() => book.SwitchToPage(3);
}
