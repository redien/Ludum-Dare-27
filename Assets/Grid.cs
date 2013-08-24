
public class Grid {
	int[] grid;
	int width;
	
	public delegate void CellWasSetDelegate(int x, int y, int oldValue, int newValue);
	public event CellWasSetDelegate CellWasSet;
	
	public Grid(int width, int height) {
		this.width = width;
		grid = new int[width * height];
		
		for (int i = 0; i < width * height; ++i) {
			grid[i] = -1;
		}
	}
	
	public void SetCell(int x, int y, int newValue) {
		int oldValue = grid[y * width + x];
		grid[y * width + x] = newValue;
		CellWasSet(x, y, oldValue, newValue);
	}
	
	public int GetCell(int x, int y, int newValue) {
		return grid[y * width + x];
	}
}
