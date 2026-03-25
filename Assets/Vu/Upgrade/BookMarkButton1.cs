using UnityEngine;

public class BookMarkButton1 : MonoBehaviour
{
    public BookManager book1;

    public void OnRodBookmark() => book1.SwitchToPage(0);
    public void OnBoatBookmark() => book1.SwitchToPage(1);
    public void OnHooksBookmark() => book1.SwitchToPage(2);
}
