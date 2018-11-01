using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

	public List<GameObject> nextTiles;
    public List<GameObject> previousTiles;
    public bool isFinal = false;
}
