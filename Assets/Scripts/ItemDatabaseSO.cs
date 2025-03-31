using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Invetory/Database")]

public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();                     //itemSO�� ����Ʈ�� �����Ѵ�

    //ĳ���� ���� ����
    private Dictionary<int, ItemSO> itemsById;                          //ID�� ������ ã�� ���� ĳ��
    private Dictionary<string, ItemSO> itemsByName;                     //�̸����� ������ ã��

    public void Initalized()                                            //�ʱ� ���� �Լ�
    {
        itemsById = new Dictionary<int, ItemSO>();                      //���� ���� �߱� ������ Dictionary �Ҵ�
        itemsByName = new Dictionary<string, ItemSO>();

        foreach (var item in items)                                     //item ����Ʈ�� ���� �Ǿ� �ִ°��� ������ Dictionary�� �Է��Ѵ�.
        {
            itemsById[item.id] = item;
            itemsByName[item.itemName] = item;
        }
    }

    //ID�� ������ ã��

    public ItemSO GetItemByld(int id)
    {
        if (itemsById == null)                                            //itemsById �� ĳ���� �Ǿ� ���� �ʴٸ� �ʱ�ȭ �Ѵ�.
        {
            Initalized();
        }

        if (itemsById.TryGetValue(id, out ItemSO item))                   //id ���� ã�Ƽ� ItemSO �� ���� �Ѵ�.
            return item;

        return null;                                                     //���� ��� null
    }

    //�̸����� ������ ã��
    public ItemSO GetItemByName(string name)
    {
        if (itemsByName == null)                                         //itemsByName �� ĳ�� �Ǿ� ���� �ʴٸ� �ʱ�ȭ �Ѵ�.
        {
            Initalized();
        }
        if (itemsByName.TryGetValue(name, out ItemSO item))              //name ���� ã�Ƽ� ItemSO�� ���� �Ѵ�.
            return item;

        return null;
    }

    public List<ItemSO> GetItemByType(ItemType type)
    {
        return items.FindAll(item => item.itemType == type);
    }
}
