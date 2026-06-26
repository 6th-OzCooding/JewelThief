using UnityEngine;
public interface IItemDropable
{
    // 함정이 이 함수를 호출하면, 인벤토리는 count만큼 아이템을 땅에 드롭.
    void ForceDropItem(int count);
}
public interface IItemInsertable
{
    // 함정이 이 함수를 호출하면, 쓸모없는 '돌'아이템을 count만큼 인벤토리에 추가.
    void ForceInsertItem(ItemData itemData, int count);
}