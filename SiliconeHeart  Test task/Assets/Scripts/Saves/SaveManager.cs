using UnityEngine;
using Esper.ESave;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{

    [SerializeField]
    private GameObject[] HousesPref;

    private SaveFile _saveFile;


    private const string saveKey = "world_save"; 

    private static List<Vector3> positions;

    private void OnEnable()
    {
        MoveController.SavePosicion += Save;
        UIController.OnSelectDestroyObject += DeleteData;

        _saveFile = GetComponent<SaveFileSetup>().GetSaveFile();
    }

    private void Start()
    {

        positions = new List<Vector3>();

        Load();
    }

    public void DeleteData(Vector3 position)
    {
        int index = positions.IndexOf(position);

        positions.RemoveAt(index);

        for (int i = index; i < positions.Count; i++)
        {
            _saveFile.AddOrUpdateData(i.ToString(), positions[i]);

            // Берём prefIndex следующего элемента, если он есть
            int nextPrefIndex = _saveFile.GetData<int>("prefIndex" + (i + 1)); // подстраховка

            _saveFile.AddOrUpdateData("prefIndex" + i, nextPrefIndex);
        }

        _saveFile.DeleteData(positions.Count.ToString());
        _saveFile.DeleteData("prefIndex" + positions.Count);

        _saveFile.AddOrUpdateData(saveKey, positions.Count);

        _saveFile.Save();

        Debug.Log("After delete: " + positions.Count);
    }

    private void Save(Vector3 position, int prefIndex)
    {

        positions.Add(position);
        _saveFile.AddOrUpdateData((positions.Count-1).ToString(), position);
        _saveFile.AddOrUpdateData("prefIndex" + (positions.Count - 1), prefIndex);
        _saveFile.AddOrUpdateData(saveKey, positions.Count);
        _saveFile.Save();
    }

    private void Load()
    {
        for(int i = 0; i <= _saveFile.GetData<int>(saveKey, -1) - 1; i++)
        {
            Vector3 position = _saveFile.GetVector3(i.ToString());
            Destroy(Instantiate(HousesPref[
                _saveFile.GetData<int>("prefIndex" + i)],
                position,
                Quaternion.identity).
                GetComponent<MoveController>());

            positions.Add(position);
        }
    }

    private void OnDestroy()
    {
        if(positions.Count == 0)
            _saveFile.DeleteFile();
    }
}
