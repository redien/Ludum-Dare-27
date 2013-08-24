using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {
	
	public GameObject[] cubeTypes;
	public int width = 8;
	public int height = 8;
	public Vector2 spread = new Vector2(2.0f, 2.0f);
	
	Grid grid;
	
	// Use this for initialization
	void Start () {
		Vector2 offset = new Vector2(-spread.x * width / 2.0f, -spread.y * height / 2.0f);

		grid = new Grid(width, height);
		
		grid.CellWasSet += (x, y, oldValue, newValue) => {
			if (oldValue == -1) {
				GameObject cube = (GameObject)GameObject.Instantiate(cubeTypes[newValue]);
				
				cube.transform.parent = this.transform;
				cube.transform.localPosition = new Vector3(x * spread.x + offset.x, y * spread.y + offset.y, 0);
				
				Cube cubeComponent = cube.GetComponent<Cube>();
				cubeComponent.X = x;
				cubeComponent.Y = y;
			}
		};
		
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				int randomNumber = Random.Range(0, cubeTypes.Length - 1);
				grid.SetCell(x, y, randomNumber);
			}
		}
	}
	
	Cube selection;
	
	void selectedCube(Cube cube) {
		
	}
	
	void selectedSecondCube(Cube cube) {
		Cube first = selection, second = cube;
		
	}

	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				GameObject target = hit.collider.transform.gameObject;
				Cube cube = target.GetComponent<Cube>();
				if (cube != null) {
					if (!selection) {
						selection = cube;
						selectedCube(cube);
					} else {
						selectedSecondCube(cube);
						selection = null;
					}
				}
			}
		}
	}
}
