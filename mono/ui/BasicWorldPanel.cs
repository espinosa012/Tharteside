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

		GetNode<LineEdit>("Container/Chunks/x").Text = TWorldManager.Chunks.X.ToString();
		GetNode<LineEdit>("Container/Chunks/y").Text = TWorldManager.Chunks.Y.ToString();

		GetNode<LineEdit>("Container/SquareSize/x").Text = TWorldManager.SquareSize.X.ToString();
		GetNode<LineEdit>("Container/SquareSize/y").Text = TWorldManager.SquareSize.Y.ToString();
		
		GetNode<LineEdit>("Container/ChunkSize/x").Text = TWorldManager.ChunkSize.X.ToString();
		GetNode<LineEdit>("Container/ChunkSize/y").Text = TWorldManager.ChunkSize.Y.ToString();

		GetNode<LineEdit>("Container/Offset/x").Text = TWorldManager.TileMapOffset.X.ToString();
		GetNode<LineEdit>("Container/Offset/y").Text = TWorldManager.TileMapOffset.Y.ToString();

		GetNode<LineEdit>("Container/Noise/LineEdit").Text = "";

		// me falta conectar una señal para que cuando cambie el item seleccionado se actualice el lineedit con el valor			GetNode<OptionButton>("Container/WorldParam/MenuButton").AddItem(param) ;
		// además, necesitamos actualizar el parametro al pulsar intro para poder actualizar varios parametros antes de renderizar
		foreach (string param in TWorldManager.GetWorldParameters().Keys)
		{
			GetNode<OptionButton>("Container/WorldParam/MenuButton").AddItem(param) ;
		}
		GetNode<OptionButton>("Container/WorldParam/MenuButton").AddItem("None") ;
	}

	public void ConnectButtonSignal()
	{
		UpdateButton.Pressed += () => {
			var newWorldSize = new Vector2I(GetNode<LineEdit>("Container/WorldSize/x").Text.ToInt(), GetNode<LineEdit>("Container/WorldSize/y").Text.ToInt());
			var offset = new Vector2I(GetNode<LineEdit>("Container/Offset/x").Text.ToInt(), GetNode<LineEdit>("Container/Offset/y").Text.ToInt());
			var chunks = new Vector2I(GetNode<LineEdit>("Container/Chunks/x").Text.ToInt(), GetNode<LineEdit>("Container/Chunks/y").Text.ToInt());
			var squareSize = new Vector2I(GetNode<LineEdit>("Container/SquareSize/x").Text.ToInt(), GetNode<LineEdit>("Container/SquareSize/y").Text.ToInt());
			var chunkSize = new Vector2I(GetNode<LineEdit>("Container/ChunkSize/x").Text.ToInt(), GetNode<LineEdit>("Container/ChunkSize/y").Text.ToInt());
			var displayBorders = GetNode<CheckBox>("Container/DisplayBorders/CheckBox");
			var showWholeWorld = GetNode<CheckBox>("Container/ShowWholeWorld/CheckBox");
			var noiseToDisplay = GetNode<LineEdit>("Container/Noise/LineEdit");
			
			TWorldManager.SetWorldSize(newWorldSize);
			TWorldManager.SetWorldSize(newWorldSize);
			TWorldManager.SetTileMapOffset(offset);
			TWorldManager.SetTileMapChunks(chunks);

			showWholeWorld.ButtonPressed = true;
			if (showWholeWorld.ButtonPressed)
			{
				TWorldManager.SetSquareSize(new Vector2I(1, 1));
				TWorldManager.SetChunkSize(new Vector2I(newWorldSize.X / TWorldManager.Chunks.X, newWorldSize.Y / TWorldManager.Chunks.Y));
				GetNode<CheckBox>("Container/DisplayBorders/CheckBox").ButtonPressed = false;
				TWorldManager.ReloadTileMap();
				return;
			}
			
			TWorldManager.SetSquareSize(squareSize);
			TWorldManager.SetChunkSize(chunkSize);
			
			TWorldManager.ReloadTileMap();
		};
	}

}
