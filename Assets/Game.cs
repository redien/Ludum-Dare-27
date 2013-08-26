using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {
	
	public GameObject[] cubeTypes;
	public int width = 8;
	public int height = 8;
	public Vector2 spread = new Vector2(2.0f, 2.0f);
	public RandomizeMaterial backgroundMaterial;
	
	public Material[] cubeMaterials = null;
	
	Grid grid;
	Cube[] cubes;
	bool inputEnabled = true;
	bool filled = false;
	bool didSomething = false;
	
	public float animationTimeScale = 1.0f;
	
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
	
	Vector3 fixedScale;
	Vector3 fixedPosititon;
	Color fixedTimeColor;
	
	bool gameOver = false;
	int highscore = 0;
	
	// Use this for initialization
	void Start () {
		grid = new Grid(width, height);
		cubes = new Cube[width * height];

		transform.localScale = Vector3.one * (7.0f / height);
		fixedScale = transform.localScale;
		fixedPosititon = transform.localPosition;
		fixedTimeColor = timeText.color;
		
		adjustForAspect();
		
		FillUp();

		addTimer(() => {
			BlowShitUp();
		}, 1.0f * animationTimeScale);
		
		Random.Range(0, 100);
		Random.Range(0, 100);
		Random.Range(0, 100);
		backgroundMaterial.Randomize();
		
		if (PlayerPrefs.HasKey("highscore")) {
			highscore = PlayerPrefs.GetInt("highscore");
		}
		
		Color[] colors = {
			new Color32(244, 14, 14, 255), new Color32(93, 188, 69, 255),
			new Color32(222, 115, 223, 255), new Color32(154, 0, 67, 255),
			new Color32(188, 229, 94, 255), new Color32(154, 118, 0, 255),
			new Color32(0, 171, 255, 255), new Color32(0, 0, 0, 255),
			new Color32(255, 255, 255, 255), new Color32(13, 171, 20, 255),
			new Color32(148, 0, 120, 255), new Color32(234, 204, 31, 255),
			new Color32(214, 24, 59, 255), new Color32(38, 4, 18, 255),
			new Color32(255, 238, 2, 255), new Color32(38, 42, 126, 255),
			new Color32(255, 255, 255, 255), new Color32(69, 114, 188, 255),
			new Color32(255, 111, 0, 255), new Color32(171, 171, 171, 255),
		};
		
		// Color list must be larger than material list
		
		List<int> indecies = new List<int>();
		while (indecies.Count < cubeMaterials.Length) {
			int index = Random.Range(0, colors.Length / 2) * 2;
			if (!indecies.Contains(index)) {
				indecies.Add(index);
			}
		}
		
		for (int i = 0; i < cubeMaterials.Length; ++i) {
			Material material = cubeMaterials[i];
			
			material.SetColor("_StripeColor", colors[indecies[i] + 0]);
			material.SetColor("_BackgroundColor", colors[indecies[i] + 1]);
		}
	}

	Cube selection;
	Vector3 selectionPoint;
	
	void selectedCube(Cube cube) {
		cube.selected = true;
		cube.animation.Play("Selected");
		cube.GetComponent<SoundEffectProxy>().Play("Pressed");
		didSomething = true;
	}
	
	void deselectedCube(Cube cube) {
		cube.selected = false;
	}
	
	void Swap(int x, int y, int otherX, int otherY) {
		Cube other = cubes[otherY * width + otherX],
			 selection = cubes[y * width + x];
		
		grid.Swap(x, y, otherX, otherY);
		
		cubes[y * width + x] = other;
		cubes[otherY * width + otherX] = selection;

		if (other) {
			other.SetOriginalPosition(CalculateCubePosition(x, y));
			other.X = x;
			other.Y = y;
		}
		
		if (selection) {
			selection.SetOriginalPosition(CalculateCubePosition(otherX, otherY));
			selection.X = otherX;
			selection.Y = otherY;
		}
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
			filled = false;
			
			Chainreaction(cubesToExplode);
			
			actualScore += cubesToExplode.Count * 10;
			foreach (Grid.SearchResult result in cubesToExplode) {
				int x = result.x, y = result.y;
				Cube cube = cubes[result.y * width + result.x];
				cube.animation["Explode"].speed = Random.Range(0.5f, 1.0f);
				cube.animation.Play("Explode");
				cube.GetComponent<SoundEffectProxy>().Play("Explode");
				grid.SetCell(x, y, -1);
				addTimer(() => {
					SetCell(x, y, -1);
				}, 0.5f * animationTimeScale);
			}
		} else {
			if (filled && gameOver) {
				addTimer(() => {
					if (actualScore > highscore) {
						PlayerPrefs.SetInt("highscore", (int)actualScore);
					}
					Application.LoadLevel(1);
				}, 3.0f);
			}
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
		
		if (newCubesToExplode.Count == 0) {
			addTimer (() => {
				FillUp();
			}, 0.7f * animationTimeScale);
		}
		addTimer(() => {
			BlowShitUp(newCubesToExplode);
		}, 0.6f * animationTimeScale);
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
	
	Vector3 CalculateCubePosition(int x, int y) {
		Vector2 offset = new Vector2(-spread.x * width / 2.0f, -spread.y * height / 2.0f);
		return new Vector3(x * spread.x + offset.x, y * spread.y + offset.y, 0);
	}
	
	void SetCell(int x, int y, int newValue) {
		Cube oldCube = cubes[y * width + x];
		if (oldCube != null) {
			Destroy(oldCube.gameObject);
			cubes[y * width + x] = null;
		}
		
		if (newValue != -1) {
			GameObject obj = (GameObject)GameObject.Instantiate(cubeTypes[newValue]);
			Cube cube = obj.GetComponent<Cube>();
			
			cube.transform.parent = this.transform;
			cube.SetOriginalPosition(CalculateCubePosition(x, y));
			cube.transform.localPosition = cube.originalPosition;
			
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
	
	void FillUp() {
		for (int x = 0; x < width; ++x) {
			int swapped;
			do {
				swapped = 0;
				for (int y = 0; y < height - 1; ++y) {
					if (grid.GetCell(x, y) == -1 && grid.GetCell(x, y + 1) != -1) {
						swapped += 1;
						Swap(x, y, x, y + 1);
					}
				}
			} while (swapped != 0);
		}
	
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				if (grid.GetCell(x, y) == -1) {
					int randomNumber = Random.Range(0, cubeTypes.Length);
					grid.SetCell(x, y, randomNumber);
					SetCell(x, y, randomNumber);
					
					Cube cube = cubes[y * width + x];
					cube.transform.localPosition += new Vector3(0.0f, 10.0f, 0.0f);
					cube.transform.localScale = cube.transform.localScale * (7.0f / height) * textRoomFactor;
				}
			}
		}
		
		filled = true;
		
		addTimer(() => {
			inputEnabled = true;
			BlowShitUp();
		}, 0.5f * animationTimeScale);
	}
	
	float comboTime = 0.0f;
	public GUIText timeText = null;
	
	float actualScore = 0.0f, animatedScore = 0.0f;
	public GUIText pointsText = null;
	
	float textRoomFactor = 1.0f;
	
	public GameObject quitButton = null; 
	
	void adjustForAspect() {
		float aspect = (float)Screen.width / Screen.height;
		Vector2 offset = Vector2.zero;
		for (int i = 0; i < texts.Length; ++i) {
			GUIText text = texts[i];
			GUIStyle style = new GUIStyle();
			style.font = text.font;
			style.fontSize = text.fontSize;
			style.fontStyle = text.fontStyle;
			Vector2 size = style.CalcSize(new GUIContent(text.text));

			if (aspect > 1.5f) {
				text.pixelOffset = new Vector2(-Screen.width / 2, Screen.height / 2);
				text.pixelOffset += new Vector2(Screen.width * 0.02f, -Screen.height * 0.05f);
				text.pixelOffset += offset;
				text.fontSize = 36;
				text.alignment = TextAlignment.Left;
				offset += new Vector2(0.0f, -size.y);
				textRoomFactor = 1.0f;
			} else {
				text.pixelOffset = new Vector2(-Screen.width / 2, -Screen.height / 2);
				text.pixelOffset += new Vector2(Screen.width * 0.05f, Screen.height * 0.05f + size.y);
				text.pixelOffset += offset;
				text.fontSize = 30;
				text.alignment = TextAlignment.Center;
				offset += new Vector2(Screen.width * 0.3f, 0.0f);
				textRoomFactor = 0.8f;
			}
		}
		
		Transform button = quitButton.transform.parent;

		if (textRoomFactor < 1.0f) {
			transform.localScale = fixedScale * textRoomFactor;
			transform.localPosition = fixedPosititon + textRoomFactor * Vector3.up;
			
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width - 40.0f, 40.0f, 0.0f));
			button.position = ray.origin + ray.direction * 7.0f;
		} else {
			transform.localScale = fixedScale;
			transform.localPosition = fixedPosititon + Vector3.right * fixedScale.x * 1.7f;
			
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(60.0f, 60.0f, 0.0f));
			button.position = ray.origin + ray.direction * 7.0f;
		}
	}
	
	public GUIText[] texts = null;
	void Update () {
		if (gameOver) {
			inputEnabled = false;
		}
		
		adjustForAspect();		

		animatedScore += (actualScore - animatedScore) * Time.deltaTime * 2;
		if (actualScore - animatedScore < 0.5f) {
			animatedScore = actualScore;
		}
		
		string newText = "" + (int)animatedScore;
		if (highscore > 0) {
			newText += "/" + highscore;
		}
		if (pointsText.text != newText) {
			GetComponent<SoundEffectProxy>().Play("Score");
		}
		pointsText.text = newText;
		
		if (!gameOver && didSomething) {
			if (!inputEnabled) {
				comboTime += Time.deltaTime;
			} else {
				comboTime = 0.0f;
			}
			if (comboTime > 2.0f) {
				newText = "   " + (int)comboTime;
				if (timeText.text != newText && comboTime > 6.0f) {
					GetComponent<SoundEffectProxy>().Play("Time");
				}
				timeText.text = newText;
			} else {
				timeText.text = "";
			}

			if (comboTime > 6.0f) {
				timeText.color = Color.red;
			} else {
				timeText.color = fixedTimeColor;
			}
			
			if (comboTime > 10.0f) {
				gameOver = true;
			}
		}
		
		for (int i = 0; i < timers.Count; ++i) {
			Timer timer = timers[i];
			if (Time.time >= timer.endTime) {
				timer.func();
				timer.endTime = float.MaxValue;
			}
		}
		cleanupTimers();

		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				GameObject target = hit.collider.transform.gameObject;
				
				if (Input.GetMouseButtonUp(0) && target == quitButton) {
					Application.LoadLevel(0);
				}
				
				if (Input.GetMouseButtonDown(0)) {
					if (target == quitButton) {
						quitButton.transform.parent.animation.Play("Selected");
						quitButton.transform.parent.audio.Play();
					}
				
					if (inputEnabled) {
						Cube cube = target.GetComponent<Cube>();
						if (cube != null) {
							selectionPoint = Input.mousePosition;
							selection = cube;
							selectedCube(cube);
						}
					}
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
							grid.Swap(selection.X, selection.Y, otherX, otherY);
							var change = grid.SearchForMatches(3);
							grid.Swap(selection.X, selection.Y, otherX, otherY);
							if (change.Count > 0) {
								success = true;
								Swap(selection.X, selection.Y, otherX, otherY);
							
								addTimer(() => {
									BlowShitUp();
								}, 0.4f * animationTimeScale);
							}
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
