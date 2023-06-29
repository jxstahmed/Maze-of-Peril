using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderManager : MonoBehaviour
{

    [SerializeField] List<RenderItem> Renderable;

    [System.Serializable]
    class RenderItem
    {
        public List<string> IDs;
        public List<GameObject> ActivateGameObjects;
        public List<GameObject> DectivateGameObjects;
    }

    public static RenderManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void ValidateItems(string ID)
    {
        if (Renderable == null) return;

        for(int i = 0; i < Renderable.Count; i++)
        {
            if(Renderable[i].IDs.Contains(ID))
            {
                Renderable[i].IDs.Remove(ID);
                if (Renderable[i].IDs.Count == 0)
                {
                    Renderable[i].ActivateGameObjects.ForEach(m => {
                        if(m)
                        {
                            m.SetActive(true);
                        }
                        
                    });
                    Renderable[i].DectivateGameObjects.ForEach(m => {
                        if(m)
                        {
                            m.SetActive(false);
                        }
                        
                    });
                }
            }
        }
    }


}
