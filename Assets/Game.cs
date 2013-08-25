using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {
	
	public GameObject[] cubeTypes;
	public int width = 8;
	public int height = 8;
	public Vector2 spread = new Vector2(2.0f, 2.0f);
	public RandomizeMaterial backgroundMaterial;
	
	Grid grid;
	Cube[] cubes;
	bool inputEnabled = true;
	
	class Timer {
		public delegate void TimerDelegate();
		public TimerDelegate func;
		public float endTime;
	}
	
	List<Timer> timers = new List<Timer>();
	void addTimer(Timer.TimerDelegate func, float delay) {
		Timer timer = new Timer();
		timer.func = func;
		timer.endTime = Time.time + delay;
		timers.Add(timer);
	}
	
	// Use this for initialization
	void Start () {
		grid = new Grid(width, height);
		cubes = new Cube[width * height];

		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				int randomNumber = Random.Range(0, cubeTypes.Length);
				grid.SetCell(x, y, randomNumber);
				SetCell(x, y, randomNumber);
			}
		}
		
		transform.localScale = Vector3.one * (7.0f / height); 
		
		addTimer(() => {
			BlowShitUp();
		}, 1.0f);
		
		backgroundMaterial.Randomize();
	}
	
	Cube selection;
	Vector3 selectionPoint;
	
	void selectedCube(Cube cube) {
		cube.selected = true;
		cube.animation.Play("Selected");
	}
	
	void deselectedCube(Cube cube) {
		cube.selected = false;
	}
	
	void Swap(int x, int y, int otherX, int otherY) {
		Cube other = cubes[otherY * width + otherX],
			 selection = cubes[y * width + x];
		
		if (other == null || selection == null) {
			return;
		}
		
		grid.Swap(selection.X, selection.Y, other.X, other.Y);

		Vector3 tempPosition = other.originalPosition;
		other.originalPosition = selection.originalPosition;
		selection.originalPosition = tempPosition;
		
		cubes[selection.Y * width + selection.X] = other;
		cubes[other.Y * width + other.X] = selection;
		
		selection.X = otherX;
		selection.Y = otherY;
		other.X = x;
		other.Y = y;
	}

	void BlowShitUp() {
		BlowShitUp(grid.SearchForMatches(3));
	}
	
	void BlowShitUp(List<Grid.SearchResult> cubesToExplode) {
		if (cubesToExplode.Count > 0) {
			inputEnabled = false;
			if (selection != null) {
				deselectedCube(selection);
				selection = null;
			}

			Chainreaction(cubesToExplode);
	
			foreach (Grid.SearchResult result in cubesToExplode) {
				int x = result.x, y = result.y;
				Cube cube = cubes[result.y * width + result.x];
				cube.animation["Explode"].speed = Random.Range(0.5f, 1.0f);
				cube.animation.Play("Explode");
				cube.GetComponent<SoundEffectProxy>().Play("Explode");
				grid.SetCell(x, y, -1);
				addTimer(() => {
					SetCell(x, y, -1);
				}, 0.5f);
			}
		} else {
			inputEnabled = true;
		}
	}
	
	void Chainreaction(List<Grid.SearchResult> cubesToExplode) {
		List<Grid.SearchResult> newCubesToExplode = new List<Grid.SearchResult>();
		foreach (Grid.SearchResult result in cubesToExplode) {
			int cell = grid.GetCell(result.x, result.y);
			addChainreaction(cell, result.x + 1, result.y, newCubesToExplode, cubesToExplode);
			addChainreaction(cell, result.x - 1, result.y, newCubesToExplode, cubesToExplode);
			addChainreaction(cell, result.x, result.y + 1, newCubesToExplode, cubesToExplode);
			addChainreaction(cell, result.x, result.y - 1, newCubesToExplode, cubesToExplode);
		}

		addTimer(() => {
			BlowShitUp(newCubesToExplode);
		}, 0.6f);
	}
	
	void addChainreaction(int cell, int x, int y, List<Grid.SearchResult> newCubesToExplode, List<Grid.SearchResult> cubesToExplode) {
		if (x >= 0 && x < width && y >= 0 && y < height) {
			if (grid.GetCell(x, y) == cell) {
				if (!alreadyExploded(x, y, cubesToExplode)) {
					Grid.SearchResult result = new Grid.SearchResult();
					result.x = x;
					result.y = y;
					newCubesToExplode.Add(result);
				}
			}
		}
	}
	
	bool alreadyExploded(int x, int y, List<Grid.SearchResult> cubesToExplode) {
		bool found = false;
		foreach (Grid.SearchResult result in cubesToExplode) {
			if (result.x == x && result.y == y) {
				found = true;
				break;
			}
		}
		
		return found;
	}
	
	void SetCell(int x, int y, int newValue) {
		Cube oldCube = cubes[y * width + x];
		if (oldCube != null) {
			Destroy(oldCube.gameObject);
			cubes[y * width + x] = null;
		}
		
		if (newValue != -1) {
			GameObject cube = (GameObject)GameObject.Instantiate(cubeTypes[newValue]);
			Vector2 offset = new Vector2(-spread.x * width / 2.0f, -spread.y * height / 2.0f);
			
			cube.transform.parent = this.transform;
			cube.transform.localPosition = new Vector3(x * spread.x + offset.x, y * spread.y + offset.y, 0);
			
			Cube cubeComponent = cube.GetComponent<Cube>();
			cubeComponent.X = x;
			cubeComponent.Y = y;
			
			cubes[y * width + x] = cubeComponent;
		}
	}
	
	void cleanupTimers() {
		int i = 0;
		while (i < timers.Count) {
			Timer timer = timers[i];
			if (timer.endTime == float.MaxValue) {
				timers.Remove(timer);
				i = 0;
			} else {
				i += 1;
			}
		}
	}
	
	void Update () {
		for (int i = 0; i < timers.Count; ++i) {
			Timer timer = timers[i];
			if (Time.time >= timer.endTime) {
				timer.func();
				timer.endTime = float.MaxValue;
			}
		}
		cleanupTimers();

		if (inputEnabled && Input.GetMouseButtonDown(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				GameObject target = hit.collider.transform.gameObject;
				Cube cube = target.GetComponent<Cube>();
				if (cube != null) {
					selectionPoint = Input.mousePosition;
					selection = cube;
					selectedCube(cube);
				}
			}
		}
		
		if (selection) {
			float animationStart = 10.0f, animationEnd = 50.0f;
			Vector3 mouseDelta = Input.mousePosition - selectionPoint;
			float animationOffset = 0.0f;
			if (mouseDelta.magnitude >= animationStart) {
				if (mouseDelta.magnitude < animationEnd) {
					animationOffset = (mouseDelta.magnitude - animationStart) / (animationEnd - animationStart);
				} else {
					animationOffset = 1.0f;
				}
			} else {
				animationOffset = 0.0f;
			}
			
			Vector3 direction;
			if (Mathf.Max(Mathf.Abs(mouseDelta.x), Mathf.Abs(mouseDelta.y)) == Mathf.Abs(mouseDelta.x)) {
				if (mouseDelta.x < 0) {
					direction = Vector3.left;
				} else {
					direction = Vector3.right;
				}
			} else {
				if (mouseDelta.y < 0) {
					direction = Vector3.down;
				} else {
					direction = Vector3.up;
				}
			}
			
			selection.transform.localPosition = selection.originalPosition + new Vector3(direction.x * spread.x, direction.y * spread.y, 0) * animationOffset;

			if (inputEnabled && Input.GetMouseButtonUp(0)) {
				if (animationOffset == 1.0f) {
					int otherX = (int)direction.x + selection.X;
					int otherY = (int)direction.y + selection.Y;
					
					bool success = false;
					if (otherX >= 0 && otherX < width && otherY >= 0 && otherY < height) {
						Cube other = cubes[otherY * width + otherX];
						if (other != null) {
							success = true;
							Swap(selection.X, selection.Y, otherX, otherY);
						
							addTimer(() => {
								BlowShitUp();
							}, 0.4f);
						}
					}
					
					if (success) {
						selection.GetComponent<SoundEffectProxy>().Play("Selected");
					} else {
						selection.GetComponent<SoundEffectProxy>().Play("NotSelected");
					}
				}
				
				deselectedCube(selection);
				selection = null;
			}
		}
	}
}
