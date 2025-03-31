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
    public int? id;             //int?는 Nullable<int>의 축약 표현입니당. 선언하면 null 값도 가질 수 있는 정수형이 되.
    public string characterName;
    public string text;
    public int? nextld;
    public string protraitPath;
    public string choiceText;
    public int? choiceNextld;
}

public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePath = "";                                   //JSON 파일 경로 문자열 값
    private string outputFolder = "Assets/ScritapleObject/items";       //출력 SO 파일을 경로 값
    private bool createDatabase = true;                                 //데이터 베이스를 사용할 것인지에 대한 bool 값
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

        //변환 타입 선택
        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type:", conversionType);

        //타입에 따라 기본 출력 폴더 설정
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
    private void convertJsonToScriptableObject()                        //JSON 파일을 저시깽이 파일로 변환 시켜주는 함수
    {
        //폴더 생성
        if (!Directory.Exists(outputFolder))                             //폴더 위치를 확인하고 없으면 생성한다.
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON 파일 읽기
        string jsonText = File.ReadAllText(jsonFilePath);               //JSON 파일을 읽는다.

        try
        {
            //JSON 파싱
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdITems = new List<ItemSO>();

            //각 아이템 데이터를 스크립터블 오브젝트로 변환
            foreach (var itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

                //데이터 복사
                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                //열거형 변환
                if (System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"아이템 '{itemData.itemName}'의 유효하지 않은 타입 : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

                //아이콘 로드 (경로가 있는 경우)
                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Asset/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon == null)
                    {
                        Debug.LogWarning($"아이템'{itemData.nameEng}'의 아이콘을 찾을 수 없습니다. : {itemData.iconPath}");
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
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }
    private void ConvertJSonTODialogScriptableObjects()
    {
        //폴더 생성
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON 파일 읽기
        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            //JSON 파싱
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(jsonText);

            //대화 데이터 재구성
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialogs = new List<DialogSO>();

            //1단계 : 대화 학목 생성
            foreach(var rowData in rowDataList)
            {
                if (rowData.id.HasValue)
                {
                    DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                    //데이터 복사
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
                            Debug.LogWarning($"대화 {rowData.id}의 초상화를 찾을 수 없습니다.");
                        }
                        //dialogMap에 추가
                        dialogMap[dialogSO.id] = dialogSO;
                        createDialogs.Add(dialogSO);
                    }
                }

            }

            //2단계 : 선택지 학목 처리 및 연결
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
                        Debug.LogWarning($"선택지 '{rowData.choiceText}'의 부모 대화를 찾을 수 없습니다.");
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
                        Debug.LogWarning($"선택지 '{rowData.choiceText}'를 연결할 대화 (ID: {parentId}를 찾을 수 없습니다.");
                    }
                }
            }

             

            //3단게 : 대화 스크립터블 오브젝트 저장
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
            Debug.LogError($"JSON 변환 오류: {e}");
        }
    }
}

#endif 