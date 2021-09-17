using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RoadGraphTest {

    [Test]
    public void File_Exists () {
        TextAsset textAsset = Resources.Load("Generated/RoadGraph") as TextAsset;
        Assert.NotNull(textAsset.text);
    }

    [Test]
    public void File_ContainsGraph() {
        TextAsset textAsset = Resources.Load("Generated/RoadGraph") as TextAsset;
        Assert.NotNull(JsonUtility.FromJson<RoadGraph>(textAsset.text));
    }
}
