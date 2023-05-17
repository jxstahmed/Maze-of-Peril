using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScriptableKey
{
    private string ID;
    private string Label;

    public ScriptableKey(string ID, string Label)
    {
        this.ID = ID;
        this.Label = Label;
    }

    public void setID(string ID)
    {
        this.ID = ID;
    }
    public void setLabel(string Label)
    {
        this.Label = Label;
    }

    public string getID()
    {
        return this.ID;
    }

    public string getLabel()
    {
        return this.Label;
    }


    
    public override string ToString()
    {
        return "Label[" + Label + "], ID[" + ID + "]" ;
    }
}
