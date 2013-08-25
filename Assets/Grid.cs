
using System.Collections.Generic;

public class Grid {
	int[] grid;
	int width, height;
	
	public delegate void CellWasSetDelegate(int x, int y, int oldValue, int newValue);
	public event CellWasSetDelegate CellWasSet;
	
	public Grid(int width, int height) {
		this.width = width;
		this.height = height;
		grid = new int[width * height];
		
		for (int i = 0; i < width * height; ++i) {
			grid[i] = -1;
		}
	}
	
	public void SetCell(int x, int y, int newValue) {
		int oldValue = grid[y * width + x];
		grid[y * width + x] = newValue;
		if (CellWasSet != null) {
			CellWasSet(x, y, oldValue, newValue);
		}
	}
	
	public int GetCell(int x, int y) {
		return grid[y * width + x];
	}
	
	public void Swap(int x, int y, int x2, int y2) {
		int temp = grid[y * width + x];
		grid[y * width + x] = grid[y2 * width + x2];
		grid[y2 * width + x2] = temp;
	}
	
	public struct SearchResult {
		public int x, y;
	}
	
	public List<SearchResult> SearchForMatches(int smallestMatch) {
		bool[] marked = new bool[width * height];
		List<SearchResult> results = new List<SearchResult>();
		
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				int matches = 0;
				int cell = GetCell(x, y);
				if (cell != -1) {
					while (x + matches < width && GetCell(x + matches, y) == cell) {
						matches += 1;
					}
				
					if (matches >= smallestMatch) {
						while (matches > 0) {
							matches -= 1;
							marked[y * width + x + matches] = true;
						}
					}
				}
			}
		}
		
		for (int x = 0; x < width; ++x) {
			for (int y = 0; y < height; ++y) {
				int matches = 0;
				int cell = GetCell(x, y);
				if (cell != -1) {
					while (y + matches < height && GetCell(x, y + matches) == cell) {
						matches += 1;
					}
					
					if (matches >= smallestMatch) {
						while (matches > 0) {
							matches -= 1;
							marked[(y + matches) * width + x] = true;
						}
					}
				}
			}
		}

		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				if (marked[y * width + x]) {
					SearchResult result = new SearchResult();
					result.x = x;
					result.y = y;
					
					results.Add(result);
				}
			}
		}
		
		return results;
	}
}
