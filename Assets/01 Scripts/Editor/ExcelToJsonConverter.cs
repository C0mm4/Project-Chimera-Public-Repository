using ExcelDataReader;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExcelToJsonConverter : EditorWindow
{
    [MenuItem("Tools/Excel to JSON")]
    static void OpenWindow() => GetWindow<ExcelToJsonConverter>("Excel to JSON");

    string excelFolder = "Assets/10 Data/Excel/";
    string outputJsonPath = "Assets/10 Data/JSON/";
    int sheetIndex = 0;

    private void OnGUI()
    {
        GUILayout.Label("Excel -> JSON 변환", EditorStyles.boldLabel);
        excelFolder = EditorGUILayout.TextField("Excel 파일 경로", excelFolder);
        outputJsonPath = EditorGUILayout.TextField("출력 JSON 경로", outputJsonPath);
        sheetIndex = EditorGUILayout.IntField("시트 인덱스", sheetIndex);

        GUILayout.Label("Excel 폴더 내 .xlsx파일 모두 변환", EditorStyles.boldLabel);
        if (GUILayout.Button("모두 변환"))
            ConvertAll();
    }

    void ConvertExcelToJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("엑셀 파일을 찾을 수 없음: " + filePath);
            return;
        }


        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var conf = new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                };

                var dataSet = reader.AsDataSet(conf);

                if (sheetIndex < 0 || sheetIndex >= dataSet.Tables.Count)
                {
                    Debug.LogError("잘못된 시트 인덱스");
                    return;
                }

                var table = dataSet.Tables[sheetIndex];
                var records = new List<Dictionary<string, object>>();

                foreach (System.Data.DataRow row in table.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (System.Data.DataColumn col in table.Columns)
                    {
                        object value = row[col];

                        if (value is double d)
                        {
                            if (Math.Abs(d % 1) < double.Epsilon)
                                value = (int)d;
                        }
                        else if (value is string str)
                        {
                            if (str.StartsWith('[') && str.EndsWith(']'))
                            {
                                value = JsonConvert.DeserializeObject<List<int>>(str);
                            }
                        }
                        dict[col.ColumnName] = value; // object로 보관
                    }
                    records.Add(dict);
                }

                // Newtonsoft로 직렬화 (편리하고 유연)
                //var wrapper = new { items = records };
                string json = JsonConvert.SerializeObject(records, Formatting.Indented);

                // 폴더 없으면 생성
                if (!Directory.Exists(outputJsonPath)) Directory.CreateDirectory(outputJsonPath);

                string jsonFilePath = outputJsonPath + System.IO.Path.GetFileNameWithoutExtension(filePath) + ".json";
                File.WriteAllText(jsonFilePath, json, Encoding.UTF8);
                AssetDatabase.Refresh();
                Debug.Log($"Excel → JSON 변환 완료: {outputJsonPath} (레코드: {records.Count})");
            }
        }
    }

    void ConvertAll()
    {
        string[] excelFiles = Directory.GetFiles(excelFolder, "*.xlsx");
        // Debug.Log(excelFiles.Length);
        foreach (string excelFile in excelFiles)
        {
            // Debug.Log(excelFile);
            ConvertExcelToJson(excelFile);
        }
    }
}
