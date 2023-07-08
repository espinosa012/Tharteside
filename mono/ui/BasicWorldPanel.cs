using Godot;
using System;

public partial class BasicWorldPanel : Panel
{
	Tartheside.mono.WorldManager TWorldManager;
	Button UpdateButton;

	public override void _Ready()
	{
		// ConnectButtonSignal();
	}

	public void SetTWorldManager(Tartheside.mono.WorldManager worldManager)
	{
		TWorldManager = worldManager;
	}

	public void InitializeUi()
	{
		UpdateButton = GetNode<Button>("Container/Button");


		GetNode<LineEdit>("Container/WorldSize/x").Text = TWorldManager.WorldSize.X.ToString();
		GetNode<LineEdit>("Container/WorldSize/y").Text = TWorldManager.WorldSize.Y.ToString();

		GetNode<LineEdit>("Container/SquareSize/x").Text = TWorldManager.SquareSize.X.ToString();
		GetNode<LineEdit>("Container/SquareSize/y").Text = TWorldManager.SquareSize.Y.ToString();
		
		GetNode<LineEdit>("Container/ChunkSize/x").Text = TWorldManager.ChunkSize.X.ToString();
		GetNode<LineEdit>("Container/ChunkSize/y").Text = TWorldManager.ChunkSize.Y.ToString();

		GetNode<LineEdit>("Container/Offset/x").Text = TWorldManager.TileMapOffset.X.ToString();
		GetNode<LineEdit>("Container/Offset/y").Text = TWorldManager.TileMapOffset.Y.ToString();

	}

	public void ConnectButtonSignal()
	{
		UpdateButton.Pressed += () => {
			Vector2I newWorldSize = new Vector2I(GetNode<LineEdit>("Container/WorldSize/x").Text.ToInt(), GetNode<LineEdit>("Container/WorldSize/y").Text.ToInt());
			Vector2I offset = new Vector2I(GetNode<LineEdit>("Container/Offset/x").Text.ToInt(), GetNode<LineEdit>("Container/Offset/y").Text.ToInt());
			Vector2I squareSize = new Vector2I(GetNode<LineEdit>("Container/SquareSize/x").Text.ToInt(), GetNode<LineEdit>("Container/SquareSize/y").Text.ToInt());
			Vector2I chunkSize = new Vector2I(GetNode<LineEdit>("Container/ChunkSize/x").Text.ToInt(), GetNode<LineEdit>("Container/ChunkSize/y").Text.ToInt());
			CheckBox displayBorders = GetNode<CheckBox>("Container/DisplayBorders/CheckBox");
			
			TWorldManager.SetWorldSize(newWorldSize);
			TWorldManager.SetTileMapOffset(offset);
			TWorldManager.SetSquareSize(squareSize);
			TWorldManager.SetChunkSize(chunkSize);
			TWorldManager.SetWorldSize(newWorldSize);
			TWorldManager.DisplayBorders = displayBorders.ButtonPressed;
			
			TWorldManager.UpdateTileMap();
		};
	}

}
