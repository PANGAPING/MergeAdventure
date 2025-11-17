using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Excel;
using System.Data;

public class ExcelTools : EditorWindow
{
    private static ExcelTools instance;

    private static List<string> selectedExcelList;

    private static string pathRoot;

    private static Vector2 scrollPos;

    private static int indexOfFormat = 0;

    private static string[] formatOption = new string[] { "JSON", "CSV", "XML" };

    private static int indexOfEncoding = 0;

    private static string[] encodingOption = new string[] { "UTF-8", "GB2312" };

    [MenuItem("Plugins/ExcelTools")]
    static void ShowExcelTools()
    {
        Init();
        LoadExcel();
        instance.Show();
    }

    void OnGUI()
    {
        DrawOptions();
        DrawExport();
        DrawGenerateLanguageFiles();

    }
    private void DrawOptions()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("2 Type:", GUILayout.Width(60));
        indexOfFormat = EditorGUILayout.Popup(indexOfFormat, formatOption, GUILayout.Width(125));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Encode Type:", GUILayout.Width(85));
        indexOfEncoding = EditorGUILayout.Popup(indexOfEncoding, encodingOption, GUILayout.Width(125));
        GUILayout.EndHorizontal();

    }

    private void DrawExport()
    {
        if (GUILayout.Button("Default Converts"))
        {
            DefaultConvert();
        }

        if (selectedExcelList == null) return;
        if (selectedExcelList.Count < 1)
        {
            EditorGUILayout.LabelField("No Excel is being Choosen.");
        }
        else
        {
            EditorGUILayout.LabelField("Follow projects will be converted to " + formatOption[indexOfFormat] + ":");
            GUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(150));
            foreach (string s in selectedExcelList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Toggle(true, s);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (GUILayout.Button("Convert Selected"))
            {
                ConvertSelected();
            }
        }

    }

    private void DrawGenerateLanguageFiles()
    {
        if (GUILayout.Button("Generate Language Files"))
        {
            GenerateLanguageFiles();
        }
    }

    private static void GenerateLanguageFiles()
    {

        string localizationExcelFilePath = Path.Combine(Application.dataPath, "Resources", "Localization", "Localization.xlsx");

        if (!File.Exists(localizationExcelFilePath))
        {
            Debug.LogError("Localization File Missed.");
        }

        ExcelUtility excelUtility = new ExcelUtility(localizationExcelFilePath);
        WriteLanguageData(excelUtility.mResultSet);
    }

    private static void WriteLanguageData(DataSet dataset)
    {
        DataTable localizationSheet = dataset.Tables[0];

        int colCount = localizationSheet.Columns.Count;
        int rowCount = localizationSheet.Rows.Count;


        string languageFilesFold = Path.Combine(Application.dataPath, "Resources", "Localization", "LanguageFiles");

        DirectoryInfo di = new DirectoryInfo(languageFilesFold);
        foreach (FileInfo file in di.GetFiles()) { 
            file.Delete();
        }


        for (int i = 1; i < colCount; i++)
        {
            string colName = localizationSheet.Rows[0][i].ToString();
            string filename = colName;
            string filepath = Path.Combine(languageFilesFold, filename) + ".txt";

            using (StreamWriter writer = new StreamWriter(filepath, true))
            {
                for (int j = 1; j < rowCount; j++)
                {
                    string key = localizationSheet.Rows[j][0].ToString();
                    string value = localizationSheet.Rows[j][i].ToString();

                    writer.WriteLine(key + "&" + value);
                }

                writer.Close();
            }
        }

    }

    private static void DefaultConvert()
    {
        List<string> excelList = new List<string>();

        string excelFolderPath = Path.Combine(Application.dataPath, "Resources", "Excel");
        string jsonFolderPath = Path.Combine(Application.dataPath, "Resources", "Json");

        if (!Directory.Exists(excelFolderPath))
        {
            Directory.CreateDirectory(excelFolderPath);
        }
        if (!Directory.Exists(jsonFolderPath))
        {
            Directory.CreateDirectory(jsonFolderPath);
        }


        string[] filePaths = Directory.GetFiles(excelFolderPath);

        foreach (string filePath in filePaths)
        {
            if (filePath.EndsWith(".xlsx"))
            {
                excelList.Add(Path.GetFileName(filePath));
            }
        }

        foreach (string excelName in excelList)
        {
            string excelPath = Path.Combine(excelFolderPath, excelName);
            ExcelUtility excelUtility = new ExcelUtility(excelPath);

            Encoding encoding = null;
            if (indexOfEncoding == 0)
            {
                encoding = Encoding.GetEncoding("utf-8");
            }
            else if (indexOfEncoding == 1)
            {
                encoding = Encoding.GetEncoding("gb2312");
            }

            string outputPath = "";
            string jsonFilename = excelName.Replace(".xlsx", ".json");
            outputPath = Path.Combine(jsonFolderPath, jsonFilename);
            excelUtility.ConvertToJson(outputPath, encoding);
            AssetDatabase.Refresh();
        }

    }


    private static void ConvertSelected()
    {
        foreach (string assetsPath in selectedExcelList)
        {
            string excelPath = pathRoot + "/" + assetsPath;
            ExcelUtility excelUtility = new ExcelUtility(excelPath);

            Encoding encoding = null;
            if (indexOfEncoding == 0)
            {
                encoding = Encoding.GetEncoding("utf-8");
            }
            else if (indexOfEncoding == 1)
            {
                encoding = Encoding.GetEncoding("gb2312");
            }

            string output = "";
            if (indexOfFormat == 0)
            {
                output = excelPath.Replace(".xlsx", ".json");
                excelUtility.ConvertToJson(output, encoding);
            }
            else if (indexOfFormat == 1)
            {
                output = excelPath.Replace(".xlsx", ".csv");
                excelUtility.ConvertToCSV(output, encoding);
            }
            else if (indexOfFormat == 2)
            {
                output = excelPath.Replace(".xlsx", ".xml");
                excelUtility.ConvertToXml(output);
            }

            AssetDatabase.Refresh();
        }

        instance.Close();
    }

    private static void LoadExcel()
    {
        if (selectedExcelList == null) selectedExcelList = new List<string>();
        selectedExcelList.Clear();
        object[] selection = (object[])Selection.objects;
        if (selection.Length == 0)
            return;
        foreach (Object obj in selection)
        {
            string objPath = AssetDatabase.GetAssetPath(obj);
            if (objPath.EndsWith(".xlsx"))
            {
                selectedExcelList.Add(objPath);
            }
        }
    }

    private static void Init()
    {
        instance = EditorWindow.GetWindow<ExcelTools>();
        pathRoot = Application.dataPath;

        pathRoot = pathRoot.Substring(0, pathRoot.LastIndexOf("/"));
        selectedExcelList = new List<string>();
        scrollPos = new Vector2(instance.position.x, instance.position.y + 75);
    }

    void OnSelectionChange()
    {
        Show();
        LoadExcel();
        Repaint();
    }
}
