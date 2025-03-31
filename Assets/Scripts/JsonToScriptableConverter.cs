#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

public enum ConversionType
{
    Items,
    Dialogs
}

[Serializable]

public class DialogRowData
{
    public int? id;             //int?�� Nullable<int>�� ��� ǥ���Դϴ�. �����ϸ� null ���� ���� �� �ִ� �������� ��.
    public string characterName;
    public string text;
    public int? nextld;
    public string protraitPath;
    public string choiceText;
    public int? choiceNextld;
}

public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePath = "";                                   //JSON ���� ��� ���ڿ� ��
    private string outputFolder = "Assets/ScritapleObject/items";       //��� SO ������ ��� ��
    private bool createDatabase = true;                                 //������ ���̽��� ����� �������� ���� bool ��
    private ConversionType conversionType = ConversionType.Items;

    [MenuItem("Tools/JSOn to Scriptable Objects")]

    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSOn to Scriptable Objects");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("JSON to Scriptable Object Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        //��ȯ Ÿ�� ����
        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type:", conversionType);

        //Ÿ�Կ� ���� �⺻ ��� ���� ����
        if (conversionType == ConversionType.Items)
        {
            outputFolder = "Asset/scriptableObjects/Items";
        }
        else if (conversionType == ConversionType.Dialogs)
        {
            outputFolder = "Asset/scriptableObjects/Dialogs";
        }

        if (GUILayout.Button("Select JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Selet JSON File", "", "json");
        }

        EditorGUILayout.LabelField("Seleted File : ", jsonFilePath);
        EditorGUILayout.Space();
        outputFolder = EditorGUILayout.TextField("Output Folder :", outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Database Asset", createDatabase);

        if (GUILayout.Button("Convert to Scriptable Object"))
        {
            EditorUtility.DisplayDialog("Error", "Please select a JSON file firest!", "OK");
            return;
        }
        
        switch (conversionType)
        {
            case ConversionType.Items:
                convertJsonToScriptableObject();
                break;

            case ConversionType.Dialogs:
                convertJsonToScriptableObject();
                break;
        }
    }
    private void convertJsonToScriptableObject()                        //JSON ������ ���ò��� ���Ϸ� ��ȯ �����ִ� �Լ�
    {
        //���� ����
        if (!Directory.Exists(outputFolder))                             //���� ��ġ�� Ȯ���ϰ� ������ �����Ѵ�.
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON ���� �б�
        string jsonText = File.ReadAllText(jsonFilePath);               //JSON ������ �д´�.

        try
        {
            //JSON �Ľ�
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdITems = new List<ItemSO>();

            //�� ������ �����͸� ��ũ���ͺ� ������Ʈ�� ��ȯ
            foreach (var itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

                //������ ����
                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                //������ ��ȯ
                if (System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"������ '{itemData.itemName}'�� ��ȿ���� ���� Ÿ�� : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

                //������ �ε� (��ΰ� �ִ� ���)
                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Asset/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon == null)
                    {
                        Debug.LogWarning($"������'{itemData.nameEng}'�� �������� ã�� �� �����ϴ�. : {itemData.iconPath}");
                    }

                    string assetPath = $"{outputFolder}/item_{itemData.id.ToString("D4")}+{itemData.nameEng}";
                    createdITems.Add(itemSO);

                    EditorUtility.SetDirty(itemSO);
                }

                if (createDatabase && createdITems.Count > 0)
                {
                    ItemDatabaseSO database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
                    database.items = createdITems;

                    AssetDatabase.CreateAsset(database, $"{outputFolder}/ItemDatabase.asset");
                    EditorUtility.SetDirty(database);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Success", $"{createdITems.Count} scriptable object", "OK");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to Covert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ���� : {e}");
        }
    }
    private void ConvertJSonTODialogScriptableObjects()
    {
        //���� ����
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON ���� �б�
        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            //JSON �Ľ�
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(jsonText);

            //��ȭ ������ �籸��
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialogs = new List<DialogSO>();

            //1�ܰ� : ��ȭ �и� ����
            foreach(var rowData in rowDataList)
            {
                if (rowData.id.HasValue)
                {
                    DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                    //������ ����
                    dialogSO.id = rowData.id.Value;
                    dialogSO.characterName = rowData.characterName;
                    dialogSO.text = rowData.text;
                    dialogSO.nextild = rowData.nextld.HasValue ? rowData.nextld.Value : -1;
                    dialogSO.portraitPath = rowData.protraitPath;
                    dialogSO.choices = new List<DialogChoiceSO>();

                    if(!string.IsNullOrEmpty(rowData.protraitPath))
                    {
                        dialogSO.portrait = Resources.Load<Sprite>(rowData.protraitPath);

                        if(dialogSO.portrait == null)
                        {
                            Debug.LogWarning($"��ȭ {rowData.id}�� �ʻ�ȭ�� ã�� �� �����ϴ�.");
                        }
                        //dialogMap�� �߰�
                        dialogMap[dialogSO.id] = dialogSO;
                        createDialogs.Add(dialogSO);
                    }
                }

            }

            //2�ܰ� : ������ �и� ó�� �� ����
            foreach (var rowData in rowDataList)
            {
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextld.HasValue)
                {
                    int parentId = -1;

                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        { 
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }

                    if (parentId == -1)
                    {
                        Debug.LogWarning($"������ '{rowData.choiceText}'�� �θ� ��ȭ�� ã�� �� �����ϴ�.");
                    }

                    if (dialogMap.TryGetValue(parentId, out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextld = rowData.choiceNextld.Value;

                        string choiceAssetPath = $"{outputFolder}/Choice_{parentId}_{parentDialog.choices.Count + 1}.asset";
                        AssetDatabase.CreateAsset(choiceSO, choiceAssetPath);
                        EditorUtility.SetDirty(choiceSO);

                        parentDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"������ '{rowData.choiceText}'�� ������ ��ȭ (ID: {parentId}�� ã�� �� �����ϴ�.");
                    }
                }
            }

             

            //3�ܰ� : ��ȭ ��ũ���ͺ� ������Ʈ ����
            foreach (var dialog in createDialogs)
            {
                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset( dialog, assetPath );

                dialog.name = $"Dailog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty ( dialog );
            }

            if(createDatabase && createDialogs.Count > 0)
            {
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                database.dialogs = createDialogs;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/DialogDatabase.asset");
                EditorUtility.SetDirty( database );
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Creatd {createDialogs.Count} dialog scriptable object!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to convert JSON: {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ����: {e}");
        }
    }
}

#endif 