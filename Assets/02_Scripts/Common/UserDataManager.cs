using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserDataManager
{
    public List<IUserData> userDataList { get; private set; } = new List<IUserData>();

    public void Init()
    {
        userDataList.Add(new UserPlayData());
    }

    public void SetDefaultUserData()
    {
        for (int i = 0; i < userDataList.Count; i++)
        {
            userDataList[i].SetDefaultData();
        }
    }

    public void LoadUserData()
    {
        for (int i = 0; i < userDataList.Count; i++)
        {
            userDataList[i].LoadData();
        }
    }

    public void SaveUserData()
    {
        for (int i = 0; i < userDataList.Count; i++)
        {
            userDataList[i].SaveData();
        }
    }

    public T GetUserData<T>() where T : class, IUserData
    {
        return userDataList.OfType<T>().FirstOrDefault();
    }
}
