using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadioView : MonoBehaviour {
    
    Permanent perm;
    
    GameObject disconnected;
    GameObject still;
    GameObject talking;
    GameObject buzz;

    Dictionary<string, GameObject> objs = new Dictionary<string, GameObject>();

    public void Refresh () {
        if (!perm) perm = Permanent.get();

        if (!objs.ContainsKey("disc")) objs["disc"] = transform.Find("Disc").gameObject;
        if (!objs.ContainsKey("still")) objs["still"] = transform.Find("Still").gameObject;
        if (!objs.ContainsKey("talking")) objs["talking"] = transform.Find("Talking").gameObject;
        if (!objs.ContainsKey("buzz")) objs["buzz"] = transform.Find("Buzz").gameObject;
        
        string state = "disc";
        if (perm && perm.net.open) {
            if (!perm.getRadio().talking) state = "still";
            else {
                if (perm.getRadio().radioTooManySources) state = "buzz";
                else state = "talking";
            }
        }

        foreach (var pair in objs) { 
            if (pair.Value.activeSelf && state != pair.Key) {
                pair.Value.SetActive(false);
			}
            if (!pair.Value.activeSelf && state == pair.Key) {
                pair.Value.SetActive(true);
			}
        }
    }
}
